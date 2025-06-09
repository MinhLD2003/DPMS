import React, { useState, useEffect, useCallback } from 'react';
import {
  Form,
  Input,
  Button,
  message,
  Card,
  Typography,
  Select,
  Space,
  Spin,
  Tag,
  Tooltip,
  Alert,
  Layout,
  Modal,
} from 'antd';
import { FeaturePostModel, featureSchema } from './models/FeaturePostModel';
import { useFormValidation } from '../../hooks/useFormValidation';
import featureService from './apis/FeatureAPIs';
import { HttpMethodType } from '../../enum/enum';
import {
  SaveOutlined,
  AppstoreOutlined,
  LinkOutlined,
  FileTextOutlined,
  ApiOutlined,
  QuestionCircleOutlined,
  FolderOutlined,
  InfoCircleOutlined,
  EditOutlined,
} from '@ant-design/icons';
import { useTranslation } from 'react-i18next';

const { Title, Text } = Typography;
const { TextArea } = Input;
const { Option } = Select;

interface FilterParams {
  pageNumber: number;
  pageSize: number;
  filters: Record<string, string>;
}

const methodColors = {
  GET: 'green',
  POST: 'blue',
  PUT: 'orange',
  PATCH: 'cyan',
  DELETE: 'red',
  OPTIONS: 'purple',
  HEAD: 'geekblue',
};

const UpdateFeatureModal: React.FC<{ id: string; onUpdateSuccess: () => void }> = ({ id, onUpdateSuccess }) => {
  const { t } = useTranslation();
  const [form] = Form.useForm<FeaturePostModel>();
  const { errors, validateField, validateForm } = useFormValidation<FeaturePostModel>(
    form,
    featureSchema,
    { validateOnChange: false }
  );
  const [loading, setLoading] = useState<boolean>(false);
  const [fetchingData, setFetchingData] = useState<boolean>(true);
  const [httpMethodNumber, setHttpMethodNumber] = useState<number>();
  const [isParent, setIsParent] = useState<boolean>(false);
  const [hasChildren, setHasChildren] = useState<boolean>(false);
  const [parentOptions, setParentOptions] = useState<{ id: string; featureName: string }[]>([]);
  const [selectedParentId, setSelectedParentId] = useState<string | null>(null);
  const [isModalVisible, setIsModalVisible] = useState(false);

  const httpMethods = Object.values(HttpMethodType);

  useEffect(() => {
    if (isModalVisible) {
      const fetchParentFeatures = async () => {
        try {
          const filter: FilterParams = {
            pageNumber: 1,
            pageSize: 100,
            filters: {
              parentId: 'null',
            },
          };

          const result = await featureService.getFeatures(filter);
          if (result.success && result.objectList) {
            setParentOptions(result.objectList);
          } else {
            console.error('Failed to fetch parent features:', result.error);
            message.error(t('updateFeature.parentFeaturesFetchFailed'));
          }
        } catch (error) {
          console.error('Exception fetching parent features:', error);
          message.error(t('updateFeature.parentFeaturesFetchFailed'));
        }
      };

      fetchParentFeatures();
    }
  }, [isModalVisible]);

  useEffect(() => {
    if (isModalVisible) {

      const fetchFeatureDetails = async () => {
        if (!id) {
          message.error(t('updateFeature.featureIdMissing'));
          setFetchingData(false);
          return;
        }

        try {
          setFetchingData(true);
          const result = await featureService.getFeatureDetail(id);
          if (result.success) {
            const hasChildFeatures = Array.isArray(result.children) && result.children.length > 0;
            setHasChildren(hasChildFeatures);

            const isParentFeature = result.parentId === null && hasChildFeatures;

            setIsParent(isParentFeature);
            setSelectedParentId(result.parentId ?? '');

            if (result.httpMethod !== undefined || isParentFeature) {
              if (!isParentFeature) {
                setHttpMethodNumber(result.httpMethod);
              }

              form.setFieldsValue({
                featureName: result.featureName,
                url: result.url,
                description: result.description,
                httpMethod: result.httpMethod !== undefined ? httpMethods[result.httpMethod] : undefined,
                parentId: result.parentId,
              });
            } else {
              if (!isParentFeature) {
                message.error(t('updateFeature.httpMethodUndefined'));
                console.error('HTTP method is undefined in result:', result);
              }
            }
          } else {
            message.error(t('updateFeature.featureDetailsFetchFailed'));
            console.error('Error details:', result.error);
          }
        } catch (error) {
          message.error(t('updateFeature.unexpectedError'));
          console.error('Exception:', error);
        } finally {
          setFetchingData(false);
        }
      };

      fetchFeatureDetails();
    }
  }, [id, form, t, isModalVisible]);

  const handleParentChange = (value: any) => {
    const isParentFeature = value === null;
    setIsParent(isParentFeature);
    setSelectedParentId(value);

    if (isParentFeature) {
      form.setFieldsValue({
        url: null,
        httpMethod: null,
      });
    }
  };

  const handleSubmit = async () => {
    try {
      if (!id) {
        message.error(t('updateFeature.featureIdMissing'));
        return;
      }

      setLoading(true);
      if (validateForm()) {
        const values = form.getFieldsValue();

        let submitValues = { ...values };

        if (!isParent && values.httpMethod) {
          const httpMethodString = values.httpMethod;
          const httpMethodIndex = httpMethods.findIndex((method) => method === httpMethodString);
          submitValues.httpMethod = httpMethodIndex !== -1 ? httpMethodIndex : 0; // Fallback to 0 if not found
        } else {
          submitValues.url = null;
          submitValues.httpMethod = null;
        }

        const result = await featureService.UpdateFeature(id, submitValues);

        if (result.success) {
          message.success(t('updateFeature.updateSuccess'));
          setIsModalVisible(false);
          onUpdateSuccess(); // Notify parent to refresh
        } else {
          message.error(
            t('updateFeature.updateFailed', { errorMessage: result.errorMessage || 'Unknown error' })
          );
          console.error('Error details:', result.errorMessage);
        }
      }
    } catch (errorInfo) {
      console.error('Form validation failed:', errorInfo);
      message.error(t('updateFeature.formError'));
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
      <Button type="primary" icon={<EditOutlined />} onClick={showModal} size="small">
      </Button>

      <Modal
        title={t('updateFeature.updateFeature')}
        visible={isModalVisible}
        onCancel={handleCancelModal}
        width="50%"
        footer={[
          <Button key="cancel" onClick={handleCancelModal}>
            {t('cancel')}
          </Button>,
          <Button
            type="primary"
            loading={loading}
            onClick={handleSubmit}
            icon={<SaveOutlined />}
          >
            {t('updateFeature.updateFeatureButton')}
          </Button>,
        ]}
      >
        {fetchingData ? (
          <div className="flex justify-center items-center h-64">
            <Spin size="large" tip={t('updateFeature.loadingData')} />
          </div>
        ) : (
          <Form
            form={form}
            layout="vertical"
            requiredMark="optional"
            className="feature-form"
          >
            <Form.Item
              label={
                <Space>
                  <AppstoreOutlined className="text-blue-600" />
                  <Text strong>{t('updateFeature.featureNameLabel')}</Text>
                  <Tooltip title={t('updateFeature.featureNameTooltip')}>
                    <QuestionCircleOutlined className="text-gray-400" />
                  </Tooltip>
                  {isParent && <Tag color="blue">{t('updateFeature.parentFeatureTag')}</Tag>}
                </Space>
              }
              name="featureName"
              validateStatus={errors.featureName ? 'error' : ''}
              help={errors.featureName}
              rules={[{ required: true, message: t('updateFeature.featureNameRequired') }]}
            >
              <Input
                onBlur={() => validateField('featureName')}
                placeholder={t('updateFeature.featureNamePlaceholder')}
                size="large"
                className="border-blue-200 hover:border-blue-400 focus:border-blue-500"
              />
            </Form.Item>

            {hasChildren && (
              <Alert
                message={t('updateFeature.parentSelectionDisabled')}
                description={t('updateFeature.parentSelectionDescription')}
                type="info"
                showIcon
                icon={<InfoCircleOutlined />}
                className="mb-4"
              />
            )}

            <Form.Item
              label={
                <Space>
                  <FolderOutlined className="text-indigo-600" />
                  <Text strong>{t('updateFeature.parentFeatureLabel')}</Text>
                  <Tooltip
                    title={
                      hasChildren
                        ? t('updateFeature.parentFeatureTooltipHasChildren')
                        : t('updateFeature.parentFeatureTooltipNoChildren')
                    }
                  >
                    <QuestionCircleOutlined className="text-gray-400" />
                  </Tooltip>
                </Space>
              }
              name="parentId"
            >
              <Select
                onBlur={() => validateField('parentId')}
                placeholder={t('updateFeature.parentFeaturePlaceholder')}
                size="large"
                allowClear
                className="border-indigo-200 hover:border-indigo-400 focus:border-indigo-500"
                onChange={handleParentChange}
                value={selectedParentId}
                disabled={hasChildren}
              >
                <Option value={null}>{t('updateFeature.none')}</Option>
                {parentOptions.map((parent) => (
                  <Option key={parent.id} value={parent.id}>
                    {parent.featureName}
                  </Option>
                ))}
              </Select>
            </Form.Item>

            {/* URL Field - conditionally required */}
            <Form.Item
              label={
                <Space>
                  <LinkOutlined className="text-green-600" />
                  <Text strong>{t('updateFeature.urlLabel')}</Text>
                  <Tooltip title={t('updateFeature.urlTooltip')}>
                    <QuestionCircleOutlined className="text-gray-400" />
                  </Tooltip>
                </Space>
              }
              name="url"
              validateStatus={errors.url ? 'error' : ''}
              help={errors.url}
              rules={[{ required: !isParent, message: t('updateFeature.urlRequired') }]}
              hidden={isParent}
            >
              <Input
                onBlur={() => validateField('url')}
                placeholder={t('updateFeature.urlPlaceholder')}
                size="large"
                className="border-green-200 hover:border-green-400 focus:border-green-500"
                prefix={<LinkOutlined className="text-gray-400" />}
                disabled={isParent}
              />
            </Form.Item>

            {/* HTTP Method Field - conditionally required */}
            <Form.Item
              label={
                <Space>
                  <ApiOutlined className="text-purple-600" />
                  <Text strong>{t('updateFeature.httpMethodLabel')}</Text>
                  <Tooltip title={t('updateFeature.httpMethodTooltip')}>
                    <QuestionCircleOutlined className="text-gray-400" />
                  </Tooltip>
                </Space>
              }
              name="httpMethod"
              validateStatus={errors.httpMethod ? 'error' : ''}
              help={errors.httpMethod}
              rules={[{ required: !isParent, message: t('updateFeature.httpMethodRequired') }]}
              hidden={isParent}
            >
              <Select
                onBlur={() => validateField('httpMethod')}
                placeholder={t('updateFeature.httpMethodPlaceholder')}
                size="large"
                className="border-purple-200 hover:border-purple-400 focus:border-purple-500"
                options={httpMethods.map((method) => ({
                  label: (
                    <Tag color={methodColors[method]} className="font-medium">
                      {method}
                    </Tag>
                  ),
                  value: method,
                }))}
                disabled={isParent}
              />
            </Form.Item>

            <Form.Item
              label={
                <Space>
                  <FileTextOutlined className="text-orange-600" />
                  <Text strong>{t('updateFeature.descriptionLabel')}</Text>
                  <Tooltip title={t('updateFeature.descriptionTooltip')}>
                    <QuestionCircleOutlined className="text-gray-400" />
                  </Tooltip>
                </Space>
              }
              name="description"
              validateStatus={errors.description ? 'error' : ''}
              help={errors.description}
            >
              <TextArea
                onBlur={() => validateField('description')}
                placeholder={t('updateFeature.descriptionPlaceholder')}
                rows={4}
                size="large"
                showCount
                maxLength={500}
                className="border-orange-200 hover:border-orange-400 focus:border-orange-500"
              />
            </Form.Item>
          </Form>
        )}
      </Modal>
    </>
  );
};

export default UpdateFeatureModal;