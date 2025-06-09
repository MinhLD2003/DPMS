import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { Button, Space, Table, message, Input, Card, Typography, Select, Tooltip, Col, Row, Spin } from 'antd';
import { useNavigate } from 'react-router-dom';
import { CheckCircleOutlined, EditOutlined, InfoCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { AccountModel, GroupVM } from '../models/AccountModel';
import accountService from '../apis/AccountAPIs';
import { addIndexToData } from '../../../utils/indexHelper';
import confirmDelete from '../../../components/common/popup-modals/ConfirmDeleteModal';
import { UserStatus } from '../../../enum/enum';
import { debounce } from 'lodash';
import NoAccountsIcon from '@mui/icons-material/NoAccounts';
import moment from 'moment';
import { getGroupText, getUserStatusText } from '../../../utils/TextColorUtils';
import { FilterParams } from '../../../common/FilterParams';
import { useTranslation } from 'react-i18next';
const { Search } = Input;
const { Title, Text } = Typography;
const { Option } = Select;
import HowToRegIcon from '@mui/icons-material/HowToReg';
import CreateAccountModal from './CreateAccount';
import ListViewContainer from '../../../components/layout/ListViewContainer';
import FeatureGuard from '../../../routes/FeatureGuard';
import TableWithSkeleton from '../../../components/forms/TableWithSkeleton';

const ViewAccounts: React.FC = () => {
  const { t } = useTranslation(); // Get translation function

  const [accounts, setAccounts] = useState<AccountModel[]>([]);
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [total, setTotal] = useState(0);

  const [searchKey, setSearchKey] = useState<string>("");
  const [selectedStatus, setSelectedStatus] = useState<number>();

  const navigate = useNavigate();

  const buildFilterParams = (): FilterParams => {
    const filters: Record<string, string> = {};
    if (searchKey) filters['fullName'] = searchKey;
    if (selectedStatus !== undefined) filters['status'] = selectedStatus.toString();
    return { pageNumber: currentPage, pageSize, filters };
  };

  const fetchAccounts = useCallback(async () => {
    setLoading(true);
    try {
      const filterParams = buildFilterParams();
      const result = await accountService.getAccounts(filterParams);

      if (result.success) {
        const indexedAccounts = addIndexToData(result.objectList, currentPage, pageSize);
        setAccounts(indexedAccounts as AccountModel[]);
        setTotal(result.totalRecords);
      } else {
        message.error('Failed to fetch accounts');
      }
    } catch (error) {
      console.error('Error fetching accounts:', error);
      message.error('Failed to fetch accounts');
    } finally {
      setLoading(false);
    }
  }, [searchKey, selectedStatus, currentPage, pageSize, t]);

  useEffect(() => {
    fetchAccounts();
  }, [fetchAccounts]);

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

  const handleStatusChange = (value: number | undefined) => {
    setSelectedStatus(value);
    setCurrentPage(1);
  };

  // const handleUpdate = (id: string) => {
  //   navigate(`/dashboard/accounts/${id}/edit`);
  // };

  const handleTableChange = (pagination: any) => {
    setCurrentPage(pagination.current);
    setPageSize(pagination.pageSize);
  };

  const resetFilters = () => {
    setSearchKey("");
    setSelectedStatus(undefined);
    setCurrentPage(1);
  };
  const goToDetail = (id: string) => {
    navigate(`/dashboard/accounts/${id}`);
  }
  const columns = useMemo(() => [
    {
      title: t("no"),
      dataIndex: 'index',
      key: 'index',
      align: 'center' as 'center'
    },
    {
      title: t("full_name"),
      dataIndex: 'fullName',
      key: 'fullName',
      render: (text: string) => <Text strong>{text}</Text>,
    },
    {
      title: t("user_name"),
      dataIndex: 'userName',
      key: 'userName',
      render: (text: string) => <Text strong>{text}</Text>,
    },
    {
      title: t("email"),
      dataIndex: 'email',
      key: 'email',
      render: (text: string) => <Text>{text}</Text>,
    },
    {
      title: t("groups"),
      dataIndex: 'groups',
      key: 'groups',
      align: 'center' as 'center',
      render: (groups: GroupVM[] = []) => (
        <div>
          {groups.length > 0 ? (
            groups.map(group => (
              <span key={group.id || group.name}>{getGroupText(group.name)}</span>
            ))
          ) : (
            getGroupText("")
          )}
        </div>
      ),
    },
    {
      title: t("status"),
      dataIndex: 'status',
      key: 'status',
      render: (status: UserStatus) => getUserStatusText(status, t),
      align: 'center' as 'center',
    },
    {
      title: t("created_at"),
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (datetime: string) => moment(datetime).format('DD/MM/YYYY HH:mm:ss'),
    },
    {
      title: t("actions"),
      key: 'actions',
      align: 'center' as 'center',
      render: (record: AccountModel) => (
        <Space size="small">
          <FeatureGuard requiredFeature='/api/Account/profile/{id}_GET'>
            <Tooltip title={t("detail")}>
              <Button
                type="primary"
                onClick={() => goToDetail(record.id)}
                icon={<InfoCircleOutlined />}
                size="small"
              />
            </Tooltip>
          </FeatureGuard>
          {/* <Tooltip title={t("edit")}>
            <Button
              type="primary"
              onClick={() => handleUpdate(record.id)}
              icon={<EditOutlined />}
              size="small"
            />
          </Tooltip> */}
          <FeatureGuard requiredFeature='/api/User/update-user-status_PUT'>

            {record.status === 1 ? (
              // Deactivate Button
              <Tooltip title={t("deactivate")}>
                <Button
                  danger
                  type="primary"
                  size="small"
                  onClick={() =>
                    confirmDelete(
                      () => handleToggleStatus({ id: record.id, status: 1 }),
                      t("confirmDelete.title", { action: t("deactivate"), itemName: t("confirmDelete.thisAccount") }),
                      t("confirmDelete.content"),
                      t("confirmDelete.okText", { action: t("deactivate") }),
                      t("confirmDelete.cancelText")
                    )
                  }
                  icon={<NoAccountsIcon fontSize="small" />}
                >
                </Button>
              </Tooltip>
            ) : (
              // Activate Button
              <Tooltip title={t("activate")}>
                <Button
                  type="primary"
                  size="small"
                  onClick={() =>
                    confirmDelete(
                      () => handleToggleStatus({ id: record.id, status: 0 }),
                      t("confirmDelete.title", { action: t("activate"), itemName: t("confirmDelete.thisAccount") }),
                      t("confirmDelete.content"),
                      t("confirmDelete.okText", { action: t("activate") }),
                      t("confirmDelete.cancelText")
                    )
                  }
                  icon={<HowToRegIcon fontSize="small" />}
                >
                </Button>
              </Tooltip>
            )}
          </FeatureGuard>
        </Space>
      ),
    },
  ], [t]);

  const handleToggleStatus = async (payload: { id: string; status: number }) => {
    try {
      const updatedStatus = payload.status === 1 ? 0 : 1;
      const result = await accountService.deactivate({ id: payload.id, status: updatedStatus });

      if (result.success) {
        message.success(`Account ${updatedStatus === 1 ? "activated" : "deactivated"} successfully`);
        fetchAccounts();
      } else {
        message.error(`Failed to ${updatedStatus === 1 ? "activate" : "deactivate"} account`);
      }
    } catch (error) {
      console.error('Status update error:', error);
      message.error("An error occurred while updating the account status");
    }
  };

  return (
    <ListViewContainer>
      <Row justify="space-between" align="middle" style={{ marginBottom: 24 }}>
        <Col>
          <Title level={2} style={{ margin: 0 }}>{t("account_management")}</Title>
        </Col>
        <Col>
          <FeatureGuard requiredFeature='/api/Account_POST'>
            <CreateAccountModal onCreateSuccess={fetchAccounts} />
          </FeatureGuard>
        </Col>
      </Row>

      <Space style={{ marginBottom: 16 }} wrap>
        <Search
          placeholder={t("search_placeholder")}
          allowClear
          onChange={(e) => debouncedSearch(e.target.value)}
        />
        <Select
          placeholder={t("status_placeholder")}
          allowClear
          value={selectedStatus}
          onChange={handleStatusChange}
          className='w-40'
        >
          <Option value={UserStatus.Activated}>{t('activated')}</Option>
          <Option value={UserStatus.Deactivated}>{t('deactivated')}</Option>
        </Select>
        <Button
          onClick={resetFilters}
          style={{ backgroundColor: '#52c41a', color: 'white' }}
        >
          {t("clear_filters")}
        </Button>
      </Space>
      <TableWithSkeleton
        columns={columns}
        rowKey={'id'}
        dataSource={accounts}
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
          pageSizeOptions: ['10', '20', '50'],
          style: { marginTop: 16 }
        }}
        onChange={handleTableChange}
        bordered
        scroll={{ x: 'max-content' }}
        size="middle"
      />
    </ListViewContainer>
  );
};



export default ViewAccounts;
