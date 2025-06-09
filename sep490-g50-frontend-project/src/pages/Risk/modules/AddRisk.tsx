import React, { useState, useEffect, useCallback } from 'react';
import { Form, Input, Select, Button, Alert, Layout, Card, Space, Typography, Tooltip, message, Divider, Badge, Tag, Col, Row, DatePicker } from 'antd';
import AxiosClient from '../../../configs/axiosConfig';
import PageHeader from '../../../components/common/PageHeader';
import { useNavigate } from 'react-router-dom';
import MatrixGuide from '../../../components/forms/MatrixGuide';
import { InfoCircleOutlined, ExclamationCircleOutlined, CheckCircleOutlined } from '@ant-design/icons';
import { categoryOptions, impactOptions, probabilityOptions, strategyOptions } from '../models/RiskModels';
import { getCellColor, getPriorityLabel, getTextColor } from '../../../utils/RiskMatrixColorUtils';
import { useTranslation } from 'react-i18next';
import dayjs from 'dayjs';

const { TextArea } = Input;
const { Option } = Select;
const { Title, Text, Paragraph } = Typography;

const RiskForm: React.FC = () => {
    const [form] = Form.useForm();
    const [submitStatus, setSubmitStatus] = useState<'success' | 'error' | null>(null);
    const [errorMessage, setErrorMessage] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [impact, setImpact] = useState(1);
    const [probability, setProbability] = useState(1);
    const [priority, setPriority] = useState(1);
    const navigate = useNavigate();
    const { t } = useTranslation();


    useEffect(() => {
        const calculatedPriority = impact * probability;
        setPriority(calculatedPriority);
        form.setFieldsValue({ priority: calculatedPriority });
    }, [impact, probability, form]);

    const handleImpactChange = (value: number) => {
        setImpact(value);
        form.setFieldsValue({ riskImpact: value });
    };

    const handleProbabilityChange = (value: number) => {
        setProbability(value);
        form.setFieldsValue({ riskProbability: value });
    };

    const handleSubmit = async (values: any) => {
        setIsSubmitting(true);
        setSubmitStatus(null);
        setErrorMessage('');

        try {
            console.log('Form values:', values);
            const response = await AxiosClient.post('/Risk', {
                ...values,
                riskImpact: Number(values.riskImpact),
                riskProbability: Number(values.riskProbability),
                priority: Number(values.priority),
                raisedAt: values.raisedAt ? values.raisedAt.format('YYYY-MM-DD') : null, // Format only date part
            });
            setSubmitStatus('success');
            message.success(t('riskAssessmentSubmittedSuccessfully'));
            form.resetFields();
            setImpact(1);
            setProbability(1);
            setPriority(1);
            navigate(-1);
        } catch (error) {
            setSubmitStatus('error');
            setErrorMessage(t('failedSubmitRiskAssessment'));
            message.error(t('failedSubmitRiskAssessment'));
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <Layout style={{ minHeight: '100vh', backgroundColor: '#f5f7fa' }}>
            <PageHeader title={t('riskAssessment')} />
            <div className="px-4 py-6">
                <Card className="w-full shadow-md rounded-lg mx-auto">
                    {submitStatus === 'success' && <Alert message={t('riskAssessmentSubmittedSuccessfully')} type="success" showIcon className="mb-4" />}
                    {submitStatus === 'error' && <Alert message={errorMessage} type="error" showIcon className="mb-4" />}

                    <Form form={form} layout="vertical" onFinish={handleSubmit} initialValues={{
                        category: 0,
                        strategy: 0,
                        riskImpact: 1,
                        riskProbability: 1,
                        priority: 1
                    }}>
                        <Space direction="vertical" size="large" className="w-full">
                            <div>
                                <Title level={4}>{t('riskDetails')}</Title>
                                <Divider className="my-2" />
                                <Row gutter={16}>
                                    <Col xs={24} sm={8}>
                                        <Form.Item
                                            name="riskName"
                                            label={t('riskName')}
                                            rules={[{ required: true, message: t('riskNameRequired') }]}
                                            tooltip={t('provideClearConciseName')}
                                        >
                                            <Input placeholder="e.g., Integration Delay" />
                                        </Form.Item>
                                    </Col>
                                    <Col xs={24} sm={8}>

                                        <Form.Item
                                            name="category"
                                            label={t('category')}
                                            tooltip={t('selectAppropriateCategory')}
                                        >
                                            <Select placeholder={t('category')}>
                                                {categoryOptions.map((label, index) => (
                                                    <Option key={index} value={index}>{label}</Option>
                                                ))}
                                            </Select>
                                        </Form.Item>
                                    </Col>
                                    <Col xs={24} sm={8}>
                                        <Form.Item
                                            name="raisedAt"
                                            label={t('raisedAt')}
                                            rules={[{ required: true, message: t('raisedAtRequired') }]}
                                            tooltip={t('specifyWhenRiskIdentified')}
                                            getValueProps={(value) => ({
                                                value: value ? dayjs(value) : null, // depends if you use moment or dayjs
                                            })}
                                        >
                                            <DatePicker placeholder={t("select_date")} style={{ width: '100%' }} 
                                            disabledDate={(current) => {
                                                // Disable dates after today
                                                return current && current > dayjs().endOf('day');
                                              }}
                                            />
                                        </Form.Item>

                                    </Col>
                                </Row>


                            </div>

                            <div>
                                <Title level={4}>{t('riskAnalysisMatrix')}</Title>
                                <Divider className="my-2" />
                                <div className="grid grid-cols-1 lg:grid-cols-5 gap-6">
                                    {/* Risk Matrix - Takes 3 columns in large screens */}
                                    <div className="lg:col-span-3">
                                        <Card
                                            title={
                                                <div className="flex items-center">
                                                    <span>{t('impactVsProbabilityMatrix')}</span>
                                                    <Tooltip title={t('matrixGuideTooltip')}>
                                                        <InfoCircleOutlined className="ml-2 text-blue-500" />
                                                    </Tooltip>
                                                </div>
                                            }
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
                                        <Card title={t('selectedRiskValues')} size="small" className="shadow-sm h-full">
                                            <div className="flex flex-col gap-4">
                                                <Form.Item
                                                    name="riskImpact"
                                                    label={<span className="font-medium">{t('impact')}</span>}
                                                    tooltip={t('higherValuesIndicateGreaterImpact')}
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
                                                    tooltip={t('howLikelyRiskOccur')}
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
                                                    label={<span className="font-medium">{t('priorityScore')}</span>}
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
                                <MatrixGuide />

                            </div>

                            <div>
                                <Title level={4}>{t('responsePlan')}</Title>
                                <Divider className="my-2" />
                                <Row gutter={16}>
                                    <Col xs={24} sm={12}>

                                        <Form.Item
                                            name="strategy"
                                            label={t('responseStrategy')}
                                            tooltip={t('selectHowPlanAddressRisk')}
                                        >
                                            <Select>
                                                {strategyOptions.map((label, index) => (
                                                    <Option key={index} value={index}>
                                                        <div className="flex items-center">
                                                            {label}
                                                            {label === 'Mitigate' && <Tag color="blue" className="ml-2">Recommended</Tag>}
                                                        </div>
                                                    </Option>
                                                ))}
                                            </Select>
                                        </Form.Item>
                                    </Col>
                                    <Col xs={24} sm={12}>
                                        <Form.Item
                                            name="riskOwner"
                                            label={t('riskOwner')}
                                            rules={[{ required: true, message: t('whoResponsibleForManagingRisk') }]}
                                            tooltip={t('whoResponsibleForManagingRisk')}
                                        >
                                            <Input placeholder="Risk Owner Email" />
                                        </Form.Item>
                                    </Col>
                                </Row>
                                <Row gutter={16}>
                                    <Col xs={24} sm={12}>
                                        <Form.Item
                                            name="riskContingency"
                                            label={t('riskContingencyPlan')}
                                            rules={[{ required: true, message: t('riskContigencyNeeded') }]}
                                            tooltip={t('riskContingencyPlan')}
                                            className="h-full"
                                        >
                                            <TextArea
                                                rows={9}
                                                placeholder={t('describeContingencyPlan')}
                                                className="h-full"
                                            />
                                        </Form.Item>
                                    </Col>
                                    <Col xs={24} sm={12}>

                                        <Form.Item
                                            name="mitigation"
                                            label={t('mitigationPlan')}
                                            rules={[{ required: true, message: t('riskMitigationNeeded') }]}
                                            tooltip={t('mitigationPlan')}
                                        >
                                            <TextArea
                                                rows={9}
                                                placeholder={t('describeStepsToReduceRisk')}
                                            />
                                        </Form.Item>
                                    </Col>
                                </Row>
                            </div>

                            <Divider />

                            <Form.Item className="mb-0">
                                <div className="flex justify-end gap-4">
                                    <Button
                                        onClick={() => navigate(-1)}
                                        size="middle"
                                    >
                                        {t('cancel')}
                                    </Button>
                                    <Button
                                        type="primary"
                                        htmlType="submit"
                                        loading={isSubmitting}
                                        size="middle"
                                        icon={<CheckCircleOutlined />}
                                    >
                                        {t('submitRiskAssessment')}
                                    </Button>
                                </div>
                            </Form.Item>
                        </Space>
                    </Form>
                </Card>
            </div>
        </Layout>
    );
};

export default RiskForm;