import React, { useState, useEffect, useCallback } from 'react';
import { Button, DatePicker, Input, Space, Table, message, Typography, Card, Row, Col, Tag, Tooltip, Select, TableProps } from 'antd';
import AxiosClient from '../../configs/axiosConfig';
import { useNavigate } from 'react-router-dom';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  ReloadOutlined,
  LinkOutlined,
  InfoCircleOutlined,
  BranchesOutlined
} from '@ant-design/icons';
import featureService from './apis/FeatureAPIs';
import confirmDelete from '../../components/common/popup-modals/ConfirmDeleteModal';
import { addIndexToData } from '../../utils/indexHelper';
import { FeatureViewModel } from './models/FeatureViewModel';
import { FeatureStatus, HttpMethodType } from '../../enum/enum';
import { debounce } from 'lodash';
import { getHttpMethodText } from '../../utils/TextColorUtils';
import TableWithSkeleton from '../../components/forms/TableWithSkeleton';
import { FilterParams } from '../../common/FilterParams';
import { useTranslation } from 'react-i18next';
import CreateFeatureModal from './CreateFeatureModal';
import UpdateFeatureModal from './UpdateFeatureModal';
import ListViewContainer from '../../components/layout/ListViewContainer';
import FeatureGuard from '../../routes/FeatureGuard';
const { Search } = Input;
const { Title, Text } = Typography;
const { Option } = Select;

type Feature = {
  index: number;
  parentId?: string;
  children?: Feature[];
} & FeatureViewModel;

const ViewFeatures: React.FC = () => {
  const { t } = useTranslation();

  const [features, setFeatures] = useState<Feature[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  const [currentPage, setCurrentPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(20);
  const [total, setTotal] = useState<number>(0);
  const [searchKey, setSearchKey] = useState<string>("");
  const [expandedRowKeys, setExpandedRowKeys] = useState<string[]>([]);

  // Add missing state variables needed for filter params

  const buildFilterParams = (): FilterParams => {
    const filters: Record<string, string> = {};
    if (searchKey) filters['featureName'] = searchKey;
    return {
      pageNumber: currentPage,
      pageSize: pageSize,
      filters: filters
    };
  };



  const fetchFeatures = useCallback(async () => {
    setLoading(true);
    try {
      const filterParams = buildFilterParams();

      const result = await featureService.getFeatures(filterParams);

      if (result.success) {
        const processedData = processHierarchicalData(result.objectList);
        const indexedFeatures = addIndexToData(processedData, currentPage, pageSize);
        setFeatures(indexedFeatures as Array<Feature>);
        setTotal(result.totalCount);
      } else {
        message.error(t('viewFeatures.fetchFailed'));
      }
    }
    catch (error) {
      console.error("Error fetching features:", error);
      message.error(t('viewFeatures.fetchFailed'));
    }
    finally {
      setLoading(false);
    }
  }, [searchKey, currentPage, pageSize, t]);

  useEffect(() => {
    fetchFeatures();
  }, [fetchFeatures]);

  const processHierarchicalData = (data: FeatureViewModel[]): FeatureViewModel[] => {
    const map = new Map();
    const roots: FeatureViewModel[] = [];

    data.forEach(item => {
      map.set(item.id, { ...item, children: [] });
    });

    data.forEach(item => {
      const processedItem = map.get(item.id);

      if (item.parentId && map.has(item.parentId)) {
        const parent = map.get(item.parentId);
        parent.children.push(processedItem);
      } else {
        roots.push(processedItem);
      }
    });
    return roots;
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


  const handleDelete = async (id: string) => {
    console.log('Delete feature with guid:', id);
    setLoading(true);
    try {
      await AxiosClient.delete(`/Feature/${id}`);

      // After successful deletion, refresh the data
      fetchFeatures();
      message.success(t('viewFeatures.deleteSuccess'));
    } catch (error) {
      console.error('Failed to delete feature:', error);
      message.error(t('viewFeatures.deleteFailed'));
      setLoading(false);
    }
  };

  // Simplified table change handler that only manages pagination
  const handleTableChange = (pagination: any) => {
    setCurrentPage(pagination.current);
    setPageSize(pagination.pageSize);
  };

  const handleExpandAll = () => {
    if (expandedRowKeys.length > 0) {
      // Collapse all
      setExpandedRowKeys([]);
    } else {
      // Expand all
      const allKeys = getAllKeys(features);
      setExpandedRowKeys(allKeys);
    }
  };

  const getAllKeys = (data: Feature[]): string[] => {
    let keys: string[] = [];
    data.forEach(item => {
      keys.push(item.id);
      if (item.children && item.children.length > 0) {
        keys = [...keys, ...getAllKeys(item.children)];
      }
    });
    return keys;
  };

  const resetFilters = () => {
    setSearchKey("");
    setCurrentPage(1);
  };

  const getHttpMethodName = (methodNumber: number): string => {
    const methods = Object.values(HttpMethodType); // ["GET", "POST", "PUT", "DELETE", "PATCH"]
    return methods[methodNumber] || "UNKNOWN";
  };


  const columns: TableProps<Feature>['columns'] = [
    {
      title: t('viewFeatures.no'),
      dataIndex: 'index',
      key: 'index',
      align: 'center',
    },
    {
      title: t('viewFeatures.featureName'),
      dataIndex: 'featureName',
      key: 'featureName',
      render: (text: string, record: Feature) => (
        <Space>
          <div style={{
            marginLeft: record.parentId ? '20px' : '0px',
            display: 'flex',
            alignItems: 'center'
          }}>
            <Text strong>{text}</Text>
            {record.children && record.children.length > 0 && (
              <Tag color="blue" style={{ marginLeft: 8 }}>
                <BranchesOutlined /> {t('viewFeatures.parent')}
              </Tag>
            )}
            {record.parentId && (
              <Tag color="green" style={{ marginLeft: 8 }}>
                <BranchesOutlined /> {t('viewFeatures.child')}
              </Tag>
            )}
          </div>
        </Space>
      ),
    },
    {
      title: t('viewFeatures.url'),
      dataIndex: 'url',
      key: 'url',
      ellipsis: true,
      render: (text: string) => (
        <Tooltip title={text}>
          <Space>
            <LinkOutlined />
            <Text ellipsis style={{ maxWidth: 250 }}>{text}</Text>
          </Space>
        </Tooltip>
      ),
    },
    {
      title: t('viewFeatures.description'),
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      render: (text: string) => (
        <Tooltip title={text}>
          <Space>
            <InfoCircleOutlined />
            <Text ellipsis style={{ maxWidth: 250 }}>{text || t('viewFeatures.noDescription')}</Text>
          </Space>
        </Tooltip>
      ),
    },
    {
      title: t('viewFeatures.httpMethod'),
      dataIndex: 'httpMethod',
      key: 'httpMethod',
      align: 'center',
      filters: [
        { text: 'GET', value: '0' },
        { text: 'POST', value: '1' },
        { text: 'PUT', value: '2' },
        { text: 'DELETE', value: '3' },
        { text: 'PATCH', value: '4' }
      ],

      render: (methodNumber: number) => { return getHttpMethodText(getHttpMethodName(methodNumber)); },
    },
    {
      title: t('actions'),
      key: 'actions',
      align: 'center',
      render: (record: Feature) => (
        <Space size="small">
          <FeatureGuard requiredFeature='/api/Feature/{id}_PUT'>
            <Tooltip title={t('viewFeatures.editFeature')}>
              <UpdateFeatureModal id={record.id} onUpdateSuccess={fetchFeatures} />
            </Tooltip>
          </FeatureGuard>
          <FeatureGuard requiredFeature='/api/Feature/{id}_DELETE'>
          <Tooltip title={t('viewFeatures.deleteFeature')}>
            <Button
              danger
              type="primary"
              size="small"
              onClick={() =>
                confirmDelete(
                  () => handleDelete(record.id),
                  t("confirmDelete.title", { action: t("delete"), itemName: t("confirmDelete.thisFeature") }),
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
          <Title level={2} style={{ margin: 0 }}>{t('viewFeatures.featureManagement')}</Title>
        </Col>
        <Col>
          <Space>
            
            <CreateFeatureModal onCreateSuccess={fetchFeatures} />
          </Space>
        </Col>
      </Row>

      {/* Filter section styled like the ticket list */}
      <Space style={{ marginBottom: '16px' }} wrap>
        {/* <Search
          placeholder={t('searchByName')}
          allowClear
          onChange={(e) => debouncedSearch(e.target.value)}
        /> */}
        <Button
              icon={<BranchesOutlined />}
              onClick={handleExpandAll}
            >
              {expandedRowKeys.length > 0 ? t('viewFeatures.collapseAll') : t('viewFeatures.expandAll')}
            </Button>
        {/* <Select
            placeholder="{t('status.placeholder')}"
            allowClear
            className='w-50'
            value={selectedStatus}
            onChange={handleStatusChange}
          >
            <Option value={FeatureStatus.Active}>{t('viewFeatures.active')}</Option>
            <Option value={FeatureStatus.Inactive}>{t('viewFeatures.inactive')}</Option>
          </Select> */}
        {/* <Button
          onClick={resetFilters}
          style={{ backgroundColor: '#52c41a', color: 'white' }}
        >
          {t('viewFeatures.clearFilters')}
        </Button> */}
      </Space>

      <TableWithSkeleton
        columns={columns}
        rowKey={"id"}
        dataSource={features}
        loading={loading}
        pagination={{
          current: currentPage,
          pageSize: pageSize,
          total: total,
          showTotal: (total, range) => t('viewFeatures.pagination', { range0: range[0], range1: range[1], total: total }),
          showSizeChanger: true,
          pageSizeOptions: ['10', '20', '50', '100', '200'],
          style: { marginTop: 16 }
        }}
        onChange={handleTableChange}
        bordered
        scroll={{ x: 'max-content' }}
        size="middle"
        expandable={{
          expandedRowKeys: expandedRowKeys,
          onExpandedRowsChange: (expandedKeys) => setExpandedRowKeys(expandedKeys as string[]),
        }}
      />
    </ListViewContainer>
  );
};

export default ViewFeatures;