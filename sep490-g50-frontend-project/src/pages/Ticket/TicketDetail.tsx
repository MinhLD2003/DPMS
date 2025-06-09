import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from 'react-router-dom';
import moment from "moment";
import {
  Form,
  Input,
  Select,
  Button,
  Space,
  Typography,
  Breadcrumb,
  Tag,
  Row,
  Col,
  Spin,
  Upload,
  message,
  Card,
  Divider,
  Table,
  Empty,
  List,
  Modal,
  Tooltip,
  Avatar
} from "antd";
import {
  LeftOutlined,
  SaveOutlined,
  PaperClipOutlined,
  DownloadOutlined,
  UploadOutlined,
  DeleteOutlined,
  EditOutlined,
  HomeOutlined,
  ClockCircleOutlined,
  UserOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  CheckOutlined,
  FileTextOutlined,
  InfoCircleOutlined,
  TagOutlined,
  FileOutlined,
  FilePdfOutlined,
  FileImageOutlined,
  FileExcelOutlined,
  FileWordOutlined
} from "@ant-design/icons";
import { IssueTicketStatus, TicketType } from "../../enum/enum";
import AxiosClient from '../../configs/axiosConfig';
import API_ENDPOINTS from '../../configs/APIEndPoint';
import { useTranslation } from 'react-i18next';
import FeatureGuard from "../../routes/FeatureGuard";

// Component constants
const { Title, Text, Link } = Typography;
const { TextArea } = Input;
const { Option } = Select;
const TicketTypes = Object.values(TicketType);
const StatusValues = Object.values(IssueTicketStatus);

// Component interfaces
interface AttachmentFile {
  id: string;
  title: string;
  fileUrl: string;
  fileFormat?: string;
}

interface Ticket {
  id: string;
  title: string;
  issueTicketStatus: string | number;
  externalSystemId: string;
  ticketType: string;
  description: string;
  documents: AttachmentFile[] | null;
  createdBy: string;
  createdAt: string;
  lastModifiedBy: string;
  lastModifiedAt: string;
}

// Interface for removed files - now using id as identifier
interface RemovedFile {
  id: string;
  fileUrl: string; // Keep fileUrl for API submission
}

/**
 * Helper function to format date strings
 */


/**
 * Helper function to safely get status as a string
 */
const getStatusString = (status: any): string => {
  if (status === undefined || status === null) return '';

  // If it's a number, map it to enum name
  if (typeof status === 'number') {
    const statusMap: Record<number, string> = {
      0: 'pending',
      1: 'accept',
      2: 'reject',
      3: 'done'
    };
    return statusMap[status] || '';
  }

  // If it's a string, convert to lowercase
  if (typeof status === 'string') {
    return status.toLowerCase();
  }

  // Otherwise, convert to string and lowercase
  return String(status).toLowerCase();
};

/**
 * Helper function to get enum value from status name
 */
const getEnumValue = (statusName: string): number => {
  const statusMap: Record<string, number> = {
    'pending': 0,
    'accept': 1,
    'reject': 2,
    'done': 3
  };
  return statusMap[statusName.toLowerCase()] || 0;
};

/**
 * UpdateTicket Component - Beautified
 */
const UpdateTicket: React.FC = () => {
  // Hooks and state
  const [form] = Form.useForm();
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation(); // Initialize useTranslation

  // State management
  const [ticket, setTicket] = useState<Ticket | undefined>();
  const [loading, setLoading] = useState<boolean>(true);
  const [fileList, setFileList] = useState<any[]>([]);
  const [removedFiles, setRemovedFiles] = useState<RemovedFile[]>([]);
  const [uploadLoading, setUploadLoading] = useState<boolean>(false);
  const [isEditMode, setIsEditMode] = useState<boolean>(false);

  // Status update states
  const [statusUpdateLoading, setStatusUpdateLoading] = useState<boolean>(false);
  const [showStatusDropdown, setShowStatusDropdown] = useState<boolean>(false);

  // Action confirmation modals
  const [isAcceptModalVisible, setIsAcceptModalVisible] = useState<boolean>(false);
  const [isRejectModalVisible, setIsRejectModalVisible] = useState<boolean>(false);
  const [isDoneModalVisible, setIsDoneModalVisible] = useState<boolean>(false);
  const [actionReason, setActionReason] = useState<string>('');

  // Helper function to determine file icon based on filename
  const getFileIcon = (fileName: string) => {
    const extension = fileName.split('.').pop()?.toLowerCase();

    switch (extension) {
      case 'pdf': return <FilePdfOutlined style={{ color: '#ff4d4f' }} />;
      case 'jpg':
      case 'jpeg':
      case 'png':
      case 'gif': return <FileImageOutlined style={{ color: '#722ed1' }} />;
      case 'xlsx':
      case 'xls': return <FileExcelOutlined style={{ color: '#52c41a' }} />;
      case 'docx':
      case 'doc': return <FileWordOutlined style={{ color: '#1890ff' }} />;
      default: return <FileOutlined style={{ color: '#faad14' }} />;
    }
  };
  const formatDate = (dateString: any) => {
    if (!dateString) return '-';
    return moment(dateString).format('DD/MM/YYYY HH:mm');
  };
  
  // Fetch ticket details on component mount or when ID changes
  useEffect(() => {
    const fetchTicketDetail = async () => {
      if (!id) return;

      try {
        setLoading(true);
        const response = await AxiosClient.get(API_ENDPOINTS.TICKETS.GET_BY_ID(id));
        const ticketData = response.data;

        setTicket(ticketData);
        form.setFieldsValue({
          title: ticketData.title,
          externalSystemId: ticketData.externalSystemId,
          ticketType: ticketData.ticketType,
          issueTicketStatus: ticketData.issueTicketStatus,
          description: ticketData.description
        });
      }
      catch (error) {
        console.error("Error fetching ticket:", error);
        message.error(t('status_code_wrong'));
      } finally {
        setLoading(false);
      }
    };

    fetchTicketDetail();
  }, [id, form, t]);

  /**
   * Handles form submission to update ticket
   */
  const handleFormSubmit = async (values: any) => {
    try {
      setUploadLoading(true);
      const formData = new FormData();

      // Add form values to formData
      formData.append('Title', values.title);
      formData.append('TicketType', values.ticketType);
      formData.append('Description', values.description);
      formData.append('ExternalSystemId', values.externalSystemId || '');
      formData.append('IssueTicketStatus', values.issueTicketStatus);

      // Handle file uploads
      if (fileList && fileList.length > 0) {
        fileList.forEach(file => {
          if (file.originFileObj) {
            formData.append('newFiles', file.originFileObj);
          } else if (file instanceof File) {
            formData.append('newFiles', file);
          }
        });
      }

      // Handle removed files
      if (removedFiles.length > 0) {
        removedFiles.forEach(file => {
          formData.append('removedFiles', file.fileUrl);
        });
      }

      // Submit update request
      await AxiosClient.put(
        API_ENDPOINTS.TICKETS.UPDATE(id ?? ''),
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data'
          }
        }
      );

      message.success(t('changes_saved_successfully'));
      setIsEditMode(false);

      // Refresh ticket data
      const response = await AxiosClient.get(API_ENDPOINTS.TICKETS.GET_BY_ID(id ?? ""));
      setTicket(response.data);

      // Reset file states
      setFileList([]);
      setRemovedFiles([]);
    } catch (error) {
      console.error("Error updating ticket:", error);
      message.error(t('failed_to_save_changes'));
    } finally {
      setUploadLoading(false);
    }
  };

  /**
   * Updates the ticket status
   */
  const updateTicketStatus = async (newStatus: string, reason?: string) => {
    try {
      setStatusUpdateLoading(true);

      // Get numeric enum value for status
      const statusValue = getEnumValue(newStatus);

      // Send the raw numeric value 
      const response = await AxiosClient.put(
        API_ENDPOINTS.TICKETS.UPDATE_STATUS(id ?? ''),
        statusValue
      );

      // Update local ticket data
      if (response.data) {
        setTicket(prev => {
          if (!prev) return undefined;

          return {
            ...prev,
            issueTicketStatus: response.data.issueTicketStatus || statusValue,
            lastModifiedAt: response.data.lastModifiedAt || new Date().toISOString()
          };
        });

        // Update form value to stay in sync
        form.setFieldValue('issueTicketStatus', response.data.issueTicketStatus || statusValue);
      }

      // Show success message
      message.success(`Ticket status updated to "${newStatus}"`);

      // Reset action reason
      setActionReason('');

    } catch (error) {
      console.error(`Error updating ticket status to ${newStatus}:`, error);
      message.error(t('failed_to_update_status'));
    } finally {
      setStatusUpdateLoading(false);

      // Close all modals
      setIsAcceptModalVisible(false);
      setIsRejectModalVisible(false);
      setIsDoneModalVisible(false);
    }
  };

  /**
   * Handle Accept action
   */
  const handleAcceptTicket = () => {
    updateTicketStatus('Accept', actionReason);
  };

  /**
   * Handle Reject action
   */
  const handleRejectTicket = () => {
    updateTicketStatus('Reject', actionReason);
  };

  /**
   * Handle Done action
   */
  const handleDoneTicket = () => {
    updateTicketStatus('Done', actionReason);
  };

  /**
   * Handle status update through dropdown
   */
  const handleStatusUpdate = async (newStatus: string) => {
    updateTicketStatus(newStatus);
    setShowStatusDropdown(false);
  };

  // Navigation and UI state handlers
  const handleBack = () => navigate('../tickets');
  const handleEdit = () => setIsEditMode(true);

  const handleCancel = () => {
    setIsEditMode(false);
    if (ticket) {
      form.setFieldsValue({
        title: ticket.title,
        externalSystemId: ticket.externalSystemId,
        ticketType: ticket.ticketType,
        issueTicketStatus: ticket.issueTicketStatus,
        description: ticket.description
      });
    }
    setFileList([]);
    setRemovedFiles([]);
  };

  /**
   * Downloads a file attachment
   */
  const handleDownload = async (fileUrl: string, fileName: string) => {
    try {
      message.loading(t('downloading_file'), 0);
      const response = await AxiosClient.get(
        `/File?fileName=${encodeURIComponent(fileUrl)}`,
        { responseType: 'blob' }
      );

      const blob = new Blob([response.data]);
      const url = window.URL.createObjectURL(blob);

      // Create virtual link element and trigger download
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', fileName);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);

      message.destroy();
      message.success(t('file_downloaded_successfully'));
    } catch (error) {
      message.destroy();
      console.error("Error downloading file:", error);
      message.error(t('failed_to_download_file'));
    }
  };

  /**
   * Handles removal of existing file attachments using file ID
   */
  const handleRemoveExistingFile = (file: AttachmentFile) => {
    setRemovedFiles(prevRemovedFiles => {
      // Add this specific file to removedFiles using ID as identifier
      const updatedRemovedFiles = [...prevRemovedFiles, { id: file.id, fileUrl: file.fileUrl }];

      // Update UI by filtering out the file with matching ID
      if (ticket?.documents) {
        const updatedDocuments = ticket.documents.filter(doc => {
          // Only remove files with matching IDs
          return !updatedRemovedFiles.some(removedFile => removedFile.id === doc.id);
        });

        setTicket(prevTicket => ({
          ...prevTicket!,
          documents: updatedDocuments
        }));
      }

      return updatedRemovedFiles;
    });
  };

  // Modified attachment table configuration with better styling
  const attachmentColumns = [
    {
      title: t('file_name'),
      dataIndex: 'title',
      key: 'title',
      render: (text: string, record: AttachmentFile) => (
        <Space>
          {getFileIcon(text)}
          <Link
            onClick={() => handleDownload(record.fileUrl, text)}
            style={{ color: '#1890ff', fontWeight: 500 }}
          >
            {text}
          </Link>
        </Space>
      ),
    },
    {
      title: t('actions'),
      key: 'actions',
      width: 120,
      render: (_: any, record: AttachmentFile) => (
        <Space>
          <Button
            type="text"
            icon={<DownloadOutlined />}
            onClick={() => handleDownload(record.fileUrl, record.title)}
            title="Download file"
            style={{ color: '#1890ff' }}
          />
          {isEditMode && (
            <Button
              type="text"
              danger
              icon={<DeleteOutlined />}
              onClick={() => handleRemoveExistingFile(record)}
              title="Remove file"
            />
          )}
        </Space>
      ),
    },
  ];

  // Upload configuration
  const uploadProps = {
    onRemove: (file: any) => {
      const index = fileList.indexOf(file);
      const newFileList = fileList.slice();
      newFileList.splice(index, 1);
      setFileList(newFileList);
    },
    beforeUpload: (file: any) => {
      // Check for duplicate files in existing documents
      const existingDocuments = ticket?.documents || [];
      const isDuplicateExisting = existingDocuments.some(
        doc => !removedFiles.some(removed => removed.id === doc.id) &&
          doc.title.toLowerCase() === file.name.toLowerCase()
      );

      // Check for duplicate files in current upload list
      const isDuplicateInUpload = fileList.some(
        item => item.name.toLowerCase() === file.name.toLowerCase()
      );

      if (isDuplicateExisting || isDuplicateInUpload) {
        message.error(`File "${file.name}" already exists. Please rename the file before uploading.`);
        return Upload.LIST_IGNORE; // Prevents the file from being added
      }

      // If no duplicates, add the file to fileList
      setFileList(prev => [...prev, {
        uid: file.uid || `-${Date.now()}`,
        name: file.name,
        status: 'done',
        url: URL.createObjectURL(file),
        originFileObj: file
      }]);
      return false; // Prevent auto upload
    },
    fileList,
    listType: "text" as const,
    showUploadList: false  // Hide the default file list
  };

  /**
   * Renders the attachments section with improved styling
   */
  const renderAttachments = () => {
    const remainingDocuments = ticket?.documents?.filter(doc => {
      return !removedFiles.some(removedFile => removedFile.id === doc.id);
    }) || [];

    if (remainingDocuments.length === 0) {
      return (
        <div style={{
          padding: '24px',
          background: '#fafafa',
          borderRadius: '6px',
          textAlign: 'center',
          border: '1px dashed #d9d9d9'
        }}>
          <Space direction="vertical" align="center">
            <PaperClipOutlined style={{ fontSize: '24px', color: '#bfbfbf' }} />
            <Text type="secondary">{t('no_attachments_available')}</Text>
          </Space>
        </div>
      );
    }

    return (
      <Table
        dataSource={remainingDocuments}
        columns={attachmentColumns}
        rowKey="id"
        pagination={false}
        size="small"
        className="attachment-table"
        bordered={false}
        style={{ borderRadius: '6px', overflow: 'hidden' }}
      />
    );
  };

  /**
   * Renders workflow action buttons based on current ticket status
   */
  const renderWorkflowButtons = () => {
    if (isEditMode) return null;

    const statusLower = getStatusString(ticket?.issueTicketStatus);

    switch (statusLower) {
      case 'pending':
        return (
          <Space>
            <FeatureGuard requiredFeature="/api/IssueTicket/{id}/update-status_PUT">
              <Tooltip title={t('accept_ticket')}>
                <Button
                  type="primary"
                  icon={<CheckCircleOutlined />}
                  onClick={() => setIsAcceptModalVisible(true)}
                  style={{
                    backgroundColor: '#52c41a',
                    borderColor: '#52c41a',
                    borderRadius: '4px'
                  }}
                >
                  {t('accept')}
                </Button>
              </Tooltip>
              <Tooltip title={t('reject_ticket')}>
                <Button
                  type="primary"
                  danger
                  icon={<CloseCircleOutlined />}
                  onClick={() => setIsRejectModalVisible(true)}
                  style={{ borderRadius: '4px' }}
                >
                  {t('reject')}
                </Button>
              </Tooltip>
            </FeatureGuard>
          </Space>
        );

      case 'accept':
        return (
          <Tooltip title={t('mark_ticket_done')}>
            <Button
              type="primary"
              icon={<CheckOutlined />}
              onClick={() => setIsDoneModalVisible(true)}
              style={{
                backgroundColor: '#1890ff',
                borderColor: '#1890ff',
                borderRadius: '4px'
              }}
            >
              {t('mark_as_done')}
            </Button>
          </Tooltip>
        );

      case 'done':
      case 'reject':
        return (
          <Tag color={statusLower === 'done' ? 'success' : 'error'} style={{
            borderRadius: '16px',
            padding: '4px 12px',
            border: 'none'
          }}>
            {typeof ticket?.issueTicketStatus === 'number'
              ? ['Pending', 'Accept', 'Reject', 'Done'][ticket.issueTicketStatus]
              : ticket?.issueTicketStatus} - No further actions available
          </Tag>
        );

      default:
        return (
          <Space>
            <Button
              type="default"
              onClick={() => setShowStatusDropdown(true)}
              style={{ borderRadius: '4px' }}
            >
              Status
            </Button>
            {showStatusDropdown && (
              <Select
                value={ticket?.issueTicketStatus.toString()}
                onChange={handleStatusUpdate}
                style={{ width: 180 }}
                onBlur={() => setShowStatusDropdown(false)}
                open={true}
                autoFocus
                dropdownStyle={{ borderRadius: '4px' }}
              >
                {StatusValues.map((status) => (
                  <Option key={status.toString()} value={status.toString()}>
                    {status.toString()}
                  </Option>
                ))}
              </Select>
            )}
          </Space>
        );
    }
  };

  // Get status display with properly formatted value
  const getStatusDisplay = (status: string | number) => {
    if (typeof status === 'number') {
      return ['Pending', 'Accept', 'Reject', 'Done'][status] || status;
    }
    return status;
  };

  // Render loading state with improved styling
  if (loading) {
    return (
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        height: '80vh',
        background: '#f5f5f5'
      }}>
        <Card style={{
          padding: '40px',
          textAlign: 'center',
          borderRadius: '8px',
          boxShadow: '0 2px 8px rgba(0,0,0,0.08)'
        }}>
          <Space direction="vertical" size="large" align="center">
            <Spin size="large" />
            <Text>{t('loading_ticket_details')}</Text>
          </Space>
        </Card>
      </div>
    );
  }

  // Render not found state with improved styling
  if (!ticket) {
    return (
      <div style={{
        padding: "24px",
        background: "#f5f5f5",
        minHeight: "100vh"
      }}>
        <Card
          style={{
            maxWidth: '600px',
            margin: '40px auto',
            textAlign: 'center',
            borderRadius: '8px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.08)'
          }}
        >
          <Space direction="vertical" size="large" style={{ width: '100%' }}>
            <InfoCircleOutlined style={{ fontSize: '48px', color: '#1890ff' }} />
            <Title level={4}>Ticket Not Found</Title>
            <Text type="secondary">
              {t('the_ticket_youre_looking_for_could_not_be_found_or_may_have_been_deleted.')}
            </Text>
            <Button
              type="primary"
              onClick={handleBack}
              icon={<LeftOutlined />}
              style={{ borderRadius: '4px' }}
            >
              {t('back_to_tickets')}
            </Button>
          </Space>
        </Card>
      </div>
    );
  }

  return (
    <div style={{ padding: "24px", background: "#f5f5f5", minHeight: "100vh" }}>
      {/* Header Card with Navigation - Beautified */}
      <Card
        style={{
          marginBottom: "24px",
          boxShadow: "0 2px 8px rgba(0,0,0,0.08)",
          borderRadius: "8px"
        }}
      >
        <Space direction="vertical" size={16} style={{ width: '100%' }}>
          {/* Navigation */}
          <Space>
            <Button
              icon={<LeftOutlined />}
              onClick={handleBack}
              style={{ borderRadius: '4px' }}
            >
              {t('back')}
            </Button>
            <Breadcrumb
              items={[
                { title: <><FileTextOutlined /> {t('tickets')}</>, href: "/tickets" },
                { title: <><InfoCircleOutlined /> {t('ticket_details')}</> }
              ]}
              separator=">"
            />
          </Space>

          {/* Ticket Header - Beautified */}
          <Row justify="space-between" align="middle">
            <Col>
              <Space direction="vertical" size={8} style={{ width: '100%' }}>
                <Title level={3} style={{ margin: 0, fontWeight: 600 }}>
                  {ticket.title}
                </Title>
                <Space align="center" size={12}>
                  <Tag color="#108ee9" style={{
                    borderRadius: '16px',
                    padding: '0 10px',
                    fontSize: '13px',
                    border: 'none'
                  }}>
                    #{ticket.id}
                  </Tag>
                  <Divider type="vertical" style={{ height: '20px', margin: '0 8px' }} />
                  <Tag
                    color={
                      getStatusString(ticket.issueTicketStatus) === "done" ? "success" :
                        getStatusString(ticket.issueTicketStatus) === "reject" ? "error" :
                          getStatusString(ticket.issueTicketStatus) === "accept" ? "processing" :
                            getStatusString(ticket.issueTicketStatus) === "pending" ? "warning" : "default"
                    }
                    style={{
                      padding: '4px 12px',
                      fontSize: '13px',
                      fontWeight: 500,
                      textTransform: 'capitalize',
                      border: 'none'
                    }}
                  >
                    {getStatusDisplay(ticket.issueTicketStatus)}
                  </Tag>
                </Space>
              </Space>
            </Col>
            <Col>
              <Space>
                {!isEditMode && (
                  <Button
                    type="primary"
                    icon={<EditOutlined />}
                    onClick={handleEdit}
                    disabled={['done', 'reject'].includes(getStatusString(ticket.issueTicketStatus))}
                    style={{ borderRadius: '4px' }}
                  >
                    {t('edit_ticket')}
                  </Button>
                )}
              </Space>
            </Col>
          </Row>
        </Space>
      </Card>

      {/* Main Content Card - Beautified */}
      <Card
        style={{
          boxShadow: "0 2px 8px rgba(0,0,0,0.08)",
          borderRadius: "8px"
        }}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleFormSubmit}
          initialValues={ticket}
        >
          {/* Combined Info & Details Panel */}
          <Row gutter={[24, 24]}>
            {/* Left side: Core ticket info */}
            <Col xs={24} md={16}>
              <Card
                className="ticket-details-card"
                bordered={false}
                style={{
                  borderRadius: "8px",
                  background: "#ffffff",
                  boxShadow: "0 1px 3px rgba(0,0,0,0.05)"
                }}
              >
                <Form.Item
                  name="title"
                  label={
                    <Space>
                      <InfoCircleOutlined style={{ color: '#1890ff' }} />
                      <span style={{ fontWeight: 500 }}>{t('ticket_title')}</span>
                    </Space>
                  }
                  rules={[{ required: true, message: t('please_enter_ticket_title') }]}
                >
                  {isEditMode ? (
                    <Input
                      placeholder="Enter ticket title"
                      style={{ borderRadius: '4px' }}
                    />
                  ) : (
                    <div className="ant-form-text" style={{
                      padding: "6px 11px",
                      background: "#fafafa",
                      borderRadius: "4px",
                      minHeight: "32px",
                      display: "flex",
                      alignItems: "center"
                    }}>
                      {ticket.title}
                    </div>
                  )}
                </Form.Item>

                <Row gutter={24}>
                  <Col xs={12}>
                    <Form.Item
                      name="ticketType"
                      label={
                        <Space>
                          <TagOutlined style={{ color: '#722ed1' }} />
                          <span style={{ fontWeight: 500 }}>{t('type')}</span>
                        </Space>
                      }
                      rules={[{ required: true, message: t('please_select_ticket_type') }]}
                    >
                      {isEditMode ? (
                        <Select
                          placeholder={t('please_select_ticket_type')}
                          style={{ borderRadius: '4px' }}
                        >
                          {TicketTypes.map((type) => (
                            <Option key={type.toString()} value={type.toString()}>
                              {type.toString()}
                            </Option>
                          ))}
                        </Select>
                      ) : (
                        <Tag color="#722ed1" style={{
                          padding: '4px 12px',
                          fontSize: '14px',
                          borderRadius: '16px',
                          border: 'none'
                        }}>
                          {ticket.ticketType}
                        </Tag>
                      )}
                    </Form.Item>
                  </Col>
                  <Col xs={12}>
                    <Form.Item
                      name="issueTicketStatus"
                      label={
                        <Space>
                          <ClockCircleOutlined style={{ color: '#1890ff' }} />
                          <span style={{ fontWeight: 500 }}>{t('status')}</span>
                        </Space>
                      }
                    >
                      
                        <Tag
                          color={
                            getStatusString(ticket.issueTicketStatus) === "done" ? "success" :
                              getStatusString(ticket.issueTicketStatus) === "reject" ? "error" :
                                getStatusString(ticket.issueTicketStatus) === "accept" ? "processing" :
                                  getStatusString(ticket.issueTicketStatus) === "pending" ? "warning" : "default"
                          }
                          style={{
                            padding: '4px 12px',
                            fontSize: '14px',
                            borderRadius: '16px',
                            textTransform: 'capitalize',
                            border: 'none'
                          }}
                        >
                          {getStatusDisplay(ticket.issueTicketStatus)}
                        </Tag>
                    </Form.Item>
                  </Col>
                </Row>

                {/* Description - more compact & styled */}
                <Form.Item
                  name="description"
                  label={
                    <Space>
                      <FileTextOutlined style={{ color: '#1890ff' }} />
                      <span style={{ fontWeight: 500 }}>{t('description')}</span>
                    </Space>
                  }
                >
                  {isEditMode ? (
                    <TextArea
                      rows={4}
                      placeholder={t('enter_ticket_description')}
                      showCount
                      maxLength={1000}
                      style={{ borderRadius: '4px' }}
                    />
                  ) : (
                    <div style={{
                      padding: '12px 16px',
                      background: "#fafafa",
                      border: "1px solid #f0f0f0",
                      borderRadius: "6px",
                      minHeight: "80px",
                      maxHeight: "200px",
                      overflow: "auto"
                    }}>
                      {ticket.description ? (
                        <Typography.Paragraph style={{ whiteSpace: "pre-line", margin: 0 }}>
                          {ticket.description}
                        </Typography.Paragraph>
                      ) : (
                        <Typography.Text type="secondary" italic>
                          No description provided
                        </Typography.Text>
                      )}
                    </div>
                  )}
                </Form.Item>
              </Card>
            </Col>

            {/* Right side: Metadata */}
            <Col xs={24} md={8}>
              <Card
                size="small"
                title={
                  <Space>
                    <InfoCircleOutlined style={{ color: '#1890ff' }} />
                    <span style={{ fontWeight: 500 }}>Ticket Information</span>
                  </Space>
                }
                style={{
                  marginBottom: "24px",
                  borderRadius: "8px",
                  boxShadow: "0 1px 3px rgba(0,0,0,0.05)"
                }}
                bodyStyle={{ padding: '16px' }}
              >
                <List
                  itemLayout="horizontal"
                  split={false}
                  dataSource={[
                    {
                      icon: <UserOutlined style={{ color: '#1890ff' }} />,
                      label: "Created by",
                      value: ticket.createdBy
                    },
                    {
                      icon: <ClockCircleOutlined style={{ color: '#52c41a' }} />,
                      label: "Created at",
                      value: formatDate(ticket.createdAt)
                    },
                    {
                      icon: <UserOutlined style={{ color: '#1890ff' }} />,
                      label: "Last modified by",
                      value: ticket.lastModifiedBy
                    },
                    {
                      icon: <ClockCircleOutlined style={{ color: '#52c41a' }} />,
                      label: "Last modified at",
                      value: formatDate(ticket.lastModifiedAt)
                    }
                  ]}
                  renderItem={item => (
                    <List.Item style={{ padding: '8px 0' }}>
                      <Space>
                        {item.icon}
                        <Text type="secondary">{item.label}:</Text>
                        <Text strong>{item.value}</Text>
                      </Space>
                    </List.Item>
                  )}
                />
              </Card>

              {!isEditMode && (
                <Card
                  size="small"
                  title={
                    <Space>
                      <TagOutlined style={{ color: '#fa8c16' }} />
                      <span style={{ fontWeight: 500 }}>{t('actions')}</span>
                    </Space>
                  }
                  style={{
                    marginBottom: '24px',
                    borderRadius: "8px",
                    boxShadow: "0 1px 3px rgba(0,0,0,0.05)"
                  }}
                  bodyStyle={{ padding: '16px', display: 'flex', justifyContent: 'center' }}
                >
                  <div>
                    {renderWorkflowButtons()}
                  </div>
                </Card>
              )}
            </Col>
          </Row>

          {/* Hidden field */}
          <Form.Item name="externalSystemId" hidden>
            <Input />
          </Form.Item>

          {/* Attachments section - styled */}
          <Card
            title={
              <Space>
                <PaperClipOutlined style={{ color: '#1890ff' }} />
                <span style={{ fontWeight: 500 }}>{t('attachments')}</span>
                {isEditMode && (
                  <Upload {...uploadProps}>
                    <Button size="small" type="link" icon={<UploadOutlined />} style={{ color: '#1890ff' }}>
                      {t('add_files')}
                    </Button>
                  </Upload>
                )}
              </Space>
            }
            style={{
              marginTop: "16px",
              marginBottom: "16px",
              borderRadius: "8px",
              boxShadow: "0 1px 3px rgba(0,0,0,0.05)"
            }}
          >
            {isEditMode ? (
              <div>
                {renderAttachments()}

                {fileList.length > 0 && (
                  <div style={{
                    marginTop: '16px',
                    padding: '12px',
                    border: '1px dashed #d9d9d9',
                    borderRadius: '6px',
                    background: '#fafafa'
                  }}>
                    <Text strong style={{ display: 'block', marginBottom: '8px' }}>
                      <UploadOutlined style={{ marginRight: '8px' }} />
                      Newly Added Files:
                    </Text>
                    <List
                      size="small"
                      dataSource={fileList}
                      renderItem={file => (
                        <List.Item
                          actions={[
                            <Button
                              type="text"
                              danger
                              icon={<DeleteOutlined />}
                              onClick={() => uploadProps.onRemove(file)}
                              size="small"
                            />
                          ]}
                          style={{ padding: '8px 0' }}
                        >
                          <List.Item.Meta
                            avatar={getFileIcon(file.name)}
                            title={file.name}
                            style={{ fontSize: '12px' }}
                          />
                        </List.Item>
                      )}
                    />
                  </div>
                )}
              </div>
            ) : (
              renderAttachments()
            )}
          </Card>

          {/* Action Buttons */}
          {isEditMode && (
            <Form.Item>
              <Row justify="end">
                <Space size="middle">
                  <Button
                    onClick={handleCancel}
                    style={{ borderRadius: '4px' }}
                  >
                    {t('cancel')}
                  </Button>
                  <Button
                    type="primary"
                    htmlType="submit"
                    icon={<SaveOutlined />}
                    loading={uploadLoading}
                    style={{
                      borderRadius: '4px',
                      boxShadow: '0 2px 0 rgba(0, 0, 0, 0.045)'
                    }}
                  >
                    {t('save_changes')}
                  </Button>
                </Space>
              </Row>
            </Form.Item>
          )}
        </Form>
      </Card>

      {/* Accept Modal */}
      <Modal
        title={
          <Space>
            <CheckCircleOutlined style={{ color: '#52c41a' }} />
            <span>{t('accept_ticket')}</span>
          </Space>
        }
        open={isAcceptModalVisible}
        onCancel={() => setIsAcceptModalVisible(false)}
        onOk={handleAcceptTicket}
        confirmLoading={statusUpdateLoading}
        okText={t('accept')}
        cancelText={t('cancel')}
        okButtonProps={{
          style: {
            backgroundColor: '#52c41a',
            borderColor: '#52c41a',
            borderRadius: '4px'
          }
        }}
        cancelButtonProps={{ style: { borderRadius: '4px' } }}
        style={{ top: 20 }}
        bodyStyle={{ padding: '16px' }}
      >
        <p style={{width: 300}}>{t('are_you_sure_want_to_accept_ticket')}</p>

      </Modal>

      {/* Reject Modal */}
      <Modal
        title={
          <Space>
            <CloseCircleOutlined style={{ color: '#ff4d4f' }} />
            <span>{t('reject_ticket')}</span>
          </Space>
        }
        open={isRejectModalVisible}
        onCancel={() => setIsRejectModalVisible(false)}
        onOk={handleRejectTicket}
        confirmLoading={statusUpdateLoading}
        okText={t('reject')}
        cancelText={t('cancel')}
        okButtonProps={{
          danger: true,
          style: { borderRadius: '4px' }
        }}
        cancelButtonProps={{ style: { borderRadius: '4px' } }}
        style={{ top: 20 }}
        bodyStyle={{ padding: '16px' }}
      >
        <p  style={{width: 300}}>{t('are_you_sure_want_to_reject_ticket')}</p>
      </Modal>

      {/* Done Modal */}
      <Modal
        title={
          <Space>
            <CheckOutlined style={{ color: '#1890ff' }} />
            <span>{t('mark_ticket_done')}</span>
          </Space>
        }
        open={isDoneModalVisible}
        onCancel={() => setIsDoneModalVisible(false)}
        onOk={handleDoneTicket}
        confirmLoading={statusUpdateLoading}
        okText={t('mark_as_done')}
        cancelText={t('cancel')}
        okButtonProps={{
          style: {
            backgroundColor: '#1890ff',
            borderColor: '#1890ff',
            borderRadius: '4px'
          }
        }}
        cancelButtonProps={{ style: { borderRadius: '4px' } }}
        style={{ top: 20 }}
        bodyStyle={{ padding: '16px' }}
      >
        <p  style={{width: 300}}>{t('are_you_sure_want_mark_ticket_done')}</p>
      </Modal>
    </div>
  );
};

export default UpdateTicket;