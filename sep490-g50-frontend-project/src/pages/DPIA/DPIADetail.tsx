import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import AxiosClient from '../../configs/axiosConfig';
import { useTranslation } from 'react-i18next';

import {
    Tooltip,
    Layout,
    Card,
    Alert,
    Table,
    Tag,
    Button,
    Descriptions,
    Typography,
    Space,
    Breadcrumb,
    Upload,
    Tabs,
    Row,
    Col,
    Form,
    Input,
    Avatar,
    Divider,
    message,
    Modal,
    Select,
    Empty,
    Checkbox,
    DatePicker,
    Steps
} from 'antd';
import {
    ArrowLeftOutlined,
    UploadOutlined,
    DownloadOutlined,
    UserOutlined,
    EditOutlined,
    DeleteOutlined,
    PlusCircleFilled,
    RightOutlined,
    LeftOutlined,
    ClockCircleOutlined,
    CheckCircleOutlined,
    CloseCircleOutlined
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import type { UploadFile } from 'antd/es/upload/interface';
import dayjs from 'dayjs';
import FeatureGuard from '../../routes/FeatureGuard';
import { useAuth } from '../../hooks/useAuth';

const { Title, Text, Paragraph } = Typography;
const { TabPane } = Tabs;
const { Dragger } = Upload;
const { TextArea } = Input;
const { Option } = Select;
const { Step } = Steps;

// Type definitions
interface DPIADetail {
    id: string;
    title: string;
    description: string;
    status: string;
    dueDate: string;
    type: string;
    externalSystemId: string;
    externalSystemName: string;
    createdBy: string | null;
    createdAt: string;
    updatedBy: string | null;
    lastModifiedAt: string;
    documents: Document[];
    members: MemberSummary[];
    responsibilities: ResponsibilitySummary[];
}

interface Document {
    title: string;
    fileUrl: string;
    fileFormat?: string;
    id: string;
    createdAt: string;
    createdBy: string;
    lastModifiedAt: string;
}

interface MemberSummary {
    id: string;
    userId: string;
    fullName: string;
    email: string;
    joinedAt: string;
}

interface ResponsibilitySummary {
    id: string;
    responsibilityId: string
    title: string;
    dueDate: Date;
    description: string;
    status: string;
    members: ResponsibilityMember[];
}

interface ResponsibilityMember {
    id: string;
    memberId: string;
    userId: string;
    fullName: string;
    isPic: boolean;
    email: string;
    joinedAt: string;
    completionStatus: string;
}

// For UI display
interface TeamMember {
    key: string;
    id: string;
    userId: string;
    fullName: string;
    email: string;
    joinedAt: string;
    responsibilities: Responsibility[];
}

interface Responsibility {
    id: string;
    responsibilityId: string;
    title: string;
    isCompleted: string;
}

interface FileItem {
    key: string;
    id: number;
    filename: string;
    uploader: string;
    uploaded: string;
    fileUrl?: string;
}

interface HistoryItem {
    id: string;
    text: string;
    type: string;
    createdBy: {
        id: string;
        fullName: string;
        email: string;
    };
    createdAt: string;
    key?: string;
}


interface CommentItem {
    createdById: string;
    key: string;
    user: string;
    date: string;
    content: string;
}


interface ResponsibilityRow {
    key: string;
    id: string;
    responsibilityId: string;
    title: string;
    description: string;
    dueDate?: Date;
    members: {
        id: string;
        userId: string;
        fullName: string;
        email: string;
        completionStatus: string;
        isPic?: boolean;
    }[];
    status: string;
}

// API Service
const dpiaService = {
    getDPIAById: async (id: string): Promise<DPIADetail> => {
        const response = await AxiosClient.get(`/dpia/dpia-detail/${id}`);
        return response.data;
    },
    getFiles: async (dpiaId: string): Promise<FileItem[]> => {
        const response = await AxiosClient.get(`/dpia/${dpiaId}/files`);
        return response.data;
    },
    getHistory: async (dpiaId: string): Promise<HistoryItem[]> => {
        const response = await AxiosClient.get(`/dpia/${dpiaId}/history`);
        return response.data.map((item: any) => ({
            ...item,
            key: item.id
        }));
    },
    getComments: async (dpiaId: string): Promise<CommentItem[]> => {
        const response = await AxiosClient.get(`/dpia/${dpiaId}/comments`);
        return response.data.map((comment: any) => ({
            key: comment.id,
            createdById: comment.userId,
            user: comment.user?.fullName || comment.user?.userName || 'Unknown',
            date: new Date(comment.createdAt).toLocaleString(),
            content: comment.content
        }));
    },
    addComment: async (dpiaId: string, content: string): Promise<CommentItem> => {
        const response = await AxiosClient.post(`/dpia/${dpiaId}/comments`, { content });
        return response.data;
    },
    uploadFile: async (dpiaId: string, formData: FormData): Promise<FileItem> => {
        const response = await AxiosClient.post(`/dpia/${dpiaId}/files`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        });
        return response.data;
    },
    deleteFile: async (dpiaId: string, fileId: string): Promise<void> => {
        await AxiosClient.delete(`/dpia/${dpiaId}/documents/${fileId}`);
    },
    downloadFile: async (dpiaId: string, fileId: number): Promise<Blob> => {
        const response = await AxiosClient.get(`/dpia/${dpiaId}/files/${fileId}/download`, {
            responseType: 'blob'
        });
        return response.data;
    }
};

const DPIADetailsScreen: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { t } = useTranslation();
    const { user } = useAuth();
    // File upload state
    const [fileList, setFileList] = useState<UploadFile[]>([]);
    const [commentForm] = Form.useForm();
    const [loading, setLoading] = useState<boolean>(true);

    const [dpiaDetail, setDpiaDetail] = useState<DPIADetail | null>(null);
    const [teamMembers, setTeamMembers] = useState<TeamMember[]>([]);
    const [files, setFiles] = useState<FileItem[]>([]);
    const [historyItems, setHistoryItems] = useState<HistoryItem[]>([]);
    const [commentItems, setCommentItems] = useState<CommentItem[]>([]);
    const [startDPIAConfirmVisible, setStartDPIAConfirmVisible] = useState(false);
    const [isFileDeleteModalVisible, setIsFileDeleteModalVisible] = useState<boolean>(false);
    const [fileToDelete, setFileToDelete] = useState<string | null>(null);

    // New state for responsibility-based rows
    const [responsibilityRows, setResponsibilityRows] = useState<ResponsibilityRow[]>([]);

    // Separate state for add and edit modals
    const [addModalVisible, setAddModalVisible] = useState(false);
    const [currentMember, setCurrentMember] = useState<TeamMember | null>(null);

    // New state for responsibility edit modal
    const [editResponsibilityVisible, setEditResponsibilityVisible] = useState(false);
    const [currentResponsibility, setCurrentResponsibility] = useState<ResponsibilityRow | null>(null);
    const [responsibilityForm] = Form.useForm();
    const [addResponsibilityForm] = Form.useForm();

    const [availableMembers, setAvailableMembers] = useState<TeamMember[]>([]);
    const [responsibilityOptions, setResponsibilityOptions] = useState<Responsibility[]>([]);
    const [approveModalVisible, setApproveModalVisible] = useState(false);
    const [rejectModalVisible, setRejectModalVisible] = useState(false);
    const [approvalComment, setApprovalComment] = useState('');
    const [rejectionReason, setRejectionReason] = useState('');
    const [approvalLoading, setApprovalLoading] = useState(false);
    const [isDeleteResponsibilityModalVisible, setIsDeleteResponsibilityModalVisible] = useState<boolean>(false);
    const [responsibilityToDelete, setResponsibilityToDelete] = useState<ResponsibilityRow | null>(null);

    const [editDpiaModalVisible, setEditDpiaModalVisible] = useState(false);
    const [dpiaEditForm] = Form.useForm();
    const [updateLoading, setUpdateLoading] = useState(false);

    const [editingCommentId, setEditingCommentId] = useState<string | null>(null);
    const [editedCommentContent, setEditedCommentContent] = useState<string>('');
    const isUserMemberOfDPIA = () => {
        if (!user || !dpiaDetail || !dpiaDetail.members) return false;
        return dpiaDetail.members.some(member => member.userId === user.sub);
    };
    const formatDate = (dateString: string) => {
        if (!dateString || dateString === "0001-01-01T00:00:00") {
            return "Not updated yet";
        }
        try {
            const date = new Date(dateString);
            if (isNaN(date.getTime())) {
                return "Invalid date";
            }

            return dayjs(dateString).format('DD/MM/YYYY');
        } catch (error) {
            return "Invalid date";
        }
    };


    const getResponsibilityStatusText = (status: number | string): string => {
        if (typeof status === 'number') {
            switch (status) {
                case 0: return 'NotStarted';
                case 1: return 'Ready';
                case 2: return 'InProgress';
                case 3: return 'Completed';
                default: return 'Unknown';
            }
        }
        return status; // If it's already a string, return as is
    };

    // Maps status (either number or string) to color
    const getResponsibilityStatusColor = (status: any) => {
        const statusText = typeof status === 'number'
            ? getResponsibilityStatusText(status)
            : status;

        switch (statusText.toLowerCase()) {
            case 'completed': return 'success';
            case 'ready': return 'blue';
            case 'inprogress': return 'processing';
            case 'notstarted': return 'default';
            default: return 'default';
        }
    };
    const showFileDeleteConfirm = (fileId: string) => {
        setFileToDelete(fileId);
        setIsFileDeleteModalVisible(true);
    };

    const handleCancelFileDelete = () => {
        setIsFileDeleteModalVisible(false);
        setFileToDelete(null);
    };

    const showApprovalRequirementsModal = () => {
        Modal.info({
            title: t('cannot_approve_dpia'),
            content: (
                <div>
                    <Paragraph>
                        {t('complete_responsibilities')}
                    </Paragraph>
                    <Divider />
                    <div>
                        <Text strong>{t("currentResponsibilityStatus")}</Text>
                        <ul style={{ marginTop: 8 }}>
                            {responsibilityRows.map(resp => (
                                <li key={resp.id}>
                                    <Text>{resp.title}: </Text>
                                    <Tag color={getResponsibilityStatusColor(resp.status)}>
                                        {typeof resp.status === 'string' ? resp.status : getResponsibilityStatusText(resp.status)}
                                    </Tag>
                                </li>
                            ))}
                        </ul>
                    </div>
                </div>
            ),
        });
    };
    const handleStartDPIAClick = () => {
        const allReady = checkAllResponsibilitiesReady();

        if (!allReady) {

            // Show a notification with details
            Modal.error({
                title: t('cannot_start_dpia'),
                content: (
                    <div>
                        <Paragraph>
                            {t('all_responsibilities_be_ready')}
                        </Paragraph>
                        <Divider />
                        <div>
                            <Text strong>{t("currentResponsibilityStatus")}</Text>
                            <ul style={{ marginTop: 8 }}>
                                {responsibilityRows.map(resp => (
                                    <li key={resp.id}>
                                        <Text>{resp.title}: </Text>
                                        <Tag color={getResponsibilityStatusColor(resp.status)}>
                                            {typeof resp.status === 'string' ? resp.status : getResponsibilityStatusText(resp.status)}
                                        </Tag>
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </div>
                ),
            });
        } else {
            // Show confirmation modal instead of directly calling handleStartDPIA
            setStartDPIAConfirmVisible(true);
        }
    };
    // Function to open the edit modal
    const handleEditDpiaClick = () => {
        if (!['draft', 'started'].includes(dpiaDetail?.status.toLowerCase() || "")) {
            message.warning(t('editing_allowed_in_progress'));
            return;
        }

        // Set initial form values from current DPIA details
        dpiaEditForm.setFieldsValue({
            title: dpiaDetail?.title,
            type: dpiaDetail?.type,
            dueDate: dpiaDetail?.dueDate ? dayjs(dpiaDetail.dueDate) : null,
            description: dpiaDetail?.description || ""
        });

        setEditDpiaModalVisible(true);
    };

    // Function to update DPIA details
    const handleUpdateDpiaDetails = async (values: any) => {
        if (!id) return;

        try {
            setUpdateLoading(true);

            let formattedDueDate = null;
            if (values.dueDate) {
                formattedDueDate = values.dueDate.format('YYYY-MM-DD');
                // Alternative option: Force UTC date at noon to avoid day boundary issues
                // formattedDueDate = values.dueDate.hour(12).minute(0).second(0).millisecond(0).toISOString();
            }

            // Prepare payload for the API
            const updatePayload = {
                title: values.title,
                dueDate: formattedDueDate,
                description: values.description
            };

            // Make API call to update DPIA
            await AxiosClient.put(`/dpia/${id}`, updatePayload);

            // Refresh data to show updated information
            const dpiaData = await dpiaService.getDPIAById(id);
            setDpiaDetail(dpiaData);

            message.success(t('dpia_details_updated_successfully'));
            setEditDpiaModalVisible(false);
        } catch (error) {
            console.error('Error updating DPIA details:', error);
            message.error(t('status_code_wrong'));
        } finally {
            setUpdateLoading(false);
        }
    };


    const checkAllResponsibilitiesReady = () => {
        if (!responsibilityRows || responsibilityRows.length === 0) {
            return true; // If no responsibilities, allow starting
        }

        return responsibilityRows.every(resp => {
            const statusText = typeof resp.status === 'number'
                ? getResponsibilityStatusText(resp.status)
                : resp.status;
            return statusText.toLowerCase() === 'ready';
        });
    };
    const navigateToResponsibilityDetail = (responsibilityId: string) => {
        if (!id) return;
        navigate(`/dashboard/dpias/detail/${id}/responsibility/${responsibilityId}`);
    };

    // Helper to determine the current step number based on DPIA status - UPDATED
    const getProcessStepNumber = (status: string) => {
        switch (status.toLowerCase()) {
            case 'draft':
                return 0;
            case 'started':
                return 1;
            case 'approved':
            case 'completed':
                return 2;
            case 'rejected':
                return 1; // Show rejection at the started stage now
            default:
                return 0;
        }
    };
    const checkAllResponsibilitiesCompleted = () => {
        if (!responsibilityRows || responsibilityRows.length === 0) {
            return true; // If no responsibilities, allow approval
        }

        return responsibilityRows.every(resp => {
            const statusText = typeof resp.status === 'number'
                ? getResponsibilityStatusText(resp.status)
                : resp.status;
            return statusText.toLowerCase() === 'completed';
        });
    };

    const getStatusColor = (status: string) => {
        switch (status.toLowerCase()) {
            case 'draft':
                return 'default';
            case 'started':
            case 'inprogress':
                return 'processing';
            case 'approved':
            case 'completed':
                return 'success';
            case 'rejected':
                return 'error';
            default:
                return 'default';
        }
    };
    // Get appropriate status icon based on DPIA status
    const getStatusIcon = (status: string) => {
        switch (status.toLowerCase()) {
            case 'draft':
                return <EditOutlined />;
            case 'started':
            case 'inprogress':
                return <ClockCircleOutlined />;
            case 'approved':
            case 'completed':
                return <CheckCircleOutlined />;
            case 'rejected':
                return <CloseCircleOutlined />;
            default:
                return <EditOutlined />;
        }
    };
    const handleApproveDPIA = async () => {
        if (!id) return;

        // Check if all responsibilities are completed
        if (!checkAllResponsibilitiesCompleted()) {
            message.error(t('complete_responsibilities'));
            setApproveModalVisible(false);
            showApprovalRequirementsModal();
            return;
        }

        try {
            setApprovalLoading(true);
            await AxiosClient.post(`/dpia/${id}/approve`, {
                comment: approvalComment
            });
            message.success(t('approved') + " " + t('dpia_for_system'));
            setApproveModalVisible(false);
            const dpiaData = await dpiaService.getDPIAById(id);
            setDpiaDetail(dpiaData);
        } catch (error) {
            console.error('Error approving DPIA:', error);
            message.error(t('status_code_wrong'));
        } finally {
            setApprovalLoading(false);
        }
    };


    const handleRejectDPIA = async () => {
        if (!id || !rejectionReason.trim()) {
            message.error(t('rejection_reason_msg'));
            return;
        }

        try {
            setApprovalLoading(true);
            await AxiosClient.post(`/dpia/${id}/reject`, {
                reason: rejectionReason
            });

            message.success(t('rejected') + " " + t('dpia_for_system'));
            setRejectModalVisible(false);

            // Refresh data
            const dpiaData = await dpiaService.getDPIAById(id);
            setDpiaDetail(dpiaData);
        } catch (error) {
            console.error('Error rejecting DPIA:', error);
            message.error(t('status_code_wrong'));
        } finally {
            setApprovalLoading(false);
        }
    };

    const handleStartDPIA = async () => {
        if (!id) return;

        try {
            setLoading(true);
            await AxiosClient.post(`/dpia/${id}/start-dpia`);

            message.success(t('start_dpia_success'));

            // Refresh data
            const dpiaData = await dpiaService.getDPIAById(id);
            setDpiaDetail(dpiaData);
        } catch (error) {
            console.error('Error starting DPIA:', error);
            message.error(t('status_code_wrong'));
        } finally {
            setLoading(false);
            setStartDPIAConfirmVisible(false); // Hide modal if it was showing
        }
    };

    const transformToResponsibilityRows = (dpiaData: DPIADetail): ResponsibilityRow[] => {
        if (!dpiaData || !dpiaData.responsibilities) return [];

        return dpiaData.responsibilities.map(resp => {
            const assignedMembers = resp.members.map(member => {
                const fullMember = dpiaData.members.find(m => m.id === member.memberId);
                return {
                    id: member.memberId,
                    userId: member.userId,
                    fullName: member.fullName || fullMember?.fullName || 'Unknown',
                    email: member.email || fullMember?.email || 'Unknown',
                    completionStatus: member.completionStatus === "Completed" ? 'true' : 'false',
                    isPic: member.isPic
                };
            });
            const dueDate = resp.dueDate ? new Date(resp.dueDate) : undefined;
            return {
                key: resp.id,
                id: resp.id,
                dueDate: dueDate,
                responsibilityId: resp.responsibilityId,
                title: resp.title,
                description: resp.description,
                members: assignedMembers,
                status: resp.status
            };
        });
    };


    useEffect(() => {
        const fetchData = async () => {
            try {
                setLoading(true);
                if (id) {
                    const dpiaData = await dpiaService.getDPIAById(id);

                    setDpiaDetail(dpiaData);
                    const formattedMembers = dpiaData.members.map((member) => {
                        const memberResponsibilities = dpiaData.responsibilities
                            .filter(resp => resp.members.some(m => m.memberId === member.id))
                            .map(resp => {
                                const memberInResp = resp.members.find(m => m.memberId === member.id);
                                return {
                                    id: resp.id,
                                    responsibilityId: resp.id,
                                    title: resp.title,
                                    isCompleted: memberInResp?.completionStatus === "Completed" ? 'true' : 'false'
                                };
                            });

                        return {
                            key: member.id,
                            ...member,
                            responsibilities: memberResponsibilities
                        };
                    });

                    setTeamMembers(formattedMembers);

                    // Transform data for responsibility-based table
                    const respRows = transformToResponsibilityRows(dpiaData);
                    setResponsibilityRows(respRows);

                    // These may need to be implemented or mocked
                    const historyData = await dpiaService.getHistory(id);
                    const sortedHistoryData = [...historyData].sort((a, b) =>
                        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
                    );
                    const commentsData = await dpiaService.getComments(id);
                    setHistoryItems(sortedHistoryData);
                    setCommentItems(commentsData);
                    console.log("COMMENT DATA", commentsData);
                    if (dpiaData.documents && dpiaData.documents.length > 0) {
                        const formattedFiles = dpiaData.documents.map((doc, index) => ({
                            key: doc.id,
                            id: index + 1,
                            filename: doc.title,
                            uploader: doc.createdBy,
                            uploaded: formatDate(doc.createdAt),
                            fileUrl: doc.fileUrl
                        }));
                        setFiles(formattedFiles);
                    }

                    fetchSystemTeamMembers(dpiaData.externalSystemId);
                    fetchAllResponsibilities();
                }
            } catch (error) {
                console.error('Error fetching DPIA data:', error);
                message.error(t('status_code_wrong'));
            } finally {
                setLoading(false);
            }
        };
        fetchData();
    }, [id, t]);

    const fetchSystemTeamMembers = async (systemId: string) => {
        setLoading(true);
        try {
            const response = await AxiosClient.get(`/DPIA/members-for-dpia`);
            setAvailableMembers(response.data);
            setLoading(false);
        } catch (error) {
            console.error('Error fetching system team members:', error);
            message.error(t('status_code_wrong'));
            setLoading(false);
        }
    };

    const fetchAllResponsibilities = async () => {
        try {
            const response = await AxiosClient.get('/Responsibility');
            const options: Responsibility[] = response.data.map((res: any) => ({
                id: res.id,
                responsibilityId: res.id,
                title: res.title
            }));

            // Filter out responsibilities that are already assigned to this DPIA
            const filteredOptions = options.filter(option =>
                !responsibilityRows.some(row => row.responsibilityId === option.id)
            );

            setResponsibilityOptions(filteredOptions);
        } catch (error) {
            console.error('Error fetching all responsibilities', error);
            message.error(t('status_code_wrong'));
        }
    };

    const handleCommentSubmit = async (values: { comment: string }) => {
        if (!id || !values.comment.trim()) return;
        try {
            await AxiosClient.post(`/dpia/${id}/comments`, JSON.stringify(values.comment), {
                headers: { 'Content-Type': 'application/json' }
            });

            const updatedCommentsResponse = await dpiaService.getComments(id);
            setCommentItems(updatedCommentsResponse);
            commentForm.resetFields();
            message.success(t('status_code_success'));
        } catch (error) {
            console.error('Error adding comment:', error);
            message.error(t('status_code_wrong'));
        }
    };

    const handleEditComment = (comment: CommentItem) => {
        setEditingCommentId(comment.key);
        setEditedCommentContent(comment.content);
    };

    const handleSaveComment = async () => {
        if (!id || !editingCommentId || !editedCommentContent.trim()) return;

        try {
            // Correct format: Sending content as a property in the request body
            await AxiosClient.put(`/dpia/${id}/comments/${editingCommentId}?content=${editedCommentContent}`);

            // Refresh comments
            const updatedCommentsResponse = await dpiaService.getComments(id);
            setCommentItems(updatedCommentsResponse);

            // Reset edit state
            setEditingCommentId(null);
            setEditedCommentContent('');

            message.success(t('status_code_success'));
        } catch (error) {
            console.error('Error updating comment:', error);
            message.error(t('status_code_wrong'));
        }
    };


    const handleCancelEdit = () => {
        setEditingCommentId(null);
        setEditedCommentContent('');
    };

    const handleFileDownload = async (fileId: number) => {
        if (!id) return;
        try {
            // Show loading message
            message.loading(t('downloading_file') + "...", 0);

            // Find the file from your files array
            const file = files.find(f => f.id === fileId);
            if (!file || !file.fileUrl) {
                message.destroy();
                message.error(t('status_code_wrong'));
                return;
            }

            // Get the file URL and encode it properly
            const fileUrl = file.fileUrl;
            const fileName = file.filename;

            // Make the direct API request with the fileUrl parameter
            const response = await AxiosClient.get(
                `/File?fileName=${encodeURIComponent(fileUrl)}`,
                {
                    responseType: 'blob',
                }
            );

            // Create downloadable blob
            const blob = new Blob([response.data]);
            const url = window.URL.createObjectURL(blob);

            // Create and trigger download link
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', fileName);
            document.body.appendChild(link);
            link.click();

            // Clean up
            document.body.removeChild(link);
            window.URL.revokeObjectURL(url);

            // Show success message
            message.destroy();
            message.success(t('status_code_success'));
        } catch (error) {
            // Handle errors
            message.destroy();
            console.error("Error downloading file:", error);
            message.error(t('status_code_wrong'));
        }
    };
    const showDeleteResponsibilityConfirm = (responsibility: ResponsibilityRow, e: React.MouseEvent) => {
        e.stopPropagation(); // Prevent row click event from triggering navigation
        setResponsibilityToDelete(responsibility);
        setIsDeleteResponsibilityModalVisible(true);
    };

    const handleCancelResponsibilityDelete = () => {
        setIsDeleteResponsibilityModalVisible(false);
        setResponsibilityToDelete(null);
    };

    const handleDeleteResponsibility = async () => {
        if (!id || !responsibilityToDelete) return;

        try {
            setLoading(true);
            await AxiosClient.delete(`/dpia/${id}/${responsibilityToDelete.responsibilityId}`);

            const dpiaData = await dpiaService.getDPIAById(id);

            const respRows = transformToResponsibilityRows(dpiaData);
            setResponsibilityRows(respRows);

            const formattedMembers = dpiaData.members.map((member) => {
                const memberResponsibilities = dpiaData.responsibilities
                    .filter(resp => resp.members.some(m => m.memberId === member.id))
                    .map(resp => {
                        const memberInResp = resp.members.find(m => m.memberId === member.id);
                        return {
                            id: resp.id,
                            responsibilityId: resp.id,
                            title: resp.title,
                            isCompleted: memberInResp?.completionStatus === "Completed" ? 'true' : 'false'
                        };
                    });

                return {
                    key: member.id,
                    ...member,
                    responsibilities: memberResponsibilities
                };
            });

            setTeamMembers(formattedMembers);
            setDpiaDetail(dpiaData);

            message.success(t('status_code_success'));
        } catch (error) {
            console.error('Error deleting responsibility:', error);
            message.error(t('status_code_wrong'));
        } finally {
            setLoading(false);
            setIsDeleteResponsibilityModalVisible(false);
            setResponsibilityToDelete(null);
        }
    };

    const handleFileDelete = async () => {
        if (!id || !fileToDelete) return;
        try {
            await dpiaService.deleteFile(id, fileToDelete);
            setFiles(prev => prev.filter(file => file.key !== fileToDelete));
            message.success(t('status_code_success'));
        } catch (error) {
            console.error('Error deleting file:', error);
            message.error(t('status_code_wrong'));
        }
        setIsFileDeleteModalVisible(false);
        setFileToDelete(null);
    };
    // Function to search responsibilities via API
    const handleResponsibilitySearch = async (value: string) => {
        try {
            // If empty search, load all responsibilities
            if (!value.trim()) {
                fetchAllResponsibilities();
                return;
            }

            const response = await AxiosClient.get('/Responsibility', { params: { title: value } });
            const options: Responsibility[] = response.data.data.map((res: any) => ({
                id: res.id,
                responsibilityId: res.id,
                title: res.title
            }));

            // Filter out responsibilities that are already assigned to this DPIA
            const filteredOptions = options.filter(option =>
                !responsibilityRows.some(row => row.responsibilityId === option.id)
            );

            setResponsibilityOptions(filteredOptions);
        } catch (error) {
            console.error('Error searching responsibilities', error);
        }
    };

    const handleAssignMembersSubmit = async (values: any) => {
        try {
            if (!id) {
                message.error(t('dpia_id_missing'));
                return;
            }

            // Extract responsibility and members from the form
            const responsibility = values.responsibility;
            const members = values.members || [];
            const picMemberId = values.picMemberId;

            if (!responsibility || !responsibility.id) {
                message.error(t('please_select_responsibility'));
                return;
            }

            if (members.length === 0) {
                message.error(t('no_members_assigned_to_responsibility'));
                return;
            }

            // When adding a new responsibility, PIC is required
            if (!picMemberId) {
                message.error(t('select_pic_required'));
                return;
            }

            if (responsibilityRows.some(row => row.responsibilityId === responsibility.id)) {
                message.error(t('this_responsibility_already_added'));
                return;
            }

            // Extract all user IDs
            const userIds = members.map((member: any) => member.userId).filter(Boolean);

            // Format the date to ISO format
            const dueDate = responsibility.dueDate ? responsibility.dueDate.toISOString() : null;

            // Create the API payload in the expected format
            const apiPayload = [
                {
                    responsibilityId: responsibility.id,
                    dueDate: dueDate,
                    userId: userIds,
                    pic: picMemberId
                }
            ];

            // Send to API
            await AxiosClient.put(`/dpia/${id}/update-members-responsibilities`, apiPayload);

            // Refresh data
            setLoading(true);
            const dpiaData = await dpiaService.getDPIAById(id);

            // Update state with fresh data
            const formattedMembers = dpiaData.members.map((member) => {
                const memberResponsibilities = dpiaData.responsibilities
                    .filter(resp => resp.members.some(m => m.memberId === member.id))
                    .map(resp => {
                        const memberInResp = resp.members.find(m => m.memberId === member.id);
                        return {
                            id: resp.id,
                            responsibilityId: resp.responsibilityId,
                            title: resp.title,
                            isCompleted: memberInResp?.completionStatus === "Completed" ? 'true' : 'false'
                        };
                    });

                return {
                    key: member.id,
                    ...member,
                    responsibilities: memberResponsibilities
                };
            });

            // Transform data for responsibility-based table
            const respRows = transformToResponsibilityRows(dpiaData);

            setTeamMembers(formattedMembers);
            setResponsibilityRows(respRows);
            setDpiaDetail(dpiaData);

            message.success(t('status_code_success'));
            setAddModalVisible(false);
            addResponsibilityForm.resetFields();
        } catch (error) {
            console.error('Error adding team members and responsibility:', error);
            message.error(t('status_code_wrong'));
        } finally {
            setLoading(false);
        }
    };

    const openAddModal = () => {
        // Provide a default value when using optional chaining
        if (!['draft', 'started'].includes(dpiaDetail?.status?.toLowerCase() || '')) {
            message.warning(t('adding_responsibilities_allowed_draft_or_started'));
            return;
        }
        setCurrentMember(null);
        addResponsibilityForm.resetFields();
        fetchAllResponsibilities();
        setAddModalVisible(true);
    };

    const handleEditResponsibility = (responsibility: ResponsibilityRow) => {
        if (!['draft', 'started'].includes(dpiaDetail?.status.toLowerCase() || "")) {
            message.warning(t('editing_allowed_in_progress'));
            return;
        }
        setCurrentResponsibility(responsibility);
        setEditResponsibilityVisible(true);
    };

    useEffect(() => {
        if (currentResponsibility && editResponsibilityVisible) {
            const memberValues = currentResponsibility.members.map(member => {
                const fullMember = availableMembers.find(m => m.userId === member.userId);

                return {
                    label: fullMember ? `${fullMember.email} (${fullMember.fullName})` : member.email,
                    value: member.userId,
                    isPic: !!member.isPic
                };
            });

            // Find the PIC member
            const picMember = currentResponsibility.members.find(m => m.isPic);
            const picMemberId = picMember ? picMember.userId : null;

            // Convert the date to a Day.js object if it exists
            const dueDateValue = currentResponsibility.dueDate
                ? dayjs(new Date(currentResponsibility.dueDate))
                : null;

            responsibilityForm.setFieldsValue({
                members: memberValues,
                picMemberId: picMemberId,
                dueDate: dueDateValue // Add the dueDate to the form values
            });
        } else {
            responsibilityForm.resetFields();
        }
    }, [currentResponsibility, editResponsibilityVisible, availableMembers]);

    const handleResponsibilityEditSubmit = async () => {
        try {
            if (!id || !currentResponsibility) return;
            const values = responsibilityForm.getFieldsValue();

            // Explicitly handle PIC - can be null when editing
            const picMemberId = values.picMemberId === undefined ? null : values.picMemberId;

            // Extract user IDs from the form values
            const selectedUserIds = Array.isArray(values.members)
                ? values.members.map((item: { value: any; }) => typeof item === 'object' ? item.value : item)
                : [];

            let formattedDueDate;

            // Get the original date from the responsibility data
            const originalDate = currentResponsibility.dueDate
                ? dayjs(new Date(currentResponsibility.dueDate)).format('YYYY-MM-DD')
                : null;

            if (values.dueDate) {
                const updatedDueDate = values.dueDate; // Day.js object
                const formDate = updatedDueDate.format('YYYY-MM-DD');
                formattedDueDate = (formDate !== originalDate) ? formDate : originalDate;
            }
            else {
                // No date in form, keep the original date
                formattedDueDate = originalDate;
            }

            const payload = [
                {
                    responsibilityId: currentResponsibility.responsibilityId,
                    dueDate: formattedDueDate,
                    userId: selectedUserIds,
                    pic: picMemberId  // This will now be explicitly null if not selected
                }
            ];

            await AxiosClient.put(`/dpia/${id}/update-members-responsibilities`, payload);
            const dpiaData = await dpiaService.getDPIAById(id);
            const respRows = transformToResponsibilityRows(dpiaData);
            setResponsibilityRows(respRows);

            // Update team members as well
            const formattedMembers = dpiaData.members.map((member) => {
                const memberResponsibilities = dpiaData.responsibilities
                    .filter(resp => resp.members.some(m => m.memberId === member.id))
                    .map(resp => {
                        const memberInResp = resp.members.find(m => m.memberId === member.id);
                        return {
                            id: resp.id,
                            responsibilityId: resp.responsibilityId,
                            title: resp.title,
                            isCompleted: memberInResp?.completionStatus === "Completed" ? 'true' : 'false'
                        };
                    });

                return {
                    key: member.id,
                    ...member,
                    responsibilities: memberResponsibilities
                };
            });

            setTeamMembers(formattedMembers);
            setDpiaDetail(dpiaData);
            message.success(t('status_code_success'));
            setEditResponsibilityVisible(false);
            setCurrentResponsibility(null);
        } catch (error) {
            console.error('Error updating responsibility:', error);
            message.error(t('status_code_wrong'));
        }
    };


    const responsibilityColumns: ColumnsType<ResponsibilityRow> = [
        {
            title: t('responsibilities'),
            dataIndex: 'title',
            key: 'title',
            width: '20%',
            ellipsis: true,
        },
        {
            title: "PIC",
            key: 'pic',
            width: '15%',
            render: (_, record) => {
                const picMember = record.members?.find(member => member.isPic);
                return picMember ? (
                    <div style={{
                        height: '32px',
                        display: 'flex',
                        alignItems: 'center'
                    }}>
                        <Tooltip title={picMember.email}>
                            <span style={{
                                maxWidth: '100%',
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                                whiteSpace: 'nowrap',
                                display: 'inline-block'
                            }}>
                                {picMember.email}
                            </span>
                        </Tooltip>
                    </div>
                ) : (
                    <Text type="secondary">{t('no_pic_assigned')}</Text>
                );
            }
        },
        {
            title: t('team_members'),
            key: 'team_members',
            width: '15%',
            render: (_, record) => {
                const regularMembers = record.members?.filter(member => !member.isPic);
                return (
                    <div>
                        {regularMembers && regularMembers.length > 0 ?
                            regularMembers.map((member: { id: string; email: string }) => (
                                <div key={member.id} style={{
                                    marginBottom: 8,
                                    height: '32px',
                                    display: 'flex',
                                    alignItems: 'center'
                                }}>
                                    <Tooltip title={member.email}>
                                        <span style={{
                                            maxWidth: '100%',
                                            overflow: 'hidden',
                                            textOverflow: 'ellipsis',
                                            whiteSpace: 'nowrap',
                                            display: 'inline-block'
                                        }}>
                                            {member.email}
                                        </span>
                                    </Tooltip>
                                </div>
                            )) :
                            <Text type="secondary">No members</Text>
                        }
                    </div>
                );
            }
        },
        {
            title: t('due_date'),
            dataIndex: 'dueDate',
            key: 'dueDate',
            width: '12%',
            render: (dueDate) => {
                if (!dueDate) return 'Not set';
                return dayjs(new Date(dueDate)).format('MMM DD, YYYY');
            }
        },
        {
            title: t('overall_status'),
            dataIndex: 'status',
            key: 'status',
            width: '13%',
            render: (status) => (
                <Tag color={getResponsibilityStatusColor(status)}>
                    {getResponsibilityStatusText(status)}
                </Tag>
            )
        },
        {
            title: t('actions'),
            key: 'actions',
            width: '10%',
            render: (_, record) => (
                <Space>
                    {['draft', 'started'].includes(dpiaDetail?.status.toLowerCase() || "") ? (
                        <>
                            <FeatureGuard requiredFeature='/api/DPIA/{id}/update-members-responsibilities_PUT'>
                                <Button
                                    type="link"
                                    icon={<EditOutlined />}
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        handleEditResponsibility(record);
                                    }}
                                />
                            </FeatureGuard>

                            {/* Delete button only shown for draft status */}
                            {dpiaDetail?.status.toLowerCase() === 'draft' && (
                                <FeatureGuard requiredFeature='/api/Responsibility/{id}_DELETE'>
                                    <Button
                                        type="link"
                                        danger
                                        icon={<DeleteOutlined />}
                                        onClick={(e) => showDeleteResponsibilityConfirm(record, e)}
                                    />
                                </FeatureGuard>
                            )}
                        </>
                    ) : (
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                            {t('editing_locked')}
                        </Text>
                    )}
                </Space>
            )

        }
    ];

    const fileColumns: ColumnsType<FileItem> = [
        {
            title: 'No.',
            dataIndex: 'id',
            key: 'id',
            width: '10%'
        },
        {
            title: t('filename'),
            dataIndex: 'filename',
            key: 'filename',
            width: '35%'
        },
        {
            title: t('Uploader'),
            dataIndex: 'uploader',
            key: 'uploader',
            width: '30%'
        },
        {
            title: t('Uploaded'),
            dataIndex: 'uploaded',
            key: 'uploaded',
            width: '25%'
        },
        {
            title: t('actions'),
            key: 'actions',
            render: (_, record) => (
                <Space>
                    <Button
                        type="text"
                        icon={<DownloadOutlined />}
                        onClick={() => handleFileDownload(record.id)}
                    />
                    <Button
                        type="text"
                        danger
                        icon={<DeleteOutlined />}
                        onClick={() => showFileDeleteConfirm(record.key)}
                    />
                </Space>
            )
        }
    ];
    const handleBack = () => {
        navigate('/dashboard/dpias');
    };
    const uploadProps = {
        name: 'file',
        multiple: false,
        showUploadList: false,
        fileList,
        beforeUpload: (file: any) => {
            const isLt25M = file.size / 1024 / 1024 < 25;
            if (!isLt25M) {
                message.error(t('fileSizeError').replace('{{size}}', '25MB'));
                return Upload.LIST_IGNORE;
            }
            const allowedTypes = ['.xlsx', '.docx', '.pdf', '.png', '.jpeg', '.jpg'];
            const fileName = file.name || '';
            const fileExtension = '.' + fileName.split('.').pop().toLowerCase();
            const isAllowedType = allowedTypes.includes(fileExtension);

            if (!isAllowedType) {
                message.error(t('fileTypeError').replace('{{types}}', allowedTypes.join(', ')));
                return Upload.LIST_IGNORE;
            }

            return true;
        },
        customRequest: ({ file, onSuccess, onError, onProgress }: any) => {
            const formData = new FormData();
            formData.append('file', file);
            AxiosClient.post(`/dpia/${id}/upload-document`, formData, {
                headers: {
                    'Content-Type': 'multipart/form-data'
                },
                onUploadProgress: (event) => {
                    // Type-safe approach for TypeScript
                    const total = event.total ?? 0;
                    const loaded = event.loaded ?? 0;
                    const percent = total > 0 ? Math.floor((loaded / total) * 100) : 0;
                    onProgress({ percent });
                }
            })
                .then(response => {
                    onSuccess(response, file);
                    if (id) {
                        dpiaService.getDPIAById(id).then(dpiaData => {
                            setDpiaDetail(dpiaData);

                            if (dpiaData.documents && dpiaData.documents.length > 0) {
                                const formattedFiles = dpiaData.documents.map((doc, index) => ({
                                    key: doc.id,
                                    id: index + 1,
                                    filename: doc.title,
                                    uploader: doc.createdBy,
                                    uploaded: formatDate(doc.createdAt),
                                    fileUrl: doc.fileUrl
                                }));
                                setFiles(formattedFiles);
                            }
                        });
                    }

                    // Show success message
                    message.success(`${file.name} uploaded successfully`);
                })
                .catch(error => {
                    onError(error);

                    if (error.response && error.response.data) {
                        message.error(`Upload failed: ${error.response.data}`);
                    } else {
                        message.error(`${file.name} upload failed`);
                    }
                });

            return {
                abort: () => console.log('Upload aborted')
            };
        },
        onDrop(e: any) {
            console.log('Dropped files', e.dataTransfer.files);
        }
    };


    if (loading) {
        return <div style={{ padding: 24, textAlign: 'center' }}>Loading...</div>;
    }
    if (!dpiaDetail) {
        return <div style={{ padding: 24, textAlign: 'center' }}>DPIA not found</div>;
    }

    return (
        <Layout
            className="site-layout"
            style={{
                width: '99.5%',
                transition: 'all 0.3s'
            }}
        >
            <Layout.Content
                style={{
                    padding: '24px',
                    background: '#f0f2f5',
                    overflow: 'auto'
                }}
            >
                <div style={{ marginBottom: '24px' }}>
                    <Breadcrumb>
                        <Breadcrumb.Item>
                            <Button
                                type="link"
                                onClick={handleBack}
                                style={{
                                    padding: 0,
                                    display: 'flex',
                                    alignItems: 'center'
                                }}
                            >
                                <ArrowLeftOutlined />
                                {t('back')}
                            </Button>
                        </Breadcrumb.Item>
                    </Breadcrumb>
                    <Row justify="space-between" align="middle" style={{ marginTop: 16 }}>
                        <Col>
                            <Title level={4}>{t('view_dpia_details')}</Title>
                        </Col>
                        <Col>
                            <Space>
                                {dpiaDetail.status.toLowerCase() === 'draft' && (
                                    <FeatureGuard requiredFeature='/api/DPIA/{id}/start-dpia_POST'>
                                        <Button
                                            type="primary"
                                            onClick={handleStartDPIAClick}
                                            style={{ backgroundColor: '#1890ff', borderColor: '#1890ff' }}
                                        >
                                            {t('start_dpia')}
                                        </Button>
                                    </FeatureGuard>
                                )}

                                {dpiaDetail.status.toLowerCase() === 'started' && (
                                    <>
                                        {checkAllResponsibilitiesCompleted() ? (
                                            <FeatureGuard requiredFeature='/api/DPIA/{id}/approve_POST'>
                                                <Button
                                                    type="primary"
                                                    onClick={() => setApproveModalVisible(true)}
                                                    style={{ backgroundColor: '#52c41a', borderColor: '#52c41a' }}
                                                >
                                                    {t('approve_dpia')}
                                                </Button>
                                            </FeatureGuard>
                                        ) : (
                                            <Tooltip title={t('all_responsibilities_must_be_completed')}>
                                                <Button
                                                    type="primary"
                                                    onClick={showApprovalRequirementsModal}
                                                    style={{ backgroundColor: '#52c41a', borderColor: '#52c41a', opacity: 0.5 }}
                                                    disabled
                                                >
                                                    {t('approve_dpia')}
                                                </Button>
                                            </Tooltip>
                                        )}
                                        <FeatureGuard requiredFeature='/api/DPIA/{id}/reject_POST'>
                                            <Button
                                                danger
                                                onClick={() => setRejectModalVisible(true)}
                                            >
                                                {t('reject_dpia')}
                                            </Button>
                                        </FeatureGuard>
                                    </>
                                )}

                                {dpiaDetail.status.toLowerCase() === 'rejected' && (
                                    <Tag color="error">{t('dpia_rejected_process_terminated')}</Tag>
                                )}
                            </Space>
                        </Col>
                    </Row>
                </div>

                {/* Main Content */}
                <Space direction="vertical" size="large" style={{ width: '100%' }}>
                    <Card bordered={false} style={{ marginBottom: '16px' }}>
                        <Row>
                            <Col span={24}>
                                <div style={{ padding: '8px 0', marginBottom: '8px' }}>
                                    <Text strong>{t('dpia_workflow_status')}</Text>
                                </div>
                                <Steps
                                    current={getProcessStepNumber(dpiaDetail.status)}
                                    status={dpiaDetail.status.toLowerCase() === 'rejected' ? 'error' : 'process'}
                                    size="small"
                                >
                                    <Step title={t('draft')} description={t("dpiaStage1Description")} />
                                    <Step title={t('started')} description={t("dpiaStage2Description")} />
                                    <Step title={t('approved')} description={t("dpiaStage3Description")} />
                                </Steps>
                            </Col>
                        </Row>
                    </Card>
                    <Card
                        bordered={false}
                        title={
                            <Row justify="space-between" align="middle">
                                <Col>{t('general_informationI')}</Col>
                                <Col>
                                    <FeatureGuard requiredFeature='/api/DPIA/{id}_PUT'>
                                        {['draft'].includes(dpiaDetail?.status.toLowerCase() || "") && (
                                            <Button
                                                type="primary"
                                                icon={<EditOutlined />}
                                                onClick={handleEditDpiaClick}
                                                size="small"
                                            >
                                                {t('edit_details')}
                                            </Button>
                                        )}
                                    </FeatureGuard>
                                </Col>
                            </Row>
                        }
                        headStyle={{
                            backgroundColor: '#FF9F43',
                            color: 'white',
                            borderTopLeftRadius: '8px',
                            borderTopRightRadius: '8px'
                        }}
                    >
                        <Row gutter={24}>
                            <Col span={12}>
                                <Descriptions column={1} bordered size="small" labelStyle={{ fontWeight: 'bold' }}>
                                    <Descriptions.Item label={t('title') + ":"}>
                                        {dpiaDetail.title}
                                    </Descriptions.Item>
                                    <Descriptions.Item label={t('dpia_for_system') + ":"}>
                                        {dpiaDetail.externalSystemName}
                                    </Descriptions.Item>
                                    <Descriptions.Item label={t('type_of_dpia') + ":"}>
                                        {dpiaDetail.type}
                                    </Descriptions.Item>
                                    <Descriptions.Item label={t('due_date') + ":"}>
                                        {formatDate(dpiaDetail.dueDate)}
                                    </Descriptions.Item>
                                    <Descriptions.Item label={t('status') + ":"}>
                                        <Tag
                                            color={getStatusColor(dpiaDetail.status)}
                                            icon={getStatusIcon(dpiaDetail.status)}
                                            style={{ padding: '4px 8px' }}
                                        >
                                            {dpiaDetail.status}
                                        </Tag>
                                    </Descriptions.Item>
                                </Descriptions>
                            </Col>
                            <Col span={12}>
                                <Descriptions column={1} bordered size="small" labelStyle={{ fontWeight: 'bold' }}>
                                    <Descriptions.Item label={t('created_by') + ":"}>
                                        {dpiaDetail.createdBy}
                                    </Descriptions.Item>
                                    <Descriptions.Item label={t('created_at') + ":"}>
                                        {formatDate(dpiaDetail.createdAt)}
                                    </Descriptions.Item>
                                    <Descriptions.Item label={t('last_modify_by') + ":"}>
                                        {dpiaDetail.updatedBy}
                                    </Descriptions.Item>
                                    <Descriptions.Item label={t('last_modify_at') + ":"}>
                                        {formatDate(dpiaDetail.lastModifiedAt)}
                                    </Descriptions.Item>
                                    <Descriptions.Item label={t('description') + ":"}>
                                        {dpiaDetail.description}
                                    </Descriptions.Item>
                                </Descriptions>
                            </Col>
                        </Row>
                    </Card>

                    {/* Team & Responsibilities Section */}
                    <Card
                        title={t('responsibilities_assignments')}
                        extra={
                            ['draft'].includes(dpiaDetail.status.toLowerCase()) ? (
                                <FeatureGuard requiredFeature='/api/DPIA/{id}/update-members-responsibilities_PUT'>
                                    <Button type="primary" onClick={openAddModal}>
                                        <PlusCircleFilled />{t('add_responsibility')}
                                    </Button>
                                </FeatureGuard>
                            ) : null
                        }
                        headStyle={{
                            backgroundColor: '#FF9F43',
                            color: 'white',
                            borderTopLeftRadius: '8px',
                            borderTopRightRadius: '8px'
                        }}
                        bodyStyle={{ padding: '12px', overflowX: 'auto' }}
                    >
                        <Table
                            columns={responsibilityColumns}
                            dataSource={responsibilityRows}
                            pagination={false}
                            size="small"
                            rowKey="key"
                            bordered
                            tableLayout="fixed"
                            scroll={{ x: 800, y: 300 }}
                            onRow={(record) => ({
                                onClick: () => {
                                    if (['approved', 'rejected'].includes(dpiaDetail?.status.toLowerCase() || "")) {
                                        message.info(`${t('view_responsibility_details')} "${record.title}"`);
                                    }
                                    navigateToResponsibilityDetail(record.responsibilityId);
                                },
                                style: { cursor: 'pointer' }
                            })}
                            rowClassName={(record, index) => {
                                let className = index % 2 === 0 ? '' : 'ant-table-row-light';
                                if (!['draft', 'started'].includes(dpiaDetail?.status.toLowerCase())) {
                                    className += ' locked-row';
                                }
                                className += ' clickable-row';
                                return className;
                            }}
                            footer={() => (
                                !['draft', 'started'].includes(dpiaDetail?.status.toLowerCase()) && (
                                    <div style={{ fontSize: '12px', color: '#999', textAlign: 'center', padding: '8px' }}>
                                        {t('editing_locked')} - DPIA is currently in {dpiaDetail.status} status
                                    </div>
                                )
                            )}
                        />
                        <style>
                            {`
                            .locked-row {
                                opacity: 0.8;
                                background-color: #fafafa !important;
                            }
                            .clickable-row:hover {
                                background-color: #e6f7ff !important;
                                transition: background-color 0.3s;
                            }
                            .clickable-row:hover td {
                                background-color: transparent !important;
                            }
                            `}
                        </style>
                    </Card>

                    {/* Files & Documents Section */}
                    <Row gutter={24}>
                        <Col span={24}>
                            <Card
                                title={t('documentation')}
                                headStyle={{
                                    backgroundColor: '#FF9F43',
                                    color: 'white',
                                    borderTopLeftRadius: '8px',
                                    borderTopRightRadius: '8px'
                                }}
                                bodyStyle={{ padding: '12px' }}
                            >
                                <Dragger {...uploadProps} style={{ height: '60px' }}>
                                    <div style={{
                                        display: 'flex',
                                        alignItems: 'center',
                                        justifyContent: 'center',
                                        padding: '5px 0'
                                    }}>
                                        <p className="ant-upload-drag-icon" style={{ margin: '0 8px 0 0', fontSize: '18px' }}>
                                            <UploadOutlined />
                                        </p>
                                        <div>
                                            <p className="ant-upload-text" style={{
                                                margin: 0,
                                                fontSize: '11px',
                                                lineHeight: '1.2'
                                            }}>
                                                {t('drag_drop_upload')}
                                            </p>
                                            <p className="ant-upload-hint" style={{
                                                fontSize: '10px',
                                                color: '#888',
                                                margin: '2px 0 0 0',
                                                lineHeight: '1'
                                            }}>
                                                {t('fileRestrictions')}
                                            </p>
                                        </div>
                                    </div>
                                </Dragger>
                                <div style={{ maxHeight: '200px', overflowY: 'auto' }}>
                                    <Table
                                        columns={fileColumns}
                                        dataSource={files}
                                        pagination={false}
                                        size="small"
                                        rowKey="id"
                                        bordered
                                        style={{ marginTop: '16px' }}
                                        rowClassName={(record, index) => index % 2 === 0 ? '' : 'ant-table-row-light'}
                                    />
                                </div>
                            </Card>
                        </Col>

                    </Row>

                    {/* Activity Tracking Section */}
                    <Card bordered={false}>
                        <Tabs defaultActiveKey="1" type="card">
                            <TabPane tab={t('comments')} key="1">
                                {/* Only show comment form to DPIA members */}
                                {isUserMemberOfDPIA() ? (
                                    <>
                                        <Form form={commentForm} onFinish={handleCommentSubmit}>
                                            <Row gutter={8} align="top">
                                                <Col flex="auto">
                                                    <Form.Item name="comment" style={{ marginBottom: 0 }}>
                                                        <TextArea rows={2} placeholder={t('add_your_comment')} />
                                                    </Form.Item>
                                                </Col>
                                                <Col>
                                                    <Form.Item style={{ marginBottom: 0 }}>
                                                        <Button htmlType="submit" type="primary">{t('submit')}</Button>
                                                    </Form.Item>
                                                </Col>
                                            </Row>
                                        </Form>
                                        <Divider />
                                    </>
                                ) : (
                                    <Alert
                                        message={t('comments_restricted')}
                                        description={t('only_dpia_members_can_comment')}
                                        type="info"
                                        showIcon
                                        style={{ marginBottom: 16 }}
                                    />
                                )}

                                <div style={{ maxHeight: '300px', overflowY: 'auto', padding: '0 5px' }}>
                                    {commentItems.map(comment => (
                                        <div key={comment.key} style={{ marginBottom: 24 }}>
                                            <Row align="middle">
                                                <Avatar icon={<UserOutlined />} style={{ marginRight: 8 }} />
                                                <Text strong>{comment.user}</Text>
                                                <Text type="secondary" style={{ marginLeft: 8 }}>{comment.date}</Text>
                                            </Row>

                                            {editingCommentId === comment.key ? (
                                                // Edit mode
                                                <div style={{ marginLeft: 32, marginTop: 8 }}>
                                                    <Form.Item style={{ marginBottom: 8 }}>
                                                        <TextArea
                                                            rows={3}
                                                            value={editedCommentContent}
                                                            onChange={(e) => setEditedCommentContent(e.target.value)}
                                                        />
                                                    </Form.Item>
                                                    <Row justify="end">
                                                        <Space>
                                                            <Button
                                                                size="small"
                                                                onClick={handleCancelEdit}
                                                            >
                                                                {t('cancel')}
                                                            </Button>
                                                            <Button
                                                                type="primary"
                                                                size="small"
                                                                onClick={handleSaveComment}
                                                            >
                                                                {t('save')}
                                                            </Button>
                                                        </Space>
                                                    </Row>
                                                </div>
                                            ) : (
                                                // View mode
                                                <>
                                                    <Paragraph style={{ marginLeft: 32, marginTop: 8 }}>
                                                        {comment.content}
                                                    </Paragraph>

                                                    {user && user.sub === comment.createdById && (
                                                        <Row justify="end">
                                                            <Button
                                                                type="text"
                                                                icon={<EditOutlined />}
                                                                size="small"
                                                                onClick={() => handleEditComment(comment)}
                                                            >
                                                                Edit
                                                            </Button>
                                                        </Row>
                                                    )}
                                                </>
                                            )}
                                        </div>
                                    ))}
                                </div>
                            </TabPane>
                            <TabPane tab={t('history')} key="2">
                                <div style={{ padding: 16, maxHeight: '300px', overflowY: 'auto' }}>
                                    {historyItems.map((item, index) => (
                                        <div key={item.key} style={{ position: 'relative' }}>
                                            <div style={{
                                                position: 'absolute',
                                                width: 24,
                                                height: 24,
                                                background: '#f0f2f5',
                                                borderRadius: '50%',
                                                display: 'flex',
                                                justifyContent: 'center',
                                                alignItems: 'center',
                                                zIndex: 1
                                            }}>
                                                <div style={{
                                                    width: 10,
                                                    height: 10,
                                                    background: '#ccc',
                                                    borderRadius: '50%'
                                                }} />
                                            </div>
                                            {index < historyItems.length - 1 && (
                                                <div style={{
                                                    position: 'absolute',
                                                    left: 12,
                                                    top: 24,
                                                    bottom: 0,
                                                    width: 1,
                                                    background: '#e8e8e8',
                                                    height: 60,
                                                    zIndex: 0
                                                }} />
                                            )}
                                            <div style={{ marginLeft: 36, marginBottom: 24 }}>
                                                <Text strong>{item.createdBy.fullName}</Text>
                                                <Text type="secondary"> at {formatDate(item.createdAt)}</Text>
                                                <br />
                                                <Text>{item.text}</Text>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            </TabPane>
                        </Tabs>
                    </Card>
                </Space>
            </Layout.Content>

            {/* Modals */}
            <Modal
                title={t('add_responsibility')}
                visible={addModalVisible}
                onCancel={() => {
                    setAddModalVisible(false);
                    addResponsibilityForm.resetFields();
                }}
                footer={[
                    <Button key="cancel" onClick={() => {
                        setAddModalVisible(false);
                        addResponsibilityForm.resetFields();
                    }}>
                        {t('cancel')}
                    </Button>,
                    <Button key="submit" type="primary" onClick={() => addResponsibilityForm.submit()}>
                        {t('submit')}
                    </Button>
                ]}
                width={700}
            >
                <Form
                    form={addResponsibilityForm}
                    layout="vertical"
                    onFinish={handleAssignMembersSubmit}
                >
                    <Card title={t('responsibilities')} style={{ marginBottom: 16 }}>
                        <Row gutter={16}>
                            <Col span={12}>
                                <Form.Item
                                    name={['responsibility', 'id']}
                                    label={t('select_responsibility')}
                                    rules={[{ required: true, message: t('please_select_responsibility') }]}
                                >
                                    <Select
                                        showSearch
                                        placeholder={t('search_responsibility')}
                                        optionFilterProp="children"
                                        onSearch={handleResponsibilitySearch}
                                        filterOption={false}
                                    >
                                        {responsibilityOptions.length === 0 ? (
                                            <Option disabled value="">{t('all_available_responsibilities_have_been_assigned')}</Option>
                                        ) : (
                                            responsibilityOptions.map(option => (
                                                <Option key={option.id} value={option.id}>
                                                    {option.title}
                                                </Option>
                                            ))
                                        )}

                                    </Select>
                                </Form.Item>
                            </Col>
                            <Col span={12}>
                                <Form.Item
                                    name={['responsibility', 'dueDate']}
                                    label={t('due_date')}
                                    rules={[{ required: true, message: t('please_set_deadline') }]}
                                >
                                    <DatePicker
                                        style={{ width: '100%' }}
                                        disabledDate={(current) => {
                                            // No need to check if current exists - it's always provided by the DatePicker
                                            // Get tomorrow's date (minimum allowed date)
                                            const tomorrow = dayjs().add(1, 'day').startOf('day');

                                            // Get DPIA due date (maximum allowed date)
                                            const dpiaDueDate = dpiaDetail?.dueDate ? dayjs(dpiaDetail.dueDate) : null;

                                            // Check if date is before tomorrow
                                            const isBeforeTomorrow = current.isBefore(tomorrow);

                                            // Check if date is after DPIA due date (only if dpiaDueDate exists)
                                            const isAfterDueDate = dpiaDueDate && dpiaDueDate.isValid()
                                                ? current.isAfter(dpiaDueDate)
                                                : false;

                                            // Return true if the date should be disabled (either before tomorrow or after due date)
                                            return isBeforeTomorrow || isAfterDueDate;
                                        }}
                                    />

                                </Form.Item>
                            </Col>
                        </Row>
                    </Card>
                    <Card title={t('select_team_members')} style={{ marginBottom: 16 }}>
                        <Form.List name="members">
                            {(fields, { add, remove }) => {
                                // Get all currently selected user IDs from the form
                                const selectedUserIds = fields.map((field, index) => {
                                    const userId = addResponsibilityForm.getFieldValue(['members', field.name, 'userId']);
                                    return userId;
                                }).filter(id => id); // Filter out undefined values

                                // Filter available members to only show unselected ones
                                const unselectedMembers = availableMembers.filter(member =>
                                    !selectedUserIds.includes(member.id)
                                );

                                return (
                                    <>
                                        {fields.length === 0 ? (
                                            <Empty
                                                description={t('no_members_assigned_to_responsibility')}
                                                image={Empty.PRESENTED_IMAGE_SIMPLE}
                                                style={{ margin: '10px 0' }}
                                            />
                                        ) : (
                                            fields.map((field, index) => (
                                                <Row key={field.key} gutter={8} align="middle" style={{ marginBottom: 8 }}>
                                                    <Col flex="1">
                                                        <Form.Item
                                                            {...field}
                                                            name={[field.name, 'userId']}
                                                            rules={[{ required: true, message: 'Please select a user' }]}
                                                            style={{ marginBottom: 0 }}
                                                        >
                                                            <Select
                                                                showSearch
                                                                placeholder={t('enter_member_email')}
                                                                optionFilterProp="children"
                                                                loading={loading}
                                                                onChange={() => {
                                                                    addResponsibilityForm.setFieldsValue({
                                                                        _refresh: Date.now()
                                                                    });
                                                                }}
                                                            >
                                                                {availableMembers
                                                                    .filter(member => {
                                                                        const currentValue = addResponsibilityForm.getFieldValue(['members', field.name, 'userId']);
                                                                        return member.id === currentValue || !selectedUserIds.includes(member.id);
                                                                    })
                                                                    .map(member => (
                                                                        <Option key={member.id} value={member.id}>
                                                                            {member.email} ({member.fullName})
                                                                        </Option>
                                                                    ))}
                                                            </Select>
                                                        </Form.Item>
                                                    </Col>
                                                    <Col>
                                                        <Button
                                                            type="text"
                                                            danger
                                                            onClick={() => remove(field.name)}
                                                            icon={<DeleteOutlined />}
                                                        />
                                                    </Col>
                                                </Row>
                                            ))
                                        )}

                                        <Form.Item style={{ marginTop: 8 }}>
                                            <Button
                                                type="dashed"
                                                onClick={() => add()}
                                                block
                                                icon={<PlusCircleFilled />}
                                                disabled={unselectedMembers.length === 0}
                                            >
                                                {t('add')} {t('member')}
                                            </Button>
                                        </Form.Item>

                                        {/* Hidden field to force re-render */}
                                        <Form.Item name="_refresh" hidden>
                                            <Input />
                                        </Form.Item>
                                    </>
                                );
                            }}
                        </Form.List>

                        <Form.Item
                            label="Person In Charge (PIC)"
                            tooltip={t('select_one_member_be_pic')}
                            rules={[{ required: true, message: t('select_pic_required') }]}
                            shouldUpdate={(prevValues, currentValues) => {
                                // Re-render this Form.Item when members change
                                return JSON.stringify(prevValues.members) !== JSON.stringify(currentValues.members);
                            }}
                        >
                            {({ getFieldValue, setFieldsValue }) => {
                                // Get members from the form context
                                const members = getFieldValue('members') || [];

                                return (
                                    <Form.Item
                                        name="picMemberId"
                                        noStyle
                                        rules={[{ required: true, message: 'Please select a Person In Charge (PIC)' }]}
                                    >
                                        <Select
                                            placeholder={t('select_pic_required')}
                                            style={{ width: '100%' }}
                                            notFoundContent={
                                                <Empty
                                                    image={Empty.PRESENTED_IMAGE_SIMPLE}
                                                    description={t('no_members_assigned_to_responsibility')}
                                                />
                                            }
                                        >
                                            {members.map((member: any, index: any) => {
                                                const userId = member?.userId;
                                                if (!userId) return null;

                                                const memberData = availableMembers.find(m => m.id === userId);
                                                if (!memberData) return null;

                                                return (
                                                    <Option key={userId} value={userId}>
                                                        <Space>
                                                            <Avatar size="small" icon={<UserOutlined />} />
                                                            {memberData.email} ({memberData.fullName})
                                                        </Space>
                                                    </Option>
                                                );
                                            }).filter(Boolean)}
                                        </Select>
                                    </Form.Item>
                                );
                            }}
                        </Form.Item>

                    </Card>
                </Form>
            </Modal>
            {/* Responsibility Edit Modal */}
            <Modal
                title={currentResponsibility ? `Edit "${currentResponsibility.title}"` : "Edit Responsibility"}
                open={editResponsibilityVisible}
                onCancel={() => {
                    setEditResponsibilityVisible(false);
                    responsibilityForm.resetFields();
                }}
                onOk={() => responsibilityForm.submit()}
                width={600}
            >
                <Form
                    form={responsibilityForm}
                    layout="vertical"
                    onFinish={handleResponsibilityEditSubmit}
                >
                    <Form.Item
                        name="dueDate"
                        label={t('due_date')}
                        rules={[{ required: false, message: 'Please select a due date' }]}

                    >
                        <DatePicker
                            style={{ width: '100%' }}
                            placeholder="Select deadline for this responsibility"
                            format="YYYY-MM-DD"
                            showTime={false}
                            disabledDate={(current) => {
                                // No need to check if current exists - it's always provided
                                // Tomorrow as minimum date
                                const tomorrow = dayjs().add(1, 'day').startOf('day');
                                // DPIA due date as maximum date
                                const dpiaDueDate = dpiaDetail.dueDate ? dayjs(dpiaDetail.dueDate) : null;
                                // Check if date is before tomorrow
                                const isBeforeTomorrow = current.isBefore(tomorrow);

                                // Check if date is after DPIA due date (only if dpiaDueDate exists and is valid)
                                const isAfterDueDate = dpiaDueDate && dpiaDueDate.isValid()
                                    ? current.isAfter(dpiaDueDate)
                                    : false;

                                // Return true if the date should be disabled
                                return isBeforeTomorrow || isAfterDueDate;
                            }}
                        />

                    </Form.Item>


                    <Form.Item
                        name="members"
                        label={<>
                            {t('select_team_members')}
                            <span className="ant-form-item-required"></span>
                        </>}
                        tooltip={t('at_least_one_member_required_tooltip')}
                        rules={[
                            {
                                validator: (_, value) => {
                                    if (value && value.length > 0) {
                                        return Promise.resolve();
                                    }
                                    return Promise.reject(new Error(t('at_least_one_member_required')));
                                }
                            }
                        ]}
                    >
                        <Select
                            mode="multiple"
                            placeholder={t('select_team_members')}
                            style={{ width: '100%' }}
                            labelInValue={true}
                            optionFilterProp="label"
                            showSearch
                            onChange={(selectedMembers) => {
                                // Get current PIC
                                const currentPIC = responsibilityForm.getFieldValue('picMemberId');

                                // Check if current PIC is still in selected members
                                if (currentPIC) {
                                    const memberValues = selectedMembers.map((member: { value: any; }) => member.value);
                                    const picStillInMembers = memberValues.includes(currentPIC);

                                    // Reset PIC if the member was removed
                                    if (!picStillInMembers) {
                                        responsibilityForm.setFieldsValue({ picMemberId: null });
                                    }
                                }
                            }}
                        >
                            {availableMembers.map(member => (
                                <Option
                                    key={member.id}
                                    value={member.id}
                                    label={`${member.email} (${member.fullName})`}
                                >
                                    {member.email} ({member.fullName})
                                </Option>
                            ))}
                        </Select>
                    </Form.Item>

                    {/* PIC Selection with reactive updates - OPTIONAL for edit */}
                    <Form.Item
                        label={<span>
                            Person In Charge (PIC)
                            <Text type="secondary" style={{ fontSize: '12px', marginLeft: '8px' }}>
                                (Optional)
                            </Text>
                        </span>}
                        tooltip={t('pic_select_one_member_responsible')}
                        shouldUpdate={(prevValues, currentValues) =>
                            prevValues.members !== currentValues.members
                        }
                    >
                        {({ getFieldValue, setFieldsValue }) => {
                            const members = getFieldValue('members') || [];
                            const currentPIC = getFieldValue('picMemberId');

                            // This ensures PIC is reset when the member is removed
                            if (currentPIC && members.length > 0) {
                                const memberValues = members.map((m: { value: any; }) => m.value);
                                if (!memberValues.includes(currentPIC)) {
                                    // Delayed setting to avoid React warning about state updates during render
                                    setTimeout(() => {
                                        setFieldsValue({ picMemberId: null });
                                    }, 0);
                                }
                            }

                            return (
                                <Form.Item
                                    name="picMemberId"
                                    noStyle
                                >
                                    <Select
                                        placeholder={t('select_pic_required')}
                                        style={{ width: '100%' }}
                                        allowClear
                                        options={members.map((member: { label: any; value: any; }) => ({
                                            label: member.label,
                                            value: member.value
                                        }))}
                                        onChange={(value) => {
                                            // Explicitly set to null when cleared
                                            if (!value) {
                                                setFieldsValue({ picMemberId: null });
                                            }
                                        }}
                                    />
                                </Form.Item>
                            );
                        }}
                    </Form.Item>

                    <Alert
                        type="info"
                        showIcon
                        message="Changes to assignments will update the team member responsibilities"
                        style={{ marginTop: 16 }}
                    />
                </Form>
            </Modal>

            <Modal
                title={t('approve_dpia')}
                open={approveModalVisible}
                onCancel={() => {
                    setApproveModalVisible(false);
                    setApprovalComment('');
                }}
                onOk={handleApproveDPIA}
                okText="Approve"
                confirmLoading={approvalLoading}
                okButtonProps={{ style: { backgroundColor: '#52c41a', borderColor: '#52c41a' } }}
            >
                <p>{t('are_you_sure_to_approve_this_dpia')}</p>
               
            </Modal>
            {/* Delete Responsibility Confirmation Modal */}
            <Modal
                title={
                    <div>
                        <DeleteOutlined style={{ color: '#ff4d4f', marginRight: '8px' }} />
                        {t('confirm_delete_responsibility')}
                    </div>
                }
                open={isDeleteResponsibilityModalVisible}
                onOk={handleDeleteResponsibility}
                onCancel={handleCancelResponsibilityDelete}
                okText={t('yes_delete')}
                cancelText={t('cancel')}
                okButtonProps={{ danger: true }}
            >
                {responsibilityToDelete && (
                    <div>
                        <p style={{ width: 400 }}>{t('delete_responsibility')} "{responsibilityToDelete.title}"?</p>
                        <p style={{ width: 400 }}>{t('remove_all_assignments')}</p>
                        <p style={{ width: 400 }}>{t('action_cannot_be_undone')}</p>
                    </div>
                )}
            </Modal>

            <Modal
                title={t('reject_dpia')}
                open={rejectModalVisible}
                onCancel={() => {
                    setRejectModalVisible(false);
                    setRejectionReason('');
                }}
                onOk={handleRejectDPIA}
                okText={t('reject')}
                okButtonProps={{ danger: true }}
                confirmLoading={approvalLoading}
            >
                <p  style={{width:300}}>{t('are_you_sure_to_reject_this_dpia')}</p>
                
            </Modal>

            <Modal
                title={t('start_process_dpia')}
                open={startDPIAConfirmVisible}
                onCancel={() => setStartDPIAConfirmVisible(false)}
                onOk={handleStartDPIA}
                okText={t('start_dpia')}
                cancelText={t('cancel')}
            >
                <Alert
                    message={t('dpiaReady')}
                    description={t('start_dpia_success')}
                    type="info"
                    showIcon
                    style={{ marginBottom: 16 }}
                />
                <div>
                    <Text>{t("currentResponsibilityStatus")}</Text>
                    <ul style={{ marginTop: 8 }}>
                        {responsibilityRows.map(resp => (
                            <li key={resp.id}>
                                <Text>{resp.title}: </Text>
                                <Tag color={getResponsibilityStatusColor(resp.status)}>
                                    {getResponsibilityStatusText(resp.status)}
                                </Tag>
                            </li>
                        ))}
                    </ul>
                </div>
            </Modal>
            <Modal
                title={
                    <div>
                        <DeleteOutlined style={{ color: '#ff4d4f', marginRight: '8px' }} />
                        {t('confirm_delete')}
                    </div>
                }
                open={isFileDeleteModalVisible}
                onOk={handleFileDelete}
                onCancel={handleCancelFileDelete}
                okText={t('yes_delete')}
                cancelText={t('cancel')}
                okButtonProps={{ danger: true }}

            >
                <p style={{ width: 500 }}>{t('are_you_sure')}</p>
                <p>{t('action_cannot_be_undone')}</p>
            </Modal>

            {/* Edit DPIA Details Modal */}
            <Modal
                title={t('edit_dpia_details')}
                open={editDpiaModalVisible}
                onCancel={() => {
                    setEditDpiaModalVisible(false);
                    dpiaEditForm.resetFields();
                }}
                confirmLoading={updateLoading}
                onOk={() => dpiaEditForm.submit()}
                width={600}
            >
                <Form
                    form={dpiaEditForm}
                    layout="vertical"
                    onFinish={handleUpdateDpiaDetails}
                >
                    <Form.Item
                        name="title"
                        label={t('title')}
                        rules={[{ required: true, message: t('please_enter_title') }]}
                    >
                        <Input placeholder={t('enter_dpia_title')} />
                    </Form.Item>



                    <Form.Item
                        name="dueDate"
                        label={t('due_date')}
                        rules={[{ required: true, message: t('please_select_due_date') }]}
                    >
                        <DatePicker
                            style={{ width: '100%' }}
                            format="YYYY-MM-DD"
                            disabledDate={(current) => {
                                return current && current.isBefore(dayjs().startOf('day'));
                            }}
                        />
                    </Form.Item>

                    <Form.Item
                        name="description"
                        label={t('description')}
                    >
                        <TextArea rows={4} placeholder={t('enter_dpia_description')} />
                    </Form.Item>

                    <Divider />

                    <div style={{ marginBottom: '8px' }}>
                        <Text type="secondary">{t('readonly_fields')}:</Text>
                    </div>

                    <Row gutter={16}>
                        <Col span={12}>
                            <Form.Item label={t('created_by')}>
                                <Input value={dpiaDetail?.createdBy || ""} disabled />
                            </Form.Item>
                        </Col>
                        <Col span={12}>
                            <Form.Item label={t('created_at')}>
                                <Input value={formatDate(dpiaDetail?.createdAt || "")} disabled />
                            </Form.Item>
                        </Col>
                    </Row>

                    <Row gutter={16}>
                        <Col span={12}>
                            <Form.Item label={"System"}>
                                <Input value={dpiaDetail?.externalSystemName || ""} disabled />
                            </Form.Item>
                        </Col>
                        <Col span={12}>
                            <Form.Item label={t('status')}>
                                <Input value={dpiaDetail?.status || ""} disabled />
                            </Form.Item>
                        </Col>
                    </Row>
                    <Row gutter={16}>
                        <Col span={12}>
                            <Form.Item
                                name="type"
                                label={t('type_of_dpia')}
                            >
                                <Input value={dpiaDetail?.type || ""} disabled />
                            </Form.Item>
                        </Col>
                    </Row>
                    <Alert
                        type="info"
                        showIcon
                        message={t('editing_dpia_details_info')}
                        description={t('changes_will_be_reflected_immediately')}
                        style={{ marginTop: 16 }}
                    />
                </Form>
            </Modal>
        </Layout>
    );

};
export default DPIADetailsScreen;