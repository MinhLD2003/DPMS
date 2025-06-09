import React, { useState } from 'react';
import {
  Form,
  Input,
  Button,
  message,
  Card,
  Layout,
  Typography,
  Modal,
} from 'antd';
import { groupSchema, PostGroupModel } from '../models/PostGroupModel';
import { useFormValidation } from '../../../hooks/useFormValidation';
import groupService from '../apis/GroupAPIs';
import { useTranslation } from 'react-i18next';
import { PlusOutlined } from '@ant-design/icons';

const { Text } = Typography;
const { TextArea } = Input;

const CreateGroupModal: React.FC<{onCreateSuccess: () => void }> = ({ onCreateSuccess }) => {
  const { t } = useTranslation();
  const [form] = Form.useForm<PostGroupModel>();
  const [loading, setLoading] = useState<boolean>(false);
  const [isModalVisible, setIsModalVisible] = useState(false);

  const { errors, validateField, validateForm } = useFormValidation<PostGroupModel>(
    form,
    groupSchema,
    { validateOnChange: false }
  );

  const handleSubmit = async () => {
    setLoading(true);
    try {
      if (validateForm()) {
        const values = form.getFieldsValue();
        const result = await groupService.createGroup(values);

        if (result.success) {
          message.success(t('createGroup.successMessage'));
          form.resetFields();
          setIsModalVisible(false); // Close modal on success
          onCreateSuccess(); // Trigger parent reload
        } else {
          message.error(t('createGroup.existed'));
          console.error('Error details:', result.error);
        }
      }
    } catch (errorInfo) {
      console.error('Form validation failed:', errorInfo);
      message.error(t('createGroup.errorMessage'));
    } finally {
      setLoading(false);
    }
  };

  const showModal = () => {
    setIsModalVisible(true);
  };

  const handleCancelModal = () => {
    setIsModalVisible(false);
    form.resetFields(); // Reset the form fields
  };

  return (
    <>
      <Button type="primary" onClick={showModal} icon={<PlusOutlined />}>
        {t('create_group_btn')}
      </Button>

      <Modal
        title={t('new_group')}
        visible={isModalVisible}
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
            {t('create_group_btn')}
          </Button>,
        ]}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
        >
          <Form.Item
            label={<Text strong>{t('groupname')}</Text>}
            name="name"
            validateStatus={errors.name ? 'error' : ''}
            help={errors.name}
          >
            <Input
              onBlur={() => validateField('name')}
              placeholder={t('name_placeholder')}
            />
          </Form.Item>

          <Form.Item
            label={<Text strong>{t('group.description')}</Text>}
            name="description"
            validateStatus={errors.description ? 'error' : ''}
            help={errors.description}
          >
            <TextArea
              onBlur={() => validateField('description')}
              placeholder={t('description_placeholder')}
              autoSize={{ minRows: 3, maxRows: 6 }}
            />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
};

export default CreateGroupModal;