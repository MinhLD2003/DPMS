import React, { useState, useEffect } from 'react';
import { Button, Input, Space, Table, message, Typography, Card, Row, Col, Tag, Tooltip, Select, DatePicker, TableProps } from 'antd';
import AxiosClient from '../../configs/axiosConfig';
import { useNavigate } from 'react-router-dom';
import moment from 'moment';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  InfoCircleOutlined
} from '@ant-design/icons';
import confirmDelete from '../../components/common/popup-modals/ConfirmDeleteModal';
import { useTranslation } from 'react-i18next';
import TableWithSkeleton from '../../components/forms/TableWithSkeleton';
import { PolicyStatus } from '../../enum/enum';
import { getPolicyStatus } from '../../utils/TextColorUtils';
import ListViewContainer from '../../components/layout/ListViewContainer';
import FeatureGuard from '../../routes/FeatureGuard';

const { Search } = Input;
const { Option } = Select;
const { Title } = Typography;

interface Policy {
  id: string;
  policyCode: string;
  title: string;
  description?: string;
  content?: string;
  status: string;
  createdAt: string;
  lastModifiedAt: string;
  createdById?: string;
  lastModifiedById?: string;
}

interface FilterParams {
  pageNumber: number;
  pageSize: number;
  sortBy: string;
  sortDirection: string;
  filters: Record<string, string>;
}

// Assuming you have predefined status values
const PolicyStatusList: string[] = ['Draft', 'Active', 'Inactive'];

const PolicyList: React.FC = () => {
  const { t } = useTranslation();

  const [searchTerm, setSearchTerm] = useState<string>('');
  const [selectedStatus, setSelectedStatus] = useState<string | null>(null);
  const [dateRange, setDateRange] = useState<[moment.Moment | null, moment.Moment | null] | null>(null);
  const [policies, setPolicies] = useState<Policy[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const navigate = useNavigate();
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [total, setTotal] = useState<number>(0);
  const [sortField, setSortField] = useState<string>('createdAt');
  const [sortOrder, setSortOrder] = useState<string>('desc');

  const buildFilterParams = (): FilterParams => {
    const filters: Record<string, string> = {};

    if (searchTerm) {
      filters['Title'] = searchTerm;
    }

    if (selectedStatus) {
      filters['Status'] = selectedStatus;
    }

    // Update date range filters to match backend implementation
    if (dateRange && dateRange[0] && dateRange[1]) {
      filters['createdAt_from'] = dateRange[0].format('YYYY-MM-DD');
      filters['createdAt_to'] = dateRange[1].format('YYYY-MM-DD');
    }

    return {
      pageNumber: currentPage,
      pageSize: pageSize,
      sortBy: sortField,
      sortDirection: sortOrder,
      filters: filters
    };
  };

  const fetchPolicies = async () => {
    setLoading(true);

    const params = buildFilterParams();

    try {
      // Convert filters to query params
      const queryParams = new URLSearchParams();
      queryParams.append('pageNumber', params.pageNumber.toString());
      queryParams.append('pageSize', params.pageSize.toString());
      queryParams.append('sortBy', params.sortBy);
      queryParams.append('sortDirection', params.sortDirection);

      Object.keys(params.filters).forEach((key) => {
        queryParams.append(`${key}`, params.filters[key]);
      });

      console.log('Generated query string:', queryParams.toString());

      const response = await AxiosClient.get(`/PrivacyPolicy?${queryParams.toString()}`);

      if (response.data) {
        const policiesData = response.data.data.map((policy: any, index: number) => ({
          id: policy.id,
          policyCode: policy.policyCode,
          title: policy.title,
          status: policy.status,
          createdAt: moment(policy.createdAt).format('DD/MM/YYYY'),
          lastModifiedAt: moment(policy.lastModifiedAt).format('DD/MM/YYYY'),
          createdById: policy.createdById,
          lastModifiedById: policy.lastModifiedById,
        }));

        setPolicies(policiesData);
        setTotal(response.data.totalRecords);
      }
    } catch (error) {
      message.error(t('policyList.fetchPoliciesFailed'));
      console.error(error);
    }
    setLoading(false);
  };

  // Fetch policies whenever filters change
  useEffect(() => {
    fetchPolicies();
  }, [currentPage, pageSize, sortField, sortOrder, searchTerm, selectedStatus, dateRange, t]);

  const handleUpdate = (id: string) => {
    navigate(`/dashboard/policies/update/${id}`);
  };

  const handleDelete = async (id: string) => {
    setLoading(true);
    try {
      await AxiosClient.delete(`PrivacyPolicy/${id}`);
      message.success(t('policyList.deletePolicySuccess'));
      fetchPolicies();
    } catch (error) {
      console.error('Failed to delete policy:', error);
      message.error(t('policyList.deletePolicyFailed'));
    }
    setLoading(false);
  };

  const handleSearch = (value: string) => {
    setSearchTerm(value);
    setCurrentPage(1); // Reset to first page when searching
  };

  const handleStatusChange = (value: string) => {
    setSelectedStatus(value || null);
    setCurrentPage(1);
  };

  const handleDateRangeChange = (dates: any) => {
    setDateRange(dates);
    setCurrentPage(1);
  };

  const handleTableChange = (pagination: any, filters: any, sorter: any) => {
    setCurrentPage(pagination.current);
    setPageSize(pagination.pageSize);
    // Handle sorting
    if (sorter && sorter.field) {
      setSortField(sorter.field);
      setSortOrder(sorter.order === 'ascend' ? 'asc' : 'desc');
    }
  };

  const handleClearFilters = () => {
    setSearchTerm('');
    setSelectedStatus(null);
    setDateRange(null);
    setCurrentPage(1);
    setSortField('createdAt');
    setSortOrder('desc');
  };

  const columns = [
    {
      title: t('policyList.policyCode'),
      dataIndex: 'policyCode',
      key: 'policyCode',
      sorter: true,
    },
    {
      title: t('policyList.title'),
      dataIndex: 'title',
      key: 'title',
      sorter: true,
    },
    {
      title: t('policyList.status'),
      dataIndex: 'status',
      key: 'status',
      render: (status: PolicyStatus) => getPolicyStatus(status, t),
      align: 'center' as 'center',
      sorter: true,
    },
    {
      title: t('policyList.createdAt'),
      dataIndex: 'createdAt',
      key: 'createdAt',
      sorter: true,
    },
    {
      title: t('actions'),
      key: 'actions',
      render: (text: any, record: Policy) => (
        <Space size="middle">
          <FeatureGuard requiredFeature='/api/PrivacyPolicy/{id}_PUT'>
            <Button
              type="primary"
              size='small'
              icon={<EditOutlined />}
              onClick={() => handleUpdate(record.id)}
            />
          </FeatureGuard>
          <FeatureGuard requiredFeature='/api/PrivacyPolicy/{id}_DELETE'>
            <Button
              type="primary"
              size='small'
              danger
              icon={<DeleteOutlined />}
              onClick={() =>
                confirmDelete(
                  () => handleDelete(record.id),
                  t("confirmDelete.title", { action: t("delete"), itemName: t("confirmDelete.thisPolicy") }),
                  t("confirmDelete.content"),
                  t("confirmDelete.okText", { action: t("delete") }),
                  t("confirmDelete.cancelText")
                )
              }
            />
          </FeatureGuard>
        </Space>
      ),
    },
  ];

  return (
    <ListViewContainer>
      <Row justify="space-between" align="middle" style={{ marginBottom: 24 }}>
        <Col>
          <Title level={2} style={{ margin: 0 }}>{t('policyList.policyManagement')}</Title>
        </Col>
        <Col>
          <FeatureGuard requiredFeature='/api/PrivacyPolicy_POST'>
            <Button type="primary"
              icon={<PlusOutlined />}
              onClick={() => navigate("create")}
              size="middle"
            >
              {t('policyList.newPolicy')}
            </Button>
          </FeatureGuard>
        </Col>
      </Row>

      <Space style={{ marginBottom: '16px' }} wrap>

        <Search
          placeholder={t('policyList.searchByTitle')}
          allowClear
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          onSearch={handleSearch}
          style={{ width: 200 }}
        />
        <Select
          style={{ width: 150 }}
          placeholder={t('policyList.status')}
          allowClear
          value={selectedStatus}
          onChange={handleStatusChange}
        >
          {PolicyStatusList.map(status => (
            <Option key={status} value={status}>{getPolicyStatus(status, t)}</Option>
          ))}
        </Select>
        {/* <RangePicker
          onChange={handleDateRangeChange}
          allowClear
          format="YYYY-MM-DD"
        /> */}
        <Button onClick={handleClearFilters}>{t('clear_filters')}</Button>
      </Space>

      <TableWithSkeleton
        columns={columns}
        rowKey="id"
        dataSource={policies}
        loading={loading}
        pagination={{
          current: currentPage,
          pageSize: pageSize,
          total: total,
          showTotal: (total, range) =>
            t("items_summary", {
              rangeStart: range[0],
              rangeEnd: range[1],
              total: total
            }),
          showSizeChanger: true,
          pageSizeOptions: ['10', '25', '50'],
        }}
        onChange={handleTableChange}
        bordered
        scroll={{ x: 'max-content' }}
        size="middle"
      />
    </ListViewContainer>
  );
};

export default PolicyList;