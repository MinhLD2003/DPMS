import React from "react";
import { Upload, Button, message } from "antd";
import { UploadOutlined } from "@ant-design/icons";
import type { UploadProps } from "antd";
import AxiosClient from "../../configs/axiosConfig";

interface ImportExcelButtonProps {
    apiUrl: string;
    buttonText: string;
    onImportSuccess?: () => void;
}

const ImportExcelButton: React.FC<ImportExcelButtonProps> = ({ apiUrl, buttonText, onImportSuccess }) => {
    const uploadProps: UploadProps = {
        name: "file",
        accept: ".xlsx,.xls",
        showUploadList: false,
        customRequest: async ({ file, onSuccess, onError }) => {
            const formData = new FormData();
            formData.append("importFile", file as Blob);

            try {
                const response = await AxiosClient.post(apiUrl, formData, {
                    headers: {
                        "Content-Type": "multipart/form-data",
                    },
                });
                message.success("File uploaded successfully!");
                if (onSuccess) {
                    onSuccess(response.data);
                    if(onImportSuccess)
                    onImportSuccess();
                }
            } catch (error: any) {
                message.error(error.data?.[0]?.message|| "Upload failed. Please try again.");
            }
        },
    };

    return (
        <Upload {...uploadProps}>
            <Button icon={<UploadOutlined style={{ fontSize: '18px' }}/>}
                style={{ backgroundColor: 'green', color: 'white' }}
                size='middle'
            >{buttonText}</Button>
        </Upload>
    );
};

export default ImportExcelButton;