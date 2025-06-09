import React, { useState, useEffect } from 'react';
import { Button, Tag, Space, Table, message, Typography, Input, DatePicker, Select, Modal, Row, Col } from 'antd';
import AxiosClient from '../../configs/axiosConfig';
import { useNavigate } from 'react-router-dom';
import moment from 'moment';
import { PlusOutlined } from '@ant-design/icons';
import { IssueTicketStatus, TicketType } from '../../enum/enum';
import { EditOutlined, DeleteOutlined } from '@ant-design/icons';
import API_ENDPOINTS from '../../configs/APIEndPoint';
import TableWithSkeleton from '../../components/forms/TableWithSkeleton';
import ListViewContainer from '../../components/layout/ListViewContainer';
import { useTranslation } from 'react-i18next'; // Import useTranslation
import FeatureGuard from '../../routes/FeatureGuard';
const { Search } = Input;
const { Option } = Select;
const { RangePicker } = DatePicker;
const { Title, Text } = Typography;

interface Ticket {
    key: string;
    id: string;
    title: string;
    systemName: string;
    ticketType: string;
    description: string;
    attachment: string[];
    status: string;
    createdAt: string;
    lastModifiedAt: string;
    createdById: string;
    lastModifiedById: string;
}

interface FilterParams {
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDirection: string;
    filters: Record<string, string>;
}

const TicketStatusList: string[] = Object.values(IssueTicketStatus);
const TicketTypes: string[] = Object.values(TicketType);

const TicketList: React.FC = () => {
    const { t } = useTranslation(); // Initialize useTranslation

    const [searchTerm, setSearchTerm] = useState<string>('');
    const [selectedStatus, setSelectedStatus] = useState<string | null>(null);
    const [selectedType, setSelectedType] = useState<string | null>(null);
    const [dateRange, setDateRange] = useState<[moment.Moment | null, moment.Moment | null] | null>(null);
    const [tickets, setTickets] = useState<Ticket[]>([]);
    const [loading, setLoading] = useState<boolean>(false);
    // Add state for delete confirmation modal
    const [isDeleteModalVisible, setIsDeleteModalVisible] = useState<boolean>(false);
    const [ticketToDelete, setTicketToDelete] = useState<string | null>(null);

    const navigate = useNavigate();

    const [currentPage, setCurrentPage] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(10);
    const [total, setTotal] = useState<number>(0);
    const [sortField, setSortField] = useState<string>('createdAt');
    const [sortOrder, setSortOrder] = useState<string>('desc');

    // Build filter params based on current state
    // Modify the buildFilterParams function in your TicketList component
    const buildFilterParams = (): FilterParams => {
        const filters: Record<string, string> = {};
        if (searchTerm) {
            filters['Title'] = searchTerm;
        }

        if (selectedStatus) {
            filters['IssueTicketStatus'] = selectedStatus;
        }

        if (selectedType) {
            filters['TicketType'] = selectedType;
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

    const fetchTickets = async () => {
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

            const response = await AxiosClient.get(`/IssueTicket?${queryParams.toString()}`);
            if (response.data) {
                const ticket = response.data.data.map((ticket: any, index: number) => ({
                    key: index.toString(),
                    id: ticket.id,
                    title: ticket.title,
                    systemName: ticket.externalSystemName,
                    ticketType: ticket.ticketType,
                    description: ticket.description,
                    attachment: ticket.attachment,
                    status: ticket.issueTicketStatus,
                    createdAt: moment(ticket.createdAt).format('DD/MM/YYYY'),
                    lastModifiedAt: moment(ticket.lastModifiedAt).format('DD/MM/YYYY'),
                    createdById: ticket.createdById,
                    lastModifiedById: ticket.lastModifiedById,
                }));
                setTickets(ticket);
                setTotal(response.data.totalRecords);
            }
        } catch (error) {
            message.error(t('failed_to_fetch_tickets'));
        }
        setLoading(false);
    };

    // Fetch tickets whenever filters change
    useEffect(() => {
        fetchTickets();
    }, [currentPage, pageSize, sortField, sortOrder, searchTerm, selectedStatus, selectedType, t]);

    const handleUpdate = (id: string) => {
        console.log('Update ticket with id:', id);
        navigate(`/dashboard/tickets/detail/${id}`);
    };

    // Show delete confirmation modal
    const showDeleteConfirm = (id: string) => {
        setTicketToDelete(id);
        setIsDeleteModalVisible(true);
    };

    // Handle cancel delete
    const handleCancelDelete = () => {
        setIsDeleteModalVisible(false);
        setTicketToDelete(null);
    };

    // Modified delete handler to work with the modal
    const handleDelete = async () => {
        if (!ticketToDelete) return;
        setLoading(true);
        try {
            await AxiosClient.delete(API_ENDPOINTS.TICKETS.DELETE(ticketToDelete));
            message.success(t('ticket_deleted_successfully'));
            fetchTickets();
        } catch (error) {
            console.error('Failed to delete ticket:', error);
            message.error(t('failed_to_delete_ticket'));
        }
        setLoading(false);
        setIsDeleteModalVisible(false);
        setTicketToDelete(null);
    };

    const handleSearch = (value: string) => {
        setSearchTerm(value);
        setCurrentPage(1); // Reset to first page when searching
        fetchTickets();
    };

    const handleStatusChange = (value: string) => {
        setSelectedStatus(value || null);
        setCurrentPage(1);
        fetchTickets();
    };

    const handleTypeChange = (value: string) => {
        setSelectedType(value || null);
        setCurrentPage(1);
        fetchTickets();
    };

    const handleDateRangeChange = (dates: any) => {
        setDateRange(dates);
        setCurrentPage(1);
        fetchTickets();
    };
    const getStatusColor = (status: string): string => {
        const statusLower = typeof status === 'string' ? status.toLowerCase() : '';

        switch (statusLower) {
            case 'pending':
            case 'new':
                return 'warning';
            case 'accept':
            case 'accepted':
            case 'in progress':
                return 'processing';
            case 'reject':
            case 'rejected':
                return 'error';
            case 'done':
            case 'completed':
                return 'success';
            default:
                return 'default';
        }
    };

    // Add this helper function to get ticket type color
    const getTicketTypeColor = (type: string): string => {
        switch (type) {
            case 'Bug':
                return '#f50';
            case 'Feature':
                return '#108ee9';
            case 'Task':
                return '#87d068';
            case 'Enhancement':
                return '#722ed1';
            default:
                return '#2db7f5';
        }
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
        setCurrentPage(1);
        setSortField('createdAt');
        setSortOrder('desc');
        fetchTickets();
    };
    const columns = [
        {
            title: t('ticket_title'),
            dataIndex: 'title',
            key: 'title',
            sorter: true,
        },
        {
            title: t('system_name'),
            dataIndex: 'systemName',
            key: 'systemName',
            sorter: true,
        },
        {
            title: t('ticket_type'),
            dataIndex: 'ticketType',
            key: 'ticketType',
            sorter: true,
            render: (type: string) => (
                <Tag color={getTicketTypeColor(type)} style={{
                    borderRadius: '12px',
                    padding: '0 8px',
                    fontWeight: 500
                }}>
                    {type}
                </Tag>
            ),
        },
        {
            title: t('status'),
            dataIndex: 'status',
            key: 'status',
            sorter: true,
            render: (status: string) => (
                <Tag
                    color={getStatusColor(status)}
                    style={{
                        borderRadius: '12px',
                        padding: '0 8px',
                        border: 'none',
                        fontWeight: 500,
                        textTransform: 'capitalize'
                    }}
                >
                    {status}
                </Tag>
            ),
        },
        {
            title: t('created_at'),
            dataIndex: 'createdAt',
            key: 'createdAt',
            sorter: true,
        },
        {
            title: t('actions'),
            key: 'actions',
            render: (text: any, record: Ticket) => (
                <Space size="middle">
                    <FeatureGuard requiredFeature='/api/IssueTicket/{id}_PUT'>
                        <Button
                            type="primary"
                            size='small'
                            icon={<EditOutlined />}
                            onClick={() => handleUpdate(record.id)}
                            disabled={record.status.toLowerCase() === "done" || record.status.toLowerCase() === "reject"}
                        >
                           
                        </Button>
                    </FeatureGuard>
                    <FeatureGuard requiredFeature='/api/IssueTicket/{id}_DELETE'>
                        <Button
                            type="primary"
                            size='small'
                            danger
                            icon={<DeleteOutlined />}
                            onClick={() => showDeleteConfirm(record.id)}
                            // Only enable the delete button for tickets with "New" status
                            disabled={record.status.toLowerCase() !== "new" && record.status.toLowerCase() !== "pending"}
                        >
                            
                        </Button>
                    </FeatureGuard>
                </Space>
            ),
        },
    ];

    return (
        <ListViewContainer>
            <Row justify="space-between" align="middle" style={{ marginBottom: 24 }}>
                <Col>
                    <Title level={2} style={{ margin: 0 }}>{t('ticket_list')}</Title>
                </Col>
                <Col>
                    <FeatureGuard requiredFeature='/api/IssueTicket_POST'>
                        <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate("new")}>
                            {t('add_new')}
                        </Button>
                    </FeatureGuard>
                </Col>
            </Row>
            <Space style={{ marginBottom: '16px' }} wrap>

                <Search
                    placeholder={t('search_by_title')}
                    allowClear
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    onSearch={handleSearch}
                    style={{ width: 200 }}
                />
                <Select
                    style={{ width: 150 }}
                    placeholder={t('ticket_type')}
                    allowClear
                    value={selectedType}
                    onChange={handleTypeChange}
                >
                    {TicketTypes.map(type => (
                        <Option key={type} value={type}>{type}</Option>
                    ))}
                </Select>
                <Select
                    style={{ width: 150 }}
                    placeholder={t('status')}
                    allowClear
                    value={selectedStatus}
                    onChange={handleStatusChange}
                >
                    {TicketStatusList.map(status => (
                        <Option key={status} value={status}>{status}</Option>
                    ))}
                </Select>
                <RangePicker
                    onChange={handleDateRangeChange}
                    allowClear
                    format="YYYY-MM-DD"
                />
                <Button onClick={handleClearFilters}>{t('clear_filters')}</Button>
            </Space>

            <TableWithSkeleton
                columns={columns}
                dataSource={tickets}
                loading={loading}
                pagination={{
                    current: currentPage,
                    pageSize: pageSize,
                    total: total,
                    showSizeChanger: true,
                    pageSizeOptions: ['10', '25', '40'],
                }}
                onChange={handleTableChange}
                rowKey="id"
            />

            {/* Delete Confirmation Modal */}
            <Modal
                title={t('confirm_delete')}
                open={isDeleteModalVisible}
                onOk={handleDelete}
                onCancel={handleCancelDelete}
                okText={t('yes_delete')}
                cancelText={t('cancel')}
                okButtonProps={{ danger: true }}
            >
                <p style={{ width: 300 }}>{t('are_you_sure_delete_ticket')}</p>
                <p>{t('action_cannot_be_undone')}</p>
            </Modal>
        </ListViewContainer>
    );
};

export default TicketList;