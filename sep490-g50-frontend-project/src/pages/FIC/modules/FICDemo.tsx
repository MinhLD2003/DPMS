import React, { useEffect, useState } from "react";
import { Form, Checkbox, Table, message, Button, Spin, Input, Space, Select, Row, Col, Card } from "antd";
import AxiosClient from "../../../configs/axiosConfig";
import { FICStructure, FormElement } from "../models/FICTemplateViewModel";
import { getTableData, sortFormElements } from "../../../utils/FICTemplateUtils";
import FICFormTable from "../../../components/forms/FICFormTable";
import { useNavigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";
import PageHeader from "../../../components/common/PageHeader";
const Text = Input;
const { Option } = Select;

interface System {
    id: string;
    name: string;
}

interface Template {
    id: string;
    name: string;
    version: string;
}

const CreateSystemFIC: React.FC = () => {
    const { t } = useTranslation();
    const { id } = useParams<{ id: string }>();

    const [formData, setFormData] = useState<FICStructure | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [system, setSystem] = useState<System | null>(null);
    const [templates, setTemplates] = useState<Template[]>([]);
    const [selectedTemplateId, setSelectedTemplateId] = useState<string | null>(null);
    const [loadingSystem, setLoadingSystem] = useState<boolean>(false);
    const [loadingTemplates, setLoadingTemplates] = useState<boolean>(false);
    const [form] = Form.useForm();
    const navigate = useNavigate();

    // Fetch single system details on component mount
    useEffect(() => {
        const fetchSystemDetails = async () => {
            if (!id) return;

            setLoadingSystem(true);
            try {
                const response = await AxiosClient.get(`/ExternalSystem/${id}/get-system-details`);
                // Assuming the API returns a single system object instead of an array
                setSystem(response.data);
            } catch (error) {
                message.error(t('createSystemFIC.systemDetailsFetchFailed'));
            }
            setLoadingSystem(false);
        };

        fetchSystemDetails();
    }, [id]);

    // Fetch templates on component mount
    useEffect(() => {
        const fetchTemplates = async () => {
            setLoadingTemplates(true);
            try {
                const response = await AxiosClient.get("/Form/get-templates?formStatus=2");
                setTemplates(response.data);
            } catch (error) {
                message.error(t('createSystemFIC.templatesFetchFailed'));
            }
            setLoadingTemplates(false);
        };

        fetchTemplates();
    }, []);
    const getNewestVersion = (templates: Template[]): string => {
        return templates
            .map(t => t.version)
            .sort((a, b) => parseFloat(b) - parseFloat(a))[0];
    };
    // Fetch form data only when template is selected (system is already known from URL)
    const fetchForm = async () => {
        if (!id || !selectedTemplateId) return;

        setLoading(true);
        try {
            const response = await AxiosClient.get(`/Form/${selectedTemplateId}`);
            const sortedElements = sortFormElements(response.data.formElements);
            setFormData(response.data);
            message.success(t('createSystemFIC.formFetchSuccess'));
        } catch (error) {
            message.error(t('createSystemFIC.formFetchFailed'));
        }
        setLoading(false);
    };

    // Trigger form fetch when template is selected
    useEffect(() => {
        if (id && selectedTemplateId) {
            fetchForm();
        }
    }, [id, selectedTemplateId, t]);

    const handleTemplateChange = (value: string) => {
        setSelectedTemplateId(value);
        // Clear form data when template changes
        setFormData(null);
        form.resetFields();
    };

    const handleSubmit = async () => {
        if (!formData || !id || !selectedTemplateId) {
            message.warning(t('createSystemFIC.selectTemplateWarning'));
            return;
        }

        try {
            const values = await form.validateFields();
            // values = { [formElementId]: value }
            const responses = Object.entries(values).map(([formElementId, value]) => ({
                formElementId,
                value: typeof value === "boolean" ? value.toString() : value, // Convert booleans to string if needed
            }));

            const jsonResponsePayload = {
                formId: selectedTemplateId,
                systemId: id,
                responses,
            };

            await AxiosClient.post("/Form/submit", jsonResponsePayload);
            message.success(t('createSystemFIC.formSubmitSuccess'));
            navigate(-1);
        } catch (error: any) {
            message.error(t('createSystemFIC.formSubmitFailed'));
        }
    };

    const dataSource = formData ? getTableData(formData.formElements) : [];

    return (
        <section>
            <PageHeader title={t('createSystemFIC.pageTitle')} />
            <Card>
                <Row gutter={16} className="mb-6">
                    <Col span={12}>
                        <div className="mb-4">
                            <label className="block mb-2 font-medium">{t('createSystemFIC.systemLabel')}:</label>
                            {loadingSystem ? (
                                <Spin size="small" />
                            ) : system ? (
                                <div className="p-2 border rounded bg-gray-50">{system.name}</div>
                            ) : (
                                <div className="p-2 border rounded bg-gray-50 text-gray-500">{t('createSystemFIC.systemNotFound')}</div>
                            )}
                        </div>
                    </Col>
                    <Col span={12}>
                        <div className="mb-4">
                            <label className="block mb-2 font-medium">{t('createSystemFIC.selectTemplateLabel')}:</label>
                            <Select
                                placeholder={t('createSystemFIC.selectTemplatePlaceholder')}
                                style={{ width: '100%' }}
                                onChange={handleTemplateChange}
                                loading={loadingTemplates}
                            >
                                {templates.map((template) => {
                                    const isNewestVersion = template.version === getNewestVersion(templates);
                                    return (
                                        <Option key={template.id} value={template.id}>
                                            <span className="flex items-center justify-between">
                                                <span>{template.name}</span>
                                                <span className={`ml-2 ${isNewestVersion
                                                        ? 'bg-green-100 text-green-800 px-2 py-0.5 rounded-full text-xs font-medium'
                                                        : 'text-gray-500 text-xs'
                                                    }`}>
                                                    v{template.version}
                                                    {isNewestVersion && ' (Latest)'}
                                                </span>
                                            </span>
                                        </Option>
                                    );
                                })}
                            </Select>
                        </div>
                    </Col>
                </Row>

                {loading && <Spin tip={t('createSystemFIC.loadingForm')} className="mt-6 mb-6 block" />}

                {formData && (
                    <Form form={form} layout="vertical">
                        <h2 className="text-xl font-bold mb-4">{formData.name}</h2>
                        <FICFormTable dataSource={getTableData(formData.formElements)} form={form} />
                        <Button
                            type="primary"
                            onClick={handleSubmit}
                            style={{ marginTop: "20px" }}
                        >
                            {t('createSystemFIC.submitButton')}
                        </Button>
                    </Form>
                )}
                {!loading && !formData && selectedTemplateId && (
                    <div className="text-center p-8">
                        <p>{t('loadingForm')}</p>
                    </div>
                )}

                {!loading && !formData && !selectedTemplateId && (
                    <div className="text-center p-8 bg-gray-100 rounded">
                        <p>{t('createSystemFIC.selectTemplatePrompt')}</p>
                    </div>
                )}
            </Card>
        </section>
    );
};

export default CreateSystemFIC;