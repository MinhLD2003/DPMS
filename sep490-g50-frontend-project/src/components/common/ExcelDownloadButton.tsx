import React from 'react';
import { Button, message } from 'antd';
import { DownloadOutlined } from '@ant-design/icons';
import { downloadFileFromApi } from '../../common/FileDownloadAPI';

interface FileDownloadButtonProps {
    apiPath: string;
    filename?: string;
    buttonText?: string;
}

const FileDownloadButton: React.FC<FileDownloadButtonProps> = ({ apiPath, filename, buttonText = '' }) => {
    const handleDownload = async () => {
        try {
            const blob = await downloadFileFromApi(apiPath);

            const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', filename || `Download_${new Date().toISOString().slice(0, 19)}.xlsx`);
            document.body.appendChild(link);
            link.click();
            link.parentNode?.removeChild(link);

            message.success('File downloaded successfully');
        } catch (error) {
            console.error('Download error:', error);
            message.error('Failed to download file');
        }
    };

    return (
        <Button
            style={{backgroundColor: 'green', color: 'white' }}
            icon={<DownloadOutlined style={{ fontSize: '18px' }}/>}
            size='middle'
            onClick={handleDownload}
        >
            {buttonText}
        </Button>
    );
};

export default FileDownloadButton;



