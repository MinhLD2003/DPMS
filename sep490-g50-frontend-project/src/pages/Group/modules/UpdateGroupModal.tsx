import React, { useState, useEffect } from 'react';
import {
  Form,
  Input,
  Button,
  message,
  Typography,
  Modal,
  Spin,
} from 'antd';
import { useFormValidation } from '../../../hooks/useFormValidation';
import { PostGroupModel, groupSchema } from '../models/PostGroupModel';
import groupService from '../apis/GroupAPIs';
import { useTranslation } from 'react-i18next';
import { SaveOutlined, EditOutlined } from '@ant-design/icons';

const { TextArea } = Input;
const { Text } = Typography;

const UpdateGroupModal: React.FC<{ id: string; onUpdateSuccess: () => void }> = ({ id, onUpdateSuccess }) => {
  const { t } = useTranslation();
  const [form] = Form.useForm<PostGroupModel>();
  const { errors, validateField, validateForm } = useFormValidation<PostGroupModel>(
    form,
    groupSchema,
    { validateOnChange: false }
  );
  const [loading, setLoading] = useState<boolean>(false);
  const [initialLoading, setInitialLoading] = useState<boolean>(true);
  const [isModalVisible, setIsModalVisible] = useState(false);

  useEffect(() => {
    if (isModalVisible) {
      const fetchGroupDetails = async () => {
        if (!id) {
          message.error(t('updateGroup.groupIdMissing'));
          setInitialLoading(false);
          return;
        }

        try {
          const result = await groupService.getGroupDetail(id);
          if (result.success) {
            form.setFieldsValue({
              name: result.name,
              description: result.description,
            });
          } else {
            message.error(t('updateGroup.fetchFailed'));
            console.error('Error details:', result.error);
          }
        } catch (error) {
          message.error(t('updateGroup.fetchFailed'));
          console.error('Error details:', error);
        } finally {
          setInitialLoading(false);
        }
      };

      fetchGroupDetails();
    }
  }, [id, form, t, isModalVisible]);

  const handleSubmit = async () => {
    setLoading(true);
    try {
      if (!id) {
        message.error(t('updateGroup.groupIdMissing'));
        return;
      }

      if (validateForm()) {
        const values = form.getFieldsValue();
        const result = await groupService.updateGroup(id, values);

        if (result.success) {
          message.success(t('updateGroup.updateSuccess'));
          setIsModalVisible(false);
          onUpdateSuccess(); // Trigger parent reload
        } else {
          message.error(t('updateGroup.updateFailed'));
          console.error('Error details:', result.errorMessage);
        }
      }
    } catch (errorInfo) {
      console.error('Form validation failed:', errorInfo);
      message.error(t('updateGroup.updateFailed'));
    } finally {
      setLoading(false);
    }
  };

  const showModal = () => {
    setIsModalVisible(true);
  };

  const handleCancelModal = () => {
    setIsModalVisible(false);
  };

  return (
    <>
      <Button type="primary" icon={<EditOutlined />} onClick={showModal} size='small'>
      </Button>

      <Modal
        width='50%'
        title={t('updateGroup.title')}
        visible={isModalVisible}
        onCancel={handleCancelModal}
        footer={[
          <Button key="cancel" onClick={handleCancelModal}>
            {t('cancel')}
          </Button>,
          <Button
            key="submit"
            type="primary"
            htmlType="submit"
            loading={loading}
            onClick={handleSubmit}
            icon={<SaveOutlined />}
          >
            {t('update_group_btn')}
          </Button>,
        ]}
      >
        {initialLoading ? (
          <div className="flex justify-center items-center h-64">
            <Spin size="large" tip="Loading..." />
          </div>
        ) :
          <Form
            form={form}
            layout="vertical"
            onFinish={handleSubmit}
            requiredMark="optional"
            validateMessages={{ required: t('form.required') }}
            className="space-y-6"
          >
            <Form.Item
              label={<Text strong>{t('groupname')}</Text>}
              name="name"
              validateStatus={errors.name ? 'error' : ''}
              help={errors.name}
              required
            >
              <Input
                onBlur={() => validateField('name')}
                placeholder={t('updateGroup.nameInputPlaceholder', 'Enter group name')}
                maxLength={100}
                showCount
              />
            </Form.Item>

            <Form.Item
              label={<Text strong>{t('description')}</Text>}
              name="description"
              validateStatus={errors.description ? 'error' : ''}
              help={errors.description}
            >
              <TextArea
                onBlur={() => validateField('description')}
                autoSize={{ minRows: 4, maxRows: 8 }}
                placeholder={t('updateGroup.descriptionInputPlaceholder', 'Enter group description')}
                maxLength={500}
                showCount
              />
            </Form.Item>
          </Form>
        }
      </Modal>
    </>
  );
};

export default UpdateGroupModal;