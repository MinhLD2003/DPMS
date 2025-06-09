import React, { useState, useEffect } from 'react';
import { Button, Col, Input, Row, Space, Table, Tooltip, Typography, message } from 'antd';
import AxiosClient from '../../../configs/axiosConfig';
import { useLocation, useNavigate } from 'react-router-dom';
import { InfoCircleOutlined } from '@ant-design/icons';
import moment from 'moment';
import FileDownloadButton from '../../../components/common/ExcelDownloadButton';
import TableWithSkeleton from '../../../components/forms/TableWithSkeleton';
import { useTranslation } from 'react-i18next';
import ListViewContainer from '../../../components/layout/ListViewContainer';
import FeatureGuard from '../../../routes/FeatureGuard';
import { getUserFeatures } from '../../../utils/jwtDecodeUtils';
import { SorterResult } from 'antd/lib/table/interface';
const { Search } = Input;
const { Title } = Typography;

interface FicForm {
  id: string;
  formId: string;
  systemId: string;
  name: string;
  externalSystemName: string;
  systemName: string;
  createdAt: string;
  createdBy: string;
}

interface TableParams {
  pagination: {
    current: number;
    pageSize: number;
    total: number;
  };
  sortField?: string;
  sortOrder?: string;
}

const FICSubmissionList: React.FC = () => {
  const { t } = useTranslation();
  const [fics, setFics] = useState<FicForm[]>([]);
  const [filteredFics, setFilteredFics] = useState<FicForm[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [tableParams, setTableParams] = useState<TableParams>({
    pagination: {
      current: 1,
      pageSize: 10,
      total: 0,
    },
  });
  
  const navigate = useNavigate();
  const userFeatures = getUserFeatures();

  useEffect(() => {
    fetchFICs();
  }, [t]);

  useEffect(() => {
    // Apply filtering when searchTerm changes
    filterFics();
  }, [searchTerm, fics]);

  const fetchFICs = async () => {
    setLoading(true);
    try {
      const response = await AxiosClient.get(`/Form/get-submissions`);
      // Assuming response.data is an object with a 'data' property that is an array
      const data = response.data.map((fic: any, index: number) => ({
        id: fic.id,
        systemName: fic.systemName || "...",
        formId: fic.formId,
        systemId: fic.systemId,
        externalSystemName: fic.externalSystemName || "...",
        name: fic.name,
        createdAt: moment(fic.createdAt).format('DD/MM/YYYY HH:mm:ss'),
        createdBy: fic.createdBy || "...",
      }));
      setFics(data);
      setFilteredFics(data);
      setTableParams({
        ...tableParams,
        pagination: {
          ...tableParams.pagination,
          total: data.length,
        },
      });
      message.success(t('ficSubmissionList.fetchSuccess'));
    } catch (error) {
      console.error('Failed to fetch FIC Forms:', error);
      message.error(t('ficSubmissionList.fetchFailed'));
    }
    setLoading(false);
  };

  const filterFics = () => {
    if (!searchTerm.trim()) {
      setFilteredFics(fics);
    } else {
      const filtered = fics.filter(
        fic => 
          fic.externalSystemName.toLowerCase().includes(searchTerm.toLowerCase()) ||
          fic.systemName.toLowerCase().includes(searchTerm.toLowerCase())
      );
      setFilteredFics(filtered);
    }
    
    // Reset pagination to first page when filtering
    setTableParams({
      ...tableParams,
      pagination: {
        ...tableParams.pagination,
        current: 1,
        total: filteredFics.length,
      },
    });
  };

  const handleSearch = (value: string) => {
    setSearchTerm(value);
  };

  const handleTableChange = (
    pagination: any,
    filters: any,
    sorter: SorterResult<FicForm> | SorterResult<FicForm>[]
  ) => {
    const singleSorter = Array.isArray(sorter) ? sorter[0] : sorter;
    
    setTableParams({
      pagination,
      sortField: singleSorter.field as string,
      sortOrder: singleSorter.order as string,
    });
  };

  const goToDetail = async (id: string) => {
    setLoading(true);
    navigate(`/dashboard/forms/submissions/${id}`);
    setLoading(false);
  };

  const columns = [
    {
      title: t('ficSubmissionList.no'),
      key: 'index',
      render: (_text: any, _record: any, index: number) => 
        (tableParams.pagination.current - 1) * tableParams.pagination.pageSize + index + 1,
      width: 60,
      align: 'center' as const,
    },
    {
      title: t('ficSubmissionList.systemName'),
      dataIndex: 'externalSystemName',
      key: 'systemName',
      sorter: (a: FicForm, b: FicForm) => a.externalSystemName.localeCompare(b.externalSystemName),
    },
    {
      title: t('ficSubmissionList.formName'),
      dataIndex: 'name',
      key: 'name',
      sorter: (a: FicForm, b: FicForm) => a.name.localeCompare(b.name),
      render: (text: string, record: FicForm) => (
        <span
          style={{ color: '#1890ff', cursor: 'pointer' }}
          onClick={() => {
            if (userFeatures.includes('/api/Form/submission/{id}_GET'))
              goToDetail(record.id);
          }}
        >
          {text}
        </span>
      ),
    },
    {
      title: t('ficSubmissionList.submissionDate'),
      dataIndex: 'createdAt',
      key: 'createdAt',
      sorter: (a: FicForm, b: FicForm) => moment(a.createdAt, 'DD/MM/YYYY HH:mm:ss').valueOf() - 
                                        moment(b.createdAt, 'DD/MM/YYYY HH:mm:ss').valueOf(),
    },
    {
      title: t('ficSubmissionList.submittedBy'),
      dataIndex: 'createdBy',
      key: 'createdBy',
      sorter: (a: FicForm, b: FicForm) => a.createdBy.localeCompare(b.createdBy),
    },
    {
      title: t('actions'),
      key: 'actions',
      render: (record: FicForm) => (
        <Space size="middle">
          <FeatureGuard requiredFeature='/api/Form/submission/{id}_GET'>
            <Tooltip title={
              <div>
                <p>{t('ficSubmissionList.submissionId')}: {record.id}</p>
              </div>
            }
            >
              <Button
                type="primary"
                onClick={() => goToDetail(record.id)}
                icon={<InfoCircleOutlined />}
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

  return (
    <ListViewContainer>
      <Row justify="space-between" align="middle" style={{ marginBottom: 24 }}>
        <Col>
          <Title level={2} style={{ margin: 0 }}>{t('ficSubmissionList.pageTitle')}</Title>
        </Col>
      </Row>
      <Space style={{ margin: '19px 0' }}>
        <Search
          placeholder={t('systemName')}
          allowClear
          onSearch={handleSearch}
          onChange={(e) => handleSearch(e.target.value)}
          style={{ width: 300 }}
        />
      </Space>
      <TableWithSkeleton 
        columns={columns}
        rowKey={'id'}
        dataSource={filteredFics}
        loading={loading}
        bordered
        scroll={{ x: 'max-content' }}
        size="middle"
        pagination={{
          ...tableParams.pagination,
          showSizeChanger: true,
          showTotal: (total, range) => `${range[0]}-${range[1]} of ${total} items`,
          pageSizeOptions: ['10', '20', '50', '100'],
        }}
        onChange={handleTableChange}
      />
    </ListViewContainer>
  );
};

export default FICSubmissionList;