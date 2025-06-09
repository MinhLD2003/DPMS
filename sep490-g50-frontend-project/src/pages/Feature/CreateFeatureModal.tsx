import React, { useState, useEffect } from 'react';
import {
  Form,
  Input,
  Button,
  message,
  Card,
  Typography,
  Select,
  Layout,
  Modal,
} from 'antd';
import { FeaturePostModel, featureSchema } from './models/FeaturePostModel';
import { useFormValidation } from '../../hooks/useFormValidation';
import featureService from './apis/FeatureAPIs';
import CustomDropdown from '../../components/common/CustomDropdown';
import { HttpMethodType } from '../../enum/enum';
import { SaveOutlined, PlusOutlined } from '@ant-design/icons';
import { useTranslation } from 'react-i18next';

const { Text } = Typography;

const CreateFeatureModal: React.FC<{ onCreateSuccess: () => void }> = ({ onCreateSuccess }) => {
  const { t } = useTranslation();
  const [form] = Form.useForm<FeaturePostModel>();
  const [loading, setLoading] = useState<boolean>(false);
  const [parentFeatures, setParentFeatures] = useState<any[]>([]);
  const [filteredParentFeatures, setFilteredParentFeatures] = useState<any[]>([]);
  const [isModalVisible, setIsModalVisible] = useState(false);

  const { errors, validateField, validateForm } = useFormValidation<FeaturePostModel>(
    form,
    featureSchema,
    { validateOnChange: false }
  );

  const httpMethods = Object.values(HttpMethodType);

  // Fetch parent features on component mount
  useEffect(() => {
    if (isModalVisible) {
      const fetchParentFeatures = async () => {
        try {
          const result = await featureService.getFeatures({
            pageNumber: 1,
            pageSize: 100,
          });
          if (result.success && result.objectList) {
            setParentFeatures(result.objectList);
            setFilteredParentFeatures(result.objectList);
          } else {
            console.error('Failed to fetch parent features:', result.error);
            message.error(t('createFeature.parentFeatureLoadFailed'));
          }
        } catch (error) {
          console.error('Error fetching parent features:', error);
          message.error(t('createFeature.parentFeatureLoadFailed'));
        }
      };

      fetchParentFeatures();
    }
  }, [isModalVisible]);

  // Filter parent features based on search text
  const handleParentFeatureSearch = (value: string) => {
    const filtered = parentFeatures.filter((feature) =>
      feature.featureName.toLowerCase().includes(value.toLowerCase())
    );
    setFilteredParentFeatures(filtered);
  };

  const handleSubmit = async () => {
    setLoading(true);
    try {
      if (validateForm()) {
        const values = form.getFieldsValue();
        const result = await featureService.createFeature(values);

        if (result.success) {
          message.success(t('createFeature.createSuccess'));
          form.resetFields();
          setIsModalVisible(false); // Close the modal
          onCreateSuccess(); // Trigger parent reload
        } else {
          message.error(t('createFeature.createFailed'));
          console.error('Error details:', result.error);
        }
      }
    } catch (errorInfo) {
      console.error('Form validation failed:', errorInfo);
      message.error(t('createFeature.createFailed'));
    } finally {
      setLoading(false);
    }
  };

  const showModal = () => {
    setIsModalVisible(true);
  };

  const handleCancelModal = () => {
    setIsModalVisible(false);
    form.resetFields();
  };

  return (
    <>
      <Button type="primary" onClick={showModal} icon={<PlusOutlined />}>
        {t('createFeature.createFeatureButton')}
      </Button>

      <Modal
        title={t('createFeature.title')}
        visible={isModalVisible}
        width='50%'
        onCancel={handleCancelModal}
        footer={[
          <Button key="cancel" onClick={handleCancelModal}>
            {t('createFeature.cancelButton')}
          </Button>,
          <Button
            key="submit"
            type="primary"
            loading={loading}
            onClick={handleSubmit}
            icon={<SaveOutlined />}
          >
            {t('createFeature.createFeatureButton')}
          </Button>,
        ]}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          requiredMark={false}
        >
          <Form.Item
            label={<Text strong>{t('createFeature.featureNameLabel')}</Text>}
            name="featureName"
            validateStatus={errors.featureName ? 'error' : ''}
            help={errors.featureName}
            rules={[{ required: true, message: t('createFeature.featureNameRequired') }]}
          >
            <Input
              onBlur={() => validateField('featureName')}
              placeholder={t('createFeature.featureNamePlaceholder')}
            />
          </Form.Item>

          <Form.Item
            label={<Text strong>{t('createFeature.parentFeatureLabel')}</Text>}
            name="parentId"
            validateStatus={errors.parentId ? 'error' : ''}
            help={errors.parentId}
          >
            <Select
              onBlur={() => validateField('parentId')}
              showSearch
              placeholder={t('createFeature.parentFeaturePlaceholder')}
              filterOption={false}
              onSearch={handleParentFeatureSearch}
              onChange={(value) => form.setFieldsValue({ parentId: value })}
              loading={parentFeatures.length === 0}
              notFoundContent={
                parentFeatures.length === 0
                  ? t('createFeature.parentFeatureLoading')
                  : t('createFeature.parentFeatureNotFound')
              }
              options={filteredParentFeatures.map((feature) => ({
                value: feature.id,
                label: feature.featureName,
              }))}
            />
          </Form.Item>

          <Form.Item
            label={<Text strong>{t('createFeature.urlLabel')}</Text>}
            name="url"
            validateStatus={errors.url ? 'error' : ''}
            help={errors.url}
            rules={[{ required: false, message: t('createFeature.urlRequired') }]}
          >
            <Input
              onBlur={() => validateField('url')}
              placeholder={t('createFeature.urlPlaceholder')}
            />
          </Form.Item>

          <Form.Item
            label={<Text strong>{t('createFeature.descriptionLabel')}</Text>}
            name="description"
            validateStatus={errors.description ? 'error' : ''}
            help={errors.description}
          >
            <Input.TextArea
              onBlur={() => validateField('description')}
              placeholder={t('createFeature.descriptionPlaceholder')}
              autoSize={{ minRows: 3, maxRows: 5 }}
            />
          </Form.Item>

          <Form.Item
            label={<Text strong>{t('createFeature.httpMethodLabel')}</Text>}
            name="httpMethod"
            validateStatus={errors.httpMethod ? 'error' : ''}
            help={errors.httpMethod}
            rules={[{ required: false, message: t('createFeature.httpMethodRequired') }]}
          >
            <CustomDropdown
              onBlur={() => validateField('httpMethod')}
              items={httpMethods}
            />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
};

export default CreateFeatureModal;