import React, { useState, useEffect, useCallback, useContext } from 'react';
import { Button, Divider, Form, Input, message, Select, Tooltip, Card, Spin, Layout, Row, Col, Upload, Space, Typography } from "antd";
import { useNavigate } from "react-router-dom";
import { z } from "zod";
import AxiosClient from "../../configs/axiosConfig";
import { AuthContext } from "../../contexts/AuthContext";
import {
    UploadOutlined,
    FileTextOutlined,
    ArrowLeftOutlined,
    SendOutlined,
    WarningOutlined
} from "@ant-design/icons";
import { jwtDecode } from "jwt-decode";
import API_ENDPOINTS from "../../configs/APIEndPoint";
import { TicketType } from '../../enum/enum';
import PageHeader from "../../components/common/PageHeader";
import { useTranslation } from "react-i18next";

const { TextArea } = Input;
const { Option } = Select;

// Constants for file validation
const MAX_FILE_SIZE = 25 * 1024 * 1024; // 25MB in bytes
const ALLOWED_EXTENSIONS = ['.xlsx', '.docx', '.pdf', '.png', '.jpeg', '.jpg'];

// Updated schema with file validation
const ticketSchema = z.object({
    title: z.string().min(6, { message: "Title needs to be longer" }).max(50),
    ticketType: z.string().min(1, { message: "Category is required" }),
    description: z.string().min(6, { message: "Description needs to be longer." }),
    attachment: z.array(z.any()).optional(),
});

const TicketTypes: string[] = Object.values(TicketType);

const AddTicket: React.FC = () => {
    const { t } = useTranslation();
    const [form] = Form.useForm();
    const values = Form.useWatch([], form);
    const authContext = useContext(AuthContext);
    const [submittable, setSubmittable] = useState<boolean>(false);
    const [loading, setLoading] = useState<boolean>(false);
    const [fileList, setFileList] = useState<any[]>([]);
    const [errors, setErrors] = useState<{
        externalSystemId?: string;
        title?: string;
        description?: string;
        ticketType?: string;
        fileError?: string;
    }>({});
    const [externalSystems, setExternalSystems] = useState<Array<{ id: string, name: string }>>([]);
    const navigate = useNavigate();

    const fetchExternalSystems = useCallback(async () => {
        try {
            const response = await AxiosClient.get('/ExternalSystem');
            const systems = response.data.map((system: any) => ({
                name: system.name,
                id: system.id
            }));
            setExternalSystems(systems);
        } catch (error) {
            message.error(t('addTicket.loadSystemsFailed'));
        }
    }, [t]);

    useEffect(() => {
        fetchExternalSystems();
    }, [fetchExternalSystems]);

    useEffect(() => {
        const result = ticketSchema.safeParse(values);
        if (result.success) {
            setSubmittable(true);
            setErrors({});
        } else {
            setSubmittable(false);
            const fieldErrors = result.error.format();
            setErrors({
                title: fieldErrors.title?._errors[0],
                description: fieldErrors.description?._errors[0],
                ticketType: fieldErrors.ticketType?._errors[0],
            });
        }
    }, [values]);

    useEffect(() => {
        if (authContext?.user?.token) {
            try {
                const decodedToken: { email: string } = jwtDecode(authContext.user.token);
                form.setFieldsValue({ email: decodedToken.email });
            } catch (error) {
                console.error("Invalid token:", error);
            }
        }
    }, [authContext?.user, form]);

    // File validation function
    const validateFile = (file: File) => {
        // Check file size
        if (file.size > MAX_FILE_SIZE) {
            message.error(`${file.name} ${t('addTicket.fileSizeError', { size: '25MB' })}`);
            return false;
        }

        // Check file extension
        const extension = `.${file.name.split('.').pop()?.toLowerCase()}`;
        if (!ALLOWED_EXTENSIONS.includes(extension)) {
            message.error(`${file.name} ${t('addTicket.fileTypeError', { types: ALLOWED_EXTENSIONS.join(', ') })}`);
            return false;
        }

        return true;
    };

    // Handle file change with validation
    const handleFileChange = ({ fileList }: any) => {
        // Filter out invalid files
        const validFiles = fileList.filter((file: any) => {
            if (file.originFileObj && !file.validated) {
                file.validated = true;
                return validateFile(file.originFileObj);
            }
            return true;
        });
        
        setFileList(validFiles);
        form.setFieldsValue({ attachments: validFiles });
    };

    const handleSubmit = async () => {
        setLoading(true);
        try {
            await form.validateFields();
            const values = form.getFieldsValue();

            // Check if there are any file validation errors
            if (errors.fileError) {
                message.error(errors.fileError);
                setLoading(false);
                return;
            }

            const formData = new FormData();
            // Only append externalSystemId if it exists and is not empty
            if (values.externalSystemId && values.externalSystemId !== '') {
                formData.append("externalSystemId", values.externalSystemId);
            }

            formData.append("title", values.title);
            formData.append("ticketType", values.ticketType);
            formData.append("description", values.description);

            if (values.attachments) {
                values.attachments.forEach((file: any) => {
                    formData.append("files", file.originFileObj);
                });
            }

            const response = await AxiosClient.post(API_ENDPOINTS.TICKETS.CREATE, formData, {
                headers: {
                    "Content-Type": "multipart/form-data",
                },
            });

            if (response.status === 201) {
                message.success({
                    content: t('addTicket.createSuccess'),
                    icon: <SendOutlined style={{ color: "#52c41a" }} />
                });
                form.resetFields();
                navigate(-1);
            }
        } catch (error) {
            console.error("Failed:", error);
            message.error(t('addTicket.createFailed'));
        }
        setLoading(false);
    };

    return (
        <Layout style={{ minHeight: '100vh', minWidth: '20vw', backgroundColor: '#f5f7fa' }}>
            <PageHeader
                title={t('addTicket.pageTitle')}
            />
            <div className='px-8 py-8'>

                <Card className='w-full shadow-xl rounded-2xl p-8 mx-auto mt-8'>

                    {loading && (
                        <div style={{
                            textAlign: 'center',
                            padding: '20px',
                            backgroundColor: 'rgba(255,255,255,0.8)',
                            position: 'absolute',
                            top: 0,
                            left: 0,
                            right: 0,
                            bottom: 0,
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            zIndex: 10,
                            borderRadius: '8px'
                        }}>
                            <Spin size="large" tip={t('addTicket.creatingTicket')} />
                        </div>
                    )}

                    <Form
                        form={form}
                        layout="vertical"
                        onFinish={handleSubmit}
                        requiredMark="optional"
                        style={{ padding: '0 0.5rem' }}
                    >
                        <Col span={24}>
                            <Form.Item
                                label={<span style={{ fontWeight: 500 }}>{t('addTicket.ticketTitleLabel')}</span>}
                                name="title"
                                tooltip={t('addTicket.ticketTitleTooltip')}
                                validateStatus={errors.title ? "error" : ""}
                                help={errors.title}
                                rules={[{ required: true, message: t('addTicket.ticketTitleRequired') }]}
                            >
                                <Input
                                    placeholder={t('addTicket.ticketTitlePlaceholder')}
                                    size="large"
                                    prefix={<FileTextOutlined style={{ color: '#bfbfbf' }} />}
                                />
                            </Form.Item>
                        </Col>
                        <Row>
                            <Col xs={24} md={12}>
                                <Form.Item
                                    label={<span style={{ fontWeight: 500 }}>{t('addTicket.systemLabel')}</span>}
                                    name="externalSystemId"
                                    tooltip={t('addTicket.systemTooltip')}
                                >
                                    <Select
                                        placeholder={t('addTicket.systemPlaceholder')}
                                        allowClear
                                    >
                                        {externalSystems.map((system) => (
                                            <Option key={system.id} value={system.id}>
                                                {system.name}
                                            </Option>
                                        ))}
                                    </Select>
                                </Form.Item>
                            </Col>

                            <Col xs={24} md={12}>
                                <Form.Item
                                    label={<span style={{ fontWeight: 500 }}>{t('addTicket.ticketTypeLabel')}</span>}
                                    name="ticketType"
                                    tooltip={t('addTicket.ticketTypeTooltip')}
                                    validateStatus={errors.ticketType ? "error" : ""}
                                    help={errors.ticketType}
                                    rules={[{ required: true, message: t('addTicket.ticketTypeRequired') }]}
                                >
                                    <Select placeholder={t('addTicket.ticketTypePlaceholder')}>
                                        {TicketTypes.map((type) => (
                                            <Option key={type} value={type}>
                                                {type}
                                            </Option>
                                        ))}
                                    </Select>
                                </Form.Item>
                            </Col>
                        </Row>
                        <Col span={24}>
                            <Form.Item
                                label={<span style={{ fontWeight: 500 }}>{t('addTicket.descriptionLabel')}</span>}
                                name="description"
                                tooltip={t('addTicket.descriptionTooltip')}
                                validateStatus={errors.description ? "error" : ""}
                                help={errors.description}
                                rules={[{ required: true, message: t('addTicket.descriptionRequired') }]}
                            >
                                <TextArea
                                    placeholder={t('addTicket.descriptionPlaceholder')}
                                    autoSize={{ minRows: 4, maxRows: 8 }}
                                    style={{ fontSize: '14px' }}
                                />
                            </Form.Item>
                        </Col>

                        <Col span={24}>
                            <Form.Item
                                label={<span style={{ fontWeight: 500 }}>{t('addTicket.attachmentsLabel')}</span>}
                                name="attachments"
                                extra={
                                    <div>
                                        {t('addTicket.attachmentsExtra')}
                                        <div style={{ marginTop: '4px', color: '#ff4d4f' }}>
                                            <WarningOutlined /> {t('addTicket.fileRestrictions', { 
                                                size: '25MB', 
                                                types: ALLOWED_EXTENSIONS.join(', ') 
                                            })}
                                        </div>
                                    </div>
                                }
                                tooltip={t('addTicket.attachmentsTooltip')}
                                validateStatus={errors.fileError ? "error" : ""}
                                help={errors.fileError}
                            >
                                <Upload
                                    multiple
                                    listType="picture"
                                    fileList={fileList}
                                    beforeUpload={(file) => {
                                        return false; // Prevent auto upload
                                    }}
                                    onChange={handleFileChange}
                                    onRemove={(file) => {
                                        const index = fileList.indexOf(file);
                                        const newFileList = fileList.slice();
                                        newFileList.splice(index, 1);
                                        setFileList(newFileList);
                                        form.setFieldsValue({ attachments: newFileList });
                                    }}
                                >
                                    <Button icon={<UploadOutlined />}>{t('addTicket.addFiles')}</Button>
                                </Upload>
                            </Form.Item>
                        </Col>

                        <div className="pt-4 flex justify-end space-x-3">
                            <Button
                                onClick={() => navigate(-1)}
                            >
                                {t('addTicket.cancelButton')}
                            </Button>
                            <Button
                                type="primary"
                                htmlType="submit"
                                disabled={!submittable}
                                loading={loading}
                                icon={<SendOutlined />}
                            >
                                {t('addTicket.submitTicket')}
                            </Button>
                        </div>
                    </Form>
                </Card>
            </div>
        </Layout>
    );
};

export default AddTicket;
