import React, { useState, useEffect } from 'react';
import {
    Button, Space, Table, message, Tooltip, Typography, Input, DatePicker,
    Select, Modal, Card, Row, Col, Badge, Statistic, Collapse, Tag, Divider
} from 'antd';
import { useNavigate } from 'react-router-dom';
import dayjs, { Dayjs } from 'dayjs';
import moment from 'moment';
import {
    PlusOutlined, EditOutlined, DeleteOutlined, InfoCircleOutlined, ExportOutlined, FilterOutlined, SearchOutlined, ReloadOutlined, BarChartOutlined, FileTextOutlined, PlayCircleOutlined, EyeOutlined,
    CheckCircleOutlined,
    CloseCircleOutlined, ClockCircleOutlined
} from '@ant-design/icons';
import AxiosClient from '../../configs/axiosConfig';
import ListViewContainer from '../../components/layout/ListViewContainer';
import FeatureGuard from '../../routes/FeatureGuard';
import { useTranslation } from 'react-i18next'; // Import useTranslation
const { Search } = Input;
const { Option } = Select;
const { RangePicker } = DatePicker;
const { Title, Text } = Typography;
const { Panel } = Collapse;

interface DPIA {
    key: string;
    id: string;
    dueDate: string;
    systemId: string;
    systemName: string;
    title: string;
    description: string;
    type: string;
    status: string;
    createdAt: string;
    createdBy: string;
}

interface FilterParams {
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDirection: string;
    filters: Record<string, string>;
}

// Interface for system mapping
interface SystemInfo {
    id: string;
    name: string;
}

const StatusOptions = ["Draft", "Started", "Approved", "Rejected"];
const DPIATypeOptions = ["NewOrUpdatedSystem", "PeriodicReview", "DataBreach"];

const DPIAList: React.FC = () => {
    const { t } = useTranslation(); // Initialize useTranslation

    const [searchTerm, setSearchTerm] = useState<string>('');
    const [selectedStatus, setSelectedStatus] = useState<string | null>(null);
    const [selectedType, setSelectedType] = useState<string | null>(null);
    const [dateRange, setDateRange] = useState<[Dayjs | null, Dayjs | null] | null>(null);
    const [dpias, setDPIAs] = useState<DPIA[]>([]);
    const [loading, setLoading] = useState<boolean>(false);
    const [isDeleteModalVisible, setIsDeleteModalVisible] = useState<boolean>(false);
    const [dpiaToDelete, setDPIAToDelete] = useState<string | null>(null);

    const navigate = useNavigate();

    const [currentPage, setCurrentPage] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(10);
    const [total, setTotal] = useState<number>(0);
    const [sortField, setSortField] = useState<string>('createdAt');
    const [sortOrder, setSortOrder] = useState<string>('desc');

    // Updated to store system info objects with both id and name
    const [systems, setSystems] = useState<SystemInfo[]>([]);
    const [selectedSystemId, setSelectedSystemId] = useState<string | null>(null);

    // Stats for dashboard
    const [stats, setStats] = useState({
        total: 0,
        draft: 0,
        started: 0,
        approved: 0,
        rejected: 0
    });

    // Build filter params based on current state
    const buildFilterParams = (): FilterParams => {
        const filters: Record<string, string> = {};
        if (searchTerm) {
            filters['Title'] = searchTerm;
        }

        if (selectedStatus) {
            filters['Status'] = selectedStatus;
        }

        if (selectedType) {
            filters['Type'] = selectedType;
        }

        // Updated to use SystemId for filtering
        if (selectedSystemId) {
            filters['ExternalSystemId'] = selectedSystemId;
        }

        if (dateRange && dateRange[0]) {
            filters['DueDate_from'] = dateRange[0].format('YYYY-MM-DD');
        }

        if (dateRange && dateRange[1]) {
            filters['DueDate_to'] = dateRange[1].format('YYYY-MM-DD');
        }
        return {
            pageNumber: currentPage,
            pageSize: pageSize,
            sortBy: sortField,
            sortDirection: sortOrder,
            filters: filters
        };
    };

    const formatDate = (dateString: string): string => {
        return dayjs(dateString).format('DD/MM/YYYY');
    };

    const fetchDPIAs = async () => {
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

            const response = await AxiosClient.get(`/DPIA?${queryParams.toString()}`);
            const formattedData = response.data.data.map((dpia: any, index: number) => ({
                key: index.toString(),
                id: dpia.id,
                title: dpia.title,
                dueDate: dpia.dueDate,
                systemId: dpia.systemId,
                systemName: dpia.systemName,
                description: dpia.description,
                type: dpia.type,
                status: dpia.status,
                pic: dpia.pic,
                createdAt: moment(dpia.createdAt).format('DD/MM/YYYY'),
                createdBy: dpia.createdBy
            }));
            extractSystemInfo(formattedData);
            setDPIAs(formattedData);
            setTotal(response.data.totalRecords);
            setStats({
                total: response.data.totalRecords,
                draft: formattedData.filter((d: { status: string; }) => d.status === 'Draft').length,
                started: formattedData.filter((d: { status: string; }) => d.status === 'Started').length,
                approved: formattedData.filter((d: { status: string; }) => d.status === 'Approved').length,
                rejected: formattedData.filter((d: { status: string; }) => d.status === 'Rejected').length
            });

        } catch (error) {
            message.error(t('failed_to_fetch_dpias'));
        }
        setLoading(false);
    };

    useEffect(() => {
        fetchDPIAs();
    }, [currentPage, pageSize, sortField, sortOrder, searchTerm, selectedStatus, selectedType, selectedSystemId, dateRange, t]);

    const handleEdit = (id: string) => {
        navigate(`/dashboard/dpias/detail/${id}`);
    };

    // Updated to extract both system ID and name
    const extractSystemInfo = (dpiaData: DPIA[]) => {
        const systemMap = new Map<string, string>();

        // Create a map of system IDs to system names
        dpiaData.forEach(dpia => {
            if (dpia.systemId && dpia.systemName) {
                systemMap.set(dpia.systemId, dpia.systemName);
            }
        });

        // Convert map to array of SystemInfo objects
        const systemInfoArray: SystemInfo[] = Array.from(systemMap).map(([id, name]) => ({
            id,
            name
        }));

        // Sort by system name for better user experience
        systemInfoArray.sort((a, b) => a.name.localeCompare(b.name));

        setSystems(systemInfoArray);
    };

    const showDeleteConfirm = (id: string) => {
        const dpiaToBeDeleted = dpias.find(dpia => dpia.id === id);

        if (dpiaToBeDeleted && dpiaToBeDeleted.status === 'Draft') {
            setDPIAToDelete(id);
            setIsDeleteModalVisible(true);
        } else {
            message.error(t('only_draft_dpias_can_be_deleted'));
        }
    };

    const handleCancelDelete = () => {
        setIsDeleteModalVisible(false);
        setDPIAToDelete(null);
    };

    const handleDelete = async () => {
        if (!dpiaToDelete) return;

        // Find the DPIA to delete
        const dpiaToBeDeleted = dpias.find(dpia => dpia.id === dpiaToDelete);
        // Double-check status before proceeding
        if (!dpiaToBeDeleted || dpiaToBeDeleted.status !== 'Draft') {
            message.error(t('only_draft_dpias_can_be_deleted'));
            setIsDeleteModalVisible(false);
            setDPIAToDelete(null);
            return;
        }

        setLoading(true);
        try {
            await AxiosClient.delete(`/dpia/${dpiaToDelete}`);
            message.success(t('dpia_deleted_successfully'));
            fetchDPIAs();
        } catch (error) {
            console.error('Failed to delete DPIA:', error);
            message.error(t('failed_to_delete_dpia'));
        }
        setLoading(false);
        setIsDeleteModalVisible(false);
        setDPIAToDelete(null);
    };

    const handleSearch = (value: string) => {
        setSearchTerm(value);
        setCurrentPage(1); // Reset to first page when searching
    };

    const handleStatusChange = (value: string | null) => {
        setSelectedStatus(value);
        setCurrentPage(1); // Reset to first page when filtering
    };

    const handleTypeChange = (value: string | null) => {
        setSelectedType(value);
        setCurrentPage(1); // Reset to first page when filtering
    };

    // Updated to use systemId instead of systemName
    const handleSystemChange = (value: string | null) => {
        setSelectedSystemId(value);
        setCurrentPage(1);
    };

    const handleDateRangeChange = (dates: [Dayjs | null, Dayjs | null] | null) => {
        setDateRange(dates);
        setCurrentPage(1);
    };

    const handleAddNewDPIA = () => {
        navigate("/dashboard/dpias/create");
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
        setSelectedType(null);
        setDateRange(null);
        setSelectedSystemId(null);
        setCurrentPage(1);
        setSortField('createdAt');
        setSortOrder('desc');
    };

    const getStatusColor = (status: string) => {
        const statusConfig: Record<string, any> = {
            'Draft': { color: '#1890ff', icon: <ClockCircleOutlined /> },
            'Started': { color: '#faad14', icon: <PlayCircleOutlined /> },
            'Approved': { color: '#52c41a', icon: <CheckCircleOutlined /> },
            'Rejected': { color: '#f5222d', icon: <CloseCircleOutlined /> },
            'Done': { color: '#52c41a', icon: <FileTextOutlined /> },
            'Close': { color: '#f5222d', icon: <DeleteOutlined /> }
        };
        return statusConfig[status] || { color: 'default', icon: null };
    };

    const columns = [
        {
            title: t('no'),
            dataIndex: 'key',
            key: 'no',
            render: (_: any, __: any, index: number) => (currentPage - 1) * pageSize + index + 1,
            width: 55
        },
        {
            title: t('title'),
            dataIndex: 'title',
            key: 'title',
            sorter: true,
            render: (text: string, record: DPIA) => (
                <div>
                    <a
                        onClick={() => handleEdit(record.id)}
                        style={{ fontWeight: 500 }}
                    >
                        {text}
                    </a>

                </div>
            ),
            width: 280
        },
        {
            title: t('systemName'),
            dataIndex: 'systemName',
            key: 'systemName',
            render : (systemName : string) => {
                return  <Tag>{systemName}</Tag>
            },
            width: 180
        },
        {
            title: t('type'),
            dataIndex: 'type',
            key: 'type',
            render: (type: string) => {
                const colorMap: Record<string, string> = {
                    'New external system': 'green',
                    'Periodically DPIA': 'orange',
                    'Data Breach DPIA': 'volcano'
                };
                return <Tag color={colorMap[type] || 'blue'}>{type}</Tag>;
            },
            width: 180
        },
        {
            title: t('due_date'),
            dataIndex: 'dueDate',
            key: 'dueDate',
            sorter: true,
            render: (dueDate: string) => (
                <div style={{ whiteSpace: 'nowrap', fontWeight: 500 }}>
                    {formatDate(dueDate)}
                </div>
            ),
            width: 120
        },

        {
            title: t('status'),
            dataIndex: 'status',
            key: 'status',
            sorter: true,
            render: (status: string) => {
                const { color, icon } = getStatusColor(status);
                return (
                    <Tag color={color} icon={icon}>
                        {status}
                    </Tag>
                );
            },
            width: 120
        },

        {
            title: t('created'),
            dataIndex: 'createdAt',
            key: 'createdAt',
            sorter: true,
            render: (date: string, record: DPIA) => (
                <div>
                    <div>{date}</div>
                    <div style={{ fontSize: '12px', color: '#8c8c8c' }}>
                        by {record.createdBy}
                    </div>
                </div>
            ),
            width: 120
        },
        {
            title: t('actions'),
            key: 'actions',
            render: (_: any, record: DPIA) => (
                <Space size="small">
                    <Tooltip title={t('edit_dpia')}>
                        <Button
                            type="primary"
                            size="small"
                            icon={<EditOutlined />}
                            onClick={() => handleEdit(record.id)}
                            ghost
                        />
                    </Tooltip>
                    {record.status === 'Draft' ? (
                        <FeatureGuard requiredFeature='/api/DPIA/{id}/{id}_DELETE'>
                            <Tooltip title={t('delete_dpia')}>
                                <Button
                                    type="primary"
                                    danger
                                    size="small"
                                    icon={<DeleteOutlined />}
                                    onClick={() => showDeleteConfirm(record.id)}
                                    ghost
                                />
                            </Tooltip>
                        </FeatureGuard>
                    ) : (
                        <Tooltip title={t('only_draft_dpias_can_be_deleted')}>
                            <Button
                                type="primary"
                                danger
                                size="small"
                                icon={<DeleteOutlined />}
                                disabled
                                ghost
                            />
                        </Tooltip>
                    )}
                </Space>
            ),
            width: 100,
            fixed: 'right' as const
        }
    ];

    return (
        <ListViewContainer>
            <div style={{ marginBottom: '20px' }}>
                <Row gutter={[16, 16]} align="middle">
                    <Col>
                        <Title level={4} style={{ margin: 0 }}>
                            <InfoCircleOutlined style={{ marginRight: '8px' }} />
                            {t('dpia_management')}
                        </Title>
                    </Col>
                    <Col flex="auto">
                        <div style={{ float: 'right' }}>
                            <Space>
                                <FeatureGuard requiredFeature='/api/DPIA_POST'>
                                    <Button
                                        type="primary"
                                        icon={<PlusOutlined />}
                                        onClick={handleAddNewDPIA}
                                    >
                                        {t('add_new_dpia')}
                                    </Button>
                                </FeatureGuard>
                            </Space>
                        </div>
                    </Col>
                </Row>


                <Row gutter={16} className="stats-row" style={{ marginBottom: '20px' }}>
                    <Col span={4}>
                        <Statistic
                            title={t('total_dpias')}
                            value={stats.total}
                            prefix={<BarChartOutlined />}
                            valueStyle={{ color: '#1890ff' }}
                        />
                    </Col>
                    <Col span={4}>
                        <Statistic
                            title={t('draft')}
                            value={stats.draft}
                            prefix={<ClockCircleOutlined />}
                            valueStyle={{ color: '#1890ff' }}
                        />
                    </Col>
                    <Col span={4}>
                        <Statistic
                            title={t('started')}
                            value={stats.started}
                            prefix={<PlayCircleOutlined />}
                            valueStyle={{ color: '#faad14' }}
                        />
                    </Col>
                    <Col span={4}>
                        <Statistic
                            title={t('approved')}
                            value={stats.approved}
                            prefix={<CheckCircleOutlined />}
                            valueStyle={{ color: '#52c41a' }}
                        />
                    </Col>
                    <Col span={4}>
                        <Statistic
                            title={t('rejected')}
                            value={stats.rejected}
                            prefix={<CloseCircleOutlined />}
                            valueStyle={{ color: '#f5222d' }}
                        />
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
                    header={<span><FilterOutlined />{t('advanced_filters')}</span>}
                    key="1"
                >
                    <Row gutter={[16, 16]}>
                        <Col xs={24} sm={12} md={8} lg={6}>
                            <Search
                                placeholder={t('search_by_title')}
                                allowClear
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                onSearch={handleSearch}
                            />
                        </Col>
                        <Col xs={24} sm={12} md={8} lg={6}>
                            <Select
                                style={{ width: '100%' }}
                                placeholder={t('select_dpia_type')}
                                allowClear
                                value={selectedType}
                                onChange={handleTypeChange}
                            >
                                {DPIATypeOptions.map(type => (
                                    <Option key={type} value={type}>{type}</Option>
                                ))}
                            </Select>
                        </Col>
                        <Col xs={24} sm={12} md={8} lg={6}>
                            <Select
                                style={{ width: '100%' }}
                                placeholder={t('filter_by_system_name')}
                                allowClear
                                value={selectedSystemId}
                                onChange={handleSystemChange}
                            >
                                {systems.map(system => (
                                    <Option key={system.id} value={system.id}>{system.name}</Option>
                                ))}
                            </Select>
                        </Col>

                        <Col xs={24} sm={12} md={8} lg={6}>
                            <Select
                                style={{ width: '100%' }}
                                placeholder={t('select_status')}
                                allowClear
                                value={selectedStatus}
                                onChange={handleStatusChange}
                            >
                                {StatusOptions.map(status => {
                                    const { color, icon } = getStatusColor(status);
                                    return (
                                        <Option key={status} value={status}>
                                            <Tag color={color} icon={icon}>{status}</Tag>
                                        </Option>
                                    );
                                })}
                            </Select>
                        </Col>
                        <Col xs={24} sm={12} md={8} lg={6}>
                            <Input.Group compact style={{ width: '100%', display: 'flex' }}>
                                <span style={{
                                    padding: '4px 11px',
                                    background: '#fafafa',
                                    border: '1px solid #d9d9d9',
                                    borderRight: 0,
                                    borderRadius: '5px',
                                    whiteSpace: 'nowrap'
                                }}>
                                    {"Due Date "}
                                </span>
                                <RangePicker
                                    style={{ flex: 1 }}
                                    value={dateRange}
                                    onChange={handleDateRangeChange}
                                    placeholder={[t('from'), t('to')]}
                                    allowClear
                                />
                            </Input.Group>
                        </Col>

                        <Col xs={24} sm={12} md={8} lg={6}>
                            <Button
                                onClick={handleClearFilters}
                                icon={<ReloadOutlined />}
                            >
                                {t('reset_filters')}
                            </Button>
                        </Col>
                    </Row>
                </Panel>
            </Collapse>
            <Table
                columns={columns}
                dataSource={dpias}
                loading={loading}
                pagination={{
                    current: currentPage,
                    pageSize: pageSize,
                    total: total,
                    showSizeChanger: true,
                    pageSizeOptions: ['10', '25', '40'],
                    showTotal: (total) => t('total_items', { total: total })
                }}
                onChange={handleTableChange}
                rowKey="id"
                size="middle"
            />
            <Modal
                title={
                    <div>
                        <DeleteOutlined style={{ color: '#ff4d4f', marginRight: '8px' }} />
                        {t('confirm_delete')}
                    </div>
                }
                open={isDeleteModalVisible}
                onOk={handleDelete}
                onCancel={handleCancelDelete}
                okText={t('yes_delete')}
                cancelText={t('cancel')}
                okButtonProps={{ danger: true }}
            >
                <p style={{ width: 350 }}>{t('are_you_sure_delete_dpia')}</p>
                <p style={{ width: 350 }}><strong>{t('note_only_draft_status')}</strong></p>
                <p style={{}}>{t('action_cannot_be_undone')}</p>
            </Modal>

        </ListViewContainer>
    );
};

export default DPIAList;
