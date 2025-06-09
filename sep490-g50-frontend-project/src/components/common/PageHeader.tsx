import React from 'react';
import { Layout, Space, Divider, Typography, Avatar } from 'antd';
import MuiBackButton from './BackButton';
import {
  SettingOutlined,
} from "@ant-design/icons";
const { Header } = Layout;

interface PageHeaderProps {
  title: string;
}

const PageHeader: React.FC<PageHeaderProps> = ({ title }) => {
  return (
    <Header style={{ backgroundColor: 'white', borderRadius:'8px', padding: '0 24px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)', display: 'flex', alignItems: 'center' }}>
      <Space align="center">
        <MuiBackButton />
        <Divider type="vertical" style={{ height: 24 }} />
        <Space>
          <Avatar style={{ backgroundColor: '#ff7a45' }}><SettingOutlined/></Avatar>
          {/* <Text strong>DPIA</Text> */}
          <text type="secondary" style={{ color: '#8c8c8c' }}>/</text>
          <text type="secondary">{title}</text>
        </Space>
      </Space>
    </Header>
  );
};

export default PageHeader;
