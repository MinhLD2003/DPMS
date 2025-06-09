import React, { useState, useCallback, useEffect } from "react";
import {
  Card,
  Typography,
  List,
  Switch,
  Button,
  Row,
  Col,
  Divider,
  Tag,
  Descriptions,
  Modal,
  Tooltip,
  message,
  Empty,
  Input,
} from "antd";
import {
  DeleteOutlined,
  PlusOutlined,
  SaveOutlined,
  ExclamationCircleOutlined,
  SearchOutlined,
} from "@ant-design/icons";
import consentService from "../apis/ConsentAPIs";
import { SimplifiedPurposeViewModel } from "../models/PurposeViewModel";
import AxiosClient from "../../../configs/axiosConfig";
import { useNavigate, useParams } from "react-router-dom";
import PageHeader from "../../../components/common/PageHeader";
import { useTranslation } from 'react-i18next'; // Import useTranslation

const { Text } = Typography;
const { confirm } = Modal;
const { Search } = Input;

interface System {
  id: string;
  name: string;
}

const CreateSystemConsentForm: React.FC = () => {
  const [availablePurposes, setAvailablePurposes] = useState<SimplifiedPurposeViewModel[]>([]);
  const [addedPurposes, setAddedPurposes] = useState<SimplifiedPurposeViewModel[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [system, setSystem] = useState<System | null>();
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation(); // Initialize useTranslation

  // Load initial data
  useEffect(() => {
    const loadData = async () => {
      if (!id) return;
      
      setLoading(true);
      try {
        // Fetch all the data in parallel
        const [purposesResult, systemDetails, systemPurposes] = await Promise.all([
          consentService.getPurposes(),
          AxiosClient.get(`/ExternalSystem/${id}/get-system-details`),
          AxiosClient.get(`/ExternalSystem/${id}/purposes`)
        ]);
        
        // Set system details
        setSystem(systemDetails.data);
        
        // Process purposes
        if (purposesResult.success) {
          const allPurposes = purposesResult.objectList;
          const systemPurposesWithDefault = systemPurposes.data.map(
            (purpose: SimplifiedPurposeViewModel) => ({
              ...purpose,
              defaultChecked: true
            })
          );
          
          // Set added purposes
          setAddedPurposes(systemPurposesWithDefault);
          
          // Calculate available purposes (all - added)
          const addedPurposeIds = new Set(systemPurposesWithDefault.map((p: SimplifiedPurposeViewModel) => p.id));
          const availablePurposesArray = allPurposes.filter(p => !addedPurposeIds.has(p.id));
          
          setAvailablePurposes(availablePurposesArray);
        } else {
          message.error(t('failed_to_fetch_purposes'));
        }
      } catch (error) {
        message.error(t('failed_to_load_data_please_try_again'));
      } finally {
        setLoading(false);
      }
    };
    
    loadData();
  }, [id, t]);

  // Add Purpose
  const addPurpose = useCallback((purpose: SimplifiedPurposeViewModel) => {
    setAddedPurposes(prev => [...prev, { ...purpose, defaultChecked: true }]);
    setAvailablePurposes(prev => prev.filter(p => p.id !== purpose.id));
  }, []);

  // Remove Purpose
  const removePurpose = useCallback((purposeId: string) => {
    confirm({
      title: t('are_you_sure_you_want_to_remove_this_purpose'),
      icon: <ExclamationCircleOutlined />,
      onOk() {
        setAddedPurposes(prev => {
          const removedPurpose = prev.find(p => p.id === purposeId);
          if (removedPurpose) {
            setAvailablePurposes(prevAvailable => [
              ...prevAvailable, 
              { id: removedPurpose.id, name: removedPurpose.name }
            ]);
          }
          return prev.filter(p => p.id !== purposeId);
        });
      },
    });
  }, [t]);

  // Toggle Default
  const toggleDefault = useCallback((id: string, checked: boolean) => {
    setAddedPurposes(prev =>
      prev.map(p => (p.id === id ? { ...p, defaultChecked: checked } : p))
    );
  }, []);

  // Save Config
  const saveConfiguration = useCallback(() => {
    const performSave = async () => {
      if (!system?.id) {
        message.error(t('system_details_not_found_cannot_save_configuration'));
        return;
      }

      const payload = {
        externalSystemId: system.id,
        purposeIds: addedPurposes.map(p => p.id),
      };

      setLoading(true);
      try {
        await AxiosClient.post("/ExternalSystem/bulk-add-purposes", payload);
        message.success(t('consent_form_configuration_saved_successfully'));
        navigate(-1);
      } catch (error) {
        message.error(t('failed_to_save_configuration_please_try_again'));
        setLoading(false);
      }
    };

    if (addedPurposes.length === 0) {
      confirm({
        title: t('no_purposes_configured'),
        content: t('are_you_sure_you_want_to_save_without_any_configured_purposes'),
        icon: <ExclamationCircleOutlined />,
        onOk() {
          performSave();
        },
      });
    } else {
      performSave();
    }
  }, [addedPurposes, system, navigate, t]);

  // Filtered Purposes
  const filteredPurposes = availablePurposes.filter(p =>
    p.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <section>
      <PageHeader title={t('consent_form_configuration')} />
      <Card style={{ marginBottom: 24 }}>
        <Descriptions bordered column={1}>
          <Descriptions.Item label={t('systemName')}>
            {loading ? (
              <Tag color="blue">{t('loading')}</Tag>
            ) : system ? (
              <Tag color="blue">{system.name}</Tag>
            ) : (
              <Tag color="red">{t('system_not_found')}</Tag>
            )}
          </Descriptions.Item>
        </Descriptions>
      </Card>

      <Row gutter={24}>
        {/* Available Purposes */}
        <Col xs={24} md={10}>
          <Card title={t('available_purposes')}>
            <Search
              placeholder={t('search_purposes')}
              allowClear
              enterButton={<SearchOutlined />}
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              style={{ marginBottom: 16 }}
            />
            {filteredPurposes.length === 0 ? (
              <Empty description={t('no_matching_purposes_available')} />
            ) : (
              <List
                dataSource={filteredPurposes}
                loading={loading}
                renderItem={(item) => (
                  <List.Item
                    key={item.id}
                    actions={[
                      <Tooltip title={t('add_purpose')} key="add">
                        <Button
                          icon={<PlusOutlined />}
                          onClick={() => addPurpose(item)}
                        />
                      </Tooltip>,
                    ]}
                  >
                    <Text>{item.name}</Text>
                  </List.Item>
                )}
              />
            )}
          </Card>
        </Col>

        {/* Configured Purposes */}
        <Col xs={24} md={14}>
          <Card>
            <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 16 }}>
              <Text strong>{t('configured_purposes')}</Text>
            </div>
            {addedPurposes.length === 0 ? (
              <Empty description={t('no_purposes_configured_yet')} />
            ) : (
              <List
                dataSource={addedPurposes}
                bordered
                renderItem={(item, index) => (
                  <List.Item
                    key={item.id}
                    style={{
                      display: "flex",
                      justifyContent: "space-between",
                      alignItems: "flex-start",
                    }}
                    actions={[
                      <Tooltip key="toggle" title={t('set_as_default')}>
                        <Switch
                          checked={true}
                          onChange={(checked) => toggleDefault(item.id, checked)}
                        />
                      </Tooltip>,
                      <Tooltip title={t('remove_purpose')} key="remove">
                        <Button
                          danger
                          icon={<DeleteOutlined />}
                          onClick={() => removePurpose(item.id)}
                        />
                      </Tooltip>,
                    ]}
                  >
                    <Text strong>{index + 1}.</Text>
                    <Text style={{ marginLeft: 8, flex: 1 }}>{item.name}</Text>
                  </List.Item>
                )}
              />
            )}
            <Divider />
            <Button
              type="primary"
              icon={<SaveOutlined />}
              onClick={saveConfiguration}
              loading={loading}
            >
              {t('save_configuration')}
            </Button>
          </Card>
        </Col>
      </Row>
    </section>
  );
};

export default CreateSystemConsentForm;