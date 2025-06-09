import React, { useCallback, useEffect, useState } from 'react';
import { Table, Tag, Button, message, Input, Space, Card, Typography, Empty, Flex, Spin, Skeleton, Select } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import moment from 'moment';
import { useNavigate, useParams } from 'react-router-dom';
import { FilterParams } from '../../../common/FilterParams';
import { debounce, set } from 'lodash';
import { DownloadOutlined, EyeOutlined, PlusOutlined } from '@ant-design/icons';
import { addIndexToData } from '../../../utils/indexHelper';
import { ConsentDataViewModel } from '../../Consent/models/ConsentLogViewModel';
import consentService from '../../Consent/apis/ConsentAPIs';
import TableWithSkeleton from '../../../components/forms/TableWithSkeleton';
import ConsentReadOnlyModal from '../../Consent/modules/ConsentReadOnlyModal';
import { useTranslation } from 'react-i18next';
import FileDownloadButton from '../../../components/common/ExcelDownloadButton';
import ImportExcelButton from '../../../components/common/ExcelImportButton';
import FeatureGuard from '../../../routes/FeatureGuard';
import { getConsentStatus } from '../../../utils/TextColorUtils';
const { Search } = Input;
const { Title, Text } = Typography;

const ConsentTab: React.FC = () => {
    const { t } = useTranslation();
    const { id } = useParams<{ id: string }>();
    const [consents, setConsents] = useState<ConsentDataViewModel[]>([]);
    const [loading, setLoading] = useState<boolean>(false);
    const navigate = useNavigate();
    const [currentPage, setCurrentPage] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(10);
    const [total, setTotal] = useState<number>(0);
    const [searchKey, setSearchKey] = useState<string>("");
    const [initialLoading, setInitialLoading] = useState<boolean>(true);

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
            if (!id) {
                return;
            }
            const result = await consentService.getSystemConsentLogs(id, filterParams);
            if (result.success) {
                const indexedConsentLogs = addIndexToData(result.objectList, currentPage, pageSize);
                setConsents(indexedConsentLogs);
                setTotal(result.totalCount);
            } else {
                setConsents([]);
                setTotal(0);
            }
        } catch (error) {
            message.error(t('consentTab.fetchConsentsFailed'));
            setConsents([]);
        } finally {
            setLoading(false);
            setInitialLoading(false);

        }
    }, [searchKey, pageSize, currentPage, selectedStatus, id, t]);

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


    const handleRedirectToCreate = () => {
        navigate(`consent-config`);
    };


    if (initialLoading) {
        return (
            <Card className="shadow-sm">
                <Skeleton active paragraph={{ rows: 6 }} />
            </Card>
        );
    }

    const columns: ColumnsType<ConsentDataViewModel> = [
        {
            title: t('consentTab.index'),
            key: 'index',
            render: (_text, _record, index) => index + 1,
        },
        {
            title: t('consentTab.email'),
            dataIndex: 'email',
            key: 'email',
        },
        {
            title: t('consentTab.consentMethod'),
            dataIndex: 'consentMethod',
            key: 'consentMethod',
            render: (method) => (method === 0 ? t('consentTab.webForm') : t('consentTab.emailMethod')),
        },
        {
            title: t('consentTab.consentIp'),
            dataIndex: 'consentIp',
            key: 'consentIp',
        },
        {
            title: t('consentTab.userAgent'),
            dataIndex: 'consentUserAgent',
            key: 'consentUserAgent',
        },
        {
            title: t('consentTab.externalSystem'),
            dataIndex: 'externalSystemName',
            key: 'externalSystemName',
        },
        {
            title: t('consentTab.consentDate'),
            dataIndex: 'consentDate',
            key: 'consentDate',
            render: (date: string) => date && moment(date).format("DD/MM/YYYY HH:mm:ss")
        },
        {
            title: t('consentTab.status'),
            dataIndex: 'isWithdrawn',
            key: 'isWithdrawn',
            render: (isWithdrawn: boolean) => (
                <Tag color={isWithdrawn ? 'red' : 'green'}>
                    {isWithdrawn ? t('consentTab.withdrawn') : t('consentTab.active')}
                </Tag>
            ),
        },
        {
            title: t('consentTab.withdrawnDate'),
            dataIndex: 'withdrawnDate',
            key: 'withdrawnDate',
            render: (date: string) =>
                date && moment(date).format("DD/MM/YYYY HH:mm:ss") === "01/01/0001 00:00:00"
                    ? <Tag color='blue'>{t('notWithdrawn')}</Tag>
                    : date
                        ? moment(date).format("YYYY-MM-DD HH:mm:ss")
                        : "-",
        },
    ];


    return (
        <Card className="shadow-sm">
            <Flex justify="space-between" align="center" style={{ marginBottom: '16px' }}>
                <Title level={4} style={{ margin: 0 }}>{t('consentTab.title')}</Title>
                <Flex gap="small">
                    <FeatureGuard requiredFeature='/api/ExternalSystem/bulk-add-purposes_POST'>
                        <Button
                            type="primary"
                            icon={<PlusOutlined />}
                            onClick={handleRedirectToCreate}
                        >
                            {t('consentTab.addConsentPurposes')}
                        </Button>
                    </FeatureGuard>
                    <FeatureGuard requiredFeature='/api/Purpose_GET'>
                        <ConsentReadOnlyModal />
                    </FeatureGuard>
                </Flex>
            </Flex>

            <Card className="filter-card" style={{ marginBottom: '16px', background: '#f9fafb' }}>
                <Flex justify="space-between" align="center">
                    <Flex wrap="wrap" gap="small">

                        <Search
                            placeholder={t('consentTab.searchByEmail')}
                            allowClear
                            onChange={(e) => debouncedSearch(e.target.value)}
                            style={{ width: 200 }}
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
                    </Flex>
                    <Flex wrap="wrap" gap="small">

                        {/* <FeatureGuard requiredFeature='/api/Consent/import-consent_POST'>
                            <ImportExcelButton
                                apiUrl="/Consent/import-consent"
                                buttonText={t('importData')}
                                onImportSuccess={fetchConsents}
                            />
                        </FeatureGuard> */}

                        <FeatureGuard requiredFeature='/api/Consent/export-logs_GET'>
                            <FileDownloadButton
                                apiPath={"/Consent/export-logs?systemId=" + id}
                                filename={`Consent_Logs_${new Date().toISOString().slice(0, 10)}.xlsx`}
                                buttonText={t('consentTab.exportToExcel')}
                            />
                        </FeatureGuard>
                        {/* <FeatureGuard requiredFeature='/api/Consent/download-template/{id}_GET'>
                            <FileDownloadButton
                                apiPath={`/Consent/download-template/${id}`}
                                filename={`Consent_Template_${new Date().toISOString().slice(0, 10)}.xlsx`}
                                buttonText={t('consentTab.downloadTemplate')}
                            />
                        </FeatureGuard> */}
                    </Flex>
                </Flex>
            </Card>

            <TableWithSkeleton
                rowKey="consentDate"
                columns={columns}
                dataSource={consents}
                loading={loading}
                pagination={{
                    current: currentPage,
                    pageSize: pageSize,
                    total: total,
                    showTotal: (total, range) => t('consentTab.pagination', { range0: range[0], range1: range[1], total: total }),
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
                            <p>{t('consentTab.consentPurposes')}:</p>
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
                locale={{ emptyText: t('consentTab.noData') }}
            />
        </Card>
    );
};

export default ConsentTab;