import { Modal, Button } from 'antd';
import { useTranslation } from 'react-i18next';

interface VersionUpdateProps {
    visible?: boolean; // Disable button
    onCancel?: () => void; // For form integration
    onConfirm?: () => void; // For form integration   
}

const VersionUpdateModal: React.FC<VersionUpdateProps> = ({
    visible = false,
    onCancel = () => { }, // Default to no-op function
    onConfirm = () => { }, // Default to no-op function
}) => {
    const { t } = useTranslation();
    return (
        <Modal
            title={t('ficTemplateConfig.updateConfirmation') || "Form Version Update"}
            open={visible}
            onCancel={onCancel}
            footer={[
                <Button key="cancel" onClick={onCancel}>
                    {t('ficTemplateConfig.cancel') || "Cancel"}
                </Button>,
                <Button key="submit" type="primary" onClick={onConfirm}>
                    {t('ficTemplateConfig.proceed') || "Create New Version"}
                </Button>
            ]}
        >
            <div className="py-2">
                <text className="mb-3">{t('ficTemplateConfig.updateExplanation') || "This action will create a new form with an incremented version based on your changes."}</text>
                <text className="mb-3">{t('ficTemplateConfig.versionExplanation') || "The original form will remain unchanged and accessible in the system."}</text>
                <br />
                <text className="text-blue-600 font-medium">{t('ficTemplateConfig.confirmQuestion') || "Do you want to proceed with creating this new version?"}</text>
            </div>
        </Modal>
    );
};

export default VersionUpdateModal;