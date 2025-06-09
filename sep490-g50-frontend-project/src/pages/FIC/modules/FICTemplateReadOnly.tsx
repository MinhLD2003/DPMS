import React, { useCallback, useEffect, useState } from "react";
import { Form, message, Button, Spin, Layout, Card } from "antd";
import { useParams } from "react-router-dom";
import AxiosClient from "../../../configs/axiosConfig";
import { FICStructure } from "../models/FICTemplateViewModel";
import { getTableData, sortFormElements } from "../../../utils/FICTemplateUtils";
import FICFormTable from "../../../components/forms/FICFormTable";
import { useTranslation } from "react-i18next";
import PageHeader from "../../../components/common/PageHeader";

const FICTemplateReadOnly: React.FC = () => {
    const { t } = useTranslation();
    const [formData, setFormData] = useState<FICStructure | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [form] = Form.useForm();
    const { id } = useParams<{ id: string }>();

    const fetchForm = useCallback(async () => {
        setLoading(true);
        try {
            const response = await AxiosClient.get(`/Form/${id}`);
            const sortedElements = sortFormElements(response.data.formElements);
            setFormData({ ...response.data, formElements: sortedElements });
        } catch (error) {
            message.error(t('ficTemplateReadOnly.fetchFailed'));
        }
        finally {
            setLoading(false);
        }
    }, [id, t]);

    useEffect(() => {
        fetchForm();
    }, [fetchForm]);


    if (!formData) return <p>{t('loadingForm')}</p>;

    const dataSource = getTableData(formData.formElements);

    return (
        <Layout style={{ minHeight: '100vh', backgroundColor: '#f5f7fa', borderRadius:'12px' }}>
            <PageHeader
                title={formData.name}
            />
            <Card>
            <h1 className="page-title my-8">{formData.name}</h1>
            <Form form={form} layout="vertical">
                <FICFormTable dataSource={dataSource} form={form} />
            </Form>
            </Card>
        </Layout>
    );
};

export default FICTemplateReadOnly;