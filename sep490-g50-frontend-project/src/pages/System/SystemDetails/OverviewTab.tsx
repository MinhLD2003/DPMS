import React, { useState, useEffect, useCallback } from 'react';
import KeyIcon from '@mui/icons-material/Key';
import { useParams } from 'react-router-dom';
import {
  Typography, Card, Button, Space, Descriptions, Table, Tag,
  Avatar, Form, Input, theme, Select, Popconfirm, message,
  Spin, Modal, Row, Col,
  Tooltip
} from 'antd';
import {
  EditOutlined, UserOutlined, MailOutlined, FileTextOutlined,
  ClockCircleOutlined, SaveOutlined, DeleteOutlined, PlusOutlined,
  CloseCircleOutlined, SearchOutlined
} from '@ant-design/icons';
import PowerSettingsNewIcon from '@mui/icons-material/PowerSettingsNew';
import EditIcon from '@mui/icons-material/Edit';
import AxiosClient from '../../../configs/axiosConfig';
import { useTranslation } from 'react-i18next';
import { Group, SearchUser, SystemData, SystemUpdateStatusModel, TeamMember } from '../models/SystemModel';
import moment from 'moment';
import ApiKeyModal from './ApiKeyModal';
import { ExternalSystemStatusText } from '../../../enum/enum';
import { getExternalSystemStatusText } from '../../../utils/TextColorUtils';
import FeatureGuard from '../../../routes/FeatureGuard';

const { Text, Paragraph } = Typography;
const { useToken } = theme;
const { Option } = Select;

const OverviewTab: React.FC = () => {
  const { token } = useToken();
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation();

  // State management
  const [systemData, setSystemData] = useState<SystemData | null>(null);
  const [teamMembers, setTeamMembers] = useState<TeamMember[]>([]);
  const [originalTeamMembers, setOriginalTeamMembers] = useState<TeamMember[]>([]);
  const [groups, setGroups] = useState<Group[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingKey, setEditingKey] = useState<string | null>(null);
  const [isEditingGeneral, setIsEditingGeneral] = useState(false);
  const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);
  const [searchModalVisible, setSearchModalVisible] = useState(false);
  const [searchResults, setSearchResults] = useState<SearchUser[]>([]);
  const [searchLoading, setSearchLoading] = useState(false);
  const [allUsers, setAllUsers] = useState<SearchUser[]>([]);
  const [searchValue, setSearchValue] = useState('');
  const [isApiKeyModalVisible, setIsApiKeyModalVisible] = useState(false);
  const [apiKey, setApiKey] = useState<string>('');

  // Form instances
  const [generalForm] = Form.useForm();
  const [editForm] = Form.useForm();

  // Memoized functions
  const allUsersHaveValidGroups = useCallback(() => {
    return teamMembers.every(member => member.groups && member.groups.length > 0);
  }, [teamMembers]);

  const filterUsers = useCallback((value: string) => {
    if (!value || value.trim() === '') {
      setSearchResults(allUsers);
      return;
    }

    const lowercaseValue = value.toLowerCase();
    const filtered = allUsers.filter(
      user =>
        user.fullName.toLowerCase().includes(lowercaseValue) ||
        user.email.toLowerCase().includes(lowercaseValue)
    );
    setSearchResults(filtered);
  }, [allUsers]);

  // API calls
  const fetchSystemData = useCallback(async () => {
    try {
      setLoading(true);
      const response = await AxiosClient.get(`/ExternalSystem/${id}/get-system-details`);
      const data = response.data;

      setSystemData(data);
      if (data.users && data.users.length > 0) {
        const formattedMembers = data.users.map((member: any, index: number) => ({
          ...member,
          no: index + 1,
          key: member.id || member.key || `user-${index}`
        }));
        setTeamMembers(formattedMembers);
        setOriginalTeamMembers([...formattedMembers]);
      }
      if (data.groups && data.groups.length > 0) {
        const formattedGroups = data.groups.map((group: any) => ({
          id: group.id,
          name: group.name
        }));
        setGroups(formattedGroups);
      }

      setHasUnsavedChanges(false);
    } catch (error) {
      console.error('Error fetching system data:', error);
      message.error(t('overviewTab.fetchSystemDataFailed'));
    } finally {
      setLoading(false);
    }
  }, [id, t]);

  const fetchAllUsers = useCallback(async () => {
    if (allUsers.length > 0) return; // Only fetch once

    setSearchLoading(true);
    try {
      const response = await AxiosClient.get('/Group/fetch-all-global-users');
      setAllUsers(response.data || []);
      setSearchResults(response.data || []);
    } catch (error) {
      console.error('Error fetching all users:', error);
      message.error(t('overviewTab.fetchAllUsersFailed'));
    } finally {
      setSearchLoading(false);
    }
  }, [allUsers.length, t]);

  const handleGetApiKey = useCallback(async () => {
    try {
      const response = await AxiosClient.get(`/ExternalSystem/get-key/${id}`);
      const fetchedApiKey = response.data;
      if (fetchedApiKey) {
        setApiKey(fetchedApiKey);
        setIsApiKeyModalVisible(true);
        // Copy to clipboard
        navigator.clipboard.writeText(fetchedApiKey)
          .then(() => message.success(t('overviewTab.apiKeyCopied')))
          .catch(() => message.warning(t('overviewTab.apiKeyCopyFailed')));
        // Reload system data to update hasApiKey status
        await fetchSystemData();
      } else {
        message.error(t('overviewTab.noApiKeyFound'));
      }
    } catch (error) {
      console.error('Error fetching API key:', error);
      message.error(t('overviewTab.fetchApiKeyFailed'));
    }
  }, [id, t, fetchSystemData]);

  // Form handling
  const startEditingGeneral = useCallback(() => {
    setIsEditingGeneral(true);
    if (systemData) {
      generalForm.setFieldsValue({
        name: systemData.name,
        description: systemData.description,
      });
    }
  }, [generalForm, systemData]);

  const saveGeneralInfo = useCallback(async () => {
    try {
      const values = await generalForm.validateFields();
      setLoading(true);

      const updateModel = {
        name: values.name,
        description: values.description,
        domain: values.domain,
      };

      await AxiosClient.put(
        `/ExternalSystem/update-system?systemId=${id}`,
        updateModel
      );

      await fetchSystemData();
      setIsEditingGeneral(false);
      message.success(t('overviewTab.systemInfoUpdated'));
    } catch (error) {
      console.error('Error updating system data:', error);
      message.error(t('overviewTab.updateSystemInfoFailed'));
    } finally {
      setLoading(false);
    }
  }, [generalForm, id, t, fetchSystemData]);

  const cancelEditingGeneral = useCallback(() => {
    setIsEditingGeneral(false);
  }, []);

  // Team member management
  const isEditing = useCallback((record: TeamMember) => record.key === editingKey, [editingKey]);

  const edit = useCallback((record: TeamMember) => {
    const validGroups = record.groups
      .map(group => typeof group === 'object' ? group.id : group)
      .filter(groupId => groups.some(g => g.id === groupId));

    editForm.setFieldsValue({
      fullName: record.fullName,
      email: record.email,
      groups: validGroups,
      status: record.status,
    });
    setEditingKey(record.key);
  }, [editForm, groups]);

  const cancel = useCallback(() => {
    setEditingKey(null);
  }, []);

  const save = useCallback(async (key: string) => {
    try {
      const row = await editForm.validateFields();

      if (!row.groups || row.groups.length === 0) {
        message.error(t('overviewTab.selectAtLeastOneGroup'));
        return;
      }

      setTeamMembers(prev => {
        const newData = [...prev];
        const index = newData.findIndex(item => key === item.key);

        if (index > -1) {
          const item = newData[index];
          const updatedMember = {
            ...item,
            ...row,
            groups: row.groups.map((groupId: string) => {
              const group = groups.find(g => g.id === groupId);
              return group || { id: groupId, name: groupId };
            })
          };
          newData[index] = updatedMember;
        }
        return newData;
      });

      setEditingKey(null);
      setHasUnsavedChanges(true);
    } catch (error) {
      console.error('Error updating team member in table:', error);
    }
  }, [editForm, groups, t]);

  const handleDelete = useCallback((record: TeamMember) => {
    setTeamMembers(prev => prev.filter(item => item.key !== record.key));
    setHasUnsavedChanges(true);
  }, []);

  const handleAdd = useCallback(() => {
    setSearchModalVisible(true);
    setSearchValue('');
  }, []);

  const addUserFromSearch = useCallback((user: SearchUser) => {
    const isExisting = teamMembers.some(member => member.email === user.email);
    if (isExisting) {
      message.warning(t('overviewTab.userAlreadyMember'));
      return;
    }

    const newMember: TeamMember = {
      key: user.id,
      id: user.id,
      no: teamMembers.length + 1,
      fullName: user.fullName,
      email: user.email,
      groups: [],
      status: 1,
      lastTimeLogin: ""
    };

    setTeamMembers(prev => [...prev, newMember]);
    setSearchModalVisible(false);

    setEditingKey(newMember.key);
    editForm.setFieldsValue({
      fullName: newMember.fullName,
      email: newMember.email,
      groups: [],
      status: newMember.status,
    });

    message.info(t('overviewTab.selectGroupsForNewUser'));
  }, [teamMembers, t, editForm]);

  const saveAllChanges = useCallback(async () => {
    try {
      setLoading(true);

      interface GroupUserMapping {
        groupId: string;
        userIds: string[];
      }

      const validUserGroups: string[] = systemData?.groups?.map(x => x.id) || [];
      const groupedMappings: Record<string, GroupUserMapping> = {};

      validUserGroups.forEach(groupId => {
        groupedMappings[groupId] = {
          groupId: groupId,
          userIds: []
        };
      });

      teamMembers.forEach(member => {
        const userId = member.id || member.key;
        if (member.groups && member.groups.length > 0) {
          member.groups.forEach(group => {
            const groupId = typeof group === 'object' ? group.id : group;
            if (validUserGroups.includes(groupId)) {
              if (!groupedMappings[groupId].userIds.includes(userId)) {
                groupedMappings[groupId].userIds.push(userId);
              }
            }
          });
        }
      });

      const formattedData: GroupUserMapping[] = Object.values(groupedMappings);
      await AxiosClient.put(`/ExternalSystem/update-system-users?systemId=${id}`, formattedData);
      await fetchSystemData();

      message.success(t('overviewTab.changesSavedSuccessfully'));
      setHasUnsavedChanges(false);
    } catch (error) {
      console.error('Error saving team member changes:', error);
      message.error(t('overviewTab.saveChangesFailed'));
    } finally {
      setLoading(false);
    }
  }, [systemData, teamMembers, id, t, fetchSystemData]);

  // Event handlers
  const handleSearchInputChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchValue(e.target.value);
  }, []);

  // Effects
  useEffect(() => {
    fetchSystemData();
  }, [fetchSystemData]);

  useEffect(() => {
    if (searchModalVisible) {
      fetchAllUsers();
    }
  }, [searchModalVisible, fetchAllUsers]);

  useEffect(() => {
    if (allUsers.length > 0) {
      filterUsers(searchValue);
    }
  }, [searchValue, allUsers, filterUsers]);
  const handleConfirmStatusChange = (id: string, status: ExternalSystemStatusText) => {
    if (status == ExternalSystemStatusText.Activated) {
      Modal.confirm({
        title: t('system.activateConfirmTitle'),
        content: t('system.activateConfirmContent'),
        okText: t('confirm'),
        cancelText: t('cancel'),
        onOk: () => handleSystemStatusChange(id, 4), // Parse status to number
      })
    }
    if (status == ExternalSystemStatusText.Deactivated) {
      Modal.confirm({
        title: t('system.deactivateConfirmTitle'),
        content: t('system.deactivateConfirmContent'),
        okText: t('confirm'),
        cancelText: t('cancel'),
        onOk: () => handleSystemStatusChange(id, 5), // Parse status to number

      });
    };
  }
  const handleSystemStatusChange = async (id: string, systemStatus: number) => {
    setLoading(true);
    try {
      const payload: SystemUpdateStatusModel = {
        systemId: id,
        status: systemStatus
      }
      await AxiosClient.put(`/ExternalSystem/update-active-status`, payload);
      message.success(t('system.activateSuccess'));
      await fetchSystemData(); // Reload the data after successful activation
    } catch (error) {
      message.error(t('system.activateFailed'));
    }
    setLoading(false);
  }
  // Table columns definition
  const columns = [
    {
      title: t('overviewTab.fullName'),
      dataIndex: 'fullName',
      key: 'fullName',
      width: '25%',
      render: (_: any, record: TeamMember) => {
        const editable = isEditing(record);
        return editable ? (
          <Form.Item
            name="fullName"
            style={{ margin: 0 }}
            rules={[{ required: true, message: t('overviewTab.fullNameRequired') }]}
          >
            <Input />
          </Form.Item>
        ) : (
          <Space>
            <Text className='no-wrap-text'>{record.fullName}</Text>
          </Space>
        );
      }
    },
    {
      title: t('overviewTab.email'),
      dataIndex: 'email',
      key: 'email',
      width: '25%',
      render: (_: any, record: TeamMember) => {
        const editable = isEditing(record);
        return editable ? (
          <Form.Item
            name="email"
            style={{ margin: 0 }}
            rules={[
              { required: true, message: t('overviewTab.emailRequired') },
              { type: 'email', message: t('overviewTab.emailInvalid') }
            ]}
          >
            <Input />
          </Form.Item>
        ) : (
          <Text type="secondary" className="no-wrap-text" copyable>{record.email}</Text>
        );
      }
    },
    {
      title: t('overviewTab.groups'),
      dataIndex: 'groups',
      key: 'groups',
      render: (_: any, record: TeamMember) => {
        const editable = isEditing(record);

        const validGroups = record.groups
          .map(group => typeof group === 'object' ? group.id : group)
          .filter(groupId => groups.some(g => g.id === groupId));

        return editable ? (
          <Form.Item
            name="groups"
            style={{ margin: 0 }}
            initialValue={validGroups}
            rules={[{ required: true, message: t('overviewTab.selectAtLeastOneGroup') }]}
          >
            <Select mode="multiple" placeholder={t('overviewTab.selectGroups')}>
              {groups.map(group => (
                <Option key={group.id} value={group.id}>{group.name}</Option>
              ))}
            </Select>
          </Form.Item>
        ) : (
          <>
            {record.groups && record.groups.map(group => {
              const groupId = typeof group === 'object' ? group.id : group;
              const matchingGroup = groups.find(g => g.id === groupId);

              return matchingGroup ? (
                <Tag key={groupId} color="blue" style={{ marginBottom: '2px' }}>
                  {matchingGroup.name}
                </Tag>
              ) : null;
            })}
          </>
        );
      }
    },
    {
      title: t('actions'),
      key: 'action',
      width: '15%',
      render: (_: any, record: TeamMember) => {
        const editable = isEditing(record);
        return (
          <FeatureGuard requiredFeature='/api/ExternalSystem/update-system-users_PUT'>
            {editable ? (
              <Space>
                <Button
                  type="primary"
                  icon={<SaveOutlined />}
                  onClick={() => save(record.key)}
                  size="small"
                />
                <Popconfirm
                  title={t('overviewTab.cancelEditConfirm')}
                  onConfirm={cancel}>
                  <Button size="small" icon={<CloseCircleOutlined />} danger />
                </Popconfirm>
              </Space>
            ) : (
              <Space>
                <Button
                  type="primary"
                  ghost
                  disabled={editingKey !== null}
                  icon={<EditOutlined />}
                  onClick={() => edit(record)}
                  size="small"
                />
                <Popconfirm
                  title={t('overviewTab.deleteConfirm')}
                  onConfirm={() => handleDelete(record)}
                  disabled={editingKey !== null}
                >
                  <Button
                    danger
                    disabled={editingKey !== null}
                    icon={<DeleteOutlined />}
                    size="small"
                  />
                </Popconfirm>
              </Space>
            )}
          </FeatureGuard>
        );
      }
    }
  ];

  // Loading state
  if (loading && !systemData) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '50vh' }}>
        <Spin size="large" tip={t('overviewTab.loadingData')} />
      </div>
    );
  }

  return (
    <>
      <Row gutter={[20, 20]}>
        <Col xs={24} md={24} lg={12} xl={12}>
          <Card
            title={
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Space>
                  <FileTextOutlined style={{ color: token.colorPrimary }} />
                  <span>{t('overviewTab.generalInformation')}</span>
                </Space>
                <div>
                  <FeatureGuard requiredFeature='/api/ExternalSystem/get-key/{id}_GET'>
                    {systemData && !systemData.hasApiKey ?
                      <Button className='mr-4'
                        type="primary"
                        ghost
                        onClick={handleGetApiKey}
                        icon={<KeyIcon />}
                      //  size='small'
                      >{t('overviewTab.getApiKey')}
                      </Button>
                      :
                      <Button className='mr-4'
                        type="primary"
                        ghost
                        //   size='small'
                        onClick={handleGetApiKey}
                        icon={<KeyIcon />}
                      >{t('overviewTab.regenerateApiKey')}
                      </Button>
                    }
                  </FeatureGuard>
                  <FeatureGuard requiredFeature='/api/ExternalSystem/update-system_PUT'>
                    {!isEditingGeneral ? (
                      <Button
                        type="primary"
                        ghost
                        icon={<EditIcon />}
                        onClick={startEditingGeneral}
                        loading={loading}
                      >
                        {t('overviewTab.edit')}
                      </Button>
                    ) : (
                      <Space>
                        <Button
                          type="primary"
                          icon={<SaveOutlined />}
                          onClick={saveGeneralInfo}
                          loading={loading}
                        >
                          {t('overviewTab.save')}
                        </Button>
                        <Popconfirm
                          title={t('overviewTab.cancelEditConfirm')}
                          onConfirm={cancelEditingGeneral}
                        >
                          <Button
                            icon={<CloseCircleOutlined />}
                            danger
                          >
                            {t('overviewTab.cancel')}
                          </Button>
                        </Popconfirm>
                      </Space>
                    )}
                  </FeatureGuard>
                </div>
              </div>
            }
            style={{
              width: '100%',
              flexGrow: 1,
              boxShadow: '0 1px 2px rgba(0, 0, 0, 0.05)'
            }}
            bordered
            headStyle={{ borderBottom: `2px solid ${token.colorPrimary}` }}
          >
            {systemData && !isEditingGeneral ? (
              <Descriptions
                column={1}
                bordered={false}
                size="small"
                labelStyle={{
                  width: '30%',
                  color: token.colorTextSecondary,
                  fontWeight: 500
                }}
                contentStyle={{
                  color: token.colorText
                }}
              >

                {
                  systemData.hasApiKey ?
                    < Descriptions.Item
                      label={<Space><FileTextOutlined />API Key</Space>}>
                      <Text className="no-wrap-text" style={{ fontSize: 14 }} code>**************************************</Text>
                    </Descriptions.Item>
                    :
                    < Descriptions.Item
                      label={<Space><FileTextOutlined />API Key</Space>}>
                      <Text className="no-wrap-text" code>Not generated</Text>
                    </Descriptions.Item>
                }
                <Descriptions.Item
                  label={<Space><FileTextOutlined />{t('overviewTab.systemID')}</Space>}>
                  <Text className="no-wrap-text" code copyable>{systemData.id}</Text>
                </Descriptions.Item>
                <Descriptions.Item label={<Space><FileTextOutlined />{t('overviewTab.systemName')}</Space>}>
                  <Text className="no-wrap-text" strong>{systemData.name}</Text>
                </Descriptions.Item>
                <Descriptions.Item
                  label={<Space><FileTextOutlined />{t('addSystem.systemDomain')}</Space>}>
                  <Text className="no-wrap-text" code copyable>{systemData.domain}</Text>
                </Descriptions.Item>
                <Descriptions.Item label={<Space><FileTextOutlined />{t('overviewTab.description')}</Space>}>
                  <Paragraph ellipsis={{ rows: 3, expandable: true, symbol: t('overviewTab.more') }} style={{ marginBottom: 0 }}>
                    {systemData.description}
                  </Paragraph>
                </Descriptions.Item>
                <Descriptions.Item label={<Space><FileTextOutlined />{t('status')}</Space>}>
                  <Paragraph ellipsis={{ rows: 3, expandable: true, symbol: t('overviewTab.more') }} style={{ marginBottom: 0 }}>
                    {getExternalSystemStatusText(systemData.status || "Not binded", t)}
                  </Paragraph>
                  <FeatureGuard requiredFeature='/api/ExternalSystem/update-active-status_PUT'>
                    {systemData && systemData.status !== ExternalSystemStatusText.Activated && ( // Only show activate button if status is not 4 (Active)
                      <Tooltip title={t('activate')}>
                        <Button
                          className='ml-4'
                          type="primary"
                          ghost
                          icon={<PowerSettingsNewIcon />}
                          onClick={() => handleConfirmStatusChange(systemData.id, ExternalSystemStatusText.Activated)}
                        >{t('activate')}</Button>
                      </Tooltip>
                    )}

                    {systemData && systemData.status == ExternalSystemStatusText.Activated && ( // Only show activate button if status is not 4 (Active)
                      <Tooltip title={t('deactivate')}>
                        <Button
                          className='ml-4'
                          type="primary"
                          ghost
                          icon={<PowerSettingsNewIcon />}
                          onClick={() => handleConfirmStatusChange(systemData.id, ExternalSystemStatusText.Deactivated)}
                        >{t('deactivate')}</Button>
                      </Tooltip>
                    )}
                  </FeatureGuard>
                </Descriptions.Item>
                <Descriptions.Item label={<Space><ClockCircleOutlined />{t('overviewTab.createdBy')}</Space>}>
                  <Space>
                    <Avatar
                      style={{ backgroundColor: token.colorWarning, verticalAlign: 'middle' }}
                      size="small"
                    >
                      {systemData.createdBy?.charAt(0) || '?'}
                    </Avatar>
                    <Text className="no-wrap-text" strong>{systemData.createdBy}</Text>
                    <Text className="no-wrap-text" type="secondary">{t('overviewTab.at')} {moment(systemData.createdAt).format("DD/MM/YYYY HH:mm:ss")}</Text> </Space>
                </Descriptions.Item>
                <Descriptions.Item label={<Space><ClockCircleOutlined />{t('overviewTab.lastUpdatedBy')}</Space>}>
                  <Space>
                    <Avatar
                      style={{ backgroundColor: token.colorWarning, verticalAlign: 'middle' }}
                      size="small"
                    >
                      {systemData.lastModifiedBy?.charAt(0) || '?'}
                    </Avatar>
                    <Text className="no-wrap-text" strong>{systemData.lastModifiedBy}</Text>
                    <Text className="no-wrap-text" type="secondary">{t('overviewTab.at')} {moment(systemData.lastModifiedAt).format("DD/MM/YYYY HH:mm:ss")}</Text>
                  </Space>
                </Descriptions.Item>

              </Descriptions>
            ) : systemData && isEditingGeneral ? (
              <Form
                form={generalForm}
                layout="vertical"
                style={{ padding: '0 8px' }}
                initialValues={systemData}
              >
                <Form.Item name="name" label={<Space><FileTextOutlined />{t('overviewTab.systemName')}</Space>} rules={[{ required: true, message: t('overviewTab.systemNameRequired') }]}>
                  <Input />
                </Form.Item>
                <Form.Item name="description" label={<Space><FileTextOutlined />{t('overviewTab.description')}</Space>}>
                  <Input.TextArea rows={3} />
                </Form.Item>
                <Form.Item name="domain" label={<Space><FileTextOutlined />Domain</Space>}>
                  <Input />
                </Form.Item>
                
                {/* Non-editable fields */}
                <Descriptions
                  column={1}
                  bordered={false}
                  size="small"
                  labelStyle={{
                    width: '30%',
                    color: token.colorTextSecondary,
                    fontWeight: 500
                  }}
                  contentStyle={{
                    color: token.colorText
                  }}
                >
                  <Descriptions.Item label={<Space><FileTextOutlined />{t('overviewTab.systemID')}</Space>}>
                    <Text code copyable>{systemData.id}</Text>
                  </Descriptions.Item>
                  <Descriptions.Item label={<Space><FileTextOutlined />{t('addSystem.systemDomain')}</Space>}>
                    <Text code copyable>{systemData.domain}</Text>
                  </Descriptions.Item>
                  <Descriptions.Item label={<Space><ClockCircleOutlined />{t('overviewTab.createdBy')}</Space>}>
                    <Space>
                      <Avatar
                        style={{ backgroundColor: token.colorWarning, verticalAlign: 'middle' }}
                        size="small"
                      >
                        {systemData.createdBy?.charAt(0) || '?'}
                      </Avatar>
                      <Text className="no-wrap-text" strong>{systemData.createdBy}</Text>
                      <Text className="no-wrap-text" type="secondary">{t('overviewTab.at')} {moment(systemData.createdAt).format("DD/MM/YYYY HH:mm:ss")}</Text>
                    </Space>
                  </Descriptions.Item>
                  <Descriptions.Item label={<Space><ClockCircleOutlined />{t('overviewTab.lastUpdatedBy')}</Space>}>
                    <Space>
                      <Avatar
                        style={{ backgroundColor: token.colorWarning, verticalAlign: 'middle' }}
                        size="small"
                      >
                        {systemData.lastModifiedBy?.charAt(0) || '?'}
                      </Avatar>
                      <Text className="no-wrap-text" strong>{systemData.lastModifiedBy}</Text>
                      <Text type="secondary">{t('overviewTab.at')} {moment(systemData.lastModifiedAt).format("DD/MM/YYYY HH:mm:ss")}</Text>
                    </Space>
                  </Descriptions.Item>
                </Descriptions>
              </Form>
            ) : (
              <Spin size="large" tip={t('overviewTab.loadingSystemData')} />
            )}
          </Card>
        </Col>
        <Col xs={24} md={24} lg={12} xl={12}>
          <Card
            title={
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Space>
                  <UserOutlined style={{ color: token.colorPrimary }} />
                  <span>{t('overviewTab.systemStaffs')}</span>
                </Space>
                <Space>
                  {hasUnsavedChanges && (
                    <Button
                      type="primary"
                      icon={<SaveOutlined />}
                      onClick={saveAllChanges}
                      loading={loading}
                      disabled={!allUsersHaveValidGroups()}
                      title={!allUsersHaveValidGroups() ? t('overviewTab.allUsersMustHaveGroup') : ""}
                    >
                      {t('overviewTab.saveChanges')}
                    </Button>
                  )}
                  <FeatureGuard requiredFeature='/api/ExternalSystem/update-system-users_PUT'>
                    <Button
                      type="primary"
                      ghost
                      icon={<PlusOutlined />}
                      onClick={handleAdd}
                      disabled={editingKey !== null || loading}
                    >
                      {t('overviewTab.addUser')}
                    </Button>
                  </FeatureGuard>
                </Space>
              </div>
            }
            style={{
              width: '100%',
              flexGrow: 1,
              boxShadow: '0 1px 2px rgba(0, 0, 0, 0.05)'
            }}
            bordered
            headStyle={{ borderBottom: `2px solid ${token.colorPrimary}` }}
          >
            <Form form={editForm} component={false}>
              <Table
                dataSource={teamMembers}
                columns={columns}
                pagination={false}
                size="small"
                style={{ borderRadius: '8px', overflow: 'hidden' }}
                rowClassName={(record, index) => {
                  const isCurrentlyEditing = record.key === editingKey;
                  const isNew = !originalTeamMembers.some(original => original.key === record.key);
                  const isModified = originalTeamMembers.some(original => {
                    return original.key === record.key && JSON.stringify(original) !== JSON.stringify(record);
                  });

                  if (isCurrentlyEditing) return 'editing-row';
                  if (isNew) return 'new-row';
                  if (isModified) return 'modified-row';
                  return index % 2 === 0 ? '' : 'ant-table-row-light';
                }}
                loading={loading}
              />
            </Form>
          </Card>
        </Col>
      </Row >

      {/* User Search Modal */}
      < Modal
        title={t('overviewTab.searchUsers')}
        open={searchModalVisible}
        onCancel={() => setSearchModalVisible(false)}
        footer={null}
        width={600}
      >
        <div style={{ marginBottom: 16 }}>
          <Input
            placeholder={t('overviewTab.searchUsersPlaceholder')}
            allowClear
            prefix={<SearchOutlined />}
            size="large"
            onChange={handleSearchInputChange}
            value={searchValue}
            style={{ width: '100%' }}
          />
        </div>
        {
          searchLoading ? (
            <div style={{ textAlign: 'center', padding: '20px 0' }}>
              <Spin tip={t('overviewTab.loadingUsers')} />
            </div>
          ) : (
            <Table
              dataSource={searchResults}
              columns={[
                {
                  title: t('overviewTab.fullName'),
                  dataIndex: 'fullName',
                  key: 'fullName',
                  render: (text) => (
                    <Space>
                      <Text>{text}</Text>
                    </Space>
                  )
                },
                {
                  title: "Email",
                  dataIndex: 'email',
                  key: 'email',
                },
                {
                  title: t('actions'),
                  key: 'action',
                  width: 100,
                  render: (_, record) => (
                    <Button
                      type="primary"
                      size="small"
                      onClick={() => addUserFromSearch(record)}
                    >
                      {t('overviewTab.add')}
                    </Button>
                  ),
                },
              ]}
              pagination={{ pageSize: 6 }}
              size="small"
              locale={{ emptyText: t('overviewTab.noUsersFound') }}
            />
          )
        }
      </Modal >

      <ApiKeyModal
        isVisible={isApiKeyModalVisible}
        apiKey={apiKey}
        onClose={() => setIsApiKeyModalVisible(false)}
      />
    </>
  );
};

export default OverviewTab;