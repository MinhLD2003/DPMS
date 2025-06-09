import React, { useCallback, useEffect, useState } from 'react';
import { Tag, Button, message, Input, Space, Card, Typography, Empty, Flex, Spin, Skeleton, Row, Col, Table, Select } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import moment from 'moment';
import { ConsentDataViewModel } from '../models/ConsentLogViewModel';
import { FilterParams } from '../../../common/FilterParams';
import consentService from '../apis/ConsentAPIs';
import { debounce } from 'lodash';
import { addIndexToData } from '../../../utils/indexHelper';
import FileDownloadButton from '../../../components/common/ExcelDownloadButton';
import { useTranslation } from 'react-i18next';
import ListViewContainer from '../../../components/layout/ListViewContainer';
import FeatureGuard from '../../../routes/FeatureGuard';
import { getConsentStatus } from '../../../utils/TextColorUtils';
const { Search } = Input;
const { Title } = Typography;

interface System {
  id: string;
  name: string;
}

const ConsentTable: React.FC = () => {
  const { t } = useTranslation();
  const [consents, setConsents] = useState<ConsentDataViewModel[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [total, setTotal] = useState<number>(0);
  const [searchKey, setSearchKey] = useState<string>("");
  // const [hasConsents, setHasConsents] = useState<boolean | null>(null);
  const [selectedStatus, setSelectedStatus] = useState<boolean>(false);
  const buildFilterParams = (): FilterParams => {
    const filters: Record<string, string> = {};
    if (searchKey) filters['email'] = searchKey;
    if (selectedStatus !== undefined) filters['isWithdrawn'] = selectedStatus.toString();
    return { pageNumber: currentPage, pageSize, filters };
  };
  const fetchConsents = useCallback(async () => {
    setLoading(true);
    try {
      const filterParams = buildFilterParams();
      const result = await consentService.getConsentLogs(filterParams);
      if (result.success) {
        const indexedConsentLogs = addIndexToData(result.objectList, currentPage, pageSize);
        setConsents(indexedConsentLogs);
        setTotal(result.totalCount);
        //setHasConsents(indexedConsentLogs.length > 0);
      } else {
        //setHasConsents(false);
        setConsents([]);
        setTotal(0);
      }
    } catch (error) {
      message.error(t('consentTable.fetchFailed'));
      //setHasConsents(false);
      setConsents([]);
    } finally {
      setLoading(false);

    }
  }, [searchKey, pageSize, currentPage, selectedStatus, t]);

  useEffect(() => {
    fetchConsents();
  }, [fetchConsents]);

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

  const handleTableChange = (pagination: any) => {
    setCurrentPage(pagination.current);
    setPageSize(pagination.pageSize);
  };

  const columns: ColumnsType<ConsentDataViewModel> = [
    {
      title: t('consentTable.index'),
      key: 'index',
      render: (_text, _record, index) => index + 1,
    },
    {
      title: t('consentTable.email'),
      dataIndex: 'email',
      key: 'email',
    },
    {
      title: t('consentTable.consentMethod'),
      dataIndex: 'consentMethod',
      key: 'consentMethod',
      render: (method) => (method === 0 ? t('consentTable.webForm') : t('consentTable.emailMethod')),
    },
    {
      title: t('consentTable.consentIp'),
      dataIndex: 'consentIp',
      key: 'consentIp',
    },
    {
      title: t('consentTable.userAgent'),
      dataIndex: 'consentUserAgent',
      key: 'consentUserAgent',
    },
    {
      title: t('consentTable.externalSystem'),
      dataIndex: 'externalSystemName',
      key: 'externalSystemName',
    },
    {
      title: t('consentTab.consentDate'),
      dataIndex: 'consentDate',
      key: 'consentDate',
      render: (date: string) =>
        date && moment(date).format("DD/MM/YYYY HH:mm:ss")
    },
    {
      title: t('consentTable.status'),
      dataIndex: 'isWithdrawn',
      key: 'isWithdrawn',
      render: (isWithdrawn: boolean) => (
        <Tag color={isWithdrawn ? 'red' : 'green'}>
          {isWithdrawn ? t('withdrawn') : t('active')}
        </Tag>
      ),
    },
    {
      title: t('consentTable.withdrawnDate'),
      dataIndex: 'withdrawnDate',
      key: 'withdrawnDate',
      render: (date: string) =>
        date && moment(date).format("DD/MM/YYYY HH:mm:ss") === "01/01/0001 00:00:00"
          ? <Tag color='blue'>{t('notWithdrawn')}</Tag>
          : date
            ? moment(date).format("DD/MM/YYYY HH:mm:ss")
            : "-",
    },
  ];



  return (
    <ListViewContainer>
      <Row justify="space-between" align="middle" style={{ marginBottom: 24 }}>
        <Col>
          <Title level={2} style={{ margin: 0 }}>{t('consentTable.title')}</Title>
        </Col>

      </Row>
      <Space style={{ marginBottom: '16px' }} wrap>
        <Search
          placeholder={t('consentTable.searchByEmail')}
          allowClear
          onChange={(e) => debouncedSearch(e.target.value)}
        />
        <Select
          style={{ width: 120 }}
          placeholder={t('consentTable.status')}
          allowClear
          value={selectedStatus}
          onChange={(value) => {
            setSelectedStatus(value);
            setCurrentPage(1);
          }}
        >
          <Select.Option value={false}>{getConsentStatus("Active", t)}</Select.Option>
          <Select.Option value={true}>{getConsentStatus("Withdrawn", t)}</Select.Option>
        </Select>
        <FeatureGuard requiredFeature='/api/Consent/export-logs_GET'>
          <FileDownloadButton
            apiPath="/Consent/export-logs"
            filename={`Consent_Logs_${new Date().toISOString().slice(0, 10)}.xlsx`}
            buttonText={t('consentTable.exportLogs')}
          />
        </FeatureGuard>
      </Space>
      <Spin spinning={loading} tip={t('processing')}>

        <Table
          rowKey="consentDate"
          columns={columns}
          dataSource={consents}
          loading={loading}
          pagination={{
            current: currentPage,
            pageSize: pageSize,
            total: total,
            showTotal: (total, range) => t('consentTable.pagination', { range0: range[0], range1: range[1], total: total }),
            showSizeChanger: true,
            pageSizeOptions: ['10', '20', '50'],
            style: { marginTop: 16 }
          }}
          onChange={handleTableChange}
          bordered
          size="middle"
          expandable={{
            expandedRowRender: (record) => (
              <div style={{ margin: 0 }}>
                <p>{t('consentTable.consentPurposes')}:</p>
                <Space wrap>
                  {record.consentPurpose.map((purpose, idx) => (
                    <Tag key={idx} color={purpose.status ? 'green' : 'red'}>
                      {purpose.name}
                    </Tag>
                  ))}
                </Space>
              </div>
            ),
          }}
        />
      </Spin>
    </ListViewContainer>
  );
};

export default ConsentTable;