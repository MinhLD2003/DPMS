import React, { useEffect, useState } from 'react';
import {
    Form,
    Input,
    Button,
    message,
    Card,
    Modal,
    Tag,
    Select,
} from 'antd';
import { useTranslation } from 'react-i18next';
import QueryStatsOutlinedIcon from '@mui/icons-material/QueryStatsOutlined';
import { impactOptions, probabilityOptions, RiskAssessPutModel, RiskBeforeAssessModel } from '../models/RiskModels';
import { getCellColor, getPriorityLabel, getTextColor } from '../../../utils/RiskMatrixColorUtils';
import AxiosClient from '../../../configs/axiosConfig';

const { Option } = Select;

const AssessRiskModal: React.FC<{ riskInfo: RiskBeforeAssessModel, onUpdateSuccess: () => void, isDisabled: boolean }> = ({ riskInfo, onUpdateSuccess, isDisabled }) => {
    const [form] = Form.useForm();
    const { t } = useTranslation();
    const [loading, setLoading] = useState<boolean>(false);
    const [isModalVisible, setIsModalVisible] = useState(false);
    const [impact, setImpact] = useState(1);
    const [probability, setProbability] = useState(1);
    const [priority, setPriority] = useState(1);

    // Calculate priority based on impact and probability
    useEffect(() => {
        if (isModalVisible) {
            const calculatedPriority = impact * probability;
            setPriority(calculatedPriority);
            form.setFieldsValue({ priority: calculatedPriority });
        }
    }, [impact, probability, form, isModalVisible]);

    const handleSubmit = async () => {
        setLoading(true);
        try {
            const values = form.getFieldsValue();
            const riskAssessment: RiskAssessPutModel = {
                riskImpactAfterMitigation: values.riskImpact,
                riskProbabilityAfterMitigation: values.riskProbability,
            };
            const result = await AxiosClient.put(`/Risk/resolve-risk/${riskInfo.id}`, riskAssessment);
            if (result.status === 200) {
                message.success(t('assess_risk_success'));
                onUpdateSuccess(); // Call the parent function to refresh the data
                setIsModalVisible(false);
            }
        }
        catch (errorInfo: any) {
            message.error("Form validation failed:", errorInfo);
        } finally {
            setLoading(false);
        }
    };

    const showModal = () => {
        setIsModalVisible(true);
        setImpact(riskInfo.riskImpact);
        setProbability(riskInfo.riskProbability);
        setPriority(riskInfo.riskImpact * riskInfo.riskProbability);
        form.setFieldsValue({
            riskImpact: riskInfo.riskImpact,
            riskProbability: riskInfo.riskProbability,
            priority: riskInfo.riskImpact * riskInfo.riskProbability
        });
    };

    const handleCancelModal = () => {
        setIsModalVisible(false);
    };
    const handleImpactChange = (value: number) => {
        setImpact(value);
        form.setFieldsValue({ riskImpact: value });
    };

    const handleProbabilityChange = (value: number) => {
        setProbability(value);
        form.setFieldsValue({ riskProbability: value });
    };

    return (
        <>
            <Button
                type="primary"
                size="small"
                disabled={isDisabled}
                onClick={showModal} icon={<QueryStatsOutlinedIcon />}
            />
            <Modal
                title={t('assess_risk')}
                open={isModalVisible}
                width='50%'
                onCancel={handleCancelModal}
                footer={[
                    <Button key="cancel" onClick={handleCancelModal}>
                        {t('cancel')}
                    </Button>,
                    <Button
                        key="submit"
                        type="primary"
                        loading={loading}
                        onClick={handleSubmit}
                    >
                        {t('assess_risk')}
                    </Button>,
                ]}
            >
                <Form form={form} layout="vertical" onFinish={handleSubmit} initialValues={{
                    riskImpact: 1,
                    riskProbability: 1,
                    priority: 1
                }}>
                    <div>
                        <div className="grid grid-cols-1 lg:grid-cols-5 gap-6">
                            {/* Risk Matrix - Takes 3 columns in large screens */}
                            <div className="lg:col-span-3">
                                <Card
                                    size="small"
                                    className="shadow-sm"
                                >
                                    {/* Add Probability label at the top */}
                                    <div className="flex">
                                        <div className="w-12"></div> {/* Empty space to align with the impact labels column */}
                                        <div className="flex-1">
                                            <div className="text-center font-medium mb-2 text-gray-700">{t('probability')}</div>
                                        </div>
                                    </div>

                                    <div className="flex">
                                        {/* Impact label (rotated vertically) */}
                                        <div className="w-12 relative mt-6">
                                            <div className="absolute -left-7 top-1/2 -translate-y-1/2 -rotate-90 transform whitespace-nowrap font-medium text-gray-700">
                                                {t('impact')}
                                            </div>

                                            {/* Impact values */}
                                            {impactOptions.map((i) => (
                                                <div key={`impact-${i}`} className="h-12 flex items-center justify-center text-xs font-medium">
                                                    {i}
                                                </div>
                                            ))}
                                        </div>

                                        <div className="flex-1">
                                            <div className="grid grid-cols-5 gap-px">
                                                {/* Column headers (probability values) */}
                                                {probabilityOptions.map(p => (
                                                    <div key={`header-${p}`} className="h-6 text-center text-xs font-medium">
                                                        {p}
                                                    </div>
                                                ))}

                                                {/* Matrix cells */}
                                                {impactOptions.map(i =>
                                                    probabilityOptions.map(p => {
                                                        const calculatedPriority = i * p;
                                                        const isSelected = i === impact && p === probability;
                                                        const bgColor = getCellColor(i, p);
                                                        const textColor = getTextColor(bgColor);

                                                        return (
                                                            <div
                                                                key={`cell-${i}-${p}`}
                                                                onClick={() => {
                                                                    handleImpactChange(i);
                                                                    handleProbabilityChange(p);
                                                                }}
                                                                style={{
                                                                    backgroundColor: bgColor,
                                                                    color: textColor,
                                                                    border: isSelected ? '3px solid #1890ff' : '1px solid #f0f0f0',
                                                                    cursor: 'pointer',
                                                                    transition: 'all 0.2s'
                                                                }}
                                                                className="h-12 flex items-center justify-center text-sm font-medium hover:opacity-80"
                                                            >
                                                                {calculatedPriority}
                                                            </div>
                                                        );
                                                    })
                                                ).reduce((acc, curr) => acc.concat(curr), [])}
                                            </div>
                                        </div>
                                    </div>
                                </Card>
                            </div>

                            {/* Selected Risk Values - Takes 2 columns in large screens */}
                            <div className="lg:col-span-2">
                                <Card title={t('selected_risk_value')} size="small" className="shadow-sm h-full">
                                    <div className="flex flex-col gap-4">
                                        <Form.Item
                                            name="riskImpact"
                                            label={<span className="font-medium">{t('impact')}</span>}
                                            tooltip="Higher values indicate greater impact on project outcomes"
                                            className="mb-2"
                                        >
                                            <Select value={impact} onChange={handleImpactChange}>
                                                {impactOptions.map(i => (
                                                    <Option key={i} value={i}>
                                                    {i} - {
                                                        i === 1 ? 'Insignificant' :
                                                        i === 2 ? 'Minor' :
                                                        i === 4 ? 'Moderate' :
                                                        i === 8 ? 'Major' :
                                                        'Severe'
                                                    }
                                                </Option>
                                                ))}
                                            </Select>
                                        </Form.Item>

                                        <Form.Item
                                            name="riskProbability"
                                            label={<span className="font-medium">{t('probability')}</span>}
                                            tooltip="How likely is this risk to occur"
                                            className="mb-2"
                                        >
                                            <Select value={probability} onChange={handleProbabilityChange}>
                                                {probabilityOptions.map(p => (
                                                    <Option key={p} value={p}>
                                                    {p} - {
                                                        p === 1 ? 'Rare' :
                                                        p === 2 ? 'Unlikely' :
                                                        p === 3 ? 'Possible' :
                                                        p === 4 ? 'Likely' :
                                                        'Almost Certain'
                                                    }
                                                </Option>
                                                ))}
                                            </Select>
                                        </Form.Item>

                                        <Form.Item
                                            name="priority"
                                            label={<span className="font-medium">{t('priority_score')}</span>}
                                            className="mb-0"
                                        >
                                            <Input
                                                value={priority}
                                                disabled
                                                addonAfter={getPriorityLabel(priority)}
                                                className="text-lg font-medium"
                                            />
                                        </Form.Item>
                                    </div>
                                </Card>
                            </div>

                        </div>
                    </div>
                </Form>
            </Modal>
        </>
    );
};

export default AssessRiskModal;