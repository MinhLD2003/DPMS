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
    Tag,
    Typography,
    Space,
    Divider,
    Avatar,
    Layout
} from 'antd';
import { UserAddOutlined, DeleteOutlined } from '@ant-design/icons';
import AxiosClient from '../../../configs/axiosConfig';
import { useNavigate, useParams } from 'react-router-dom';
import accountService from '../../Account/apis/AccountAPIs';
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

// Add this type and helper function after the Account interface
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

const AddUsersToGroup: React.FC = () => {
    const { t } = useTranslation();
    const { groupId, groupName } = useParams<{ groupId: string, groupName: string }>();
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
        return {
            pageNumber: currentPage,
            pageSize,
            filters
        };
    };

    // Modify fetchAccounts to store all accounts
    const fetchAccounts = useCallback(async () => {
        setLoading(true);
        try {
            const response = await AxiosClient.get(`/Group/users-not-in-group/${groupId}`);
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
    }, [groupId]);

    useEffect(() => {
        fetchAccounts();
    }, [fetchAccounts]);

    // Modify the search effect
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

    // Modify the debounced search to not trigger API call
    const debouncedSearch = useCallback(
        debounce((value: string) => {
            setSearchKey(value);
            setCurrentPage(1);
            setSelectedAccounts({});
        }, 300), // Reduced debounce time since we're filtering locally
        []
    );

    useEffect(() => {
        return () => {
            debouncedSearch.cancel();
        };
    }, [debouncedSearch]);

    const handleDateRangeChange = (dates: any) => {
        setDateRange(dates);
        // You might want to trigger search here or implement a separate filter button
    };

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
            await AxiosClient.post(`/Group/add-user-to-group?groupId=${groupId}`, selectedIds);
            message.success(t('accountActions.addSuccess'));
            setSelectedAccounts({});
            navigate(-1);
        } catch (error) {
            console.error('Failed to add users to group:', error);
            message.error(t('accountActions.addFailed'));
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
                    onClick={() => handleSelect(record)}
                    icon={selectedAccounts[record.id] ? <DeleteOutlined /> : <UserAddOutlined />}
                >
                    {selectedAccounts[record.id] ? t("remove") : t("add")}
                </Button>
            ),
        },
    ];

    const selectedCount = Object.keys(selectedAccounts).length;

    return (
        <Layout style={{ minHeight: '100vh', backgroundColor: '#f5f7fa' }}>
            <PageHeader
                title={t("add_users_to_group", { groupName })}
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
                            <Col xs={24} md={16}>
                                {/* <Space wrap>

                                    <Button onClick={() => {
                                        setDateRange([null, null]);
                                        setSearchKey("");
                                    }}>
                                        {t("clear_filters")}
                                    </Button>
                                </Space> */}
                            </Col>
                        </Row>
                    </Card>

                    {/* Main Content */}
                    <Row gutter={24}>
                        {/* Users Table */}
                        <Col xs={24} lg={16}>
                            <Card
                                title={t("available_users")}
                                extra={<Text type="secondary">{t("users_found", { total: total })}</Text>}
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
                                title={t("selected_users", { selectedCount })}
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
                                                            <Avatar size="small" style={{ backgroundColor: "#1890ff" }}>
                                                                {account.fullName?.charAt(0)?.toUpperCase() || account.email?.charAt(0)?.toUpperCase() || "U"}
                                                            </Avatar>
                                                            <Text ellipsis style={{ maxWidth: 180 }}>{account.email}</Text>
                                                        </Space>
                                                    </Col>
                                                    <Col>
                                                        <Button
                                                            type="text"
                                                            danger
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
                                            size="large"
                                            block
                                            onClick={handleSubmit}
                                            loading={submitting}
                                            icon={<UserAddOutlined />}
                                        >
                                            {t("add_users_to_group_button", { selectedCount })}
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
                                        <UserAddOutlined style={{ fontSize: 48, marginBottom: 16 }} />
                                        <Text type="secondary">{t("no_users_selected")}</Text>
                                        <Text type="secondary">{t("select_users_from_table")}</Text>
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

export default AddUsersToGroup;