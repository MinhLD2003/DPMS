import React, { useEffect, useState } from "react";
import { Form, Checkbox, message, Spin, Typography, Layout, Card } from "antd";
import { useParams } from "react-router-dom";
import AxiosClient from "../../../configs/axiosConfig";
import { FICStructure, FormElement } from "../models/FICSubmissionViewModel";
import { sortFormElements } from "../../../utils/FICTemplateUtils";
import TableWithSkeleton from "../../../components/forms/TableWithSkeleton";
import { useTranslation } from "react-i18next";
import PageHeader from "../../../components/common/PageHeader";

const { Text } = Typography;

const SubmissionDetail: React.FC = () => {
  const { t } = useTranslation();
  const [formData, setFormData] = useState<FICStructure | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [form] = Form.useForm();
  const { id } = useParams<{ id: string }>();

  useEffect(() => {
    const fetchForm = async () => {
      setLoading(true);
      try {
        const response = await AxiosClient.get(`/Form/submission/${id}`);
        const sortedElements = sortFormElements(response.data.formElements);
        setFormData({ ...response.data, formElements: sortedElements });
        message.success(t('submissionDetail.formFetchSuccess'));
      } catch (error) {
        message.error(t('submissionDetail.formFetchFailed'));
      }
      finally {
        setLoading(false);
      }
    };
    fetchForm();
  }, [id, t]);

  // --- Update Checkbox Value ---
  const updateValue = (elements: FormElement[], elementId: string, checked: boolean): FormElement[] => {
    return elements.map((el) => ({
      ...el,
      value: el.id === elementId ? checked.toString() : el.value,
      children: el.children.length > 0 ? updateValue(el.children, elementId, checked) : el.children,
    }));
  };

  const handleCheckboxChange = (elementId: string, checked: boolean) => {
    if (formData) {
      const updatedElements = updateValue(formData.formElements, elementId, checked);
      setFormData({ ...formData, formElements: updatedElements });
    }
  };

  // --- Extract Table Data ---
  const getTableData = (elements: FormElement[], parentKey: string = ""): any[] => {
    let data: any[] = [];

    elements.forEach((element, index) => {
      const key = `${parentKey}${index}`;

      if (element.dataType === "Boolean") {
        data.push({
          key,
          name: element.name,
          input: element.children.length === 0 ? (
            <Checkbox
              checked={element.value === "true"}
              disabled // 🔥 Read-only
              onChange={(e) => handleCheckboxChange(element.id, e.target.checked)}
            />
          ) : null,
          hasChildren: element.children.length > 0,
        });
      }

      if (element.dataType === "Text") {
        data.push({
          key,
          name: element.name,
          input: element.children.length === 0 ? <Text>{element.value}</Text> : null,
          hasChildren: element.children.length > 0,
        });
      }
      if (element.dataType === null) {
        data.push({
          key,
          name: element.name,
          hasChildren: element.children.length > 0,
        });
      }


      if (element.children.length > 0) {
        data = data.concat(getTableData(element.children, `${key}-`));
      }
    });

    return data;
  };

  // --- Loading / No Data ---
  if (!formData) return <p>{t('loadingForm')}</p>;


  const columns = [
    {
      title: t('submissionDetail.tableHeader'),
      dataIndex: "name",
      key: "name",
      width: "80%",
    },
    {
      title: t('submissionDetail.input'),
      dataIndex: "input",
      key: "input",
      align: "center" as const,
      width: "20%",
    },
  ];

  const dataSource = getTableData(formData.formElements);

  // --- Render ---
  return (
    <Layout style={{ minHeight: '100vh', backgroundColor: '#f5f7fa', borderRadius: '12px' }}>
      <PageHeader
        title={formData.name}
      />
      <h1 className="page-title my-8">{formData.name}</h1>
      <Card>
        <Form form={form} layout="vertical">
          <TableWithSkeleton
            columns={columns}
            loading={loading}
            dataSource={dataSource}
            pagination={false}
            bordered
            rowClassName={(record) => (record.hasChildren ? "ml-12 font-bold bg-gray-200" : "italic")}
          />
        </Form>
      </Card>
    </Layout>
  );
};

export default SubmissionDetail;