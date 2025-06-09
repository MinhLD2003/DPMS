import React, { useState } from 'react';
import {
  Form,
  Input,
  Button,
  message,
  Checkbox,
  Select,
  Card,
  Space,
  Typography,
  Tooltip,
  Modal,
  Layout,
} from 'antd';
import {
  UserOutlined,
  MailOutlined,
  LockOutlined,
  InfoCircleOutlined,
  SafetyOutlined,
  PlusOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useFormValidation } from '../../../hooks/useFormValidation';
import { accountSchema, PostAccountModel } from '../models/PostAccountModel';
import accountService from '../apis/AccountAPIs';
import { NumberedUserStatus } from '../../../enum/enum';
import { useTranslation } from 'react-i18next';

const { Text } = Typography;
const { Option } = Select;

const CreateAccountModal: React.FC<{ onCreateSuccess: () => void }> = ({ onCreateSuccess }) => {
  const { t } = useTranslation();
  const [form] = Form.useForm<PostAccountModel>();
  const [loading, setLoading] = useState<boolean>(false);
  const { errors, validateForm, validateField } = useFormValidation<PostAccountModel>(form, accountSchema, {
    validateOnChange: false,
  });
  const [isModalVisible, setIsModalVisible] = useState(false);
  const navigate = useNavigate();

  const showModal = () => {
    setIsModalVisible(true);
  };

  const handleCancelModal = () => {
    setIsModalVisible(false);
    form.resetFields(); // Clear the form when closing the modal
  };

  const handleSubmit = async () => {
    setLoading(true);
    try {
      if (validateForm()) {
        const values = form.getFieldsValue();
        const result = await accountService.createAccount(values);

        if (result.success) {
          message.success(t('account_created_success'));
          form.resetFields();
          setIsModalVisible(false); // Close modal on successful submission
          onCreateSuccess(); // Trigger parent reload
        } else if (result.error) {
          message.error(t('account_already_exists'));
        } else {
          message.error(t('account_create_failed'));
        }
      }
    } catch (errorInfo) {
      console.error('Form validation failed:', errorInfo);
      message.error(t('account_create_failed'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <Button type="primary" onClick={showModal} icon={<PlusOutlined />}>
        {t('create_account')}
      </Button>

      <Modal
        title={t('create_account')}
        visible={isModalVisible}
        width='50%'
        onCancel={handleCancelModal}
        footer={[
          <Button key="cancel" onClick={handleCancelModal}>
            {t('cancel')}
          </Button>,
          <Button key="submit" type="primary" loading={loading} onClick={handleSubmit}>
            {t('create')}
          </Button>,
        ]}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          initialValues={{
            status: NumberedUserStatus.Activated,
          }}
          className="space-y-4"
        >
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <Form.Item
              label={<Text strong>{t('full_name')}</Text>}
              name="fullName"
              validateStatus={errors.fullName ? 'error' : ''}
              help={errors.fullName}
              tooltip={t('full_name_tooltip', "Enter user's full name")}
            >
              <Input
                onBlur={() => validateField('fullName')}
                prefix={<UserOutlined className="text-gray-400" />}
                placeholder={t('full_name_placeholder', 'Enter full name')}
                maxLength={100}
                showCount
              />
            </Form.Item>
            <Form.Item
              label={<Text strong>{t('email')}</Text>}
              name="email"
              validateStatus={errors.email ? 'error' : ''}
              help={errors.email}
              tooltip={t('email_tooltip', 'Enter a valid email address')}
            >
              <Input
                onBlur={() => validateField('email')}
                prefix={<MailOutlined className="text-gray-400" />}
                placeholder={t('email_placeholder', 'example@domain.com')}
                type="email"
              />
            </Form.Item>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <Form.Item
              label={<Text strong>{t('user_name')}</Text>}
              name="username"
              validateStatus={errors.username ? 'error' : ''}
              help={errors.username}
              tooltip={t('username_tooltip', 'Username for login')}
            >
              <Input
                onBlur={() => validateField('username')}
                prefix={<UserOutlined className="text-gray-400" />}
                placeholder={t('username_placeholder', 'Enter username')}
              />
            </Form.Item>

            <Form.Item
              label={<Text strong>{t('password')}</Text>}
              name="password"
              validateStatus={errors.password ? 'error' : ''}
              help={errors.password}
              tooltip={
                <div>
                  {t('password_tooltip', 'Password must contain:')}
                  <ul className="pl-5 mt-1">
                    <li>{t('password_requirement_1', 'At least 8 characters')}</li>
                    <li>{t('password_requirement_2', 'Uppercase and lowercase letters')}</li>
                    <li>{t('password_requirement_3', 'At least one number')}</li>
                  </ul>
                </div>
              }
            >
              <Input.Password
                onBlur={() => validateField('password')}
                prefix={<LockOutlined className="text-gray-400" />}
                placeholder={t('password_placeholder', 'Enter password')}
              />
            </Form.Item>
          </div>
          <Form.Item
            label={<Text strong>{t('status')}</Text>}
            name="status"
            validateStatus={errors.status ? 'error' : ''}
            help={errors.status}
            tooltip={t('status_tooltip', 'Set the initial account status')}
          >
            <Select
              placeholder={t('select_status')}
              suffixIcon={<SafetyOutlined />}
              onBlur={() => validateField('status')}
            >
              <Option value={NumberedUserStatus.Activated}>
                <span className="text-green-600">{t('activate')}</span>
              </Option>
              <Option value={NumberedUserStatus.Deactivated}>
                <span className="text-red-600">{t('deactivate')}</span>
              </Option>
            </Select>
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
};
export default CreateAccountModal;