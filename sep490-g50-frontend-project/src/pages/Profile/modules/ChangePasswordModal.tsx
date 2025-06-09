import React, { useContext, useState } from 'react';
import { Modal, Form, Input, Button, message } from 'antd';
import { LockOutlined } from '@ant-design/icons';
import { ChangePasswordPayload, changePasswordSchema } from '../models/ChangePasswordModel';
import { useFormValidation } from '../../../hooks/useFormValidation';
import AxiosClient from '../../../configs/axiosConfig';
import { AuthContext } from '../../../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

interface PasswordChangeModalProps {
    visible: boolean;
    onCancel: () => void;
}

const PasswordChangeModal: React.FC<PasswordChangeModalProps> = ({ visible, onCancel }) => {
    const { t } = useTranslation();
    const [form] = Form.useForm<ChangePasswordPayload>();
    const [loading, setLoading] = useState(false);
    const { errors, validateField, validateForm } = useFormValidation<ChangePasswordPayload>(form, changePasswordSchema, { validateOnChange: false });
    const { logout } = useContext(AuthContext)!; // Get logout function from context
    const navigate = useNavigate();
    const handleSubmit = async () => {
        try {
            setLoading(true);
            if (validateForm()) {
                const values = form.getFieldsValue();
                const response = await AxiosClient.post('/User/change-password', values);

                message.success(t('changePassword.successMessage'));
                form.resetFields();
                onCancel();

                Modal.success({
                    title: t('changePassword.modalTitle'),
                    content: t('changePassword.modalContent'),
                    okText: t('changePassword.modalOkText'),
                    onOk: () => {
                        logout(); // Clear JWT, user state
                        navigate('/login'); // Redirect to login page
                    },
                });
            }
        } catch (error) {
            console.error('Validation failed:', error);
            message.error(t('changePassword.errorMessage')); //  Old password is incorrect
        } finally {
            setLoading(false);
        }
    };

    return (
        <Modal
            title={t("pw_change_modal.title")}
            open={visible}
            onCancel={onCancel}
            footer={[
                <Button key="cancel" onClick={onCancel}>
                    {t("pw_change_modal.cancel")}
                </Button>,
                <Button key="submit" type="primary" loading={loading} onClick={handleSubmit}>
                    {t("pw_change_modal.update")}
                </Button>
            ]}
        >
            <Form form={form} layout="vertical" name="password_change_form">
                <Form.Item
                    name="oldPassword"
                    label={t("pw_change_form.currentPassword")}
                    validateStatus={errors.oldPassword ? "error" : ""}
                    help={errors.oldPassword}
                >
                    <Input.Password
                        onBlur={() => validateField('oldPassword')}
                        prefix={<LockOutlined />}
                        placeholder={t("pw_change_form.currentPasswordPlaceholder")} />
                </Form.Item>

                <Form.Item
                    name="newPassword"
                    label={t("pw_change_form.newPassword")}
                    validateStatus={errors.newPassword ? "error" : ""}
                    help={errors.newPassword}
                >
                    <Input.Password
                        onBlur={() => validateField('newPassword')}
                        prefix={<LockOutlined />}
                        placeholder={t("pw_change_form.newPasswordPlaceholder")} />
                </Form.Item>

                <Form.Item
                    name="confirmPassword"
                    label={t("pw_change_form.confirmNewPassword")}
                    validateStatus={errors.confirmPassword ? "error" : ""}
                    help={errors.confirmPassword}
                >
                    <Input.Password
                        onBlur={() => validateField('confirmPassword')}
                        prefix={<LockOutlined />}
                        placeholder={t("pw_change_form.confirmNewPasswordPlaceholder")} />
                </Form.Item>
            </Form>
        </Modal>
    );
};

export default PasswordChangeModal;