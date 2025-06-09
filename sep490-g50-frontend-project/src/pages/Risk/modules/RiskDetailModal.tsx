import React, { useState } from 'react';
import { Modal, Space, Typography, Progress, Descriptions, Button, Tag } from 'antd';
import { getImpactColor, getProbabilityColor, getRiskColor, getScoreStepPercent, getImpactStepPercent } from '../../../utils/RiskMatrixColorUtils';
import { InfoCircleOutlined } from '@ant-design/icons';
import { useTranslation } from 'react-i18next';

const { Text } = Typography;

interface RiskDetailsModalProps {
    risk: any;
}

const RiskDetailsModal: React.FC<RiskDetailsModalProps> = ({ risk }) => {
    const { t } = useTranslation();
    const [isModalVisible, setIsModalVisible] = useState(false);

    const showModal = () => setIsModalVisible(true);
    const handleClose = () => setIsModalVisible(false);

    return (
        <>
            <Button
                size="small"
                icon={<InfoCircleOutlined />}
                type='primary'
                onClick={showModal}
            />
            <Modal
                title={t('riskDetailsModal.title')}
                open={isModalVisible}
                onCancel={handleClose}
                footer={null}
                width={700}
            >
                <Descriptions bordered column={1}>
                    <Descriptions.Item label={t('riskDetailsModal.riskID')}>{risk.id}</Descriptions.Item>
                    <Descriptions.Item label={t('riskDetailsModal.riskAssessment')}>
                        <Space direction="vertical" size="small" style={{ width: '100%' }}>
                            <Space>
                                <Text type="secondary">{t('riskDetailsModal.impact')}:</Text>
                                <Progress
                                    percent={getImpactStepPercent(risk.riskImpact)}
                                    steps={5}
                                    strokeColor={getImpactColor(risk.riskImpact)}
                                    size="small"
                                    showInfo={false}
                                />
                                <Text>{risk.riskImpact}</Text>
                            </Space>
                            <Space>
                                <Text type="secondary">{t('riskDetailsModal.probability')}:</Text>
                                <Progress
                                    percent={risk.riskProbability * 20}
                                    steps={5}
                                    strokeColor={getProbabilityColor(risk.riskProbability)}
                                    size="small"
                                    showInfo={false}
                                />
                                <Text>{risk.riskProbability}</Text>
                            </Space>
                            <Space>
                                <Text type="secondary">{t('riskDetailsModal.score')}:</Text>
                                <Progress
                                    percent={getScoreStepPercent(risk.score)}
                                    steps={5}
                                    strokeColor={getRiskColor(risk.score)}
                                    size="small"
                                    showInfo={false}
                                />
                                <Text>{risk.score}</Text>
                            </Space>
                        </Space>
                    </Descriptions.Item>
                    <Descriptions.Item label={t('riskDetailsModal.riskAfterMitigation')}>
                        {risk.riskImpactAfterMitigation == 0 && <Tag color='red'>{t('riskDetailsModal.unresolved')}</Tag>}
                        {risk.riskImpactAfterMitigation !== 0 &&
                            <Space direction="vertical" size="small" style={{ width: '100%' }}>
                                <Space>
                                    <Text type="secondary">{t('riskDetailsModal.impact')}:</Text>
                                    <Progress
                                        percent={getImpactStepPercent(risk.riskImpactAfterMitigation)}
                                        steps={5}
                                        strokeColor={getImpactColor(risk.riskImpactAfterMitigation)}
                                        size="small"
                                        showInfo={false}
                                    />
                                    <Text>{risk.riskImpactAfterMitigation}</Text>
                                </Space>
                                <Space>
                                    <Text type="secondary">{t('riskDetailsModal.probability')}:</Text>
                                    <Progress
                                        percent={risk.riskProbabilityAfterMitigation * 20}
                                        steps={5}
                                        strokeColor={getProbabilityColor(risk.riskProbabilityAfterMitigation)}
                                        size="small"
                                        showInfo={false}
                                    />
                                    <Text>{risk.riskProbabilityAfterMitigation}</Text>
                                </Space>
                                <Space>
                                    <Text type="secondary">{t('riskDetailsModal.score')}:</Text>
                                    <Progress
                                        percent={getScoreStepPercent(risk.scoreAfterMitigation)}
                                        steps={5}
                                        strokeColor={getRiskColor(risk.scoreAfterMitigation)}
                                        size="small"
                                        showInfo={false}
                                    />
                                    <Text>{risk.scoreAfterMitigation}</Text>
                                </Space>
                            </Space>
                        }
                    </Descriptions.Item>
                    <Descriptions.Item label={t('riskDetailsModal.contingencyPlan')}>{risk.riskContingency}</Descriptions.Item>
                    <Descriptions.Item label={t('riskDetailsModal.mitigationPlan')}>{risk.mitigation}</Descriptions.Item>
                </Descriptions>
            </Modal>
        </>
    );
};

export default RiskDetailsModal;