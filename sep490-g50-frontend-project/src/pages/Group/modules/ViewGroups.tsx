import React, { useState, useEffect, useCallback } from 'react';
import { Button, DatePicker, Input, Space, Table, Tooltip, message, TableProps, Row, Col, Typography, Card, Tag, Select } from 'antd';
import AxiosClient from '../../../configs/axiosConfig';
import { useNavigate } from 'react-router-dom';
import { DeleteOutlined, EditOutlined, PlusOutlined } from '@ant-design/icons';
import GroupAddIcon from '@mui/icons-material/GroupAdd';
import GroupRemoveIcon from '@mui/icons-material/GroupRemove';
import { GroupModel } from '../models/GroupModel';
import groupService from '../apis/GroupAPIs';
import confirmDelete from '../../../components/common/popup-modals/ConfirmDeleteModal';
import { debounce } from 'lodash';
import BookmarkAddOutlinedIcon from '@mui/icons-material/BookmarkAddOutlined';
import TableWithSkeleton from '../../../components/forms/TableWithSkeleton';
import { getGroupText } from '../../../utils/TextColorUtils';
import { useTranslation } from 'react-i18next';
import UpdateGroupModal from './UpdateGroupModal';
import CreateGroupModal from './CreateGroupModal';
import { FilterParams } from '../../../common/FilterParams';
import ListViewContainer from '../../../components/layout/ListViewContainer';
import FeatureGuard from '../../../routes/FeatureGuard';
const { Search } = Input;
const { Title, Text } = Typography;

const ViewGroups: React.FC = () => {
  const navigate = useNavigate();
  const { t } = useTranslation();

  const [groups, setGroups] = useState<GroupModel[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  const [currentPage, setCurrentPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(20);
  const [total, setTotal] = useState<number>(0);
  const [searchKey, setSearchKey] = useState<string>("");
  const [isGlobalFilter, setIsGlobalFilter] = useState<string>("true"); // "" means all, "true", "false"


  const buildFilterParams = (): FilterParams => {
    const filters: Record<string, string | boolean> = {}; // Updated type
    if (searchKey) filters['name'] = searchKey;

    if (isGlobalFilter === "true") {
      filters['isGlobal'] = true;
    } else if (isGlobalFilter === "false") {
      filters['isGlobal'] = false;
    }
    // Otherwise, no isGlobal filter is applied.

    return { pageNumber: currentPage, pageSize, filters };
  };
  const fetchGroups = useCallback(async () => {
    setLoading(true);
    try {
      const filterParams = buildFilterParams();
      const result = await groupService.getGroups(filterParams);
      if (result.success) {
        setGroups(result.objectList);
        setTotal(result.totalCount);
      } else {
        message.error("Failed to fetch groups");
      }
    }
    catch (error) {
      message.error("Failed to fetch groups");
    }
    finally {
      setLoading(false);
    }
  }, [searchKey, pageSize, currentPage, isGlobalFilter]);

  useEffect(() => {
    fetchGroups();
  }, [fetchGroups]);


  const handleDelete = async (id: string) => {
    // console.log('Delete group with guid:', guid);
    setLoading(true);
    try {
      await AxiosClient.delete(`/Group/${id}`);
      fetchGroups();
      message.success('Group deleted successfully!');
    } catch (error) {
      // console.error('Failed to delete group:', error);
      message.error('Failed to delete group.');
    }
    setLoading(false);
  };
  const handleTableChange = (pagination: any) => {
    setCurrentPage(pagination.current);
    setPageSize(pagination.pageSize);
  };
  const navigateToAddUsers = async (groupId: string, groupName: string) => {
    navigate(`/dashboard/groups/${groupId}/add-users`, {
      state: { groupName }
    });
  };
  const navigateToRemoveUsers = async (groupId: string, groupName: string) => {
    navigate(`/dashboard/groups/${groupId}/${groupName}/remove-users`, {
      //state: { groupName }
    });
  };
  const resetFilters = () => {
    setSearchKey('');
    setCurrentPage(1);
  };
  const handleIsGlobalFilterChange = (value: string) => {
    setIsGlobalFilter(value);
    setCurrentPage(1); // Reset to first page when filter changes
  };
  const debouncedSearch = useCallback(
    debounce((value: string) => {
      setSearchKey(value);
      setCurrentPage(1);
    }, 2000),
    []
  );
  useEffect(() => {
    return () => {
      debouncedSearch.cancel();
    };
  }, [debouncedSearch]);

  const columns: TableProps<GroupModel>['columns'] = [
    {
      title: t('no'),
      key: 'index',
      render: (_text: any, _record: any, index: number) =>
        (currentPage - 1) * pageSize + index + 1,
      align: 'center' as const,
    },
    {
      title: t('groupname'),
      dataIndex: 'name',
      key: 'name',
      render: (groupName: string) => getGroupText(groupName),
    },
    {
      title: t('description'),
      dataIndex: 'description',
      key: 'description',
    },
    // {
    //   title: t('isGlobal'),
    //   dataIndex: 'isGlobal',
    //   key: 'isGlobal',
    //   render: (isGlobal: boolean) => isGlobal ? <Tag color='red'>Global</Tag> : <Tag color='red'>System</Tag>
    // },
    {
      title: t('actions'),
      key: 'actions',
      align: 'center',
      render: (record: GroupModel) => (
        <Space size="middle">
          <FeatureGuard requiredFeature='/api/Group/add-user-to-group_POST'>
          <Tooltip title={t('tooltip.add_users')}>
            <Button
              type="primary"
              size="small"
              onClick={() => navigateToAddUsers(record.id, record.name)}
              icon={<GroupAddIcon fontSize='small' />}
            />
          </Tooltip>
          </FeatureGuard>
          <FeatureGuard requiredFeature='/api/Group/{id}_PUT'>
            <Tooltip title={t('tooltip.edit_group')}>
              <UpdateGroupModal id={record.id} onUpdateSuccess={fetchGroups} />
            </Tooltip>
          </FeatureGuard>
          <FeatureGuard requiredFeature='/api/Group/remove-user-from-group_DELETE'>
            <Tooltip title={t('tooltip.remove_users')}>
              <Button
                type="primary"
                size="small"
                danger
                onClick={() => navigateToRemoveUsers(record.id, record.name)}
                icon={<GroupRemoveIcon fontSize='small' />}
              />
            </Tooltip>
          </FeatureGuard>
          <FeatureGuard requiredFeature='/api/Group/{id}_DELETE'>
            <Tooltip title={t('tooltip.delete_group')}>
              <Button
                danger
                type="primary"
                size="small"
                onClick={() =>
                  confirmDelete(
                    () => handleDelete(record.id),
                    t("confirmDelete.title", { action: t("delete"), itemName: t("confirmDelete.thisGroup") }),
                    t("confirmDelete.content"),
                    t("confirmDelete.okText", { action: t("delete") }),
                    t("confirmDelete.cancelText")
                  )
                }
                icon={<DeleteOutlined />}
              />
            </Tooltip>
          </FeatureGuard>
        </Space>
      ),
    },
  ];

  return (
    <ListViewContainer>

      <Row justify="space-between" align="middle" style={{ marginBottom: 24 }}>
        <Col>
          <Title level={2} style={{ margin: 0 }}>{t("page_title.group_list")}</Title>
        </Col>
        <Col>
          <FeatureGuard requiredFeature='/api/Group_POST'>
            <CreateGroupModal onCreateSuccess={fetchGroups} />
          </FeatureGuard>
        </Col>
      </Row>
      <Space style={{ marginBottom: 16 }} wrap>
        <FeatureGuard requiredFeature='/api/Feature/add-feature-to-group_POST'>
          <Button type="primary" icon={<BookmarkAddOutlinedIcon />}
            onClick={() => navigate("/dashboard/features/add-feature-to-group")} >{t('tooltip.add_feature_to_group')}</Button>
        </FeatureGuard>
        <Search
          placeholder={t('search_placeholder')}
          allowClear
          onChange={(e) => debouncedSearch(e.target.value)}
        />
        {/* <Select
          placeholder={t('isGlobal')}
          onChange={handleIsGlobalFilterChange}
          value={isGlobalFilter}
          allowClear
          className='w-40'
        >
          <Select.Option value="">{t('all')}</Select.Option>
          <Select.Option value="true">{t('global')}</Select.Option>
          <Select.Option value="false">{t('notGlobal')}</Select.Option>
        </Select> */}
        <Button
          onClick={resetFilters}
          style={{ backgroundColor: '#52c41a', color: 'white' }}
        >
          {t('clear_filters')}
        </Button>
      </Space>
      <TableWithSkeleton
        columns={columns}
        rowKey={'id'}
        dataSource={groups}
        loading={loading}
        pagination={{
          current: currentPage,
          pageSize: pageSize,
          total: total,
          showTotal: (total, range) => `${range[0]}-${range[1]} ${t('table.of')} ${total} ${t('table.items')}`,
          showSizeChanger: true,
          pageSizeOptions: ['10', '20', '50'],
        }}
        onChange={handleTableChange}
        bordered
        scroll={{ x: 'max-content' }}
        size="middle"
      />
    </ListViewContainer>
  );
};

export default ViewGroups;


