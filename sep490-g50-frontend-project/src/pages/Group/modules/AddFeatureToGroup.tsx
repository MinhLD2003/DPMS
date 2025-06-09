import React, { useState, useEffect } from 'react';
import {
  Typography,
  Card,
  Form,
  Input,
  Select,
  Button,
  Table,
  Space,
  Tag,
  Checkbox,
  Divider,
  message,
  Row,
  Col,
  Tooltip,
  Spin,
  Alert,
  Empty,
  theme,
  Layout
} from 'antd';
import {
  UserOutlined,
  TeamOutlined,
  AppstoreOutlined,
  SearchOutlined,
  PlusOutlined,
  SaveOutlined,
  UndoOutlined,
  InfoCircleOutlined,
  LockOutlined,
  UnlockOutlined
} from '@ant-design/icons';
import AxiosClient from '../../../configs/axiosConfig';
import API_ENDPOINTS from '../../../configs/APIEndPoint';
import { useTranslation } from 'react-i18next';
import PageHeader from '../../../components/common/PageHeader';
const { Title, Text } = Typography;
const { Option } = Select;
const { useToken } = theme;

// Interfaces
interface UserGroup {
  id: string;
  name: string;
  description: string;
  memberCount: number;
}

export type FeatureModel = {
  id: string;
  featureName: string;
  description?: string;
  url?: string;
  httpMethod?: string;
  children?: FeatureModel[];
  isChecked: boolean
};

interface Permission {
  id: string;
  name: string;
  code: string
  isGranted: boolean;
}


interface FeatureWithState extends FeatureModel {
  isAssigned: boolean;
  permissions: Permission[];
}

const AddFeatureToGroupScreen: React.FC = () => {
  const { t } = useTranslation();
  const { token } = useToken();
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [groupsLoading, setGroupsLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [selectedGroup, setSelectedGroup] = useState<string | null>(null);
  const [features, setFeatures] = useState<FeatureWithState[]>([]);
  const [userGroups, setUserGroups] = useState<UserGroup[]>([]);
  const [searchText, setSearchText] = useState('');
  const [selectedFeatures, setSelectedFeatures] = useState<FeatureWithState[]>([]);

  // Fetch user groups on component mount
  useEffect(() => {
    fetchUserGroups();
  }, []);

  useEffect(() => {
    if (selectedGroup) {
      fetchFeatures();
    } else {
      setFeatures([]);
      setSelectedFeatures([]);
    }
  }, [selectedGroup]);

  const fetchUserGroups = async () => {
    setGroupsLoading(true);
    try {
      const params = { isGlobal: true };
      const response = await AxiosClient.get(API_ENDPOINTS.GROUPS.GET_ALL, { params });
      setUserGroups(response.data.data);
    } catch (error) {
      console.error('Error fetching user groups:', error);
      message.error(t('addFeatureToGroup.userGroupsLoadFailed'));
    } finally {
      setGroupsLoading(false);
    }
  };

  const fetchFeatures = async () => {
    setLoading(true);
    try {
      // Replace with your actual API endpoint
      const response = await AxiosClient.get(API_ENDPOINTS.FEATURES.GET_LIST_FEATURES(selectedGroup ?? ""));
      const featuresWithState: FeatureWithState[] = response.data.map((feature: FeatureModel) => {
        const permissions: Permission[] = feature.children?.map(child => ({
          id: child.id,
          name: child.featureName,
          code: child.url || child.id,
          isGranted: child.isChecked,
        })) || [];

        return {
          ...feature,
          isAssigned: feature.isChecked,
          permissions,
        };
      });
      setFeatures(featuresWithState);
    } catch (error) {
      console.error('Error fetching features:', error);
      message.error(t('addFeatureToGroup.featuresLoadFailed'));
    } finally {
      setLoading(false);
    }
  };

  const handleGroupChange = (value: string) => {
    setSelectedGroup(value);
  };

  const handleSearch = (value: string) => {
    setSearchText(value);
  };

  const handleFeatureSelection = (featureId: string, checked: boolean) => {
    const updatedFeatures = features.map(feature => {
      if (feature.id === featureId) {
        return { ...feature, isAssigned: checked };
      }
      return feature;
    });
    setFeatures(updatedFeatures);

    const selected = updatedFeatures.filter(feature => feature.isAssigned);
    setSelectedFeatures(selected);
  };

  const handlePermissionChange = (featureId: string, permissionId: string, checked: boolean) => {
    const updatedFeatures = features.map(feature => {
      if (feature.id === featureId) {
        const updatedPermissions = feature.permissions.map(permission => {
          if (permission.id === permissionId) {
            return { ...permission, isGranted: checked };
          }
          return permission;
        });
        return { ...feature, permissions: updatedPermissions };
      }
      return feature;
    });
    setFeatures(updatedFeatures);

    const selected = updatedFeatures.filter(feature => feature.isAssigned);
    setSelectedFeatures(selected);
  };

  const handleSelectAllPermissions = (featureId: string, checked: boolean) => {
    const updatedFeatures = features.map(feature => {
      if (feature.id === featureId) {
        const updatedPermissions = feature.permissions.map(permission => {
          return { ...permission, isGranted: checked };
        });
        return { ...feature, permissions: updatedPermissions };
      }
      return feature;
    });
    setFeatures(updatedFeatures);

    const selected = updatedFeatures.filter(feature => feature.isAssigned);
    setSelectedFeatures(selected);
  };

  const handleSave = async () => {
    if (!selectedGroup) {
      message.warning(t('addFeatureToGroup.selectGroupWarning'));
      return;
    }

    if (selectedFeatures.length === 0) {
      message.warning(t('addFeatureToGroup.selectFeatureWarning'));
      return;
    }

    setSaving(true);

    try {
      const featureIds = selectedFeatures.flatMap(feature =>
        feature.permissions
          .filter(p => p.isGranted)
          .map(p => p.id)
      );

      await AxiosClient.post(API_ENDPOINTS.FEATURES.ADD_FEATURES_TO_GROUP, JSON.stringify({
        featureIds: featureIds,
        groupId: selectedGroup
      }));
      message.success(t('addFeatureToGroup.saveSuccess', { groupName: userGroups.find(g => g.id === selectedGroup)?.name }));

      form.resetFields();
      setSelectedGroup(null);
      setFeatures([]);
      setSelectedFeatures([]);
      setSearchText('');
    } catch (error) {
      console.error('Error saving feature assignments:', error);
      message.error(t('addFeatureToGroup.saveFailed'));
    } finally {
      setSaving(false);
    }
  };

  const handleReset = () => {
    form.resetFields();
    setSelectedGroup(null);
    setFeatures([]);
    setSelectedFeatures([]);
    setSearchText('');
  };

  const filteredFeatures = features.filter(feature =>
    feature.featureName.toLowerCase().includes(searchText.toLowerCase()) ||
    (feature.description && feature.description.toLowerCase().includes(searchText.toLowerCase())) ||
    (feature.url && feature.url.toLowerCase().includes(searchText.toLowerCase()))
  );

  return (
    <Layout style={{ minHeight: '100vh', backgroundColor: '#f5f7fa' }}>
      <PageHeader
        title={t('addFeatureToGroup.title')}
      />
      <div className='px-4 py-8'>
        <Card className='w-full shadow-xl rounded-2xl p-8 mx-auto mt-8'>
          <Form
            form={form}
            layout="vertical"
          >
            <Row gutter={16}>
              <Col xs={24} md={12}>
                <Form.Item
                  label={<Space><TeamOutlined />{t('addFeatureToGroup.userGroupLabel')}</Space>}
                  name="userGroup"
                  rules={[{ required: true, message: t('addFeatureToGroup.userGroupRequired') }]}
                >
                  <Select
                    placeholder={t('addFeatureToGroup.userGroupPlaceholder')}
                    onChange={handleGroupChange}
                    showSearch
                    loading={groupsLoading}
                    filterOption={(input, option) => {
                      // Find the group by ID
                      const group = userGroups.find(g => g.id === option?.value);

                      // If group doesn't exist, don't include it
                      if (!group) return false;

                      // Check if input matches name or description
                      const nameMatch = group.name.toLowerCase().includes(input.toLowerCase());
                      const descMatch = group.description ?
                        group.description.toLowerCase().includes(input.toLowerCase()) :
                        false;

                      // Return true if either matches
                      return nameMatch || descMatch;
                    }}
                  >
                    {userGroups.map(group => (
                      <Option key={group.id} value={group.id}>
                        <Space>
                          <Text>{group.name}</Text>
                          {/* <Tag color="blue">{group.memberCount} {t('addFeatureToGroup.members')}</Tag> */}
                        </Space>
                      </Option>
                    ))}
                  </Select>
                </Form.Item>
              </Col>
              <Col xs={24} md={12}>
                <Form.Item
                  label={<Space><SearchOutlined />{t('addFeatureToGroup.searchFeaturesLabel')}</Space>}
                >
                  <Input
                    placeholder={t('addFeatureToGroup.searchFeaturesPlaceholder')}
                    allowClear
                    onChange={e => handleSearch(e.target.value)}
                    disabled={!selectedGroup}
                  />
                </Form.Item>
              </Col>
            </Row>

            <div style={{ marginTop: '8px' }}>
              {selectedGroup && (
                <Alert
                  message={
                    <Space>
                      <InfoCircleOutlined />
                      <Text strong>{t('addFeatureToGroup.configuringFeatures')}</Text>
                      <Text>{userGroups.find(g => g.id === selectedGroup)?.name}</Text>
                      <Text type="secondary">({userGroups.find(g => g.id === selectedGroup)?.description})</Text>
                    </Space>
                  }
                  type="info"
                  showIcon={false}
                  style={{ marginBottom: '20px' }}
                />
              )}
            </div>

            <Divider orientation="left">{t('addFeatureToGroup.availableFeatures')}</Divider>

            {loading ? (
              <div style={{ textAlign: 'center', padding: '40px' }}>
                <Spin size="large" />
                <div style={{ marginTop: '16px' }}>
                  <Text type="secondary">{t('addFeatureToGroup.loadingFeatures')}</Text>
                </div>
              </div>
            ) : (
              <>
                {!selectedGroup ? (
                  <Empty
                    description={t('addFeatureToGroup.selectGroupPrompt')}
                    image={Empty.PRESENTED_IMAGE_SIMPLE}
                  />
                ) : (
                  <>
                    {filteredFeatures.length === 0 ? (
                      <Empty
                        description={t('addFeatureToGroup.noFeaturesFound')}
                        image={Empty.PRESENTED_IMAGE_SIMPLE}
                      />
                    ) : (
                      <div style={{ maxHeight: '600px', overflowY: 'auto' }}>
                        {filteredFeatures.map(feature => (
                          <Card
                            key={feature.id}
                            size="small"
                            style={{ marginBottom: '16px' }}
                            title={
                              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                <Checkbox
                                  checked={feature.isAssigned}
                                  onChange={e => handleFeatureSelection(feature.id, e.target.checked)}
                                >
                                  <Space>
                                    <Text strong>{feature.featureName}</Text>
                                    {feature.url && (
                                      <Tag color={token.colorPrimaryBg} style={{ color: token.colorPrimary }}>
                                        {feature.httpMethod || 'GET'} {feature.url}
                                      </Tag>
                                    )}
                                  </Space>
                                </Checkbox>
                                <Tooltip title={feature.isAssigned ? t('addFeatureToGroup.selectAllPermissionsTooltip') : t('addFeatureToGroup.assignFeatureFirstTooltip')}>
                                  <Button
                                    size="small"
                                    icon={feature.isAssigned ? <UnlockOutlined /> : <LockOutlined />}
                                    disabled={!feature.isAssigned}
                                    onClick={() => handleSelectAllPermissions(feature.id, true)}
                                  >
                                    {t('addFeatureToGroup.selectAll')}
                                  </Button>
                                </Tooltip>
                              </div>
                            }
                          >
                            <Text type="secondary">{feature.description}</Text>

                            <div style={{ marginTop: '16px' }}>
                              <Text strong>{t('addFeatureToGroup.permissions')}:</Text>
                              <div style={{ marginTop: '8px', display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
                                {feature.permissions.map(permission => (
                                  <Checkbox
                                    key={permission.id}
                                    checked={permission.isGranted}
                                    disabled={!feature.isAssigned}
                                    onChange={e => handlePermissionChange(feature.id, permission.id, e.target.checked)}
                                  >
                                    <Tag color={permission.isGranted ? "green" : "default"}>
                                      {permission.name}
                                    </Tag>
                                  </Checkbox>
                                ))}
                              </div>
                            </div>
                          </Card>
                        ))}
                      </div>
                    )}
                  </>
                )}
              </>
            )}

            <Divider />

            <div style={{ background: token.colorBgContainer, padding: '16px', borderRadius: '8px', marginBottom: '16px' }}>
              <Title level={5}>{t('addFeatureToGroup.summary')}</Title>
              {selectedFeatures.length === 0 ? (
                <Text type="secondary">{t('addFeatureToGroup.noFeaturesSelected')}</Text>
              ) : (
                <>
                  <Text>{t('addFeatureToGroup.summaryText', { featureCount: selectedFeatures.length, permissionCount: selectedFeatures.reduce((total, feature) => total + feature.permissions.filter(p => p.isGranted).length, 0) })}</Text>
                  <div style={{ marginTop: '8px' }}>
                    {selectedFeatures.map(feature => (
                      <Tag key={feature.id} color="blue" style={{ marginBottom: '8px' }}>
                        {feature.featureName} ({feature.permissions.filter(p => p.isGranted).length}/{feature.permissions.length} {t('addFeatureToGroup.permissionsTag')})
                      </Tag>
                    ))}
                  </div>
                </>
              )}
            </div>
            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px' }}>
              <Button
                icon={<UndoOutlined />}
                onClick={handleReset}
              >
                {t('addFeatureToGroup.resetButton')}
              </Button>
              <Button
                type="primary"
                icon={<SaveOutlined />}
                onClick={handleSave}
                disabled={!selectedGroup || selectedFeatures.length === 0}
                loading={saving}
              >
                {t('addFeatureToGroup.saveButton')}
              </Button>
            </div>
          </Form>
        </Card>
      </div>
    </Layout>
  );
};

export default AddFeatureToGroupScreen;