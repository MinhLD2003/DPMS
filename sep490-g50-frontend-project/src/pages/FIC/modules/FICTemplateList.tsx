import React, { useState, useEffect, useCallback } from 'react';
import { Button, Card, Col, DatePicker, Input, Modal, Row, Space, Table, Tooltip, Typography, message } from 'antd';
import AxiosClient from '../../../configs/axiosConfig';
import { useLocation, useNavigate } from 'react-router-dom';
import moment from 'moment';
import { DeleteOutlined, InfoCircleOutlined, EditOutlined, PlusOutlined } from '@ant-design/icons';
import TaskAltIcon from '@mui/icons-material/TaskAlt';
import BlockIcon from '@mui/icons-material/Block';
import { FICListViewModel } from '../models/FICListViewModel';
import { addIndexToUnpagedData } from '../../../utils/indexHelper';
import { FormStatus, FormType } from '../../../enum/enum';
import { getFormStatusText, getFormTypeText } from '../../../utils/TextColorUtils';
import confirmDelete from '../../../components/common/popup-modals/ConfirmDeleteModal';
import TableWithSkeleton from '../../../components/forms/TableWithSkeleton';
import { useTranslation } from 'react-i18next';
import ListViewContainer from '../../../components/layout/ListViewContainer';
import { TemplateUpdateStatusModel } from '../models/FICPostModel';
import FeatureGuard from '../../../routes/FeatureGuard';
import { getUserFeatures } from '../../../utils/jwtDecodeUtils';
const { Search } = Input;
const { Title, Text } = Typography;

const FICTemplateList: React.FC = () => {
  const { t } = useTranslation();
  const [fics, setFics] = useState<FICListViewModel[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const navigate = useNavigate();
  const location = useLocation();
  const userFeatures = getUserFeatures();
  const fetchFICs = useCallback(async () => {
    setLoading(true);
    try {
      const response = await AxiosClient.get(`/Form/get-templates`);
      const data: FICListViewModel[] = response.data.map((fic: any) => ({
        ...fic,
      }));
      const indexedTemplates = addIndexToUnpagedData(data);
      setFics(indexedTemplates);
      message.success(t('ficTemplateList.fetchSuccess'));
    } catch (error) {
      console.error('Failed to fetch FIC Forms:', error);
      message.error(t('ficTemplateList.fetchFailed'));
    }
    setLoading(false);
  }, [t]); // Only depends on translation function

  useEffect(() => {
    fetchFICs();
  }, [fetchFICs]);


  const handleUpdate = (id: string) => {
    navigate(`${id}/edit`);
  };

  const handleDelete = async (id: string) => {
    setLoading(true);
    try {
      await AxiosClient.delete(`/Form/${id}`);
      setFics(fics.filter(fic => fic.id !== id));
      message.success(t('ficTemplateList.deleteSuccess'));
    } catch (error) {
      console.error('Failed to delete FIC:', error);
      message.error(t('ficTemplateList.deleteFailed'));
    }
    setLoading(false);
  };
  const goToDetail = async (id: string) => {
    setLoading(true);
    navigate(`/dashboard/forms/templates/${id}`);
    setLoading(false);
  }

  const handleConfirmStatusChange = (id: string, status: number) => {
    if (status == 2)
      Modal.confirm({
        title: t('ficTemplateList.activateConfirmTitle'),
        content: t('ficTemplateList.activateConfirmContent'),
        okText: t('confirm'),
        cancelText: t('cancel'),
        onOk: () => handleChangeTemplateStatus(id, status),
      });
    if (status == 3)
      Modal.confirm({
        title: t('ficTemplateList.deactivateConfirmTitle'),
        content: t('ficTemplateList.deactivateConfirmContent'),
        okText: t('confirm'),
        cancelText: t('cancel'),
        onOk: () => handleChangeTemplateStatus(id, status),
      });
  };
  const handleChangeTemplateStatus = async (formId: string, status: number) => {
    setLoading(true);
    try {
      await AxiosClient.put(`/Form/update-status/${formId}?formStatus=${status}`);
      if (status == 2)
        message.success(t('ficTemplateList.activateSuccess'));
      if (status == 3)
        message.success(t('ficTemplateList.deactivateSuccess'));
      await fetchFICs(); // Reload the data after successful activation
    } catch (error) {
      message.error(t('ficTemplateList.activateFailed'));
    }
    setLoading(false);
  }
  const columns = [
    {
      title: t('ficTemplateList.no'),
      dataIndex: 'index',
      key: 'index',
      width: 60,
      align: 'center' as const,
    },
    {
      title: t('ficTemplateList.formName'),
      dataIndex: 'name',
      key: 'name',
      render: (text: string, record: FICListViewModel) => (
        <span
          style={{ color: '#1890ff', cursor: 'pointer' }}
          onClick={() => {
            if (userFeatures.includes('/api/Form/{id}_GET'))
              goToDetail(record.id)
          }
          }
        >
          {text}
        </span>
      ),
    },
    {
      title: t('ficTemplateList.version'),
      dataIndex: 'version',
      key: 'version',
    },
    {
      title: t('ficTemplateList.formType'),
      dataIndex: 'formType',
      key: 'formType',
      render: (formType: FormType) => (
        getFormTypeText(formType)
      )
    },
    {
      title: t('ficTemplateList.status'),
      dataIndex: 'status',
      key: 'status',
      render: (formStatus: FormStatus) => (
        getFormStatusText(formStatus, t)
      )
    },
    {
      title: t('ficTemplateList.createdAt'),
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (date: string) => (date ? moment(date).format('DD/MM/YYYY HH:mm:ss') : '-'),
    },
    {
      title: t('actions'),
      key: 'actions',
      render: (record: FICListViewModel) => (
        <Space size="middle">
          <FeatureGuard requiredFeature='/api/Form/update-status/{id}_PUT'>
            {record.status == 1 && ( // Only show activate button if status is not 1 (Draft)
              <Tooltip title={t('activate')}>
                <Button
                  type="primary"
                  icon={<TaskAltIcon fontSize='small' />}
                  onClick={() => handleConfirmStatusChange(record.id, 2)}
                  size="small"
                />
              </Tooltip>
            )}
          </FeatureGuard>
          <FeatureGuard requiredFeature='/api/Form/update_POST'>
            <Button
              type="primary"
              size='small'
              onClick={() => handleUpdate(record.id)}
              icon={<EditOutlined />}
            />
          </FeatureGuard>
          <FeatureGuard requiredFeature='/api/Form/{id}_GET'>
            <Button
              type="primary"
              onClick={() => goToDetail(record.id)}
              icon={<InfoCircleOutlined />}
              size="small"
            />
          </FeatureGuard>

          <FeatureGuard requiredFeature='/api/Form/update-status/{id}_PUT'>

            {record.status == 2 && ( // Only show deactivate button if status is not 2 (Activated)
              <Tooltip title={t('deactivate')}>
                <Button
                  type="primary"
                  danger
                  icon={<BlockIcon fontSize='small' />}
                  onClick={() => handleConfirmStatusChange(record.id, 3)}
                  size="small"
                />
              </Tooltip>
            )}
          </FeatureGuard>
          <FeatureGuard requiredFeature='/api/Form/{id}_DELETE'>
            {record.status == 1 && //only delete draft (= 1)
              <Button
                danger
                type="primary"
                size="small"
                onClick={() =>
                  confirmDelete(
                    () => handleDelete(record.id),
                    t("confirmDelete.title", { action: t("delete"), itemName: t('ficTemplateList.thisTemplate') }),
                    t("confirmDelete.content"),
                    t("confirmDelete.okText", { action: t("delete") }),
                    t("confirmDelete.cancelText")
                  )
                }
                icon={<DeleteOutlined />}
              />
            }
          </FeatureGuard>

        </Space>
      ),
    },
  ];

  return (
    <ListViewContainer>
      <Row justify="space-between" align="middle" style={{ marginBottom: 24 }}>
        <Col>
          <Title level={2} style={{ margin: 0 }}>{t('ficTemplateList.pageTitle')}</Title>
        </Col>
        <Col>
          <FeatureGuard requiredFeature='/api/Form/save_POST'>
            <Button type="primary" icon={<PlusOutlined />} size='middle' onClick={() => navigate("config")}>
              {t('ficTemplateList.newTemplate')}
            </Button>
          </FeatureGuard>
        </Col>
      </Row>
      <Space style={{ marginBottom: 16 }} wrap>
        {/* <Search
          placeholder={t('ficTemplateList.searchByName')}
          allowClear
          //onSearch={handleSearch}
          style={{ width: 200 }}
        />
        <DatePicker placeholder={t('ficTemplateList.fromDate')} />
        <DatePicker placeholder={t('ficTemplateList.toDate')} /> */}

      </Space>
      <TableWithSkeleton
        columns={columns}
        rowKey='id'
        dataSource={fics}
        loading={loading}
        bordered
        scroll={{ x: 'max-content' }}
        size="middle"
      />
    </ListViewContainer>
  );
};

export default FICTemplateList;