import React, { useState, useEffect, useCallback } from 'react';
import { Button, message, Input, Tooltip, Typography, Empty, Spin, Badge, Dropdown } from 'antd';
import { DeleteOutlined, InfoCircleOutlined, PlusOutlined, SortAscendingOutlined, SortDescendingOutlined, MoreOutlined } from '@ant-design/icons';
import AxiosClient from '../../configs/axiosConfig';
import { useNavigate } from 'react-router-dom';
import { SystemModel } from './models/SystemModel';
import confirmDelete from '../../components/common/popup-modals/ConfirmDeleteModal';
import moment from 'moment';
import { useTranslation } from 'react-i18next';
import AddSystemModal from './AddSystem';
import ListViewContainer from '../../components/layout/ListViewContainer';
import { getExternalSystemStatusText } from '../../utils/TextColorUtils';
import FeatureGuard from '../../routes/FeatureGuard';
import Paragraph from 'antd/es/typography/Paragraph';

const { Title, Text } = Typography;
const { Search } = Input;

const SystemList: React.FC = () => {
    const { t } = useTranslation();
    const [systems, setSystems] = useState<SystemModel[]>([]);
    const [filteredSystems, setFilteredSystems] = useState<SystemModel[]>([]);
    const [loading, setLoading] = useState<boolean>(false);
    const [searchTerm, setSearchTerm] = useState<string>('');
    const navigate = useNavigate();
    const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');
    // Fetch systems from API
    const fetchSystems = useCallback(async () => {
        setLoading(true);
        try {
            const response = await AxiosClient.get('/ExternalSystem');
            const data: SystemModel[] = response.data.map((system: any) => ({
                id: system.id,
                name: system.name,
                description: system.description,
                domain: system.domain||"https://nullorblankdomain.com",
                status: system.status,
                createdAt: system.createdAt,
                createdBy: system.createBy,
            }));

            // Sort systems by date (newest first as default)
            const sortedSystems = sortSystemsByDate(data, 'desc');
            setSystems(sortedSystems);
            setFilteredSystems(sortedSystems);
            message.success(t('systemList.fetchSuccess'));
        } catch (error) {
            message.error(t('systemList.fetchFailed'));
            console.error(error);
        } finally {
            setLoading(false);
        }
    }, [t]);

    useEffect(() => {
        fetchSystems();
    }, [fetchSystems]);

    // Sort systems by date
    const sortSystemsByDate = (systemsToSort: SystemModel[], order: 'asc' | 'desc'): SystemModel[] => {
        return [...systemsToSort].sort((a, b) => {
            const dateA = new Date(a.createdAt).getTime();
            const dateB = new Date(b.createdAt).getTime();
            return order === 'asc' ? dateA - dateB : dateB - dateA;
        });
    };

    // Toggle sort order
    const toggleSortOrder = () => {
        const newOrder = sortOrder === 'asc' ? 'desc' : 'asc';
        setSortOrder(newOrder);
        setFilteredSystems(sortSystemsByDate(filteredSystems, newOrder));
    };

    // Handle search
    const handleSearch = (value: string) => {
        setSearchTerm(value);
        if (!value.trim()) {
            setFilteredSystems(sortSystemsByDate(systems, sortOrder));
        } else {
            const filtered = systems.filter(
                system =>
                    system.name.toLowerCase().includes(value.toLowerCase()) ||
                    system.description.toLowerCase().includes(value.toLowerCase())
            );
            setFilteredSystems(sortSystemsByDate(filtered, sortOrder));
        }
    };

    // Navigate to system detail
    const goToDetail = (id: string) => {
        navigate(`detail/${id}`);
    };

    // Handle system deletion
    const handleDelete = async (id: string) => {
        setLoading(true);
        try {
            await AxiosClient.delete(`/ExternalSystem?systemId=${id}`);
            setSystems(systems.filter(system => system.id !== id));
            setFilteredSystems(filteredSystems.filter(system => system.id !== id));
            message.success(t('systemList.deleteSuccess'));
        } catch (error) {
            message.error(t('systemList.deleteFailed'));
        }
        setLoading(false);
    };

    return (
        <ListViewContainer>
            <div className="flex justify-between items-center mb-8">
                <Title level={2} style={{ margin: 0 }}>{t("systemList.systemManagement")}</Title>
                <div className="flex space-x-4 items-center">
                    <Tooltip title={sortOrder === 'asc' ? t('systemList.sortNewest') : t('systemList.sortOldest')}>
                        <Button
                            icon={sortOrder === 'asc' ? <SortAscendingOutlined /> : <SortDescendingOutlined />}
                            onClick={toggleSortOrder}
                        />
                    </Tooltip>
                    <Search
                        placeholder={t('systemList.searchByName')}
                        allowClear
                        onSearch={handleSearch}
                        onChange={(e) => handleSearch(e.target.value)}
                        style={{ width: 250 }}
                    />
                    <FeatureGuard requiredFeature='/api/ExternalSystem_POST'>
                        <AddSystemModal onCreateSuccess={fetchSystems} />
                    </FeatureGuard>
                </div>
            </div>

            {loading ? (
                <div className="flex justify-center items-center h-64">
                    <Spin size="large" />
                </div>
            ) : filteredSystems.length === 0 ? (
                <Empty
                    description={searchTerm ? t('systemList.noSearchResults') : t('systemList.noSearchResults')}
                    image={Empty.PRESENTED_IMAGE_SIMPLE}
                />
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {filteredSystems.map(system => (
                        <div
                            key={system.id}
                            className="bg-white rounded-lg shadow-md border border-gray-200 hover:shadow-lg transition-shadow duration-300"
                        >
                            <div
                                className="p-5 cursor-pointer"
                                onClick={() => goToDetail(system.id)}
                                style={{ pointerEvents: 'all' }}
                            >
                                <div className="flex justify-between items-start mb-2">
                                    <div className="text-xl font-medium text-blue-600 hover:text-blue-800 truncate max-w-[80%]">
                                        {system.name}
                                    </div>
                                    <div className="flex items-center space-x-1">
                                        <span className="text-sm">{getExternalSystemStatusText(system.status, t)}</span>
                                    </div>
                                </div>
                                <div className="overflow-hidden mb-3">
                                    <Text
                                        className="no-wrap-text"
                                        code
                                        onClick={(e) => {
                                            e.stopPropagation(); // Prevent card click event
                                            if (system.domain) {
                                                // Add https if not present
                                                const url = system.domain.startsWith('http') ? system.domain : `https://${system.domain}`;
                                                window.open(url, '_blank');
                                            }
                                        }}
                                        style={{
                                            cursor: system.domain ? 'pointer' : 'default',
                                            color: system.domain ? '#1890ff' : 'inherit'
                                        }}
                                    >
                                        {system.domain}
                                    </Text>
                                </div>
                                <div className="h-16 overflow-hidden mb-3">
                                    <Paragraph ellipsis={{ rows: 2 }}>{system.description}</Paragraph>
                                </div>

                                <div className="flex justify-between items-center text-sm text-gray-500 mt-4">
                                    <div>
                                        <div>{moment(system.createdAt).format('DD/MM/YYYY')}</div>
                                        <div>{t('systemList.createdBy')} {system.createdBy}</div>
                                    </div>

                                    <div
                                        className="flex space-x-2"
                                        onClick={(e) => e.stopPropagation()} // Prevent card click when clicking buttons
                                    >
                                        <Tooltip title={t('systemList.viewDetails')}>
                                            <Button
                                                type="primary"
                                                icon={<InfoCircleOutlined />}
                                                onClick={() => goToDetail(system.id)}
                                                shape="circle"
                                                size="middle"
                                            />
                                        </Tooltip>
                                        <FeatureGuard requiredFeature='/api/ExternalSystem_DELETE'>
                                            <Tooltip title={t('systemList.removeTooltip')}>
                                                <Button
                                                    danger
                                                    type="primary"
                                                    icon={<DeleteOutlined />}
                                                    shape="circle"
                                                    size="middle"
                                                    onClick={() => confirmDelete(
                                                        () => handleDelete(system.id),
                                                        t("confirmDelete.title", { action: t("delete"), itemName: t('systemList.thisSystem') }),
                                                        t("confirmDelete.content"),
                                                        t("confirmDelete.okText", { action: t("delete") }),
                                                        t("confirmDelete.cancelText")
                                                    )}
                                                />
                                            </Tooltip>
                                        </FeatureGuard>
                                    </div>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {filteredSystems.length > 0 && (
                <div className="text-center text-gray-500 mt-6">
                    {t('systemList.totalSystems', { count: filteredSystems.length })}
                </div>
            )}
        </ListViewContainer>
    );
};

export default SystemList;