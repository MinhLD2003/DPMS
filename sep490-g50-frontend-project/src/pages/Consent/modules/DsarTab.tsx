import React, { useState } from "react";
import { Button, Form, Input, Select, message } from "antd";
import { DSARRequest, DSARSchema } from "../models/DsarPostModel";
import { useFormValidation } from "../../../hooks/useFormValidation";
import AxiosClient from "../../../configs/axiosConfig";
import { useParams } from "react-router-dom";
import { useTranslation } from 'react-i18next'; // Ensure you have i18next set up
const { Option } = Select;

const DSARPage: React.FC = () => {
    const { t } = useTranslation(); // 'dsaRPage' is the namespace for your translations

    const { token } = useParams<{ token: string }>();
    const [loading, setLoading] = useState(false);
    const [form] = Form.useForm<DSARRequest>();
    const { errors, validateForm, validateField } =
        useFormValidation<DSARRequest>(form, DSARSchema, { validateOnChange: false });

    const handleSubmit = async () => {
        setLoading(true);
        try {
            if (validateForm()) {
                const values = form.getFieldsValue();
                await AxiosClient.post(`/Dsar/${token}`, values);
                message.success(t('successMessage'));
                form.resetFields();
            }
        }
        catch (errorInfo) {
            message.error(t('errorMessage'));
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="w-full px-2">
            <div className="bg-gradient-to-br from-orange-300 to-orange-500 text-white p-6 rounded-t-xl text-center mb-3">
                <h2 className="text-3xl font-extrabold text-center mb-2">
                    {t('dsarTitle')}
                </h2>
                <text className="text-center text-300 opacity-90">
                    {t('dsarSubtitle')}
                </text>
            </div>

            <Form
                form={form}
                layout="vertical"
                onFinish={handleSubmit}
                className="bg-white p-8 rounded-b-xl space-y-6 shadow-xl"
            >
                <div className="p-4">
                    <div className="grid md:grid-cols-2 gap-6">
                        <Form.Item
                            label={t('fullNameLabel')}
                            name="requesterName"
                            className="md:col-span-1"
                            validateStatus={errors.requesterName ? "error" : ""}
                            help={errors.requesterName}
                        >
                            <Input
                                onBlur={() => validateField('requesterName')}
                                placeholder={t('fullNamePlaceholder')}
                                className="rounded-lg h-12 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-all"
                            />
                        </Form.Item>
                        <Form.Item
                            label={t('emailLabel')}
                            name="requesterEmail"
                            className="md:col-span-1"
                            validateStatus={errors.requesterEmail ? "error" : ""}
                            help={errors.requesterEmail}

                        >
                            <Input
                                onBlur={() => validateField('requesterEmail')}
                                placeholder={t('emailPlaceholder')}
                                className="rounded-lg h-12 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-all"
                            />
                        </Form.Item>
                    </div>

                    <div className="grid md:grid-cols-2 gap-6">
                        <Form.Item
                            label={t('phoneLabel')}
                            name="phoneNumber"
                            className="md:col-span-1"
                            validateStatus={errors.phoneNumber ? "error" : ""}
                            help={errors.phoneNumber}
                        >
                            <Input
                                onBlur={() => validateField('phoneNumber')}
                                placeholder={t('phonePlaceholder')}
                                className="rounded-lg h-12 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-all"
                            />
                        </Form.Item>
                        <Form.Item
                            label={t('requestTypeLabel')}
                            name="type"
                            className="md:col-span-1"
                            validateStatus={errors.type ? "error" : ""}
                            help={errors.type}
                        >
                            <Select
                                placeholder={t('requestTypePlaceholder')}
                                className="w-full"
                                onBlur={() => validateField('type')}
                                onChange={(value) => form.setFieldValue("type", Number(value))}
                            >
                                <Option value={0}>{t('accessRequest')}</Option>
                                <Option value={1}>{t('deleteRequest')}</Option>
                            </Select>
                        </Form.Item>
                    </div>

                    <Form.Item
                        label={t('addressLabel')}
                        name="address"
                        validateStatus={errors.address ? "error" : ""}
                        help={errors.address}
                    >
                        <Input
                            placeholder={t('addressPlaceholder')}
                            className="rounded-lg h-12 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-all"
                        />
                    </Form.Item>

                    <Form.Item
                        label={t('descriptionLabel')}
                        name="description"
                        validateStatus={errors.description ? "error" : ""}
                        help={errors.description}
                    >
                        <Input.TextArea
                            rows={4}
                            placeholder={t('descriptionPlaceholder')}
                            className="rounded-lg focus:border-blue-500 focus:ring-2 focus:ring-blue-200 transition-all"
                        />
                    </Form.Item>

                    <Form.Item className="mt-8">
                        <button
                            type="submit"
                            disabled={loading}
                            className={`
                            w-full py-3.5 rounded-lg text-white font-bold uppercase tracking-wider
                            transition duration-300 ease-in-out transform shadow-lg
                            ${!loading
                                    ? 'bg-gradient-to-r from-orange-300 to-orange-500 hover:from-orange-400 hover:to-orange-600 hover:scale-[1.02]'
                                    : 'bg-gradient-to-r from-orange-200 to-orange-300'
                                }
                        `}
                        >
                            {loading ? t('submitting') : t('submitRequest')}
                        </button>
                    </Form.Item>
                </div>

            </Form>
        </div>
    );
};

export default DSARPage;