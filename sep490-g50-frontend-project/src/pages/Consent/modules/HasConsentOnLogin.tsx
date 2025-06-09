import React, { useMemo, useState } from 'react';
import { Modal, Button, Typography, Space } from 'antd';
import { NavigateFunction } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

const { Title, Text } = Typography;

interface ConsentModalProps {
    visible: boolean;
    onClose: () => void;
    consentUrl?: string;
    type: number;
}
const ConsentModal: React.FC<ConsentModalProps> = ({ visible, onClose, consentUrl, type }) => {
    const { t } = useTranslation();
    const modalTitle = useMemo(() => {
        switch (type) {
            case 1:
                return t("dashboard.noConsentFound");
            case 2:
                return t("dashboard.newConsentRequired");
            default:
                return t("dashboard.consentRequired");
        }
    }, [type, t]);
    return (
        <Modal
            open={visible}
            onCancel={onClose}
            footer={null}
            centered
            width={500}
            styles={{ body: { textAlign: 'center', padding: '24px' } }}
            destroyOnClose
            closable={false}
            maskClosable={false}
        >
            <Space direction="vertical" size="large" style={{ width: '100%' }}>
                <Title level={4}>{t(modalTitle)}</Title>
                {/* <Text strong>{t('consentModal.link')}: {localStorage.getItem("consentUrl")}</Text> */}

                <Button
                    type="primary"
                    size="large"
                    block
                    onClick={() => {
                        onClose(); // Close the modal first
                        window.open(consentUrl+"?fromDPMS=true" || '')

                    }}
                >
                    {t('consentModal.goToLink')}
                </Button>

                {/* {!navigateFn && (
                    <Button
                        type="primary"
                        size="large"
                        block
                        onClick={() => {
                            onClose(); // Close the modal first
                            window.open('/public-banner', '_blank')
                        }}
                    >
                        {t('consentModal.goToLink')}
                    </Button>
                )} */}
                <Button disabled size="middle" block onClick={onClose}>
                    {t('consentModal.close')}
                </Button>
            </Space>

            <Text type="secondary" style={{ display: 'block', marginTop: 16 }}>
                {t('consentModal.privacyNotice')}
            </Text>
        </Modal>
    );
};

export default ConsentModal;