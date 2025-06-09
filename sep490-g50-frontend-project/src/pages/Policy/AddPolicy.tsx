import React, { useState, useEffect } from 'react';
import { Card, Form, Input, Button, Space, Select, message, Typography, Row, Col, Spin, Divider } from 'antd';
import { SaveOutlined, ArrowLeftOutlined, FileTextOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import AxiosClient from '../../configs/axiosConfig';
import { CKEditor } from '@ckeditor/ckeditor5-react';
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';

const { Title, Text } = Typography;
const { Option } = Select;
const { TextArea } = Input;

interface PolicyFormData {
    policyCode: string;
    title: string;
    description: string;
    content: string;
    status: string;
}

const AddPolicy: React.FC = () => {
    const [form] = Form.useForm();
    const navigate = useNavigate();
    const [loading, setLoading] = useState<boolean>(false);
    const [submitting, setSubmitting] = useState<boolean>(false);
    const [editorData, setEditorData] = useState<string>('');

    useEffect(() => {
        const styleEl = document.createElement('style');
        styleEl.innerHTML = `
        /* CKEditor Container Styling */
        .ckeditor-container {
            width: 100% !important;
            margin-bottom: 24px;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 1px 3px rgba(0,0,0,0.05);
            transition: box-shadow 0.3s ease;
        }
        
        .ckeditor-container:hover {
            box-shadow: 0 3px 6px rgba(0,0,0,0.1);
        }

        /* Editor Frame - Ensure full width */
        .ck-editor {
            width: 100% !important;
            border-radius: 8px;
            overflow: hidden;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
        }
        
        /* Fix for editor root element width */
        .ck.ck-editor__main {
            width: 100% !important;
            max-width: none !important;
        }

        /* Toolbar Styling */
        .ck.ck-toolbar {
            background: #f5f7fa !important;
            border-bottom: 1px solid #e8e8e8 !important;
            padding: 8px !important;
            border-radius: 8px 8px 0 0 !important;
            width: 100% !important;
        }
        
        .ck.ck-toolbar__separator {
            background: #d9d9d9;
            margin: 0 10px;
        }
        
        /* Toolbar Button Styling */
        .ck.ck-button {
            border-radius: 4px !important;
            padding: 6px 10px !important;
            transition: all 0.2s ease;
        }
        
        .ck.ck-button:hover {
            background: #e6f7ff !important;
        }
        
        .ck.ck-button.ck-on {
            background: #e6f7ff !important;
            color: #1890ff !important;
            border-color: #91d5ff !important;
        }
        
        /* Content Area Styling - Fix narrow text */
        .ck-editor__editable_inline {
            width: 100% !important;
            min-height: 450px !important;
            max-height: 650px !important;
            padding: 20px !important; /* Reduced padding */
            background-color: #ffffff !important;
            color: #333333;
            line-height: 1.6;
            font-size: 15px;
            border: none !important;
            box-shadow: none !important;
            transition: background-color 0.3s ease;
            box-sizing: border-box !important; 
            max-width: none !important;
        }
        
        /* Ensure content inside editor uses full width */
        .ck-editor__editable_inline > * {
            max-width: none !important;
            width: 100% !important;
        }
        
        /* Ensure editor root takes up full width */
        .ck-blurred, .ck-focused {
            width: 100% !important;
            max-width: none !important;
        }
        
        .ck-editor__editable_inline:focus {
            background-color: #ffffff !important;
        }
        
        /* Typography in Editor - fix width issues */
        .ck-editor__editable h1,
        .ck-editor__editable h2,
        .ck-editor__editable h3,
        .ck-editor__editable p,
        .ck-editor__editable div,
        .ck-editor__editable blockquote {
            max-width: 100% !important;
            width: auto !important;
            margin-right: 0 !important;
            margin-left: 0 !important;
            box-sizing: border-box !important;
        }
        
        .ck-editor__editable h1 {
            font-size: 2em !important;
            margin-bottom: 0.75em !important;
            color: #222222;
            font-weight: 500;
        }
        
        .ck-editor__editable h2 {
            font-size: 1.5em !important;
            margin-bottom: 0.75em !important;
            color: #333333;
            font-weight: 500;
        }
        
        .ck-editor__editable h3 {
            font-size: 1.25em !important;
            margin-bottom: 0.75em !important;
            color: #444444;
            font-weight: 500;
        }
        
        .ck-editor__editable p {
            margin-bottom: 1em !important;
        }
        
        /* List Styling */
        .ck-editor__editable ul,
        .ck-editor__editable ol {
            padding-left: 24px !important;
            margin-bottom: 1em !important;
            width: auto !important;
            max-width: 100% !important;
        }
        
        /* Table Styling */
        .ck-editor__editable table {
            border-collapse: collapse;
            margin: 16px 0;
            width: 100% !important;
            max-width: 100% !important;
        }
        
        .ck-editor__editable table td,
        .ck-editor__editable table th {
            border: 1px solid #d9d9d9;
            padding: 8px 12px;
        }
        
        .ck-editor__editable table th {
            background-color: #f5f7fa;
        }
        
        /* Blockquote Styling */
        .ck-editor__editable blockquote {
            border-left: 4px solid #1890ff;
            padding-left: 16px;
            margin-left: 0;
            font-style: italic;
            color: #666666;
            width: auto !important;
            max-width: 100% !important;
        }
        
        /* Link Styling */
        .ck-editor__editable a {
            color: #1890ff;
            text-decoration: none;
        }
        
        .ck-editor__editable a:hover {
            text-decoration: underline;
        }
        
        /* Dropdown styling */
        .ck.ck-dropdown .ck-dropdown__panel {
            border-radius: 4px !important;
            border-color: #d9d9d9 !important;
            box-shadow: 0 3px 6px -4px rgba(0,0,0,0.12), 0 6px 16px 0 rgba(0,0,0,0.08), 0 9px 28px 8px rgba(0,0,0,0.05) !important;
        }
        
        /* Fix for content restrictions */
        .ck-restricted-editing_mode_standard {
            width: 100% !important;
        }
        
        /* Override any CKEditor internal width restrictions */
        .ck-content {
            width: 100% !important;
            max-width: none !important;
        }
        
        /* Override any container constraints */
        .ck.ck-editor__editable > .ck-restricted-editing_mode_standard {
            width: 100% !important;
            max-width: none !important;
        }
        
        /* Editor wrapper components */
        .editor-header {
            background: #fafafa;
            padding: 12px 16px;
            border-bottom: 1px solid #f0f0f0;
            border-radius: 8px 8px 0 0;
        }
        
        .editor-footer {
            background: #fafafa;
            padding: 10px 16px;
            border-top: 1px solid #f0f0f0;
            border-radius: 0 0 8px 8px;
            display: flex;
            justify-content: space-between;
        }
        
        /* Responsive adjustments */
        @media (max-width: 768px) {
            .ck-editor__editable_inline {
                min-height: 350px !important;
                padding: 16px !important;
            }
        }
        
        .add-policy-container {
            padding: 24px;
        }
        
        .main-card {
            box-shadow: 0 4px 12px rgba(0,0,0,0.08);
            border-radius: 12px;
        }
        
        .form-section {
            margin-bottom: 24px;
        }
        
        .action-buttons {
            margin-top: 32px;
        }
        
        .page-header {
            border-bottom: 1px solid #f0f0f0;
            padding-bottom: 16px;
            margin-bottom: 24px;
        }
        `;
        document.head.appendChild(styleEl);

        return () => {
            document.head.removeChild(styleEl);
        };
    }, []);

    // Form validation rules
    const rules = {
        required: [{ required: true, message: 'This field is required' }],
        policyCode: [
            { required: true, message: 'Policy code is required' },
            { max: 50, message: 'Policy code cannot exceed 50 characters' },
            { pattern: /^[A-Za-z0-9-_]+$/, message: 'Only alphanumeric characters, hyphens and underscores are allowed' }
        ],
        title: [
            { required: true, message: 'Title is required' },
            { max: 100, message: 'Title cannot exceed 100 characters' }
        ],
        description: [
            { max: 500, message: 'Description cannot exceed 500 characters' }
        ]
    };

    const styles = {
        errorText: {
            color: '#ff4d4f',
            fontSize: '14px',
            marginTop: '5px'
        }
    };

    const handleSave = async () => {
        try {
            // Validate form fields
            await form.validateFields();
            // Prepare form data
            const values = form.getFieldsValue();
            values.content = editorData;

            if (editorData.length === 0) {
                message.error('Policy content is required');
                return;
            }

            setSubmitting(true);
            try {
                const response = await AxiosClient.post('/PrivacyPolicy', values);
                if (response.data) {
                    message.success('Policy created successfully!');
                    navigate('/dashboard/policy');
                }
            } catch (error: any) {
                console.error('Error creating policy:', error);
                message.error(error.data.message || 'Failed to create policy');
            } finally {
                setSubmitting(false);
            }
        } catch (errorInfo) {
            message.error('Please fill in all required fields correctly');
        }
    };

    

    const handleCancel = () => {
        navigate('../policies');
    };

    const handleEditorChange = (_event: any, editor: any) => {
        const data = editor.getData();
        setEditorData(data);
    };

    // Calculate character and word count from editor data
    const getContentStats = () => {
        if (!editorData) return { chars: 0, words: 0 };

        // Strip HTML tags for accurate word count
        const plainText = editorData.replace(/<[^>]*>/g, ' ');
        const words = plainText.trim().split(/\s+/).filter(word => word.length > 0);

        return {
            chars: plainText.length,
            words: words.length
        };
    };

    const contentStats = getContentStats();

    return (
        <div className="add-policy-container">
            <Card bordered={false} className="main-card">
                <div className="page-header">
                    <Row justify="space-between" align="middle">
                        <Col>
                            <Space align="center">
                                <FileTextOutlined style={{ fontSize: '24px', color: '#1890ff' }} />
                                <Title level={2} style={{ margin: 0 }}>Add New Policy</Title>
                            </Space>
                        </Col>
                    </Row>
                </div>

                <Form
                    form={form}
                    layout="vertical"
                    initialValues={{
                        status: 'Active',
                        description: '',
                        content: ''
                    }}
                >
                    <div className="form-section">
                        <Title level={5}>Basic Information</Title>
                        <Divider style={{ margin: '12px 0 24px' }} />

                        <Row gutter={24}>
                            <Col span={12}>
                                <Form.Item name="policyCode" label="Policy Code" rules={rules.policyCode}>
                                    <Input placeholder="Enter policy code (e.g. POL-001)" maxLength={50} />
                                </Form.Item>
                            </Col>
                            <Col span={12}>
                                <Form.Item name="title" label="Policy Title" rules={rules.title}>
                                    <Input placeholder="Enter policy title" maxLength={100} />
                                </Form.Item>
                            </Col>
                        </Row>
                        <Form.Item name="description" label="Description" rules={rules.description}>
                            <TextArea
                                placeholder="Enter a brief description of this policy"
                                rows={3}
                                maxLength={500}
                                showCount
                            />
                        </Form.Item>
                    </div>

                    <div className="form-section">
                        <Title level={5}>Policy Content</Title>
                        <Divider style={{ margin: '12px 0 24px' }} />
                        <Form.Item label="Policy Content" required>
                            <div className="ckeditor-container">
                                <div className="editor-header">
                                    <Text strong>Format your policy content with the toolbar below</Text>
                                </div>
                                <CKEditor
                                    editor={ClassicEditor}
                                    data={editorData}
                                    onChange={handleEditorChange}
                                    config={{
                                        licenseKey: "eyJhbGciOiJFUzI1NiJ9.eyJleHAiOjE3NDY4MzUxOTksImp0aSI6IjNhM2U1ODAxLWVhM2EtNDUzOS04YTc0LTJiYTBiZjBhYzU3NCIsInVzYWdlRW5kcG9pbnQiOiJodHRwczovL3Byb3h5LWV2ZW50LmNrZWRpdG9yLmNvbSIsImRpc3RyaWJ1dGlvbkNoYW5uZWwiOlsiY2xvdWQiLCJkcnVwYWwiLCJzaCJdLCJ3aGl0ZUxhYmVsIjp0cnVlLCJsaWNlbnNlVHlwZSI6InRyaWFsIiwiZmVhdHVyZXMiOlsiKiJdLCJ2YyI6ImM0OTI0MGQyIn0.8TX48Zkirql8hXRqQxnovygEAameZ_AEnOjdxpO4McRSJtXtSQ7_DQDElkhyM1j1fCYdT6H5orwT4oi4g-NmBw",
                                        placeholder: 'Write your policy content here...',

                                        toolbar: {
                                            items: [
                                                // Text formatting
                                                'heading', '|',
                                                'fontSize', 'fontFamily', '|',
                                                'bold', 'italic', 'underline', 'strikethrough', '|',
                                                'fontColor', 'fontBackgroundColor', '|',

                                                // Alignment & structure
                                                'alignment', '|',
                                                'bulletedList', 'numberedList', '|',
                                                'outdent', 'indent', '|',

                                                // Advanced elements
                                                'link', 'blockQuote', 'insertTable', 'codeBlock', '|',

                                                // Actions
                                                'undo', 'redo'
                                            ],
                                            shouldNotGroupWhenFull: true
                                        },
                                        heading: {
                                            options: [
                                                { model: 'paragraph', title: 'Paragraph', class: 'ck-heading_paragraph' },
                                                { model: 'heading1', view: 'h1', title: 'Heading 1', class: 'ck-heading_heading1' },
                                                { model: 'heading2', view: 'h2', title: 'Heading 2', class: 'ck-heading_heading2' },
                                                { model: 'heading3', view: 'h3', title: 'Heading 3', class: 'ck-heading_heading3' }
                                            ]
                                        },
                                        fontSize: {
                                            options: [
                                                'tiny', 'small', 'default', 'big', 'huge'
                                            ]
                                        },
                                        fontFamily: {
                                            options: [
                                                'default',
                                                'Arial, Helvetica, sans-serif',
                                                'Courier New, Courier, monospace',
                                                'Georgia, serif',
                                                'Lucida Sans Unicode, Lucida Grande, sans-serif',
                                                'Tahoma, Geneva, sans-serif',
                                                'Times New Roman, Times, serif',
                                                'Trebuchet MS, Helvetica, sans-serif',
                                                'Verdana, Geneva, sans-serif'
                                            ]
                                        },
                                        image: {
                                            toolbar: [
                                                'imageTextAlternative', 'imageStyle:full', 'imageStyle:side'
                                            ]
                                        },
                                        table: {
                                            contentToolbar: [
                                                'tableColumn', 'tableRow', 'mergeTableCells',
                                                'tableProperties', 'tableCellProperties'
                                            ]
                                        },
                                        mediaEmbed: {
                                            previewsInData: true
                                        },
                                        // Fix content width issues
                                        removePlugins: ['RestrictedEditingMode'],
                                        autosave: {
                                            save(editor) {
                                                const data = editor.getData();
                                                console.log('Autosaving data:', data);
                                                return Promise.resolve();
                                            },
                                            waitingTime: 2000 // Save every 2 seconds
                                        },
                                        language: 'en',
                                    }}
                                />
                                <div className="editor-footer">
                                    <Text type="secondary">
                                        {editorData.length > 0 ?
                                            `Characters: ${contentStats.chars} | Words: ${contentStats.words}` :
                                            "Start typing to create your policy content"}
                                    </Text>
                                    {editorData.length > 0 && (
                                        <Text type="secondary">
                                            Last edited: {new Date().toLocaleTimeString()}
                                        </Text>
                                    )}
                                </div>
                            </div>
                            {editorData.length === 0 && (
                                <div style={styles.errorText}>
                                    Policy content is required
                                </div>
                            )}
                        </Form.Item>
                    </div>

                    <Form.Item>
                        <div className="action-buttons">
                            <Space size="middle">
                                <Button
                                    type="default"
                                    icon={<ArrowLeftOutlined />}
                                    onClick={handleCancel}
                                    size="large"
                                >
                                    Back to Policy List
                                </Button>
                                <Button
                                    type="primary"
                                    icon={<SaveOutlined />}
                                    onClick={handleSave}
                                    loading={submitting}
                                    size="large"
                                    disabled={editorData.length === 0}
                                >
                                    Save Policy
                                </Button>
                            </Space>
                        </div>
                    </Form.Item>
                </Form>
            </Card>
        </div>
    );
};

export default AddPolicy;
