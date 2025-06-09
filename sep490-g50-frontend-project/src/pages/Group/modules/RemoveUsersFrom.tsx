import React, { useState, useEffect, useCallback } from 'react';
import {
    Button,
    DatePicker,
    Input,
    Row,
    Col,
    Table,
    message,
    Card,
    Typography,
    Space,
    Divider,
    Avatar,
    Layout
} from 'antd';
import { UserDeleteOutlined, DeleteOutlined } from '@ant-design/icons';
import AxiosClient from '../../../configs/axiosConfig';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { FilterParams } from '../../../common/FilterParams';
import { debounce } from 'lodash';
import TableWithSkeleton from '../../../components/forms/TableWithSkeleton';
import { useTranslation } from 'react-i18next';
import PageHeader from '../../../components/common/PageHeader';
const { Search } = Input;
const { Title, Text } = Typography;

interface Account {
    id: string;
    fullName: string;
    email: string;
}

// Add this interface and helper function after the Account interface
interface FilteredData {
    filteredAccounts: Account[];
    totalCount: number;
}

const filterAccountsLocally = (
    accounts: Account[],
    searchKey: string,
    currentPage: number,
    pageSize: number
): FilteredData => {
    // Filter accounts based on search key
    const filteredAccounts = accounts.filter(account => {
        const searchLower = searchKey.toLowerCase();
        return (
            account.email.toLowerCase().includes(searchLower) ||
            (account.fullName?.toLowerCase() || '').includes(searchLower)
        );
    });

    // Calculate pagination
    const start = (currentPage - 1) * pageSize;
    const end = start + pageSize;
    const paginatedAccounts = filteredAccounts.slice(start, end);

    return {
        filteredAccounts: paginatedAccounts,
        totalCount: filteredAccounts.length
    };
};

// Modify the component to use local filtering
const RemoveUsersFromGroup: React.FC = () => {
    const { t } = useTranslation();
    const location = useLocation();
    const { groupId, groupName } = useParams<{ groupId: string; groupName: string }>();
    const [accounts, setAccounts] = useState<Account[]>([]);
    const [selectedAccounts, setSelectedAccounts] = useState<{ [key: string]: Account }>({});

    const [loading, setLoading] = useState<boolean>(false);
    const [submitting, setSubmitting] = useState<boolean>(false);

    const [total, setTotal] = useState<number>(0);
    const [currentPage, setCurrentPage] = useState<number>(1);
    const [pageSize, setPageSize] = useState<number>(10);
    const [dateRange, setDateRange] = useState<[any, any]>([null, null]);
    const [searchKey, setSearchKey] = useState<string>("");

    const navigate = useNavigate();
    const [allAccounts, setAllAccounts] = useState<Account[]>([]); // Add this state

    const buildFilterParams = (): FilterParams => {
        const filters: Record<string, string> = {};
        if (searchKey) filters['email'] = searchKey;
        return { pageNumber: currentPage, pageSize, filters };
    };

    // Modify fetchAccounts to store all accounts
    const fetchAccounts = useCallback(async () => {
        setLoading(true);
        try {
            const response = await AxiosClient.get(`/Group/get-users-in-group?groupName=${groupName}`);
            if (response) {
                setAllAccounts(response.data as Account[]); // Store all accounts
                const { filteredAccounts, totalCount } = filterAccountsLocally(
                    response.data as Account[],
                    searchKey,
                    currentPage,
                    pageSize
                );
                setAccounts(filteredAccounts);
                setTotal(totalCount);
            } else {
                message.error(t('accountActions.fetchFailed'));
            }
        } catch (error) {
            message.error(t('accountActions.fetchFailed'));
        } finally {
            setLoading(false);
        }
    }, [groupName]); // Remove dependencies that are now handled locally

    // Add effect for local filtering
    useEffect(() => {
        if (allAccounts.length > 0) {
            const { filteredAccounts, totalCount } = filterAccountsLocally(
                allAccounts,
                searchKey,
                currentPage,
                pageSize
            );
            setAccounts(filteredAccounts);
            setTotal(totalCount);
        }
    }, [searchKey, currentPage, pageSize, allAccounts]);

    useEffect(() => {
        fetchAccounts();
    }, [fetchAccounts]);

    // Modify the debounced search to work locally
    const debouncedSearch = useCallback(
        debounce((value: string) => {
            setSearchKey(value);
            setCurrentPage(1);
            setSelectedAccounts({});
        }, 300), // Reduced debounce time for better responsiveness
        []
    );

    useEffect(() => {
        return () => {
            debouncedSearch.cancel();
        };
    }, [debouncedSearch]);

    const handleSelect = (account: Account) => {
        setSelectedAccounts(prev => {
            const newSelection = { ...prev };
            if (newSelection[account.id]) {
                delete newSelection[account.id]; // Deselect user
            } else {
                newSelection[account.id] = account; // Select user
            }
            return newSelection;
        });
    };

    const handleSubmit = async () => {
        const selectedIds = Object.keys(selectedAccounts);
        if (selectedIds.length === 0) {
            message.warning(t('accountActions.selectUserWarning'));
            return;
        }
    
        setSubmitting(true);
        try {
            await AxiosClient.delete(`/Group/remove-user-from-group?groupId=${groupId}`, {
                data: selectedIds, // Send as an array
            });
            message.success(t('accountActions.removeSuccess'));
            setSelectedAccounts({});
            navigate(-1);
        } catch (error) {
            console.error('Failed to remove users from group:', error);
            message.error(t('accountActions.removeFailed'));
        }
        setSubmitting(false);
    };
    
    

    const handleRemoveSelected = (accountId: string) => {
        setSelectedAccounts(prev => {
            const newSelection = { ...prev };
            delete newSelection[accountId];
            return newSelection;
        });
    };

    // Update handleTableChange to work with local data
    const handleTableChange = (pagination: any) => {
        setCurrentPage(pagination.current);
        setPageSize(pagination.pageSize);
    };

    const handleClearAll = () => {
        setSelectedAccounts({});
    };

    const columns = [
        {
            title: t("user"),
            key: "user",
            render: (_: any, record: Account) => (
                <Space>
                    <Avatar style={{ backgroundColor: "#1890ff" }}>
                        {record.fullName?.charAt(0)?.toUpperCase() || record.email?.charAt(0)?.toUpperCase() || "U"}
                    </Avatar>
                    <Space direction="vertical" size={0}>
                        <Text strong>{record.fullName || t("user")}</Text>
                        <Text type="secondary">{record.email}</Text>
                    </Space>
                </Space>
            ),
        },
        {
            title: t("actions"),
            key: "action",
            width: 120,
            render: (_: any, record: Account) => (
                <Button
                    type={selectedAccounts[record.id] ? "primary" : "default"}
                    danger
                    onClick={() => handleSelect(record)}
                    icon={<DeleteOutlined />}
                >
                    {selectedAccounts[record.id] ? t("selected") : t("select")}
                </Button>
            ),
        },
    ];

    const selectedCount = Object.keys(selectedAccounts).length;

    return (
        <Layout style={{ minHeight: '100vh', backgroundColor: '#f5f7fa' }}>
            <PageHeader
                title={t("remove_users_from_group", { groupName })}
            />
            <div className='px-4 py-8'>
                <Space direction="vertical" style={{ width: "100%" }} size="large">
                    <Card>
                        <Row gutter={[16, 16]} align="middle">
                            <Col xs={24} md={8}>
                                <Search
                                    placeholder={t("search_by_name")}
                                    allowClear
                                    onChange={(e) => debouncedSearch(e.target.value)}
                                    style={{ width: 200 }}
                                />
                            </Col>
                        </Row>
                    </Card>

                    {/* Main Content */}
                    <Row gutter={24}>
                        {/* Users Table */}
                        <Col xs={24} lg={16}>
                            <Card
                                title={t("group_members")}
                                extra={<Text type="secondary">{t("users_found", { total })}</Text>}
                            >
                                <TableWithSkeleton
                                    columns={columns}
                                    dataSource={accounts}
                                    loading={loading}
                                    rowKey="id"
                                    pagination={{
                                        current: currentPage,
                                        pageSize: pageSize,
                                        total: total,
                                        showSizeChanger: true,
                                        pageSizeOptions: ["5", "10", "20", "50"],
                                        showTotal: (total) => t("total_items", { total }),
                                    }}
                                    onChange={handleTableChange}
                                />
                            </Card>
                        </Col>

                        {/* Selected Users Panel */}
                        <Col xs={24} lg={8}>
                            <Card
                                title={t("selected_for_removal", { selectedCount })}
                                extra={
                                    selectedCount > 0 && (
                                        <Button type="text" danger onClick={handleClearAll}>
                                            {t("clear_all")}
                                        </Button>
                                    )
                                }
                                style={{ height: "100%" }}
                            >
                                {selectedCount > 0 ? (
                                    <Space direction="vertical" style={{ width: "100%" }}>
                                        {Object.values(selectedAccounts).map((account) => (
                                            <Card key={account.id} size="small" style={{ marginBottom: 8 }}>
                                                <Row justify="space-between" align="middle">
                                                    <Col>
                                                        <Space>
                                                            <Avatar size="small" style={{ backgroundColor: "#ff4d4f" }}>
                                                                {account.fullName?.charAt(0)?.toUpperCase() || account.email?.charAt(0)?.toUpperCase() || "U"}
                                                            </Avatar>
                                                            <Text ellipsis style={{ maxWidth: 180 }}>{account.email}</Text>
                                                        </Space>
                                                    </Col>
                                                    <Col>
                                                        <Button
                                                            type="text"
                                                            icon={<DeleteOutlined />}
                                                            onClick={() => handleRemoveSelected(account.id)}
                                                            size="small"
                                                        />
                                                    </Col>
                                                </Row>
                                            </Card>
                                        ))}

                                        <Divider />

                                        <Button
                                            type="primary"
                                            danger
                                            size="large"
                                            block
                                            onClick={handleSubmit}
                                            loading={submitting}
                                            icon={<UserDeleteOutlined />}
                                        >
                                            {t("remove_selected_users", { selectedCount })}
                                        </Button>
                                    </Space>
                                ) : (
                                    <div
                                        style={{
                                            display: "flex",
                                            flexDirection: "column",
                                            alignItems: "center",
                                            justifyContent: "center",
                                            padding: "40px 0",
                                            color: "#8c8c8c",
                                        }}
                                    >
                                        <UserDeleteOutlined style={{ fontSize: 48, marginBottom: 16 }} />
                                        <Text type="secondary">{t("no_users_selected")}</Text>
                                        <Text type="secondary">{t("select_users_to_remove")}</Text>
                                    </div>
                                )}
                            </Card>
                        </Col>
                    </Row>
                </Space>
            </div>
        </Layout>
    );
};

export default RemoveUsersFromGroup;