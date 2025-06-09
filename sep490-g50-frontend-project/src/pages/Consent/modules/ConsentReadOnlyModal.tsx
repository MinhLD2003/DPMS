import React, { useState, useEffect, useCallback } from 'react';
import { Typography, List, Button, Modal, Spin, Tag, Descriptions, Collapse, Space, Tooltip, Empty, message } from "antd";
import { EyeOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import dayjs from "dayjs";
import { useParams } from "react-router-dom";
import consentService from "../apis/ConsentAPIs";
import { PurposeViewModel } from "../models/PurposeViewModel";

const { Text, Title, Paragraph } = Typography;

// This component can be added to your existing ConsentManagementTab or used separately
const ConsentReadOnlyModal: React.FC = () => {
    const { t } = useTranslation();
    const { id } = useParams<{ id: string }>();

    const [isModalVisible, setIsModalVisible] = useState(false);
    const [loading, setLoading] = useState(false);
    const [consentPurposes, setConsentPurposes] = useState<PurposeViewModel[]>([]);

    if (!id) return <Empty description={t('consentReadOnlyModal.noSystemIdProvided')} />;

    const fetchConsentPurposes = useCallback(async () => {
        setLoading(true);
        try {
            const result = await consentService.getSystemPurposes(id);
            if (result.success) {
                setConsentPurposes(Array.isArray(result.purposeList) ? result.purposeList : []);
            } else {
                message.error(t('consentReadOnlyModal.fetchPurposesFailed'));
            }
        }
        catch (error) {
            message.error(t('consentReadOnlyModal.fetchPurposesFailed'));
        }
        finally {
            setLoading(false);
        }
    }, [id, t]);

    const showModal = () => {
        setIsModalVisible(true);
        fetchConsentPurposes();
    };
    const handleCancel = () => {
        setIsModalVisible(false);
    };


    const formatDate = (dateString: string): string => {
        return dayjs(dateString).format('DD/MM/YYYY HH:mm');
    };

    return (
        <>
            <Button
                type="primary"
                icon={<EyeOutlined />}
                onClick={showModal}
                style={{
                    backgroundColor: "#1890ff",
                    borderColor: "#1890ff"
                }}
            >
                {t('consentReadOnlyModal.viewConsentStructure')}
            </Button>

            <Modal
                title={<Title level={4}>{t('consentReadOnlyModal.consentPurposesReadOnly')}</Title>}
                open={isModalVisible}
                footer={null}
                onCancel={handleCancel}
                width={800}
            >
                {loading ? (
                    <div style={{ textAlign: 'center', padding: '40px 0' }}>
                        <Spin size="large" tip={t('consentReadOnlyModal.loadingPurposes')} />
                    </div>
                ) : (
                    <>
                        <Paragraph style={{ marginBottom: 16 }}>
                            {t('consentReadOnlyModal.readOnlyDescription')}
                        </Paragraph>

                        <Descriptions
                            bordered
                            size="small"
                            layout="vertical"
                            style={{ marginBottom: 24 }}
                        >
                            <Descriptions.Item label={t('consentReadOnlyModal.totalPurposes')}>
                                {consentPurposes.length}
                            </Descriptions.Item>
                            <Descriptions.Item label={t('consentReadOnlyModal.activePurposes')}>
                                {consentPurposes.filter(p => p.status === "Active").length}
                            </Descriptions.Item>
                            <Descriptions.Item label={t('consentReadOnlyModal.lastUpdated')}>
                                {consentPurposes.length > 0 ? formatDate(
                                    consentPurposes.reduce((latest, purpose) =>
                                        new Date(purpose.lastModifiedAt) > new Date(latest) ? purpose.lastModifiedAt : latest,
                                        consentPurposes[0].lastModifiedAt
                                    )
                                ) : 'N/A'}
                            </Descriptions.Item>
                        </Descriptions>

                        <List
                            itemLayout="vertical"
                            dataSource={consentPurposes}
                            renderItem={purpose => (
                                <List.Item
                                    key={purpose.id}
                                    extra={
                                        <Space direction="vertical" align="end">
                                            <Tag color={purpose.status === "Active" ? "green" : "red"}>
                                                {purpose.status}
                                            </Tag>
                                            <Text type="secondary" style={{ fontSize: '12px' }}>
                                                {formatDate(purpose.lastModifiedAt)}
                                            </Text>
                                        </Space>
                                    }
                                >
                                    <List.Item.Meta
                                        title={
                                            <Space>
                                                <Text strong>{purpose.name}</Text>
                                                <Text type="secondary" style={{ fontSize: '12px' }}>
                                                    ID: {purpose.id}
                                                </Text>
                                            </Space>
                                        }
                                        description={purpose.description}
                                    />
                                </List.Item>
                            )}
                            style={{
                                border: '1px solid #f0f0f0',
                                borderRadius: '4px',
                                padding: '0 12px'
                            }}
                        />
                    </>
                )}
            </Modal>
        </>
    );
};

export default ConsentReadOnlyModal;