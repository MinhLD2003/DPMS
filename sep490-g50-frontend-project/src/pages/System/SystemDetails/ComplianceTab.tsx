import React, { useState, useEffect } from 'react';
import {
  Button,
  Space,
  message,
  Tooltip,
  Input,
  DatePicker,
  Select,
  Empty,
  Card,
  Typography,
  Skeleton,
  Alert,
  Flex,
  Tag
} from 'antd';
import AxiosClient from '../../../configs/axiosConfig';
import { useNavigate, useParams } from 'react-router-dom';
import moment from 'moment';
import {
  InfoCircleOutlined,
  DownloadOutlined,
  EditOutlined,
  PlusOutlined,
  FilterOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import { FICListViewModel } from '../../FIC/models/FICListViewModel';
import FileDownloadButton from '../../../components/common/ExcelDownloadButton';
import TableWithSkeleton from '../../../components/forms/TableWithSkeleton';
import { useTranslation } from 'react-i18next';
import FeatureGuard from '../../../routes/FeatureGuard';
import { SorterResult, TablePaginationConfig } from 'antd/lib/table/interface';
import { FilterValue } from 'antd/lib/table/interface';
import { TableCurrentDataSource } from 'antd/lib/table/interface';
import { getUserFeatures } from '../../../utils/jwtDecodeUtils';
const { Search } = Input;
const { RangePicker } = DatePicker;
const { Title, Text } = Typography;

interface FilterParams {
  systemId: string;
}

type SortDirection = 'ascend' | 'descend' | undefined;

interface SortState {
  field: string;
  order: SortDirection;
}
interface ExtendedFICListViewModel extends FICListViewModel {
  createdAtRaw?: number;
}
const ComplianceTab: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();

  // State variables
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [selectedStatus, setSelectedStatus] = useState<string>('');
  const [dateRange, setDateRange] = useState<[moment.Moment, moment.Moment] | undefined>(undefined);
  const [FICs, setFICs] = useState<ExtendedFICListViewModel[]>([]);
  const [displayedFICs, setDisplayedFICs] = useState<ExtendedFICListViewModel[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [initialLoading, setInitialLoading] = useState<boolean>(true);
  const [hasSubmissions, setHasSubmissions] = useState<boolean>(false);
  const [error, setError] = useState<string>('');
  const userFeatures = getUserFeatures();

  // Pagination and sorting
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [total, setTotal] = useState<number>(0);
  const [sortState, setSortState] = useState<SortState>({
    field: 'createdAt',
    order: 'descend'
  });
  const [filtersApplied, setFiltersApplied] = useState<boolean>(false);

  // Build filter params based on current state
  const buildFilterParams = (): FilterParams => {
    return {
      systemId: id || '',
    };
  };

  const fetchSubmissions = async (showLoading = true) => {
    if (!id) return;

    if (showLoading) {
      setLoading(true);
    }
    setError('');

    const params = buildFilterParams();

    try {
      const response = await AxiosClient.get(`/Form/get-submissions`, {
        params: { systemId: params.systemId }
      });

      if (response.data) {
        const formattedFICs = response.data.map((item: Record<string, unknown>) => ({
          id: String(item.id || ''),
          name: String(item.name || ''),
          description: String(item.description || ''),
          createdAt: moment(item.createdAt as string).format('DD/MM/YYYY HH:mm:ss'),
          createdAtRaw: moment(item.createdAt as string).valueOf(), // Raw timestamp for sorting
          lastModifiedAt: moment(item.lastModifiedAt as string).format('DD/MM/YYYY'),
          createdBy: String(item.createdBy || '...'),
          createdById: String(item.createdById || ''),
          lastModifiedById: String(item.lastModifiedById || ''),
          index: 0, // Will be set by addIndexToUnpagedData
        }));

        setFICs(formattedFICs);
        setTotal(formattedFICs.length);
        setHasSubmissions(formattedFICs.length > 0);

        // Apply initial sorting and filtering
        applyFiltersAndSort(formattedFICs);
      } else {
        setHasSubmissions(false);
        setFICs([]);
        setDisplayedFICs([]);
        setTotal(0);
      }
    } catch (err) {
      console.error('Error fetching submissions:', err);
      setError('Failed to fetch submissions. Please try again later.');
      setHasSubmissions(false);
      setFICs([]);
      setDisplayedFICs([]);
    } finally {
      setLoading(false);
      setInitialLoading(false);
    }
  };

  // Sort and filter function that will be called whenever filters or sort change
  const applyFiltersAndSort = (data: ExtendedFICListViewModel[] = FICs) => {
    let filteredData = [...data];

    // Apply search filter
    if (searchTerm) {
      filteredData = filteredData.filter(item =>
        item.name.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Apply status filter if needed
    if (selectedStatus) {
      filteredData = filteredData.filter(item =>
        // Assuming there's a status field in your model
        // Adjust accordingly
        String(item.status).toLowerCase() === selectedStatus.toLowerCase()
      );
    }

    // Apply date range filter
    if (dateRange && dateRange[0] && dateRange[1]) {
      const startDate = dateRange[0].startOf('day');
      const endDate = dateRange[1].endOf('day');

      filteredData = filteredData.filter(item => {
        const itemDate = moment(item.createdAt, 'DD/MM/YYYY HH:mm:ss');
        return itemDate.isBetween(startDate, endDate, undefined, '[]'); // inclusive
      });
    }

    // Apply sorting
    const { field, order } = sortState;

    if (field && order) {
      filteredData.sort((a, b) => {
        let comparison = 0;

        // Special handling for date fields
        if (field === 'createdAt') {
          const aValue: any = a.createdAtRaw;
          const bValue: any = b.createdAtRaw;
          comparison = aValue < bValue ? -1 : aValue > bValue ? 1 : 0;
        } else {
          // Generic string comparison for other fields
          const aValue = String(a[field as keyof ExtendedFICListViewModel] || '').toLowerCase();
          const bValue = String(b[field as keyof ExtendedFICListViewModel] || '').toLowerCase();
          comparison = aValue < bValue ? -1 : aValue > bValue ? 1 : 0;
        }

        return order === 'ascend' ? comparison : -comparison;
      });
    }

    // Calculate pagination
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedData = filteredData.slice(startIndex, endIndex);

    // Update total for pagination
    setTotal(filteredData.length);

    // Update displayed data
    setDisplayedFICs(paginatedData);
  };

  useEffect(() => {
    fetchSubmissions();
  }, [id]);

  // Handle changes in filters and sorting
  useEffect(() => {
    if (initialLoading) return;

    const timer = setTimeout(() => {
      setCurrentPage(1);
      setFiltersApplied(!!(searchTerm || selectedStatus || dateRange));
      applyFiltersAndSort();
    }, 300);

    return () => clearTimeout(timer);
  }, [searchTerm, selectedStatus, dateRange, sortState]);

  // Handle pagination changes
  useEffect(() => {
    if (initialLoading) return;
    applyFiltersAndSort();
  }, [currentPage, pageSize]);

  const goToDetail = (id: string) => {
    navigate(`/dashboard/forms/submissions/${id}`);
  };

  const handleRedirectToCreate = () => {
    navigate(`create-system-fic`);
  };


  const handleTableChange = (
    pagination: TablePaginationConfig,
    filters: Record<string, FilterValue | null>,
    sorter: SorterResult<FICListViewModel> | SorterResult<FICListViewModel>[],
    extra: TableCurrentDataSource<FICListViewModel>
  ) => {
    setCurrentPage(pagination.current || 1);
    setPageSize(pagination.pageSize || 10);

    // Handle sorting
    if (!Array.isArray(sorter) && sorter.field && sorter.order) {
      setSortState({
        field: sorter.field as string,
        order: sorter.order
      });
    } else {
      // Default sort if sorter is cleared
      setSortState({
        field: 'createdAt',
        order: 'descend'
      });
    }
  };

  const handleClearFilters = () => {
    setSearchTerm('');
    setSelectedStatus('');
    setDateRange(undefined);
    setFiltersApplied(false);
    setSortState({
      field: 'createdAt',
      order: 'descend'
    });
    setCurrentPage(1);
    // Let the effect handle reapplying filters and sort
  };

  const handleRefresh = () => {
    fetchSubmissions(true);
  };

  const columns = [
    {
      title: t('complianceTab.no'),
      dataIndex: 'index',
      key: 'index',
      width: '60px',
      render: (_: any, __: any, index: number) => {
        return ((currentPage - 1) * pageSize) + index + 1;
      }
    },
    {
      title: t('complianceTab.name'),
      dataIndex: 'name',
      key: 'name',
      sorter: true,
      render: (text: string, record: FICListViewModel) => (
        <span
          style={{ color: '#1890ff', cursor: 'pointer' }}

          onClick={() => {
            if (userFeatures.includes('/api/Form/submission/{id}_GET'))
              goToDetail(record.id)
          }
          }
        >
          {text}
        </span>
      )
    },
    {
      title: t('complianceTab.createdAt'),
      dataIndex: 'createdAt',
      key: 'createdAt',
      sorter: true,
      defaultSortOrder: 'descend' as const
    },
    {
      title: t('complianceTab.createdBy'),
      dataIndex: 'createdBy',
      key: 'createdBy',
      sorter: true
    },
    {
      title: t('actions'),
      key: 'actions',
      width: '120px',
      render: (_: unknown, record: FICListViewModel) => (
        <Space size="small">
          <FeatureGuard requiredFeature='/api/Form/submission/{id}_GET'>
            <Tooltip title={t('complianceTab.viewDetails')}>
              <Button
                type="primary"
                onClick={() => goToDetail(record.id)}
                icon={<InfoCircleOutlined />}
                size="middle"
              />
            </Tooltip>
          </FeatureGuard>
          <FeatureGuard requiredFeature='/api/Form/export-submission/{id}_GET'>
            <FileDownloadButton
              apiPath={`/Form/export-submission/${record.id}`}
              filename={`FIC_Submission_${new Date().toISOString().slice(0, 10)}.xlsx`}
            />
          </FeatureGuard>
        </Space>
      ),
    },
  ];

  // Render loading state
  if (initialLoading) {
    return (
      <Card className="shadow-sm">
        <Skeleton active paragraph={{ rows: 6 }} />
      </Card>
    );
  }

  // Render error state
  if (error) {
    return (
      <Card className="shadow-sm">
        <Alert
          message={t('complianceTab.error')}
          description={
            <div>
              <p>{error}</p>
              <Button
                type="primary"
                icon={<ReloadOutlined />}
                onClick={handleRefresh}
              >
                {t('complianceTab.tryAgain')}
              </Button>
            </div>
          }
          type="error"
          showIcon
        />
      </Card>
    );
  }

  // Render empty state if no submissions
  if (!hasSubmissions) {
    return (
      <Card className="shadow-sm">
        <div style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', padding: '40px 0' }}>
          <Empty
            description={
              <div style={{ textAlign: 'center' }}>
                <Text style={{ fontSize: '16px', color: '#6b7280', display: 'block', marginBottom: '8px' }}>
                  {t('complianceTab.noSubmissionsFound')}
                </Text>
                {filtersApplied && (
                  <Text type="secondary">
                    {t('complianceTab.adjustFilters')}
                  </Text>
                )}
              </div>
            }
            image={Empty.PRESENTED_IMAGE_SIMPLE}
          />
          <Space style={{ marginTop: '24px' }}>
            <Button
              type="primary"
              icon={<PlusOutlined />}
              size="large"
              onClick={handleRedirectToCreate}
            >
              {t('complianceTab.createNewSubmission')}
            </Button>
            {filtersApplied && (
              <Button
                icon={<FilterOutlined />}
                onClick={handleClearFilters}
              >
                {t('complianceTab.clearFilters')}
              </Button>
            )}
          </Space>
        </div>
      </Card>
    );
  }

  // Main content with table
  return (
    <Card className="shadow-sm">
      <Flex justify="space-between" align="center" style={{ marginBottom: '16px' }}>
        <Title level={4} style={{ margin: 0 }}>{t('complianceTab.complianceSubmissions')}</Title>
        <FeatureGuard requiredFeature='/api/Form/submit_POST'>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            size="middle"
            onClick={handleRedirectToCreate}
          >
            {t('complianceTab.createNewSubmission')}
          </Button>
        </FeatureGuard>
      </Flex>

      {/* Filter components - uncomment if needed
      <Card className="filter-card" style={{ marginBottom: '16px', background: '#f9fafb' }}>
        <Flex wrap="wrap" gap="small">
          <Search
            placeholder={t('complianceTab.searchByTitle')}
            allowClear
            value={searchTerm}
            onChange={e => handleSearch(e.target.value)}
            style={{ width: 250 }}
          />

          <RangePicker
            onChange={handleDateRangeChange}
            format="YYYY-MM-DD"
            value={dateRange}
            placeholder={[t('complianceTab.startDate'), t('complianceTab.endDate')]}
          />

          <Button
            onClick={handleClearFilters}
            icon={<FilterOutlined />}
            disabled={!filtersApplied}
          >
            {t('complianceTab.clearFilters')}
          </Button>

          <Button
            onClick={handleRefresh}
            icon={<ReloadOutlined />}
          >
            {t('complianceTab.refresh')}
          </Button>
        </Flex>
      </Card>
      */}

      <TableWithSkeleton
        columns={columns}
        dataSource={displayedFICs}
        loading={loading}
        pagination={{
          current: currentPage,
          pageSize,
          total,
          showSizeChanger: true,
          pageSizeOptions: ['10', '20', '50'],
          showTotal: (total) => t('complianceTab.totalItems', { total })
        }}
        onChange={handleTableChange}
        rowKey="id"
        locale={{ emptyText: t('complianceTab.noSubmissions') }}
        scroll={{ x: 'max-content' }}
        rowClassName={() => 'table-row-hover'}
      />
    </Card>
  );
};

export default ComplianceTab;