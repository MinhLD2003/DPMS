import React, { useState } from 'react';
import {
  ArrowLeftOutlined,
  DesktopOutlined,
  FileTextOutlined,
  CodeOutlined
} from '@ant-design/icons';
import {
  Typography,
  Breadcrumb,
  Tabs,
  Button,
  Space,
  theme,
  Layout,
  Card
} from 'antd';
import OverviewTab from './OverviewTab.tsx';
import ComplianceTab from './ComplianceTab.tsx';
import ConsentTab from './ConsentTab.tsx';
import { useTranslation } from 'react-i18next';
import DSARList from './DsarTab.tsx';
import PageHeader from '../../../components/common/PageHeader.tsx';
import { getUserFeatures } from '../../../utils/jwtDecodeUtils.ts';

const SystemDetail: React.FC = () => {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = useState('overview');

  // Handle tab change
  const handleTabChange = (key: string) => {
    setActiveTab(key);
  };
  const userFeatures = getUserFeatures();

  return (
    <Layout style={{ minHeight: '80vh', backgroundColor: '#f5f7fa', borderRadius: '12px' }}>
      <PageHeader title={t('systemDetail.systemDetails')} />
      <div className="px-4 py-6">
        {/* Tabs with custom styling */}
        <Tabs
          activeKey={activeTab}
          onChange={handleTabChange}
          type="card"
          className="custom-tabs"
          items={[
            {
              key: 'overview',
              label: (
                <Space>
                  <FileTextOutlined />
                  {t('systemDetail.overview')}
                </Space>
              ),
              children: <OverviewTab />
            },
            ...(userFeatures.includes('/api/Form/get-submissions_GET') ? [{

              key: 'compliance',
              label: (
                <Space>
                  <FileTextOutlined />
                  {t('systemDetail.compliance')}
                </Space>
              ),
              children: <ComplianceTab />
            }] : []),
            ...(userFeatures.includes('/api/Consent/consent-log/{id}_GET') ? [{
              key: 'consent-management',
              label: (
                <Space>
                  <FileTextOutlined />
                  {t('systemDetail.consentManagement')}
                </Space>
              ),
              children: <ConsentTab />
            }] : []),

            ...(userFeatures.includes('/api/Dsar/get-list/{id}_GET') ? [{
              key: 'dsar-management',
              label: (
                <Space>
                  <FileTextOutlined />
                  {t('systemDetail.dsar')}
                </Space>
              ),
              children: <DSARList />
            }] : []),
          ]}
        />
      </div>
    </Layout>
  );
};

export default SystemDetail;