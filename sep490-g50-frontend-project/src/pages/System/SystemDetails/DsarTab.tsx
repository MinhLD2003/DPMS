import React, { useState, useEffect, useCallback } from 'react';
import {
    Button, Space, Table, message, Tooltip, Typography, Input, DatePicker,
    Select, Modal, Card, Row, Col, Collapse, Tag, Divider,
    Descriptions, Form, Radio
} from 'antd';
import { useParams } from 'react-router-dom';
import { Dayjs } from 'dayjs';
import moment from 'moment';
import {
    InfoCircleOutlined, FilterOutlined, ReloadOutlined,
    PlayCircleOutlined, EyeOutlined, CheckCircleOutlined, CloseCircleOutlined,
    ClockCircleOutlined, ArrowRightOutlined
} from '@ant-design/icons';
import AxiosClient from '../../../configs/axiosConfig';
import ImportExcelButton from '../../../components/common/ExcelImportButton';
import FileDownloadButton from '../../../components/common/ExcelDownloadButton';
import { DSARStatus, DSARType } from '../../../enum/enum';
import FeatureGuard from '../../../routes/FeatureGuard';
import { useTranslation } from 'react-i18next';
import { debounce } from 'lodash';

const { Search } = Input;
const { Option } = Select;
const { RangePicker } = DatePicker;
const { Title, Text } = Typography;
const { Panel } = Collapse;
const { TextArea } = Input;

// Define enum mappings for DSARType
const DSARTypeDisplay: Record<number, string> = {
    [DSARType.Access]: "Access Request",
    [DSARType.Delete]: "Deletion Request"
};

// Status mappings for display
const DSARStatusDisplay: Record<number, string> = {
    [DSARStatus.Submitted]: "Submitted",
    [DSARStatus.RequiredReponse]: "Required Response",
    [DSARStatus.Completed]: "Completed",
    [DSARStatus.Rejected]: "Rejected"
};

// Using the provided DSAR interface
interface DSAR {
    id?: string; // Adding ID for record identification
    requesterName: string;
    requesterEmail: string;
    phoneNumber: string | null;
    address: string | null;
    description: string;
    type: number; // Changed to number for enum
    status: number; // Changed to number for enum
    requiredResponse: Date | null;
    completedDate: Date | null;
    externalSystemId: string;
    externalSystem?: any; // Optional as it might not be needed in the list view
    // Additional fields for UI functionality
    key?: string;
    createdAt?: string;
    createdBy?: string;
}

interface FilterParams {
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDirection: string;
    filters: Record<string, string>;
}

const DSARList: React.FC = () => {
    const { t } = useTranslation();
    const { id } = useParams<{ id: string }>();
    const [searchTerm, setSearchTerm] = useState<string>('');
    const [selectedStatus, setSelectedStatus] = useState<number | undefined>();
    const [selectedType, setSelectedType] = useState<number | undefined>();
    const [dateRange, setDateRange] = useState<[Dayjs | null, Dayjs | null] | null>(null);
    const [dsars, setDsars] = useState<DSAR[]>([]);
    const [loading, setLoading] = useState<boolean>(false);

    // State variables for detail modal
    const [isDetailModalVisible, setIsDetailModalVisible] = useState<boolean>(false);
    const [selectedDsar, setSelectedDsar] = useState<DSAR | null>(null);

    // New state variables for status change modals
    const [isStatusChangeModalVisible, setIsStatusChangeModalVisible] = useState<boolean>(false);
    const [statusChangeTarget, setStatusChangeTarget] = useState<number | null>(null);
    const [dsarToUpdate, setDsarToUpdate] = useState<DSAR | null>(null);
    const [statusUpdateLoading, setStatusUpdateLoading] = useState<boolean>(false);
    const [form] = Form.useForm();


    const [currentPage, setCurrentPage] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(10);
    const [total, setTotal] = useState<number>(0);
    const [sortField, setSortField] = useState<string>('createdAt');
    const [sortOrder, setSortOrder] = useState<string>('desc');

    // Build filter params based on current state
    const buildFilterParams = (): FilterParams => {
        const filters: Record<string, string> = {};
        if (searchTerm) {
            filters['RequesterName'] = searchTerm;
        }

        if (selectedStatus !== undefined) {
            filters['Status'] = selectedStatus.toString();
        }

        if (selectedType !== undefined) {
            filters['Type'] = selectedType.toString();
        }
        if (dateRange && dateRange[0]) {
            filters['RequiredResponse_from'] = dateRange[0].format('YYYY-MM-DD');
        }

        if (dateRange && dateRange[1]) {
            filters['RequiredResponse_to'] = dateRange[1].format('YYYY-MM-DD');
        }
        return {
            pageNumber: currentPage,
            pageSize: pageSize,
            sortBy: sortField,
            sortDirection: sortOrder,
            filters: filters
        };
    };

    const fetchDsars = useCallback(async () => {
        setLoading(true);
        try {
            const params = buildFilterParams();
            // Convert filters to query params
            const queryParams = new URLSearchParams();
            queryParams.append('pageNumber', params.pageNumber.toString());
            queryParams.append('pageSize', params.pageSize.toString());
            queryParams.append('sortBy', params.sortBy);
            queryParams.append('sortDirection', params.sortDirection);

            Object.keys(params.filters).forEach((key) => {
                queryParams.append(`${key}`, params.filters[key]);
            });

            const response = await AxiosClient.get(`/Dsar/get-list/${id}?${queryParams.toString()}`);

            // Map API response to DSAR interface
            const formattedData = response.data.data.map((dsar: any, index: number) => ({
                key: index.toString(),
                id: dsar.id,
                requesterName: dsar.requesterName,
                requesterEmail: dsar.requesterEmail,
                phoneNumber: dsar.phoneNumber,
                address: dsar.address,
                description: dsar.description,
                type: dsar.type, // Numeric enum value from API
                status: dsar.status, // Numeric enum value from API
                requiredResponse: dsar.requiredResponse ? new Date(dsar.requiredResponse) : null,
                completedDate: dsar.completedDate ? new Date(dsar.completedDate) : null,
                externalSystemId: dsar.externalSystemId,
                externalSystem: dsar.externalSystem,
                createdAt: dsar.createdAt ? moment(dsar.createdAt).format('DD/MM/YYYY') : undefined,
                createdBy: dsar.createdBy
            }));

            setDsars(formattedData);
            setTotal(response.data.totalRecords);
        } catch (error) {
            message.error(t('fetchError'));
        }
        finally {
            setLoading(false);
        }
    }, [currentPage, pageSize, sortField, sortOrder, searchTerm, selectedStatus, selectedType, dateRange, t]);

    // Fetch initial data
    useEffect(() => {
        fetchDsars();
    }, [fetchDsars]);

    // View Detail Modal Functions
    const showDetailModal = (dsar: DSAR) => {
        setSelectedDsar(dsar);
        setIsDetailModalVisible(true);
    };

    const handleDetailModalCancel = () => {
        setIsDetailModalVisible(false);
        setSelectedDsar(null);
    };

    // Status change functions
    const showStatusChangeModal = (dsar: DSAR, targetStatus: number) => {
        setDsarToUpdate(dsar);
        setStatusChangeTarget(targetStatus);
        setIsStatusChangeModalVisible(true);

        if (targetStatus === DSARStatus.Rejected) {
            form.resetFields();
        }
    };

    const handleStatusChangeCancel = () => {
        setIsStatusChangeModalVisible(false);
        setDsarToUpdate(null);
        setStatusChangeTarget(null);
        form.resetFields();
    };

    const handleStatusChangeSubmit = async () => {
        if (!dsarToUpdate || statusChangeTarget === null) return;

        setStatusUpdateLoading(true);

        try {
            let payload: any = {
                id: dsarToUpdate.id,
                status: statusChangeTarget
            };

            if (statusChangeTarget === DSARStatus.Rejected) {
                await form.validateFields();
                const values = form.getFieldsValue();
                payload.rejectionReason = values.rejectionReason;
            }

            if (statusChangeTarget === DSARStatus.Completed) {
                payload.completedDate = new Date().toISOString();
            }

            await AxiosClient.put(`/Dsar/update-status`, payload);
            message.success(t('statusUpdateSuccess', { status: DSARStatusDisplay[statusChangeTarget] }));
            setIsStatusChangeModalVisible(false);
            fetchDsars();
        } catch (error) {
            console.error('Error updating status:', error);
            message.error(t('statusUpdateError'));
        } finally {
            setStatusUpdateLoading(false);
        }
    };

    const debouncedSearch = useCallback(
        debounce((value: string) => {
            setSearchTerm(value);
            setCurrentPage(1); // Reset to first page when searching
        }, 2000),
        []
    );
    useEffect(() => {
        return () => {
            debouncedSearch.cancel();
        };
    }, [debouncedSearch]);
    const handleStatusFilterChange = (value: number) => {
        setSelectedStatus(value);
        setCurrentPage(1); // Reset to first page when filtering
    };
    const handleTypeChange = (value: number) => {
        setSelectedType(value);
        setCurrentPage(1); // Reset to first page when filtering
    };

    const handleDateRangeChange = (dates: [Dayjs | null, Dayjs | null] | null) => {
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
        setSelectedStatus(undefined);
        setSelectedType(undefined);
        setDateRange(null);
        setCurrentPage(1);
        setSortField('createdAt');
        setSortOrder('desc');
    };

    const getStatusColor = (status: number) => {
        const statusConfig: Record<number, any> = {
            [DSARStatus.Submitted]: { color: '#005bb8', icon: <ClockCircleOutlined /> },
            [DSARStatus.RequiredReponse]: { color: '#faad14', icon: <PlayCircleOutlined /> },
            [DSARStatus.Completed]: { color: '#52c41a', icon: <CheckCircleOutlined /> },
            [DSARStatus.Rejected]: { color: '#f5222d', icon: <CloseCircleOutlined /> }
        };
        return statusConfig[status] || { color: 'default', icon: null };
    };

    const getTypeColor = (type: number) => {
        const colorMap: Record<number, string> = {
            [DSARType.Access]: 'green',
            [DSARType.Delete]: 'volcano'
        };
        return colorMap[type] || 'blue';
    };

    // Helper to render status change buttons based on current status
    const renderStatusChangeButtons = (record: DSAR) => {
        const buttons = [];

        // From Submitted directly to Completed or Rejected
        if (record.status === DSARStatus.Submitted) {
            buttons.push(
                <Tooltip title={t('acceptRequest')} key="toCompleted">
                    <Button
                        type="primary"
                        size="small"
                        icon={<CheckCircleOutlined />}
                        onClick={() => showStatusChangeModal(record, DSARStatus.Completed)}
                        style={{ backgroundColor: '#52c41a', borderColor: '#52c41a', marginRight: '4px' }}
                    >
                    </Button>
                </Tooltip>
            );

            buttons.push(
                <Tooltip title={t('rejectRequest')} key="toRejected">
                    <Button
                        danger
                        size="small"
                        icon={<CloseCircleOutlined />}
                        onClick={() => showStatusChangeModal(record, DSARStatus.Rejected)}
                    >
                    </Button>
                </Tooltip>
            );
        }

        // Also show the buttons for Required Response status (in case existing records have this status)
        if (record.status === DSARStatus.RequiredReponse) {
            buttons.push(
                <Tooltip title={t('markCompleted')} key="toCompleted">
                    <Button
                        type="primary"
                        size="small"
                        icon={<CheckCircleOutlined />}
                        onClick={() => showStatusChangeModal(record, DSARStatus.Completed)}
                        style={{ backgroundColor: '#52c41a', borderColor: '#52c41a', marginRight: '4px' }}
                    >
                        {t('complete')}
                    </Button>
                </Tooltip>
            );

            buttons.push(
                <Tooltip title={t('markRejected')} key="toRejected">
                    <Button
                        danger
                        size="small"
                        icon={<CloseCircleOutlined />}
                        onClick={() => showStatusChangeModal(record, DSARStatus.Rejected)}
                    >
                        {t('reject')}
                    </Button>
                </Tooltip>
            );
        }

        return buttons.length > 0 ? <Space size={4}>{buttons}</Space> : null;
    };


    // Updated columns to include View action
    const columns = [
        {
            title: t('no'),
            dataIndex: 'key',
            key: 'no',
            width: 70,
            render: (_: any, __: any, index: number) => (currentPage - 1) * pageSize + index + 1,
            onHeaderCell: () => ({
                style: { backgroundColor: '#005bb8', color: 'white' }
            })

        },
        {
            title: t('requesterName'),
            dataIndex: 'requesterName',
            key: 'requesterName',
            sorter: true,
            onHeaderCell: () => ({
                style: { backgroundColor: '#005bb8', color: 'white' }
            })
        },
        {
            title: t('email'),
            dataIndex: 'requesterEmail',
            key: 'requesterEmail',
            onHeaderCell: () => ({
                style: { backgroundColor: '#005bb8', color: 'white' }
            })
        },
        {
            title: t('requestType'),
            dataIndex: 'type',
            key: 'type',
            render: (type: number) => {
                const color = getTypeColor(type);
                return <Tag color={color}>{DSARTypeDisplay[type] || t('unknownType', { type })}</Tag>;
            },
            onHeaderCell: () => ({
                style: { backgroundColor: '#005bb8', color: 'white' }
            })
        },
        {
            title: t('status'),
            dataIndex: 'status',
            key: 'status',
            sorter: true,
            render: (status: number) => {
                const { color, icon } = getStatusColor(status);
                return (
                    <Tag color={color} icon={icon}>
                        {DSARStatusDisplay[status] || t('unknownStatus', { status })}
                    </Tag>
                );
            },
            onHeaderCell: () => ({
                style: { backgroundColor: '#005bb8', color: 'white' }
            })
        },
        {
            title: t('requiredResponse'),
            dataIndex: 'requiredResponse',
            key: 'requiredResponse',
            sorter: true,
            render: (date: Date | null) =>
                date ? moment(date).format('DD/MM/YYYY') : '-',
            width: 140,
            onHeaderCell: () => ({
                style: { backgroundColor: '#005bb8', color: 'white' }
            })
        },

        {
            title: t('actions'),
            key: 'actions',
            render: (_: any, record: DSAR) => (
                <Space size="small" wrap>
                    <Tooltip title={t('viewDetails')}>
                        <Button
                            type="default"
                            size="small"
                            icon={<EyeOutlined />}
                            onClick={() => showDetailModal(record)}
                        />
                    </Tooltip>
                    <FeatureGuard requiredFeature='/api/Dsar/update-status_PUT'>

                        {renderStatusChangeButtons(record)}
                    </FeatureGuard>
                </Space>
            ),
            fixed: 'right' as const,
            onHeaderCell: () => ({
                style: { backgroundColor: '#005bb8', color: 'white' }
            })
        }
    ];

    return (
        <div style={{ padding: '10px 0px 0px 0px' }}>
            <Card style={{ border: '1px solid #d9d9d9', borderRadius: '8px' }}>
                <div style={{ marginBottom: '20px' }}>
                    <Row gutter={[16, 16]} align="middle">
                        <Col>
                            <Title level={4} style={{ margin: 0 }}>
                                {t('dsarPageTitle')}
                            </Title>
                        </Col>
                        <Col>
                            <Space>
                                <FeatureGuard requiredFeature='/api/Dsar/import_POST'>
                                    <ImportExcelButton
                                        onImportSuccess={fetchDsars}
                                        apiUrl="/Dsar/import"
                                        buttonText={t('importData')}
                                    />
                                </FeatureGuard>
                                <FeatureGuard requiredFeature='/api/Dsar/download-import-template_GET'>
                                    <FileDownloadButton
                                        apiPath={`/Dsar/download-import-template`}
                                        filename={`DSAR_Template_${new Date().toISOString().slice(0, 10)}.xlsx`}
                                        buttonText={t('dsarTemplate')}
                                    />
                                </FeatureGuard>
                            </Space>
                        </Col>
                    </Row>
                </div>

                <Divider style={{ margin: '0 0 16px 0' }} />

                <Collapse
                    defaultActiveKey={['1']}
                    style={{ marginBottom: '16px' }}
                    ghost

                >
                    <Panel
                        header={<span><FilterOutlined /> {t('advancedFilters')}</span>}
                        key="1"
                    >
                        <Row gutter={[16, 16]}>
                            <Col xs={24} sm={12} md={6} lg={4}>
                                <Search
                                    placeholder={t('searchByName')}
                                    allowClear
                                    onChange={(e) => debouncedSearch(e.target.value)}
                                />
                            </Col>
                            <Col xs={24} sm={12} md={6} lg={4}>
                                <Select
                                    style={{ width: '100%' }}
                                    placeholder={t('selectDsarType')}
                                    allowClear
                                    value={selectedType}
                                    onChange={handleTypeChange}
                                >
                                    {Object.entries(DSARType)
                                        .filter(([key]) => !isNaN(Number(key))) // Filter numeric keys only
                                        .map(([key, value]) => (
                                            <Option key={key} value={Number(key)}>
                                                <Tag color={getTypeColor(Number(key))}>{DSARTypeDisplay[Number(key)]}</Tag>
                                            </Option>
                                        ))}
                                </Select>
                            </Col>
                            <Col xs={24} sm={12} md={6} lg={4}>
                                <Select
                                    style={{ width: '100%' }}
                                    placeholder={t('selectStatus')}
                                    allowClear
                                    value={selectedStatus}
                                    onChange={handleStatusFilterChange}
                                >
                                    {Object.entries(DSARStatus)
                                        .filter(([key]) => !isNaN(Number(key))) // Filter numeric keys only
                                        .map(([key, value]) => {
                                            const { color, icon } = getStatusColor(Number(key));
                                            return (
                                                <Option key={key} value={Number(key)}>
                                                    <Tag color={color} icon={icon}>{DSARStatusDisplay[Number(key)]}</Tag>
                                                </Option>
                                            );
                                        })}
                                </Select>
                            </Col>
                            <Col xs={24} sm={12} md={6} lg={4}>
                                <RangePicker
                                    style={{ width: '100%' }}
                                    value={dateRange}
                                    onChange={handleDateRangeChange}
                                    placeholder={[t('startDate'), t('endDate')]}
                                    allowClear
                                />
                            </Col>
                            <Col xs={24} sm={12} md={6} lg={4}>
                                <Button
                                    onClick={handleClearFilters}
                                    icon={<ReloadOutlined />}
                                >
                                    {t('resetFilters')}
                                </Button>
                            </Col>
                        </Row>
                    </Panel>
                </Collapse>
                <Table
                    columns={columns}
                    dataSource={dsars}
                    loading={loading}
                    pagination={{
                        current: currentPage,
                        pageSize: pageSize,
                        total: total,
                        showSizeChanger: true,
                        pageSizeOptions: ['10', '25', '40'],
                        showTotal: (total) => t('totalItems', { total })
                    }}
                    onChange={handleTableChange}
                    rowKey="id"
                    size="middle"
                    scroll={{ x: 1100 }}
                />
            </Card>

            {/* Detail View Modal */}
            <Modal
                title={
                    <div style={{ display: 'flex', alignItems: 'center' }}>
                        <InfoCircleOutlined style={{ color: '#005bb8', marginRight: '8px', fontSize: '18px' }} />
                        <span>{t('dsarDetails')}</span>
                    </div>
                }
                open={isDetailModalVisible}
                onCancel={handleDetailModalCancel}
                width={700}
                footer={[
                    <Button key="close" onClick={handleDetailModalCancel}>
                        {t('close')}
                    </Button>,
                ]}
            >
                {selectedDsar && (
                    <Descriptions bordered column={2} size="small">
                        <Descriptions.Item label={t('requesterName')} span={2}>
                            {selectedDsar.requesterName}
                        </Descriptions.Item>
                        <Descriptions.Item label={t('email')} span={2}>
                            {selectedDsar.requesterEmail}
                        </Descriptions.Item>
                        <Descriptions.Item label={t('phoneNumber')}>
                            {selectedDsar.phoneNumber || t('notAvailable')}
                        </Descriptions.Item>
                        <Descriptions.Item label={t('status')}>
                            <Tag color={getStatusColor(selectedDsar.status).color}>
                                {DSARStatusDisplay[selectedDsar.status] || t('unknownStatus', { status: selectedDsar.status })}
                            </Tag>
                        </Descriptions.Item>
                        <Descriptions.Item label={t('requestType')}>
                            <Tag color={getTypeColor(selectedDsar.type)}>
                                {DSARTypeDisplay[selectedDsar.type] || t('unknownType', { type: selectedDsar.type })}
                            </Tag>
                        </Descriptions.Item>
                        <Descriptions.Item label={t('requiredResponse')}>
                            {selectedDsar.requiredResponse
                                ? moment(selectedDsar.requiredResponse).format('DD/MM/YYYY')
                                : t('notAvailable')}
                        </Descriptions.Item>
                        <Descriptions.Item label={t('address')} span={2}>
                            {selectedDsar.address || t('notAvailable')}
                        </Descriptions.Item>
                        <Descriptions.Item label={t('externalSystemId')}>
                            {selectedDsar.externalSystemId}
                        </Descriptions.Item>
                        <Descriptions.Item label={t('completedDate')}>
                            {selectedDsar.completedDate
                                ? moment(selectedDsar.completedDate).format('DD/MM/YYYY')
                                : t('notCompleted')}
                        </Descriptions.Item>
                        <Descriptions.Item label={t('description')} span={2}>
                            <div style={{ whiteSpace: 'pre-wrap' }}>
                                {selectedDsar.description}
                            </div>
                        </Descriptions.Item>
                    </Descriptions>
                )}
            </Modal>

            {/* Status Change Modal */}
         <Modal
    title={
        <div style={{ display: 'flex', alignItems: 'center' }}>
            {statusChangeTarget !== null && (
                <>
                    {getStatusColor(statusChangeTarget).icon}
                    <span style={{ marginLeft: '8px' }}>
                        {t('changeStatusTo', { status: DSARStatusDisplay[statusChangeTarget] })}
                    </span>
                </>
            )}
        </div>
    }
    open={isStatusChangeModalVisible}
    onCancel={handleStatusChangeCancel}
    onOk={handleStatusChangeSubmit}
    confirmLoading={statusUpdateLoading}
    okButtonProps={{
        style: statusChangeTarget !== null
            ? { backgroundColor: getStatusColor(statusChangeTarget).color, borderColor: getStatusColor(statusChangeTarget).color }
            : {}
    }}
    okText={statusChangeTarget !== null ? t('confirmStatus', { status: DSARStatusDisplay[statusChangeTarget] }) : t('confirm')}
>
    {dsarToUpdate && (
        <div>
            <div style={{
                marginBottom: '20px',
                padding: '16px',
                backgroundColor: '#f5f5f5',
                borderRadius: '4px',
                borderLeft: statusChangeTarget !== null ? `4px solid ${getStatusColor(statusChangeTarget).color}` : '4px solid #d9d9d9'
            }}>
                <Text>
                    {t('statusChangeConfirmation', {
                        name: dsarToUpdate.requesterName,
                        fromStatus: DSARStatusDisplay[dsarToUpdate.status],
                        toStatus: statusChangeTarget !== null ? DSARStatusDisplay[statusChangeTarget] : ''
                    })}
                </Text>
            </div>

            {statusChangeTarget === DSARStatus.Rejected && (
                <Form form={form} layout="vertical">
                </Form>
            )}

            {statusChangeTarget === DSARStatus.Completed && (
                <div style={{
                    marginBottom: '16px',
                    backgroundColor: '#f6ffed',
                    padding: '12px',
                    borderRadius: '4px',
                    border: '1px solid #b7eb8f'
                }}>
                    <Text>{t('completionDateNote')}</Text>
                </div>
            )}
        </div>
    )}
</Modal>


        </div>
    );
};

export default DSARList;