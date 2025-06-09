import React, { useState, useEffect } from 'react';
import { Button, Space, Table, message, Tooltip, Typography, Input, DatePicker, Dropdown, Select, Modal, Form, Divider, Row, Col, Card, Spin, Alert } from 'antd';
import AxiosClient from '../../../configs/axiosConfig';
import { useNavigate } from 'react-router-dom';
import moment from 'moment';
import { PlusOutlined, CheckCircleOutlined, CloseCircleOutlined, ConsoleSqlOutlined } from '@ant-design/icons';
import { EditOutlined, DeleteOutlined, InfoCircleOutlined, ExclamationCircleOutlined } from '@ant-design/icons';
import API_ENDPOINTS from '../../../configs/APIEndPoint';
import { useTranslation } from 'react-i18next';
import ListViewContainer from '../../../components/layout/ListViewContainer';
import FeatureGuard from '../../../routes/FeatureGuard';
import { getPolicyStatus } from '../../../utils/TextColorUtils';
const { Search, TextArea } = Input;
const { Option } = Select;
const { Title } = Typography;
interface Purpose {
    id: string;
    name: string;
    description: string;
    status: string;
    purposeType?: string;
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

const purposeStatusList: string[] = ['Draft', 'Active', 'Inactive'];

const PurposeList: React.FC = () => {
    const { t } = useTranslation(); // Get translation function
    const [searchTerm, setSearchTerm] = useState<string>('');
    const [selectedStatus, setSelectedStatus] = useState<string | null>(null);
    const [purposes, setPurposes] = useState<Purpose[]>([]);
    const [loading, setLoading] = useState<boolean>(false);
    const [isDeleteModalVisible, setIsDeleteModalVisible] = useState<boolean>(false);
    const [purposeToDelete, setPurposeToDelete] = useState<string | null>(null);
    const [showInactiveWarning, setShowInactiveWarning] = useState<boolean>(false);
    const [previousStatus, setPreviousStatus] = useState<string>('');

    // Create and Edit Modal States
    const [isCreateModalVisible, setIsCreateModalVisible] = useState<boolean>(false);
    const [isEditModalVisible, setIsEditModalVisible] = useState<boolean>(false);
    const [editingPurpose, setEditingPurpose] = useState<Purpose | null>(null);
    const [createForm] = Form.useForm();
    const [editForm] = Form.useForm();
    const [submitLoading, setSubmitLoading] = useState<boolean>(false);

    const navigate = useNavigate();

    const [currentPage, setCurrentPage] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(10);
    const [total, setTotal] = useState<number>(0);
    const [sortField, setSortField] = useState<string>('createdAt');
    const [sortOrder, setSortOrder] = useState<string>('desc');

    // Build filter params based on current state
    const buildFilterParams = (): FilterParams => {
        const filters: Record<string, string> = {};
        if (searchTerm) {
            filters['Name'] = searchTerm;
        }

        if (selectedStatus) {
            filters['Status'] = selectedStatus;
        }
        return {
            pageNumber: currentPage,
            pageSize: pageSize,
            sortBy: sortField,
            sortDirection: sortOrder,
            filters: filters
        };
    };

    const fetchPurposes = async () => {
        setLoading(true);

        const params = buildFilterParams();

        try {

            // Convert filters to query params
            const queryParams = new URLSearchParams();
            queryParams.append('pageNumber', params.pageNumber.toString()); // Capital 'P'
            queryParams.append('pageSize', params.pageSize.toString()); // Capital 'P'
            queryParams.append('sortBy', params.sortBy); // Capital 'S'
            queryParams.append('sortDirection', params.sortDirection); // Capital 'S'
            Object.keys(params.filters).forEach((key) => {
                queryParams.append(`${key}`, params.filters[key]);
            });

            const response = await AxiosClient.get(`/purpose?${queryParams.toString()}`);
            console.log(response);
            if (response.data) {
                const purpose = response.data.data.map((purpose: any, index: number) => ({
                    key: index.toString(),
                    id: purpose.id,
                    name: purpose.name,
                    description: purpose.description,
                    status: purpose.status,
                    purposeType: purpose.purposeType || 'Other',
                    createdAt: moment(purpose.createdAt).format('DD/MM/YYYY'),
                    lastModifiedAt: moment(purpose.lastModifiedAt).format('DD/MM/YYYY'),
                    createdById: purpose.createdById || 'null',
                    lastModifiedById: purpose.lastModifiedById,
                }));
                setPurposes(purpose);
                setTotal(response.data.totalRecords || purpose.length);
            }
        } catch (error) {
            message.error(t('purposeList.fetchFailed'));
        }
        setLoading(false);
    };

    // Fetch purposes whenever filters change
    useEffect(() => {
        fetchPurposes();
    }, [currentPage, pageSize, sortField, sortOrder, searchTerm, selectedStatus, t]);

    const handleUpdate = (purpose: Purpose) => {
        setEditingPurpose(purpose);
        setPreviousStatus(purpose.status); // Store the previous status
        editForm.setFieldsValue({
            id: purpose.id,
            name: purpose.name,
            description: purpose.description,
            status: purpose.status,
        });
        setIsEditModalVisible(true);
    };

    const showDeleteConfirm = (id: string) => {
        setPurposeToDelete(id);
        setIsDeleteModalVisible(true);
    };

    const handleCancelDelete = () => {
        setIsDeleteModalVisible(false);
        setPurposeToDelete(null);
    };

    // Modified delete handler to work with the modal
    const handleDelete = async () => {
        if (!purposeToDelete) return;
        setLoading(true);
        try {
            await AxiosClient.delete(API_ENDPOINTS.PURPOSES.DELETE(purposeToDelete));
            message.success(t('purposeList.deleteSuccess'));
            fetchPurposes();
        } catch (error) {
            console.error('Failed to delete purpose:', error);
            message.error(t('purposeList.deleteFailed'));
        }
        setLoading(false);
        setIsDeleteModalVisible(false);
        setPurposeToDelete(null);
    };

    const handleSearch = (value: string) => {
        setSearchTerm(value);
        setCurrentPage(1); // Reset to first page when searching
    };

    const handleStatusChange = (value: string) => {
        setSelectedStatus(value || null);
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
        setCurrentPage(1);
        setSortField('createdAt');
        setSortOrder('desc');
    };

    // Create Modal Handlers
    const showCreateModal = () => {
        createForm.resetFields();
        setIsCreateModalVisible(true);
    };

    const handleCreateCancel = () => {
        createForm.resetFields();
        setIsCreateModalVisible(false);
    };

    const handleCreateSubmit = async () => {
        try {
            const values = await createForm.validateFields();
            setSubmitLoading(true);
            const payload = {
                name: values.name,
                description: values.description,
                status: values.status,
            };
            try {
                await AxiosClient.post(`/purpose`, payload
                );
                message.success(t('purposeList.createSuccess'));
                setIsCreateModalVisible(false);
                createForm.resetFields();
                fetchPurposes();
            } catch (error) {
                console.error('Failed to create purpose:', error);
                message.error(t('purposeList.createFailed'));
            }

            setSubmitLoading(false);
        } catch (error) {
            console.error('Validation failed:', error);
        }
    };

    // Edit Modal Handlers
    const handleEditCancel = () => {
        setIsEditModalVisible(false);
        setEditingPurpose(null);
        setShowInactiveWarning(false);
    };

    // Handle status change in edit form
    const handleStatusChangeEdit = (value: string) => {
        if (value === 'Inactive' && previousStatus !== 'Inactive') {
            setShowInactiveWarning(true);
        } else {
            setShowInactiveWarning(false);
        }
    };

    const handleEditSubmit = async () => {
        if (!editingPurpose) return;

        try {
            const values = await editForm.validateFields();
            console.log(values);
            setSubmitLoading(true);
            try {
                await AxiosClient.put(`/purpose/${editingPurpose.id}`, {
                    id: values.id,
                    name: values.name,
                    description: values.description,
                    status: values.status
                });

                message.success(t('purposeList.updateSuccess'));
                setIsEditModalVisible(false);
                setEditingPurpose(null);
                setShowInactiveWarning(false);
                fetchPurposes();
            } catch (error) {
                console.error('Failed to update purpose:', error);
                message.error(t('purposeList.updateFailed'));
            }

            setSubmitLoading(false);
        } catch (error) {
            console.error('Validation failed:', error);
        }
    };

    const columns = [
        {
            title: t('purposeList.purposeTitle'),
            dataIndex: 'name',
            key: 'name',
            sorter: true,
        },
        {
            title: t('purposeList.status'),
            dataIndex: 'status',
            key: 'status',
            sorter: true,
            render: (status: string) => {
                let icon;
                const normalizedStatus = status.toLowerCase();
                if (normalizedStatus === 'active') {
                    icon = <CheckCircleOutlined style={{ color: '#52c41a' }} />;
                } else if (normalizedStatus === 'inactive') {
                    icon = <CloseCircleOutlined style={{ color: '#ff4d4f' }} />;
                } else if (normalizedStatus === 'draft') {
                    icon = <InfoCircleOutlined style={{ color: '#faad14' }} />;
                } else {
                    icon = null;
                }
                return <span>{icon} {getPolicyStatus(status, t)}</span>;
            },
        },
        {
            title: t('purposeList.createdAt'),
            dataIndex: 'createdAt',
            key: 'createdAt',
            sorter: true,
        },
        // {
        //     title: t('purposeList.createdBy'),
        //     dataIndex: 'createdById',
        //     key: 'createdById',
        //     sorter: true,
        // },
        {
            title: t('actions'),
            key: 'actions',
            render: (text: any, record: Purpose) => (
                <Space size="middle">
                    <FeatureGuard requiredFeature='/api/Purpose/{id}_PUT'>
                        <Button
                            type="primary"
                            icon={<EditOutlined />}
                            onClick={() => handleUpdate(record)}
                            size='small'
                        />
                    </FeatureGuard>
                    <FeatureGuard requiredFeature='/api/Purpose/{id}_DELETE'>
                        <Button
                            type="primary"
                            danger
                            icon={<DeleteOutlined />}
                            size='small'
                            onClick={() => showDeleteConfirm(record.id)}
                            // Updated: Allow deletion of Draft and Inactive status purposes
                            disabled={record.status !== "Draft"}
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
                    <Title level={2} style={{ margin: 0 }}>{t("purposeList.purposeManagement")}</Title>
                </Col>
                <Col>
                    <FeatureGuard requiredFeature='/api/Purpose_POST'>
                        <Button type="primary" icon={<PlusOutlined />} onClick={showCreateModal}>
                            {t("purposeList.add")}
                        </Button>
                    </FeatureGuard>
                </Col>
            </Row>

            <Space style={{ marginBottom: 16 }} wrap>
                <Search
                    placeholder={t("searchByName")}
                    allowClear
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    onSearch={handleSearch}
                />

                <Select
                    placeholder={t("status")}
                    allowClear
                    value={selectedStatus}
                    onChange={handleStatusChange}
                    style={{ minWidth: 150 }}
                >
                    {purposeStatusList.map(status => (
                        <Option key={status} value={status}>{getPolicyStatus(status, t)}</Option>
                    ))}
                </Select>

                <Button onClick={handleClearFilters}>{t("clear_filters")}</Button>
            </Space>
            <Spin spinning={loading} tip={t('processing')}>
                <Table
                    columns={columns}
                    rowKey="id"
                    dataSource={purposes}
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
                        pageSizeOptions: ['5', '10', '20'],
                    }}
                    onChange={handleTableChange}
                    bordered
                    scroll={{ x: 'max-content' }}
                    size="middle"
                />
            </Spin>

            {/* Delete Confirmation Modal */}
            <Modal
                title={t('purposeList.deleteConfirmTitle')}
                open={isDeleteModalVisible}
                onOk={handleDelete}
                onCancel={handleCancelDelete}
                okText={t('purposeList.deleteOkText')}
                cancelText={t('purposeList.deleteCancelText')}
                okButtonProps={{ danger: true }}
            >
                <p>{t('purposeList.deleteConfirmContent')}</p>
                <p>{t('purposeList.deleteConfirmWarning')}</p>
            </Modal>

            {/* Create Purpose Modal */}
            <Modal
                title={
                    <div style={{ borderBottom: '1px solid #f0f0f0', padding: '0 0 10px 0' }}>
                        <Typography.Title level={4} style={{ margin: 0 }}>
                            <PlusOutlined /> {t('purposeList.createModalTitle')}
                        </Typography.Title>
                    </div>
                }
                open={isCreateModalVisible}
                onCancel={handleCreateCancel}
                footer={[
                    <Button key="back" onClick={handleCreateCancel}>
                        {t('purposeList.cancelButton')}
                    </Button>,
                    <Button
                        key="submit"
                        type="primary"
                        loading={submitLoading}
                        onClick={handleCreateSubmit}
                    >
                        {t('purposeList.createButton')}
                    </Button>,
                ]}
                width={700}
            >
                <Card bordered={false} className="card-form" style={{ marginTop: '20px' }}>
                    <Form
                        form={createForm}
                        layout="vertical"
                        initialValues={{ status: 'Draft' }}
                    >
                        <Row gutter={16}>
                            <Col span={24}>
                                <Form.Item
                                    name="name"
                                    label={t('purposeList.purposeNameLabel')}
                                    rules={[{ required: true, message: t('purposeList.purposeNameRequired') }]}
                                >
                                    <Input placeholder={t('purposeList.purposeNamePlaceholder')} maxLength={100} />
                                </Form.Item>
                            </Col>
                        </Row>

                        <Row gutter={16}>

                            <Col span={12}>
                                <Form.Item
                                    name="status"
                                    label={t('purposeList.statusLabel')}

                                    rules={[{ required: true, message: t('purposeList.statusRequired') }]}
                                >
                                    <Select placeholder={t('purposeList.statusPlaceholder')} disabled={true}>
                                        {purposeStatusList.map(status => (
                                            <Option key={status} value={status}>{getPolicyStatus(status, t)}</Option>
                                        ))}
                                    </Select>
                                </Form.Item>
                            </Col>
                        </Row>

                        <Form.Item
                            name="description"
                            label={t('purposeList.descriptionLabel')}
                            rules={[{ required: true, message: t('purposeList.descriptionRequired') }]}
                        >
                            <TextArea
                                placeholder={t('purposeList.descriptionPlaceholder')}
                                autoSize={{ minRows: 4, maxRows: 8 }}
                                maxLength={500}
                                showCount
                            />
                        </Form.Item>
                    </Form>
                </Card>
            </Modal>

            {/* Edit Purpose Modal */}
            <Modal
                title={
                    <div style={{ borderBottom: '1px solid #f0f0f0', padding: '0 0 10px 0' }}>
                        <Typography.Title level={4} style={{ margin: 0 }}>
                            <EditOutlined /> {t('purposeList.editModalTitle')}
                        </Typography.Title>
                    </div>
                }
                open={isEditModalVisible}
                onCancel={handleEditCancel}
                footer={[
                    <Button key="back" onClick={handleEditCancel}>
                        {t('purposeList.cancelButton')}
                    </Button>,
                    <Button
                        key="submit"
                        type="primary"
                        loading={submitLoading}
                        onClick={handleEditSubmit}
                    >
                        {t('purposeList.updateButton')}
                    </Button>,
                ]}
                width={700}
            >
                <Card bordered={false} className="card-form" style={{ marginTop: '20px' }}>
                    {editingPurpose && (
                        <Form
                            form={editForm}
                            layout="vertical"
                        >
                            <Form.Item
                                name="id"
                                hidden
                            >
                                <Input />
                            </Form.Item>
                            <Row gutter={16}>

                                <Col span={24}>
                                    <Form.Item
                                        name="name"
                                        label={t('purposeList.purposeNameLabel')}
                                        rules={[{ required: true, message: t('purposeList.purposeNameRequired') }]}
                                    >
                                        <Input placeholder={t('purposeList.purposeNamePlaceholder')} maxLength={100} />
                                    </Form.Item>
                                </Col>
                            </Row>

                            {/* Warning message for Inactive status */}
                            {showInactiveWarning && (
                                <Alert
                                    message={t("warning")}
                                    description={t("deactivatePurposeWarning")}
                                    type="warning"
                                    showIcon
                                    icon={<ExclamationCircleOutlined />}
                                    style={{ marginBottom: 16 }}
                                />
                            )}

                            <Row gutter={16}>
                                <Col span={12}>
                                    <Form.Item
                                        name="status"
                                        label={t('purposeList.statusLabel')}
                                        rules={[{ required: true, message: t('purposeList.statusRequired') }]}
                                    >
                                        <Select
                                            placeholder={t('purposeList.statusPlaceholder')}
                                            onChange={handleStatusChangeEdit}
                                        >
                                            {purposeStatusList.map(status => (
                                                <Option key={status} value={status}>{getPolicyStatus(status, t)}</Option>
                                            ))}
                                        </Select>
                                    </Form.Item>
                                </Col>
                            </Row>

                            <Form.Item
                                name="description"
                                label={t('purposeList.descriptionLabel')}
                                rules={[{ required: true, message: t('purposeList.descriptionRequired') }]}
                            >
                                <TextArea
                                    placeholder={t('purposeList.descriptionPlaceholder')}
                                    autoSize={{ minRows: 4, maxRows: 8 }}
                                    maxLength={500}
                                    showCount
                                />
                            </Form.Item>

                            <Divider style={{ margin: '12px 0' }} />
                            <div style={{ color: '#888' }}>
                                <Typography.Text type="secondary">ID: {editingPurpose.id}</Typography.Text>
                                <br />
                                <Typography.Text type="secondary">Created: {editingPurpose.createdAt}</Typography.Text>
                                <br />
                                <Typography.Text type="secondary">Last Modified: {editingPurpose.lastModifiedAt}</Typography.Text>
                            </div>
                        </Form>
                    )}
                </Card>
            </Modal>
        </ListViewContainer>
    );
};

export default PurposeList;