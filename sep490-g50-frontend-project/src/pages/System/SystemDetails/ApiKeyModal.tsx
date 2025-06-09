import React from 'react';
import { Modal, Button, Alert, Space, Typography } from 'antd';
import { KeyOutlined } from '@ant-design/icons';
import { useTranslation } from 'react-i18next';

const { Text } = Typography;

interface ApiKeyModalProps {
    isVisible: boolean;
    apiKey: string;
    onClose: () => void;
}

const ApiKeyModal: React.FC<ApiKeyModalProps> = ({ isVisible, apiKey, onClose }) => {
    const { t } = useTranslation();

    return (
        <Modal
            title={<Space><KeyOutlined />{t('overviewTab.apiKeyGenerated')}</Space>}
            open={isVisible}
            onOk={onClose}
            onCancel={onClose}
            destroyOnClose
            maskClosable={false}
            footer={[
                <Button key="ok" type="primary" onClick={onClose}>
                    {t('overviewTab.understood')}
                </Button>
            ]}
    >
            <Alert
                message={t('overviewTab.apiKeyWarning')}
                description={t('overviewTab.apiKeyWarningDesc')}
                type="warning"
                showIcon
                style={{ marginBottom: 16 }}
            />
            <div style={{
                background: '#f5f5f5',
                padding: '12px',
                borderRadius: '4px',
                wordBreak: 'break-all'
            }}>
                <Text code copyable strong>{apiKey}</Text>
            </div>
        </Modal>
    );
};

export default ApiKeyModal;