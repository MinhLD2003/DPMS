import React, { useState, useEffect, useCallback } from "react";
import {
  Form,
  Input,
  Button,
  Spin,
  Divider,
  Typography,
  message,
  Space,
  Avatar,
  Layout,
  Card,
  Modal,
} from "antd";
import { UserAddOutlined, TeamOutlined, InfoCircleOutlined, SaveOutlined, PlusOutlined } from "@ant-design/icons";
import AxiosClient from "../../configs/axiosConfig";
import CustomSelectList from "../../components/common/CustomSelectList";
import { useFormValidation } from "../../hooks/useFormValidation";
import { PostSystemModel, systemSchema } from "./models/PostSystemModel";
import { useTranslation } from "react-i18next";

const { Title, Text, Paragraph } = Typography;
const { TextArea } = Input;

const AddSystemModal: React.FC<{ onCreateSuccess: () => void }> = ({ onCreateSuccess }) => {
  const { t } = useTranslation();
  const [form] = Form.useForm<PostSystemModel>();
  const [loading, setLoading] = useState<boolean>(false);
  const [fetchingData, setFetchingData] = useState<boolean>(true);
  const [businessOwnerEmails, setBusinessOwnerEmails] = useState<string[]>([]);
  const [productDevEmails, setProductDevEmails] = useState<string[]>([]);
  const [isModalVisible, setIsModalVisible] = useState(false);

  const BO = 'BusinessOwner';
  const PD = 'ProductDeveloper';

  const fetchEmails = useCallback(async (groupName: string) => {
    try {
      const response = await AxiosClient.get(`/Group/get-users-in-group?groupName=${groupName}`);
      const emails = response.data.map((user: { email: string }) => user.email);
      return emails;
    } catch (error) {
      console.error(`Error fetching ${groupName} emails:`, error);
      message.error(t('addSystem.fetchEmailsFailed', { groupName }));
      return [];
    }
  }, [t]);

  useEffect(() => {
    if (isModalVisible) {
      const fetchData = async () => {
        setFetchingData(true);
        form.setFieldsValue({
          businessOwnerEmails: [],
          productDevEmails: []
        });

        const [businessOwnerEmailsResult, productDevEmailsResult] = await Promise.all([
          fetchEmails(BO),
          fetchEmails(PD)
        ]);

        setBusinessOwnerEmails(businessOwnerEmailsResult);
        setProductDevEmails(productDevEmailsResult);
        setFetchingData(false);
      };

      fetchData();
    }
  }, [form, fetchEmails, isModalVisible]);

  const handleSubmit = async () => {
    setLoading(true);
    try {
      if (validateForm()) {
        const values = form.getFieldsValue();
        await AxiosClient.post('/ExternalSystem', values);
        message.success(t('addSystem.createSuccess'));
        setIsModalVisible(false);
        form.resetFields();
        onCreateSuccess(); // Trigger parent reload
      }
    } catch (errorInfo) {
      console.log("Failed:", errorInfo);
      message.error(t('addSystem.createFailed'));
    } finally {
      setLoading(false);
    }
  };

  const handleCancelModal = () => {
    setIsModalVisible(false);
  };

  const showModal = () => {
    setIsModalVisible(true);
  };

  const { errors, validateField, validateForm } = useFormValidation<PostSystemModel>(form, systemSchema, { validateOnChange: false });
  return (
    <>
      <Button type="primary" onClick={showModal} icon={<PlusOutlined />}>
        {t('addSystem.createSystem')}
      </Button>

      <Modal
        title={t('addSystem.pageTitle')}
        visible={isModalVisible}
        onCancel={handleCancelModal}
        width="50%"
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
            {t('addSystem.createSystem')}
          </Button>,
        ]}
      >
        {fetchingData ? (
          <div className="flex justify-center items-center py-12">
            <Spin size="large" />
            <Text className="ml-4">{t('processing')}</Text>
          </div>
        ) : (
          <Form
            form={form}
            layout="vertical"
            onFinish={handleSubmit}
            className="px-2"
          >
            <Form.Item
              label={<Text strong>{t('addSystem.systemNameLabel')}</Text>}
              name="name"
              tooltip={{
                title: t('addSystem.systemNameTooltip'),
                icon: <InfoCircleOutlined />
              }}
              validateStatus={errors.name ? "error" : ""}
              help={errors.name}
              className="mb-6"
            >
              <Input
                onBlur={() => validateField('name')}
                placeholder={t('addSystem.systemNamePlaceholder')} />
            </Form.Item>

            <Form.Item
              label={<Text strong>{t('addSystem.systemDomain')}</Text>}
              name="domain"
              tooltip={{
                title: t('addSystem.domainTooltip'),
                icon: <InfoCircleOutlined />
              }}
              validateStatus={errors.name ? "error" : ""}
              help={errors.domain}
              className="mb-6"
            >
              <Input
                onBlur={() => validateField('domain')}
                placeholder={t('addSystem.domainPlaceholder')} />
            </Form.Item>

            <Form.Item
              label={<Text strong>{t('addSystem.descriptionLabel')}</Text>}
              name="description"
              tooltip={{
                title: t('addSystem.descriptionTooltip'),
                icon: <InfoCircleOutlined />
              }}
              validateStatus={errors.description ? "error" : ""}
              help={errors.description}
              className="mb-6"
            >
              <TextArea
                onBlur={() => validateField('description')}
                placeholder={t('addSystem.descriptionPlaceholder')}
                rows={4}
                showCount
                maxLength={500}
              />
            </Form.Item>

            <Divider className="my-8" />

            <Title level={4} className="mb-6">
              <TeamOutlined className="mr-2" />
              {t('addSystem.personInCharge')}
            </Title>

            <Paragraph className="text-gray-500 mb-6">
              {t('addSystem.personInChargeHelp')}
            </Paragraph>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
              <Form.Item
                label={
                  <div className="flex items-center">
                    <Avatar icon={<UserAddOutlined />} className="mr-2 bg-blue-400" />
                    <span><Text strong>{t('addSystem.businessOwner')}</Text></span>
                  </div>
                }
                name="businessOwnerEmails"
                validateStatus={errors.businessOwnerEmails ? "error" : ""}
                help={errors.businessOwnerEmails}
              >
                <CustomSelectList
                  //onBlur={() => validateField('businessOwnerEmails')}
                  items={businessOwnerEmails}
                  defaultLabel=""
                />
              </Form.Item>

              <Form.Item
                label={
                  <div className="flex items-center">
                    <Avatar icon={<UserAddOutlined />} className="mr-2 bg-green-400" />
                    <span><Text strong>{t('addSystem.productDeveloper')}</Text></span>
                  </div>
                }
                name="productDevEmails"
                validateStatus={errors.productDevEmails ? "error" : ""}
                help={errors.productDevEmails}
              >
                <CustomSelectList
                  //onBlur={() => validateField('productDevEmails')}
                  items={productDevEmails}
                  defaultLabel=""
                />
              </Form.Item>
            </div>
          </Form>
        )}
      </Modal>
    </>
  );
};

export default AddSystemModal;