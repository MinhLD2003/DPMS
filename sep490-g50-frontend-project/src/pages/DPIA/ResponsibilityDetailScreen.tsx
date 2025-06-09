import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import AxiosClient from '../../configs/axiosConfig';
import { useAuth } from '../../hooks/useAuth';
import {
    Tooltip,
    Descriptions,
    Card,
    Table,
    Tag,
    Button,
    Typography,
    Space,
    Breadcrumb,
    Row,
    Col,
    Form,
    Select,
    Upload,
    Divider,
    message,
    Spin,
    ConfigProvider,
    Modal,
    Radio,
    Input,
    Tabs,
    Progress,
    Alert,
    List,
    Badge,
    Empty,
    Avatar,
    Steps,
} from 'antd';
import moment from 'moment';
import {
    ArrowLeftOutlined,
    UploadOutlined,
    DownloadOutlined,
    DeleteOutlined,
    CheckCircleOutlined,
    ClockCircleOutlined,
    EditOutlined,
    UserOutlined,
    TeamOutlined,
    ExclamationCircleOutlined,
    PlusOutlined,
    SearchOutlined,
    FileOutlined,
    LockOutlined,
    RollbackOutlined, // Added for continue work button
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import type { UploadFile } from 'antd/es/upload/interface';
import { useTranslation } from 'react-i18next';
import FeatureGuard from '../../routes/FeatureGuard';
const { Title, Text, Paragraph } = Typography;
const { Dragger } = Upload;


enum CompletionStatus {
    NotStarted = 'NotStarted',
    InProgress = 'InProgress',
    Completed = 'Completed',
}
interface DPIADetail {
    id: string;
    title: string;
    description: string;
    status: string;
    type: string;
    externalSystemId: string;
    externalSystemName: string;
    createdBy: string | null;
    createdAt: string;
    updatedBy: string | null;
    lastModifiedAt: string;
}
enum ResponsibilityStatus {
    NotStarted = 'NotStarted',
    Ready = 'Ready',           // Note this is position 1 in C#
    InProgress = 'InProgress', // Position 2 in C#
    Completed = 'Completed'    // Position 3 in C#
}

interface DPIADocument {
    dpiaId: string;
    responsibleId?: string;
    dpia?: any;
    title: string;
    fileUrl: string;
    fileFormat?: string;
    id: string;
    createdAt: string;
    lastModifiedAt: string;
    createdById?: string;
    lastModifiedById?: string;

    // UI display properties
    key?: string;
    no?: number;
    uploader?: string;
    uploaded?: string;
}

// Interface for Member in responsibility
interface MemberResponsibilityVM {
    id: string;
    memberId: string;
    userId: string;
    fullName: string | null;
    email: string | null;
    isPic: boolean;
    joinedAt: string;
    completionStatus: CompletionStatus | string;
}

// Interface for Responsibility Detail matching your response structure
interface DPIAResponsibilityDetailVM {
    id: string;
    responsibilityId: string;
    responsibilityName: string;
    responsibilityDescription: string;
    dueDate: string;
    comment: string | null;
    status: ResponsibilityStatus | number;
    members: MemberResponsibilityVM[];
    documents: DPIADocument[];
}

// Interface for user options in member selection
interface UserOption {
    id: string;
    fullName: string;
    email: string;
}


const responsibilityService = {
    getDPIAById: async (id: string): Promise<DPIADetail> => {
        const response = await AxiosClient.get(`/dpia/dpia-detail/${id}`);
        return response.data;
    },
    getResponsibilityById: async (dpiaId: string, responsibilityId: string): Promise<DPIAResponsibilityDetailVM> => {
        const response = await AxiosClient.get(`/dpia/${dpiaId}/${responsibilityId}`);
        
        return response.data;
    },

    updateResponsibilityStatus: async (dpiaId: string, memberResponsibilityId: string, status: CompletionStatus, notes?: string): Promise<void> => {
        const payload = {
            MemberResponsibilityId: memberResponsibilityId,
            completionStatus: status,
            notes: notes,
        };
        await AxiosClient.put(`/dpia/${dpiaId}/update-member-responsibility-status`, JSON.stringify(payload), {
            headers: {
                'Content-Type': 'application/json',
            },
        });
    },

    uploadFileForResponsibility: async (dpiaId: string, responsibilityId: string, formData: FormData): Promise<DPIADocument> => {
        const fileFromForm = formData.get('file');
        if (!fileFromForm) {
            throw new Error('No file selected for upload');
        }
        // Adjusted to match your endpoint pattern
        const response = await AxiosClient.post(`/dpia/${dpiaId}/${responsibilityId}/upload-documents`, formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
        return response.data;
    },

    getAvailableUsers: async (): Promise<UserOption[]> => {
        const response = await AxiosClient.get('/dpia/members-for-dpia');
        return response.data;
    },

    updateResponsibilityMembers: async (dpiaId: string, responsibilityId: string, memberData: any): Promise<void> => {
        await AxiosClient.put(`/dpia/${dpiaId}/responsibilities/${responsibilityId}/members`, memberData, {
            headers: {
                'Content-Type': 'application/json',
            },
        });
    },

    markResponsibilityAsReady: async (dpiaId: string, dpiaResponsibilityId: string): Promise<void> => {
        const payload = {
            dpiaResponsibilityId: dpiaResponsibilityId,
            status: 1
        };

        await AxiosClient.put(`/dpia/${dpiaId}/update-responsibility-status`, payload, {
            headers: {
                'Content-Type': 'application/json',
            },
        });
    },

    downloadFile: async (fileUrl: string): Promise<Blob> => {
        const response = await AxiosClient.get(`/File?fileName=${encodeURIComponent(fileUrl)}`, {
            responseType: 'blob',
        });
        return response.data;
    },

    deleteResponsibilityFile: async (dpiaId: string, fileId: string): Promise<void> => {
        await AxiosClient.delete(`/dpia/${dpiaId}/documents/${fileId}`);
    },

    // New method to handle changing responsibility status back to In Progress
    changeResponsibilityStatus: async (dpiaId: string, dpiaResponsibilityId: string, status: number): Promise<void> => {
        const payload = {
            dpiaResponsibilityId: dpiaResponsibilityId,
            status: status
        };

        await AxiosClient.put(`/dpia/${dpiaId}/update-responsibility-status`, payload, {
            headers: {
                'Content-Type': 'application/json',
            },
        });
    },
    restartResponsibility: async (dpiaId: string, responsibilityId: string): Promise<void> => {
        await AxiosClient.put(`/dpia/${dpiaId}/restart-responsibility/${responsibilityId}`);
    },
};

const ResponsibilityDetailScreen: React.FC = () => {
    // Route parameters
    const { dpiaId, responsibilityId } = useParams<{ dpiaId: string; responsibilityId: string; }>();
    const navigate = useNavigate();
    const { user } = useAuth();
    const { t } = useTranslation();
    // State variables
    const [currentMemberId, setCurrentMemberId] = useState<string>();
    const [loading, setLoading] = useState<boolean>(true);
    const [responsibility, setResponsibility] = useState<DPIAResponsibilityDetailVM | null>(null);
    const [isPIC, setIsPIC] = useState<boolean>(false);
    const [availableUsers, setAvailableUsers] = useState<UserOption[]>([]);
    const [editingMembers, setEditingMembers] = useState<boolean>(false);
    const [targetKeys, setTargetKeys] = useState<string[]>([]);
    const [picUserIds, setPicUserIds] = useState<string[]>([]); // New state for tracking PICs
    const [statusUpdateSuccess, setStatusUpdateSuccess] = useState<boolean>(false);
    const [showStatusModal, setShowStatusModal] = useState<boolean>(false);
    const [saveLoading, setSaveLoading] = useState<boolean>(false);
    const [statusForm] = Form.useForm();
    const [showCompleteModal, setShowCompleteModal] = useState<boolean>(false);
    const [uploadLoading, setUploadLoading] = useState<boolean>(false);
    const [viewMode, setViewMode] = useState<'myFiles' | 'allFiles'>('myFiles');
    const [showReadyModal, setShowReadyModal] = useState<boolean>(false);
    const [showMemberStatusModal, setShowMemberStatusModal] = useState<boolean>(false);
    const [selectedMember, setSelectedMember] = useState<MemberResponsibilityVM | null>(null);
    const [memberStatusForm] = Form.useForm();
    const [showContinueWorkModal, setShowContinueWorkModal] = useState<boolean>(false);
    const [dpiaDetail, setDpiaDetail] = useState<DPIADetail | null>(null);
    const checkResponsibilityStatus = (status: ResponsibilityStatus | number, targetStatus: ResponsibilityStatus | number) => {
        if (typeof status === 'number') {
            return status === targetStatus;
        }
        return status === targetStatus;
    };

    // Helper function to get status text
    const getStatusText = (status: ResponsibilityStatus | number) => {
        if (typeof status === 'number') {
            // Use the C# enum order: 0=NotStarted, 1=Ready, 2=InProgress, 3=Completed
            const statusMap = ['NotStarted', 'Ready', 'InProgress', 'Completed'];
            return statusMap[status] || 'Unknown';
        }
        return status;
    };

    const getStatusColor = (status: ResponsibilityStatus | number) => {
        if (typeof status === 'number') {
            // Use the C# enum order: 0=NotStarted, 1=Ready, 2=InProgress, 3=Completed
            switch (status) {
                case 0: return 'default'; // NotStarted
                case 1: return 'blue';    // Ready
                case 2: return 'processing'; // InProgress
                case 3: return 'success'; // Completed
                default: return 'default';
            }
        }

        switch (status) {
            case ResponsibilityStatus.Completed: return 'success';
            case ResponsibilityStatus.Ready: return 'blue';
            case ResponsibilityStatus.InProgress: return 'processing';
            default: return 'default';
        }
    };

    // Fetch data on component mount
    useEffect(() => {
        if (!dpiaId || !responsibilityId) {
            message.error(t('required_parameters_missing'));
            return;
        }
        fetchResponsibilityData();
        fetchDPIAData();
        fetchAvailableUsers();
    }, [dpiaId, responsibilityId, t]);
    // Handle opening member status update modal (PIC only)
    const handleUpdateMemberStatus = (member: MemberResponsibilityVM) => {
        if (!isPIC) return;

        setSelectedMember(member);

        // Set initial values in form
        memberStatusForm.setFieldsValue({
            memberStatus: member.completionStatus || CompletionStatus.NotStarted,
            notes: '',
        });

        setShowMemberStatusModal(true);
    };

    // Handle member status modal submit
    const handleMemberStatusModalSubmit = async () => {
        if (!responsibility || !selectedMember) return;

        try {
            await memberStatusForm.validateFields();
            const values = memberStatusForm.getFieldsValue();
            setSaveLoading(true);

            const completionStatus = values.memberStatus as CompletionStatus;
            const notes = values.notes;

            // Update via API
            await responsibilityService.updateResponsibilityStatus(dpiaId!, selectedMember.id, completionStatus, notes);

            // Refresh data
            fetchResponsibilityData();

            setShowMemberStatusModal(false);
            setStatusUpdateSuccess(true);
            message.success(`${selectedMember.fullName}'s status updated successfully!`);

            setTimeout(() => {
                setStatusUpdateSuccess(false);
            }, 5000);
        } catch (error) {
            console.error('Error updating member status:', error);
            message.error('Failed to update member status');
        } finally {
            setSaveLoading(false);
            setSelectedMember(null);
        }
    };

    // Handle member status modal cancel
    const handleMemberStatusModalCancel = () => {
        setShowMemberStatusModal(false);
        memberStatusForm.resetFields();
        setSelectedMember(null);
    };

    // Initialize PIC user IDs when responsibility data changes
    useEffect(() => {
        if (responsibility) {
            // Initialize PIC user IDs
            const pics = responsibility.members
                .filter(m => m.isPic)
                .map(m => m.userId);
            setPicUserIds(pics);
        }
    }, [responsibility]);

    const fetchResponsibilityData = async () => {
        try {
            setLoading(true);
            const respData = await responsibilityService.getResponsibilityById(dpiaId!, responsibilityId!);

            // Make sure we only include documents that belong to this responsibility
            if (respData.documents) {
                // Filter documents to ensure they belong to this responsibility
                respData.documents = respData.documents.filter(doc =>
                    doc.responsibleId === responsibilityId
                );
            }
            setResponsibility(respData);
            const memberIds = respData.members.map(member => member.userId);
            setTargetKeys(memberIds);

            // Find the current user directly in one step
            const userMember = respData.members.find(member => member.userId === user?.sub);

            if (userMember) {
                setCurrentMemberId(userMember.memberId);
                setIsPIC(userMember.isPic || false);
            }

        } catch (error) {
            console.error('Error fetching responsibility data:', error);
            message.error(t('failed_to_load_responsibility_details'));
        } finally {
            setLoading(false);
        }
    };
    const fetchDPIAData = async () => {
        try {
            setLoading(true);
            if (!dpiaId) {
                message.error(t('dpia_id_missing'));
                return;
            }

            // Get the DPIA data from the API
            const responseData = await responsibilityService.getDPIAById(dpiaId);

            const dpiaDetailData: DPIADetail = {
                id: responseData.id,
                title: responseData.title,
                description: responseData.description,
                status: responseData.status,
                type: responseData.type,
                externalSystemId: responseData.externalSystemId,
                externalSystemName: responseData.externalSystemName,
                createdBy: responseData.createdBy,
                createdAt: responseData.createdAt,
                updatedBy: responseData.updatedBy,
                lastModifiedAt: responseData.lastModifiedAt
            };
            setDpiaDetail(dpiaDetailData);
        } catch (error) {
            console.error('Error fetching DPIA data:', error);
            message.error(t('failed_to_load_dpia_details'));
        } finally {
            setLoading(false);
        }
    };



    // Fetch available users for member selection
    const fetchAvailableUsers = async () => {
        try {
            const users = await responsibilityService.getAvailableUsers();
            setAvailableUsers(users);
        } catch (error) {
            console.error('Error fetching available users:', error);
            message.error(t('failed_to_load_available_users'));
        }
    };

    const handleBack = () => {
        navigate(-1);
    };
    const showCompleteConfirmation = () => {
        setShowCompleteModal(true);
    };
    const handleMarkAsCompleted = async () => {
        try {
            setSaveLoading(true);
            await responsibilityService.changeResponsibilityStatus(dpiaId!, responsibility?.id!, 3);
            message.success(t('mark_as_completed'));

            // Update local state
            if (responsibility) {
                setResponsibility({
                    ...responsibility,
                    status: 3 // Status 3 is Completed in C# enum
                });
            }
            return true;
        } catch (error) {
            console.error('Error marking responsibility as completed:', error);
            message.error('Failed to update responsibility status');
            return false;
        } finally {
            setSaveLoading(false);
            setShowCompleteModal(false);
        }
    };

    const showMarkAsReadyConfirmation = () => {
        setShowReadyModal(true);
    };

    // Show Continue Work confirmation modal
    const showContinueWorkConfirmation = () => {
        setShowContinueWorkModal(true);
    };

    // Handle continuing work on a completed responsibility
    const handleContinueWork = async () => {
        try {
            setSaveLoading(true);
            await responsibilityService.restartResponsibility(dpiaId!, responsibility?.responsibilityId!);
            message.success(t('this_responsibility_is_now_back_in_progress'));
            // Update local state
            if (responsibility) {
                setResponsibility({
                    ...responsibility,
                    status: 2 // Status 2 is InProgress in C# enum
                });
            }
            return true;
        } catch (error) {
            console.error('Error restarting responsibility:', error);
            message.error('Failed to restart responsibility');
            return false;
        } finally {
            setSaveLoading(false);
            setShowContinueWorkModal(false);
        }
    };
    // Handle saving member changes
    const handleSaveMembers = async () => {
        try {
            setSaveLoading(true);

            // Make sure all PIC users are included in the targetKeys
            // This ensures PICs cannot be removed
            let finalTargetKeys = [...targetKeys];

            // Ensure all PICs are included in the final selection
            picUserIds.forEach(picId => {
                if (!finalTargetKeys.includes(picId)) {
                    finalTargetKeys.push(picId);
                }
            });
            const memberUpdates = {
                userIds: finalTargetKeys,
                pic: picUserIds.length > 0 ? picUserIds[0] : null // Take the first PIC if available
            };

            // Call the API endpoint
            await AxiosClient.put(
                `/dpia/${dpiaId}/${responsibilityId}/update-responsibility-members`,
                memberUpdates,
                {
                    headers: {
                        'Content-Type': 'application/json',
                    },
                }
            );

            setEditingMembers(false);
            message.success('Team members updated successfully');
            fetchResponsibilityData(); // Refresh data
        } catch (error : any) {
            console.error('Error updating members:', error);
            message.error(error.apiSingleErrorMessage?.data || error.apiErrorMessage?.data || 'Failed to update members');
        } finally {
            setSaveLoading(false);
        }
    };

    // Handle canceling member editing
    const handleCancelEditMembers = () => {
        // Reset to original state
        if (responsibility) {
            const memberIds = responsibility.members.map(member => member.userId);
            setTargetKeys(memberIds);

            const pics = responsibility.members
                .filter(m => m.isPic)
                .map(m => m.userId);
            setPicUserIds(pics);
        }

        setEditingMembers(false);
    };

    // Handle member selection change (prevent PIC removal)
    const handleMemberSelectionChange = (newSelectedKeys: string[]) => {
        // Get current PICs
        const currentPics = picUserIds;

        // Make sure all PICs remain selected
        const finalSelection = [...newSelectedKeys];

        currentPics.forEach(picId => {
            if (!finalSelection.includes(picId)) {
                finalSelection.push(picId);
            }
        });

        setTargetKeys(finalSelection);
    };

    // Handle marking responsibility as ready (PIC only)
    const handleMarkAsReady = async () => {
        try {
            await responsibilityService.markResponsibilityAsReady(dpiaId!, responsibility?.id!);
            message.success(t('responsibility') + ' ' + t('mark_as_ready'));

            // Update local state
            if (responsibility) {
                setResponsibility({
                    ...responsibility,
                    status: 1 // Status 1 is Ready in C# enum
                });
            }
            return true;
        } catch (error : any) {
            console.error('Error marking responsibility as ready:', error);
            message.error(error.apiSingleErrorMessage?.data || error.apiErrorMessage?.data|| 'Failed to update responsibility status')
            return false;
        }
    };

    // Handle opening status update modal
    const handleUpdateStatus = () => {
        if (!responsibility) return;

        // Find current member's data
        const currentMember = responsibility.members.find(m => m.memberId === currentMemberId);
        if (!currentMember) {
            message.error(t('your_data_not_found_in_responsibility'));
            return;
        }

        // Set initial values in form
        statusForm.setFieldsValue({
            status: currentMember.completionStatus || CompletionStatus.NotStarted,
            notes: '',
        });

        setShowStatusModal(true);
    };

    // Handle status modal submit
    const handleStatusModalSubmit = async () => {
        if (!responsibility) return;

        try {
            await statusForm.validateFields();
            const values = statusForm.getFieldsValue();
            setSaveLoading(true);

            const completionStatus = values.status as CompletionStatus;
            const notes = values.notes;

            // Get the current member's responsibility ID
            const currentMember = responsibility.members.find(m => m.memberId === currentMemberId);
            if (!currentMember) {
                message.error(t('your_data_not_found_in_responsibility'));
                return;
            }

            // Update via API
            await responsibilityService.updateResponsibilityStatus(dpiaId!, currentMember.id, completionStatus, notes);

            // Refresh data
            fetchResponsibilityData();

            setShowStatusModal(false);
            setStatusUpdateSuccess(true);
            message.success(t('status_updated_successfully') + "!");

            setTimeout(() => {
                setStatusUpdateSuccess(false);
            }, 5000);
        } catch (error : any) {
            console.error('Error updating status:', error);
            message.error(error.apiSingleErrorMessage?.data || error.apiErrorMessage?.data ||  t('failed_to_update_status'));
        } finally {
            setSaveLoading(false);
        }
    };

    // Handle status modal cancel
    const handleStatusModalCancel = () => {
        setShowStatusModal(false);
        statusForm.resetFields();
    };

    // File uploader component with duplicate file name prevention
    const ResponsibilityFileUploader = () => {
        const [fileList, setFileList] = useState<UploadFile[]>([]);

        const uploadProps = {
            name: 'file',
            multiple: false,
            fileList: fileList,
            beforeUpload: (file: any) => {
                // Check file size - 25MB limit
                const isLt25M = file.size / 1024 / 1024 < 25;
                if (!isLt25M) {
                    message.error(t('fileSizeError').replace('{{size}}', '25MB'));
                    return Upload.LIST_IGNORE;
                }

                // Check file type
                const allowedTypes = ['.xlsx', '.docx', '.pdf', '.png', '.jpeg', '.jpg'];
                const fileName = file.name || '';
                const fileExtension = '.' + fileName.split('.').pop().toLowerCase();
                const isAllowedType = allowedTypes.includes(fileExtension);

                if (!isAllowedType) {
                    message.error(t('fileTypeError').replace('{{types}}', allowedTypes.join(', ')));
                    return Upload.LIST_IGNORE;
                }
                // Check for duplicate filename (keep existing check)
                if (responsibility?.documents) {
                    const fileExists = responsibility.documents.some(
                        doc => doc.title.toLowerCase() === fileName.toLowerCase()
                    );
                    if (fileExists) {
                        message.error(t('a_file_named_already_exists', { filename: fileName }));
                        return Upload.LIST_IGNORE;
                    }
                }
                return true;
            },

            customRequest: async ({ file, onSuccess, onError }: any) => {
                if (!dpiaId || !responsibilityId) {
                    message.error(t('dpia_id_or_responsibility_id_not_found'));
                    return;
                }
                if (!file || file.size === 0) {
                    message.error(t('invalid_or_empty_file'));
                    onError(new Error('Invalid or empty file'));
                    return;
                }
                try {
                    setUploadLoading(true);
                    const formData = new FormData();
                    formData.append('file', file);
                    const response = await responsibilityService.uploadFileForResponsibility(dpiaId, responsibilityId, formData);

                    // Update local state with the new file
                    if (responsibility) {
                        setResponsibility({
                            ...responsibility,
                            documents: [...responsibility.documents, response]
                        });
                    }
                    await fetchResponsibilityData();
                    message.success(t('uploaded_successfully', { filename: file.name }));
                    onSuccess(response, file);

                    setFileList([]);
                } catch (error) {
                    console.error('Error uploading file:', error);
                    message.error(t('upload_failed', { filename: file.name }));
                    onError(error);
                } finally {
                    setUploadLoading(false);
                }
            },
            onChange(info: any) {
                let fileList = [...info.fileList];
                fileList = fileList.slice(-1); // Only keep the latest file
                setFileList(fileList);
            }
        };

        return (
            <div>
                <Dragger {...uploadProps}
                    style={{
                        padding: '15px',
                        background: '#fafafa',
                        borderRadius: '6px',
                        minHeight: '80px',
                        border: '1px dashed #d9d9d9'
                    }}
                    disabled={uploadLoading}
                >
                    {uploadLoading ? (
                        <div style={{ textAlign: 'center' }}>
                            <Spin />
                            <p style={{ marginTop: 8 }}>{t('uploading_file')}</p>
                        </div>
                    ) : (
                        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                            <UploadOutlined style={{ fontSize: '22px', color: '#1890ff', marginRight: '12px' }} />
                            <div>
                                <p style={{ margin: 0, fontSize: '14px', fontWeight: 500 }}>{t('upload_file')}</p>
                                <p style={{ margin: 0, fontSize: '12px', color: '#999' }}>
                                    {t('fileRestrictions')}
                                </p>
                            </div>
                        </div>
                    )}
                </Dragger>
            </div>
        );
    };

    // File handling functions
    const handleDownloadFile = async (file: DPIADocument) => {
        if (!file.fileUrl) {
            message.error(t('file_url_not_found'));
            return;
        }

        try {
            message.loading(t('downloading_file'), 0);
            const blob = await responsibilityService.downloadFile(file.fileUrl);
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = file.title;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
            document.body.removeChild(a);
            message.destroy();
            message.success(t('file_downloaded_successfully'));
        } catch (error) {
            message.destroy();
            console.error('Error downloading file:', error);
            message.error(t('failed_to_download_file'));
        }
    };

    const handleDeleteFile = async (file: DPIADocument) => {
        if (!dpiaId || !file.id) {
            message.error(t('required_parameters_missing'));
            return;
        }

        Modal.confirm({
            title: t('delete_file'),
            content: t('are_you_sure_delete_file', { filename: file.title }),
            okText: t('delete'),
            okType: 'danger',
            cancelText: t('cancel'),
            onOk: async () => {
                try {
                    await responsibilityService.deleteResponsibilityFile(dpiaId, file.id);

                    // Update local state
                    if (responsibility) {
                        setResponsibility({
                            ...responsibility,
                            documents: responsibility.documents.filter(d => d.id !== file.id)
                        });
                    }

                    message.success(t('file_deleted_successfully'));
                } catch (error) {
                    console.error('Error deleting file:', error);
                    message.error(t('failed_to_delete_file'));
                }
            }
        });
    };

    // Calculate member completion status
    const calculateMemberProgress = () => {
        if (!responsibility || !responsibility.members) return 0;
        const total = responsibility.members.length;
        const completed = responsibility.members.filter(member =>
            member.completionStatus === CompletionStatus.Completed
        ).length;

        return total > 0 ? Math.round((completed / total) * 100) : 0;
    };

    // Helper function to get responsibility workflow step
    const getResponsibilityStepNumber = (status: ResponsibilityStatus | number) => {
        if (typeof status === 'number') {
            return status; // Already numeric: 0=NotStarted, 1=Ready, 2=InProgress, 3=Completed
        }

        // Handle string status values
        switch (status) {
            case ResponsibilityStatus.NotStarted: return 0;
            case ResponsibilityStatus.Ready: return 1;
            case ResponsibilityStatus.InProgress: return 2;
            case ResponsibilityStatus.Completed: return 3;
            default: return 0;
        }
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

            return moment(dateString).format('DD/MM/YYYY HH:mm');
        } catch (error) {
            return "Invalid date";
        }
    };
    // Check if a user is a PIC
    const isUserPIC = (userId: string) => {
        if (!responsibility) return false;
        return responsibility.members.some(m => m.userId === userId && m.isPic);
    };

    // Get user's documents
    const getUserDocuments = () => {
        if (!responsibility || !responsibility.documents) return [];

        // Filter documents to only show those created by the current user
        return responsibility.documents.filter(doc =>
            doc.createdById === user?.sub
        );
    };

    // Get all documents
    const getAllDocuments = () => {
        if (!responsibility || !responsibility.documents) return [];
        return responsibility.documents;
    };

    // Get documents based on view mode
    const getDisplayedDocuments = () => {
        return viewMode === 'myFiles' ? getUserDocuments() : getAllDocuments();
    };

    // Table columns for files
    const fileColumns: ColumnsType<DPIADocument> = [
        { title: 'No.', key: 'no', width: 60, render: (_, __, index) => index + 1 },
        {
            title: t('filename'),
            dataIndex: 'title',
            key: 'title',
            render: (text, record) => (
                <Space>
                    <FileOutlined />
                    <Text>{text || t('unnamed_document')}</Text>
                    {record.createdById === user?.sub && (
                        <Tag color="blue" >{t('your_file')}</Tag>
                    )}
                </Space>
            )
        },
        {
            title: t('uploaded_at'),
            dataIndex: 'createdAt',
            key: 'createdAt',
            render: (text) => new Date(text).toLocaleString()
        },
        {
            title: t('actions'),
            key: 'actions',
            render: (_, record) => (
                <Space>
                    <Button
                        type="link"
                        icon={<DownloadOutlined />}
                        onClick={() => handleDownloadFile(record)}
                    >
                    </Button>
                    {record.createdById === user?.sub && (
                        <Button
                            type="link"
                            danger
                            icon={<DeleteOutlined />}
                            onClick={() => handleDeleteFile(record)}
                        >
                        </Button>
                    )}
                </Space>
            )
        }
    ];

    // Loading state
    if (loading) {
        return <Spin size="large" style={{ display: 'block', margin: '100px auto' }} tip={t('failed_to_load_responsibility_details')} />;
    }

    // Error state
    if (!responsibility) {
        return <div style={{ padding: 24, textAlign: 'center' }}>{t('responsibility_not_found')}</div>;
    }

    // Current member status
    const currentMember = responsibility.members.find(m => m.memberId === currentMemberId);

    // Get current documents based on view mode
    const currentDocuments = getDisplayedDocuments();

    // Filter users based on search text
    function getFilteredUsers(users: UserOption[], searchQuery: string) {
        if (!searchQuery) return users;

        return users.filter(user =>
            user.fullName?.toLowerCase().includes(searchQuery.toLowerCase()) ||
            user.email?.toLowerCase().includes(searchQuery.toLowerCase())
        );
    }

    return (
        <ConfigProvider
            theme={{
                components: {
                    Table: {
                        headerBg: '#f0f7ff',
                        headerColor: '#1890ff',
                    },
                    Card: {
                        headerBg: '#f0f7ff',
                    }
                },
            }}
        >
            <div style={{ padding: '16px', backgroundColor: '#f5f5f5', minHeight: '100vh' }}>
                {/* Header */}
                <div style={{ marginBottom: '16px' }}>
                    <Space>
                        <Button icon={<ArrowLeftOutlined />} type="text" onClick={handleBack} />
                        <Breadcrumb items={[
                            { title: "DPIA Detail", onClick: () => handleBack },
                            { title: t('responsibility_details') },
                        ]} />
                    </Space>
                </div>

                {statusUpdateSuccess && (
                    <Alert
                        message={t('status_updated_successfully')}
                        description={t('status_updated_successfully_description')}
                        type="success"
                        showIcon
                        closable
                        style={{ marginBottom: '16px' }}
                        onClose={() => setStatusUpdateSuccess(false)}
                    />
                )}

                {/* Responsibility Header Card */}
                <Card
                    title={
                        <Space size="middle">
                            <Text strong style={{ fontSize: '18px' }}>{responsibility.responsibilityName}</Text>

                        </Space>
                    }
                    extra={
                        <Space>
                            {/* Show Mark as Ready button only for PICs when status is NotStarted */}
                            {isPIC && (checkResponsibilityStatus(responsibility.status, 0) ||
                                checkResponsibilityStatus(responsibility.status, ResponsibilityStatus.NotStarted)) && (
                                    <Button
                                        type="primary"
                                        onClick={showMarkAsReadyConfirmation}
                                    >
                                        {t('mark_as_ready')}
                                    </Button>
                                )}
                            {isPIC &&
                                (checkResponsibilityStatus(responsibility.status, 2) ||
                                    checkResponsibilityStatus(responsibility.status, ResponsibilityStatus.InProgress)) &&
                                calculateMemberProgress() === 100 && (
                                    <Button
                                        type="primary"
                                        icon={<CheckCircleOutlined />}
                                        onClick={showCompleteConfirmation}
                                    >
                                        {t('mark_as_completed')}
                                    </Button>
                                )}
                            {/* Show Continue Work button only for PICs when status is Completed */}
                            <FeatureGuard requiredFeature='/api/DPIA/{id}/restart-responsibility/{id}_PUT'>
                                {(checkResponsibilityStatus(responsibility.status, 3) ||
                                    checkResponsibilityStatus(responsibility.status, ResponsibilityStatus.Completed)) && (
                                        <Button
                                            type="primary"
                                            icon={<RollbackOutlined />}
                                            onClick={showContinueWorkConfirmation}
                                        >
                                            {t('continue_work')}
                                        </Button>
                                    )}
                            </FeatureGuard>
                        </Space>
                    }
                >
                    <Row gutter={24}>
                        <Col xs={24} md={16}>
                            {dpiaDetail && (
                                <Card
                                    bordered={false}
                                    title={t('i_dpia_information')}
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
                                                <Descriptions.Item label={t('status') + ":"}>

                                                    {dpiaDetail.status}

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
                            )}

                            {/* Description Card */}
                            <Card
                                title={t('responsibility_information')}
                                bordered={false}
                                size="small"
                                style={{ marginBottom: '16px' }}
                                headStyle={{
                                    backgroundColor: '#FF9F43',
                                    color: 'white',
                                    borderTopLeftRadius: '8px',
                                    borderTopRightRadius: '8px'
                                }}
                            >
                                {responsibility.comment && (
                                    <>
                                        <Paragraph>{responsibility.comment}</Paragraph>
                                        <Divider style={{ margin: '12px 0' }} />
                                    </>
                                )}

                                <Row gutter={[16, 16]}>
                                    <Col span={12}>
                                        <Text strong>{t('due_date')}: </Text>
                                        <Text>{new Date(responsibility.dueDate).toLocaleDateString()}</Text>
                                    </Col>
                                    <Col span={12}>
                                        <Text strong>{t('status')}: </Text>
                                        <Tag color={getStatusColor(responsibility.status)}>
                                            {getStatusText(responsibility.status)}
                                        </Tag>
                                    </Col>
                                    <Col span={24}>
                                        <Text strong>{t('description')}: </Text>
                                        <Text>{responsibility.responsibilityDescription}</Text>
                                    </Col>
                                    <Col span={24}>
                                        <Text strong>Member Completion: </Text>
                                        <Progress
                                            percent={calculateMemberProgress()}
                                            size="small"
                                            style={{ width: 200, display: 'inline-block', marginLeft: 8 }}
                                        />
                                    </Col>
                                </Row>

                                {/* Responsibility Workflow Status - Add this section */}
                                <Divider style={{ margin: '16px 0 12px' }} />
                                <div style={{ marginBottom: '8px' }}>
                                    <Text strong>{t('responsibility_workflow')}</Text>
                                </div>
                                <Steps
                                    current={getResponsibilityStepNumber(responsibility.status)}
                                    size="small"
                                    style={{ marginBottom: '8px' }}
                                >
                                    <Steps.Step title={t('initial_setup')} description="Initial setup" />
                                    <Steps.Step title={t('team_assembled')} description="Team assembled" />
                                    <Steps.Step title={t('work_ongoing')} description="Work ongoing" />
                                    <Steps.Step title={t('work_finished')} description="Work finished" />
                                </Steps>
                            </Card>

                            {/* Documents Card */}
                            <Card
                                title={
                                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                        <span>Documents</span>
                                        <Radio.Group
                                            value={viewMode}
                                            onChange={(e) => setViewMode(e.target.value)}
                                            buttonStyle="solid"
                                            size="small"
                                        >
                                            <Radio.Button value="myFiles">{t('my_files')} ({getUserDocuments().length})</Radio.Button>
                                            <Radio.Button value="allFiles">{t('all_files')} ({getAllDocuments().length})</Radio.Button>
                                        </Radio.Group>
                                    </div>
                                }
                                bordered={false}
                                size="small"
                            >
                                <Row gutter={[0, 16]}>
                                    <Col span={24}>
                                        {(checkResponsibilityStatus(responsibility.status, 2) ||
                                            checkResponsibilityStatus(responsibility.status, ResponsibilityStatus.InProgress)) ? (
                                            <ResponsibilityFileUploader />
                                        ) : (
                                            <Alert
                                                message="File Upload Disabled"
                                                description={t('upload_disabled_responsibility_not_in_progress')}
                                                type="info"
                                                showIcon
                                                style={{ marginBottom: '16px' }}
                                            />
                                        )}
                                    </Col>
                                    <Col span={24}>
                                        <Table
                                            columns={fileColumns}
                                            dataSource={currentDocuments}
                                            pagination={currentDocuments.length > 10 ? { pageSize: 10 } : false}
                                            size="small"
                                            rowKey="id"
                                            locale={{
                                                emptyText: viewMode === 'myFiles' ?
                                                    <Empty description="You haven't uploaded any documents yet" /> :
                                                    <Empty description="No documents uploaded yet" />
                                            }}
                                        />
                                    </Col>
                                </Row>
                            </Card>
                        </Col>

                        <Col xs={24} md={8}>
                            {/* Team Members Card */}
                            <Card
                                title={
                                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                                        <Space>
                                            <TeamOutlined />
                                            <span>{t('team_members')}</span>
                                        </Space>
                                        {isPIC && !editingMembers && (
                                            <Button
                                                type="link"
                                                icon={<EditOutlined />}
                                                onClick={() => setEditingMembers(true)}
                                                size="small"
                                            >
                                                {t('edit')}
                                            </Button>
                                        )}
                                    </div>
                                }
                                bordered={false}
                                size="small"
                                headStyle={{
                                    backgroundColor: '#FF9F43',
                                    color: 'white',
                                    borderTopLeftRadius: '8px',
                                    borderTopRightRadius: '8px'
                                }}
                            >
                                {editingMembers ? (
                                    <div className="member-edit-container">
                                        <div style={{ marginBottom: 16 }}>
                                            <div style={{
                                                background: '#f6f6f6',
                                                padding: '8px 12px',
                                                borderRadius: '4px',
                                                marginBottom: '12px'
                                            }}>
                                                <Text type="secondary">
                                                    {t('select_team_members_search_and_select')}
                                                    <br />
                                                    <b>Note:</b> {t('select_team_members_pic_members_cannot_be_removed')}
                                                </Text>
                                            </div>

                                            {/* User selection with dropdown */}
                                            <Form layout="vertical">
                                                <Form.Item
                                                    label={t('select_team_members')}
                                                    help={t('use_the_search_to_find_users_quickly')}
                                                >
                                                    <Select
                                                        mode="multiple"
                                                        style={{ width: '100%' }}
                                                        placeholder="Search and select users"
                                                        value={targetKeys}
                                                        onChange={handleMemberSelectionChange}
                                                        optionFilterProp="label"
                                                        allowClear
                                                        showSearch
                                                        filterOption={(input, option) =>
                                                            (option?.label?.toLowerCase() || '').includes(input.toLowerCase())
                                                        }
                                                        options={availableUsers.map(user => ({
                                                            label: `${user.fullName} (${user.email})`,
                                                            value: user.id,
                                                            disabled: isUserPIC(user.id) // Disable PIC options to prevent removal
                                                        }))}
                                                    />
                                                </Form.Item>
                                            </Form>

                                            {/* Selected members preview */}
                                            <Divider orientation="left">{t('team_preview')}</Divider>
                                            <List
                                                size="small"
                                                bordered
                                                header={
                                                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                                                        <Text strong>{t('selected_team_members')}</Text>
                                                        <Badge count={targetKeys.length} style={{ backgroundColor: '#108ee9' }} />
                                                    </div>
                                                }
                                                dataSource={availableUsers.filter(user => targetKeys.includes(user.id))}
                                                renderItem={user => {
                                                    // Check if user is a PIC in the original data
                                                    const isPic = responsibility.members
                                                        .some(m => m.userId === user.id && m.isPic);
                                                    return (
                                                        <List.Item>
                                                            <List.Item.Meta
                                                                avatar={<Avatar icon={<UserOutlined />} />}
                                                                title={
                                                                    <Space>
                                                                        {user.fullName}
                                                                        {isPic && (
                                                                            <Space>
                                                                                <Tag color="blue">PIC</Tag>
                                                                                <Tooltip title={t('pic_members_cannot_be_removed')}>
                                                                                    <LockOutlined style={{ color: '#1890ff' }} />
                                                                                </Tooltip>
                                                                            </Space>
                                                                        )}
                                                                    </Space>
                                                                }
                                                                description={user.email}
                                                            />
                                                        </List.Item>
                                                    );
                                                }}
                                                locale={{ emptyText: t('selected_team_members_no_members_selected') }}
                                                style={{ maxHeight: '200px', overflow: 'auto' }}
                                            />
                                        </div>

                                        <div style={{ textAlign: 'right', marginTop: 16 }}>
                                            <Space>
                                                <Button onClick={handleCancelEditMembers}>
                                                    {t('cancel')}
                                                </Button>
                                                <Button
                                                    type="primary"
                                                    onClick={handleSaveMembers}
                                                    loading={saveLoading}
                                                    disabled={targetKeys.length === 0}
                                                >
                                                    {t('save_changes')}
                                                </Button>
                                            </Space>
                                        </div>
                                    </div>
                                ) : (
                                    <List
                                        dataSource={responsibility.members}
                                        renderItem={member => (
                                            <List.Item
                                                actions={[
                                                    // Only include the Edit button in actions
                                                    isPIC && (checkResponsibilityStatus(responsibility.status, 2) ||
                                                        checkResponsibilityStatus(responsibility.status, ResponsibilityStatus.InProgress)) &&
                                                        member.userId !== user?.sub ? (
                                                        <Button
                                                            type="link"
                                                            icon={<EditOutlined />}
                                                            size="small"
                                                            onClick={() => handleUpdateMemberStatus(member)}
                                                        >
                                                            {t('update')}
                                                        </Button>
                                                    ) : null
                                                ].filter(Boolean)} // Filter out null values
                                            >
                                                <List.Item.Meta
                                                    avatar={<Avatar icon={<UserOutlined />} />}
                                                    title={
                                                        <Space>
                                                            {member.fullName || 'Unknown User'}
                                                            {member.isPic && <Tag color="blue">PIC</Tag>}
                                                            {member.userId === user?.sub && <Tag color="green">You</Tag>}
                                                            {/* Status tag moved here */}
                                                            <Tag color={
                                                                member.completionStatus === CompletionStatus.Completed ? 'success' :
                                                                    member.completionStatus === CompletionStatus.InProgress ? 'processing' : 'default'
                                                            }>
                                                                {member.completionStatus}
                                                            </Tag>
                                                        </Space>
                                                    }
                                                    description={member.email || 'No email available'}
                                                />
                                            </List.Item>
                                        )}
                                    />

                                )}

                            </Card>

                            {/* My Status Card - Only show when status is InProgress */}
                            {currentMember && (checkResponsibilityStatus(responsibility.status, 2) ||
                                checkResponsibilityStatus(responsibility.status, ResponsibilityStatus.InProgress)) && (
                                    <Card
                                        title={t('my_status')}
                                        bordered={false}
                                        size="small"
                                        style={{ marginTop: '16px' }}
                                    >
                                        <Space direction="vertical" style={{ width: '100%' }}>
                                            <div>
                                                <Text strong>Current Status: </Text>
                                                <Tag color={
                                                    currentMember.completionStatus === CompletionStatus.Completed ? 'success' :
                                                        currentMember.completionStatus === CompletionStatus.InProgress ? 'processing' : 'default'
                                                }>
                                                    {currentMember.completionStatus}
                                                </Tag>
                                            </div>

                                            <Button
                                                type="primary"
                                                icon={<EditOutlined />}
                                                onClick={handleUpdateStatus}
                                                block
                                            >
                                                {t('update')} {t('my_status')}
                                            </Button>
                                        </Space>
                                    </Card>
                                )}
                        </Col>
                    </Row>
                </Card>
            </div>
            <Modal
                title={t('mark_as_completed')}
                open={showCompleteModal}
                onCancel={() => setShowCompleteModal(false)}
                onOk={handleMarkAsCompleted}
                okText={t('mark_as_completed')}
                confirmLoading={saveLoading}
            >
                <div style={{ display: 'flex', alignItems: 'flex-start', gap: 16 }}>
                    <CheckCircleOutlined style={{ color: '#52c41a', fontSize: 22, marginTop: 4 }} />
                    <div>
                        <Paragraph>
                            {t('are_you_sure_you_want_to_mark_this_responsibility_as_completed')}
                        </Paragraph>
                        <Paragraph type="secondary">
                            {t('this_action_indicates_that_all_work_has_been_completed')}
                        </Paragraph>
                    </div>
                </div>
            </Modal>
            {/* Mark as Ready Modal */}
            <Modal
                title={t('confirm_status_change')}
                open={showReadyModal}
                onCancel={() => setShowReadyModal(false)}
                onOk={async () => {
                    try {
                        setSaveLoading(true);
                        await handleMarkAsReady();
                        setShowReadyModal(false);
                    } finally {
                        setSaveLoading(false);
                    }
                }}
                okText={t('mark_as_ready')}
                confirmLoading={saveLoading}
            >
                <div style={{ display: 'flex', alignItems: 'flex-start', gap: 16 }}>
                    <ExclamationCircleOutlined style={{ color: '#faad14', fontSize: 22, marginTop: 4 }} />
                    <div>
                        <Paragraph>
                            {t('are_you_sure_you_want_to_mark_this_responsibility_as_ready')}
                        </Paragraph>
                        <Paragraph type="secondary">
                            {t('the_action_indicates_start_dpia_process')}
                            {t('member_selection_for_this_responsibility_has_been_completed')}
                        </Paragraph>
                    </div>
                </div>
            </Modal>

            <Modal
                title={t('continue_work')}
                open={showContinueWorkModal}
                onCancel={() => setShowContinueWorkModal(false)}
                onOk={handleContinueWork}
                okText={t('continue_work')}
                confirmLoading={saveLoading}
            >
                <div style={{ display: 'flex', alignItems: 'flex-start', gap: 16 }}>
                    <RollbackOutlined style={{ color: '#1890ff', fontSize: 22, marginTop: 4 }} />
                    <div>
                        <Paragraph>
                            {t('are_you_sure_you_want_to_change_this_responsibility_back_to_in_progress')}
                        </Paragraph>
                        <Paragraph type="secondary">
                            {t('this_action_will_reopen_the_responsibility_for_further_work')}
                        </Paragraph>
                    </div>
                </div>
            </Modal>

            {/* Status Update Modal */}
            <Modal
                title={t('update') + " " + t('my_status')}
                open={showStatusModal}
                onCancel={handleStatusModalCancel}
                onOk={handleStatusModalSubmit}
                okText={t('update') + " " + t('status')}
                confirmLoading={saveLoading}
                width={600}
            >
                <Form form={statusForm} layout="vertical">
                    <Form.Item
                        name="status"
                        label={t('status')}
                        rules={[{ required: true, message: t('please_select_a_status') }]}
                    >
                        <Radio.Group>
                            <Space direction="vertical" style={{ width: '100%' }}>

                                <Radio value={CompletionStatus.InProgress}>
                                    <Space align="start">
                                        <ClockCircleOutlined style={{ color: '#faad14' }} />
                                        <div>
                                            <Text strong>In Progress</Text>
                                            <br />
                                            <Text type="secondary">{t('work_is_currently_being_done')}</Text>
                                        </div>
                                    </Space>
                                </Radio>

                                <Radio value={CompletionStatus.Completed}>
                                    <Space align="start">
                                        <CheckCircleOutlined style={{ color: '#52c41a' }} />
                                        <div>
                                            <Text strong>Completed</Text>
                                            <br />
                                            <Text type="secondary">{t('all_required_work_has_been_completed')}</Text>
                                        </div>
                                    </Space>
                                </Radio>
                            </Space>
                        </Radio.Group>
                    </Form.Item>

                </Form>
            </Modal>
            {/* Member Status Update Modal */}
            <Modal
                title={`${t('update')}  ${t('status_update_for')} ${selectedMember?.fullName || t('team_members')}`}
                open={showMemberStatusModal}
                onCancel={handleMemberStatusModalCancel}
                onOk={handleMemberStatusModalSubmit}
                okText={t('update') + " " + t('status')}
                confirmLoading={saveLoading}
                width={600}
            >
                <Form form={memberStatusForm} layout="vertical">
                    <Form.Item
                        name="memberStatus"
                        label={t('status')}
                        rules={[{ required: true, message: 'Please select a status' }]}
                    >
                        <Radio.Group>
                            <Space direction="vertical" style={{ width: '100%' }}>
                                <Radio value={CompletionStatus.InProgress}>
                                    <Space align="start">
                                        <ClockCircleOutlined style={{ color: '#faad14' }} />
                                        <div>
                                            <Text strong>In Progress</Text>
                                            <br />
                                            <Text type="secondary">{t('work_is_currently_being_done')}</Text>
                                        </div>
                                    </Space>
                                </Radio>

                                <Radio value={CompletionStatus.Completed}>
                                    <Space align="start">
                                        <CheckCircleOutlined style={{ color: '#52c41a' }} />
                                        <div>
                                            <Text strong>Completed</Text>
                                            <br />
                                            <Text type="secondary">{t('all_required_work_has_been_completed')}</Text>
                                        </div>
                                    </Space>
                                </Radio>
                            </Space>
                        </Radio.Group>
                    </Form.Item>

                </Form>
            </Modal>

        </ConfigProvider>
    );
};

export default ResponsibilityDetailScreen;