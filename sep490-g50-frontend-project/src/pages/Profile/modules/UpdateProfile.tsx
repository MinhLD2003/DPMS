import React, { useState, useEffect } from 'react';
import {
    Form,
    Input,
    Button,
    message,
    Typography,
    Modal,
    DatePicker,
} from 'antd';
import { useFormValidation } from '../../../hooks/useFormValidation';
import { useTranslation } from 'react-i18next';
import { SaveOutlined, EditOutlined } from '@ant-design/icons';
import { ProfilePostModel, profileSchema } from '../models/ProfileModel';
import AxiosClient from '../../../configs/axiosConfig';
import dayjs from 'dayjs';

const { TextArea } = Input;
const { Text } = Typography;


const UpdateProfileModal: React.FC<{ profile: ProfilePostModel; onUpdateSuccess: () => void }> = ({ profile, onUpdateSuccess }) => {
    const { t } = useTranslation();
    const [form] = Form.useForm<ProfilePostModel>();
    const { errors, validateField, validateForm } = useFormValidation<ProfilePostModel>(
        form,
        profileSchema,
        { validateOnChange: false }
    );
    const [loading, setLoading] = useState<boolean>(false);
    const [isModalVisible, setIsModalVisible] = useState(false);

    useEffect(() => {
        if (profile && isModalVisible) {
            const formValues = {
                email: profile.email,
                fullName: profile.fullName,
                userName: profile.userName,
                dob: dayjs(profile.dob),  // Convert string to dayjs object
            };

            form.setFieldsValue(formValues);
        }
    }, [profile, form, isModalVisible]);

    const handleSubmit = async () => {
        setLoading(true);
        try {
            if (validateForm()) {
                const values = form.getFieldsValue();
                const payload = {
                    ...values,
                    dob: values.dob ? values.dob.toISOString() : null, // Ensure proper format
                };
                // Submit the values directly - the form already has the correct date string format
                const result = await AxiosClient.put(`/Account/profile`, payload);

                if (result) {
                    message.success(t('updateProfile.updateSuccess'));
                    setIsModalVisible(false);
                    onUpdateSuccess(); // Trigger parent reload
                } else {
                    message.error(t('updateProfile.updateFailed'));
                }
            }
        } catch (errorInfo) {
            console.error('Update error:', errorInfo);
            message.error(t('updateProfile.updateFailed'));
        } finally {
            setLoading(false);
        }
    };

    const showModal = () => {
        setIsModalVisible(true);
    };

    const handleCancelModal = () => {
        setIsModalVisible(false);
        form.resetFields(); // Reset form when modal is closed
    };

    // Function to convert a string date to Dayjs object if valid


    return (
        <>
            <Button type="primary" icon={<EditOutlined />} onClick={showModal} size='middle'>
                {t('edit')}
            </Button>

            <Modal
                width='50%'
                title={t('updateProfile.title')}
                open={isModalVisible}
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
                        icon={<SaveOutlined />}
                    >
                        {t('update_profile_btn')}
                    </Button>,
                ]}
            >
                <Form
                    form={form}
                    layout="vertical"
                    requiredMark="optional"
                    validateMessages={{ required: t('form.required') }}
                    className="space-y-6"
                >
                    <Form.Item
                        label={<Text strong>{t('email')}</Text>}
                        name="email"
                        validateStatus={errors.email ? 'error' : ''}
                        help={errors.email}
                        required
                    >
                        <Input
                            onBlur={() => validateField('email')}
                            placeholder={t('updateProfile.emailInputPlaceholder', 'Enter email')}
                            maxLength={100}
                            disabled={true}
                        />
                    </Form.Item>

                    <Form.Item
                        label={<Text strong>{t('full_name')}</Text>}
                        name="fullName"
                        validateStatus={errors.fullName ? 'error' : ''}
                        help={errors.fullName}
                        required
                    >
                        <Input
                            onBlur={() => validateField('fullName')}
                            placeholder={t('updateProfile.nameInputPlaceholder', 'Enter new full name')}
                            maxLength={100}
                            showCount
                        />
                    </Form.Item>

                    <Form.Item
                        label={<Text strong>{t('user_name')}</Text>}
                        name="userName"
                        validateStatus={errors.userName ? 'error' : ''}
                        help={errors.userName}
                        required
                    >
                        <Input
                            onBlur={() => validateField('userName')}
                            placeholder={t('updateProfile.userNameInputPlaceholder', 'Enter new username')}
                            maxLength={50}
                            showCount
                        />
                    </Form.Item>

                    <Form.Item
                        label={<Text strong>{t('dob')}</Text>}
                        name="dob"
                        validateStatus={errors.dob ? 'error' : ''}
                        help={errors.dob}
                    >
                        <DatePicker
                            style={{ width: "100%" }}
                            format="YYYY-MM-DD"
                            onBlur={() => validateField('dob')}
                            onChange={(date) => form.setFieldValue('dob', date)} // Set the new value
                        />
                    </Form.Item>

                </Form>
            </Modal>
        </>
    );
};

export default UpdateProfileModal;