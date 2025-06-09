import React, { useEffect, useState } from 'react';
import { Card, Button, Badge, Spin, Row, Col, Typography, Space } from 'antd';
import {
  BellOutlined,
  FileTextOutlined,
  UserOutlined,
  ReloadOutlined,
  DashboardOutlined,
  WarningOutlined,
  ClockCircleOutlined,
  LaptopOutlined,
  TagOutlined
} from '@ant-design/icons';
import Clock from '../components/common/Clock';
import AxiosClient from '../configs/axiosConfig';
import { useNavigate } from 'react-router-dom';
import FeatureGuard from '../routes/FeatureGuard';
import { useTranslation } from 'react-i18next'; // Import useTranslation

const { Title, Text } = Typography;

interface StatItem {
  label: string; // Label will now come from t()
  value: number;
  icon: React.ReactNode;
  color: string;
  onRefresh?: () => Promise<void>;
  loading?: boolean;
  route?: string;
  requiredFeature: string;
}

// Define QuickAction interface for clarity
interface QuickActionItem {
  title: string; // Title will now come from t()
  icon: React.ReactNode;
  onClick: () => void;
  type: 'primary' | 'link' | 'text' | 'default' | 'dashed'; // Removed 'ghost' type
  requiredFeature: string;
}


const MainDashboard: React.FC = () => {
  const { t } = useTranslation(); // Initialize translation hook
  const navigate = useNavigate();
  const [totalSystem, setTotalSystem] = useState<number>(0);
  const [totalSubmittedDsar, setTotalSubmittedDsar] = useState<number>(0);
  const [totalRequiredDsar, setTotalRequiredDsar] = useState<number>(0);
  const [totalRisk, setTotalRisk] = useState<number>(0);
  const [totalDpia, setTotalDpia] = useState<number>(0);
  const [totalPendingTickets, setTotalPendingTickets] = useState<number>(0);
  const [loading, setLoading] = useState<boolean>(true);
  const [loadingStats, setLoadingStats] = useState<Record<string, boolean>>({
    systems: false,
    submittedDsars: false,
    requiredDsars: false,
    risks: false,
    dpias: false,
    tickets: false,
  });

  // --- Fetching functions remain the same ---
  const fetchSystems = async () => {
    setLoadingStats(prev => ({ ...prev, systems: true }));
    try {
      const systemsResponse = await AxiosClient.get('/ExternalSystem');
      const systems = Array.isArray(systemsResponse.data) ? systemsResponse.data : [];
      setTotalSystem(systems.length);
    } catch (error) {
      console.error('Error fetching systems:', error);
    } finally {
      setLoadingStats(prev => ({ ...prev, systems: false }));
    }
  };

  const fetchSubmittedDsars = async () => {
    setLoadingStats(prev => ({ ...prev, submittedDsars: true }));
    try {
      const systemsResponse = await AxiosClient.get('/ExternalSystem');
      const systems = Array.isArray(systemsResponse.data) ? systemsResponse.data : [];

      const dsarCounts = await Promise.all(
        systems.map(async (system) => {
          try {
            const response = await AxiosClient.get(`/Dsar/get-list/${system.id}?status=0`);
            return response.data.totalRecords || 0;
          } catch (error) {
            console.error(`Error fetching submitted DSARs for system ${system.id}:`, error);
            return 0;
          }
        })
      );

      const total = dsarCounts.reduce((acc, curr) => acc + curr, 0);
      setTotalSubmittedDsar(total);
    } catch (error) {
      console.error('Error fetching submitted DSARs:', error);
    } finally {
      setLoadingStats(prev => ({ ...prev, submittedDsars: false }));
    }
  };

  const fetchRequiredDsars = async () => {
    setLoadingStats(prev => ({ ...prev, requiredDsars: true }));
    try {
      const systemsResponse = await AxiosClient.get('/ExternalSystem');
      const systems = Array.isArray(systemsResponse.data) ? systemsResponse.data : [];

      const dsarCounts = await Promise.all(
        systems.map(async (system) => {
          try {
            const response = await AxiosClient.get(`/Dsar/get-list/${system.id}?status=1`);
            return response.data.totalRecords || 0;
          } catch (error) {
            console.error(`Error fetching required DSARs for system ${system.id}:`, error);
            return 0;
          }
        })
      );

      const total = dsarCounts.reduce((acc, curr) => acc + curr, 0);
      setTotalRequiredDsar(total);
    } catch (error) {
      console.error('Error fetching required DSARs:', error);
    } finally {
      setLoadingStats(prev => ({ ...prev, requiredDsars: false }));
    }
  };

  const fetchRisks = async () => {
    setLoadingStats(prev => ({ ...prev, risks: true }));
    try {
      const response = await AxiosClient.get('/Risk?riskImpactAfterMitigation=0');
      setTotalRisk(response.data.totalRecords || 0);
    } catch (error) {
      console.error('Error fetching risks:', error);
    } finally {
      setLoadingStats(prev => ({ ...prev, risks: false }));
    }
  };

  const fetchDpias = async () => {
    setLoadingStats(prev => ({ ...prev, dpias: true }));
    try {
      const response = await AxiosClient.get('/DPIA');
      setTotalDpia(response.data.totalRecords || 0);
    } catch (error) {
      console.error('Error fetching DPIAs:', error);
    } finally {
      setLoadingStats(prev => ({ ...prev, dpias: false }));
    }
  };

  const fetchPendingTickets = async () => {
    setLoadingStats(prev => ({ ...prev, tickets: true }));
    try {
      const response = await AxiosClient.get('/IssueTicket?IssueTicketStatus=Pending');
      setTotalPendingTickets(response.data.totalRecords || 0);
    } catch (error) {
      console.error('Error fetching pending tickets:', error);
    } finally {
      setLoadingStats(prev => ({ ...prev, tickets: false }));
    }
  };

  const fetchAllData = async () => {
    setLoading(true);
    try {
      await Promise.all([
        fetchSystems(),
        fetchSubmittedDsars(),
        fetchRequiredDsars(),
        fetchRisks(),
        fetchDpias(),
        fetchPendingTickets()
      ]);
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
    } finally {
      setLoading(false);
    }
  };

  // Renamed to avoid conflict and ensure data is fetched on mount
  const initialFetch = async () => {
    setLoading(true);
    try {
      await Promise.all([
        fetchSystems(),
        fetchSubmittedDsars(),
        fetchRequiredDsars(),
        fetchRisks(),
        fetchDpias(),
        fetchPendingTickets()
      ]);
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    initialFetch();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []); // Removed t from dependencies as fetch logic doesn't depend on it directly

  // Define stats array INSIDE the component to access `t`
  const stats: StatItem[] = [
    {
      label: t('dashboard.stats.managingSystems'), // Use t()
      value: totalSystem,
      icon: <LaptopOutlined style={{ fontSize: '24px' }} />,
      color: '#1677ff',
      onRefresh: fetchSystems,
      loading: loadingStats.systems,
      route: 'systems',
      requiredFeature: '/api/ExternalSystem_GET'
    },
    {
      label: t('dashboard.stats.ongoingDpias'), // Use t()
      value: totalDpia,
      icon: <FileTextOutlined style={{ fontSize: '24px' }} />,
      color: '#faad14',
      onRefresh: fetchDpias,
      loading: loadingStats.dpias,
      route: 'dpias',
      requiredFeature: '/api/DPIA_GET'
    },
    {
      label: t('dashboard.stats.dsarPending'), // Use t()
      value: totalSubmittedDsar,
      icon: <UserOutlined style={{ fontSize: '24px' }} />,
      color: '#52c41a',
      onRefresh: fetchSubmittedDsars,
      loading: loadingStats.submittedDsars,
      route: '',
      requiredFeature: '/api/Dsar/get-list/{id}_GET'
    },
    {
      label: t('dashboard.stats.dsarRequired'), // Use t()
      value: totalRequiredDsar,
      icon: <BellOutlined style={{ fontSize: '24px' }} />,
      color: '#ff4d4f',
      onRefresh: fetchRequiredDsars,
      loading: loadingStats.requiredDsars,
      route: '',
      requiredFeature: '/api/Dsar/get-list/{id}_GET'
    },
    {
      label: t('dashboard.stats.unresolvedRisks'), // Use t()
      value: totalRisk,
      icon: <WarningOutlined style={{ fontSize: '24px' }} />,
      color: '#f5222d',
      onRefresh: fetchRisks,
      loading: loadingStats.risks,
      route: 'risk-management',
      requiredFeature: '/api/Risk_GET'
    },
    {
      label: t('dashboard.stats.pendingTickets'), // Use t()
      value: totalPendingTickets,
      icon: <ClockCircleOutlined style={{ fontSize: '24px' }} />,
      color: '#fa8c16',
      onRefresh: fetchPendingTickets,
      loading: loadingStats.tickets,
      route: 'tickets',
      requiredFeature: '/api/IssueTicket_GET'
    }
  ];

  // Define quickActions array INSIDE the component to access `t`
  const quickActions: QuickActionItem[] = [
    {
      title: t('dashboard.quickActions.newDpia'), // Use t()
      icon: <FileTextOutlined />,
      onClick: () => navigate('dpias/create'),
      type: 'primary',
      requiredFeature: '/api/DPIA_POST'
    },
    {
      title: t('dashboard.quickActions.newRisk'), // Use t()
      icon: <WarningOutlined />,
      onClick: () => navigate('risk-management/new'),
      type: 'primary',
      requiredFeature: '/api/Risk_POST'
    },
    {
      title: t('dashboard.quickActions.newTicket'), // Use t()
      icon: <TagOutlined />,
      onClick: () => navigate('tickets/new'),
      type: 'primary',
      requiredFeature: '/api/IssueTicket_POST'
    },
  ];

  // StatCard component remains largely the same, but receives translated label
  const StatCard = ({ stat }: { stat: StatItem }) => {
    return (
      <Card
        hoverable
        className="shadow-md hover:shadow-lg transition-shadow duration-300"
        onClick={() => stat.route && navigate(stat.route)}
        bodyStyle={{ padding: '16px' }}
      >
        <div className="flex items-center justify-between">
          <div>
            <Text type="secondary" className="text-xs">{stat.label}</Text> {/* Label is already translated */}
            <div className="flex items-baseline gap-2 mt-1">
              <Title level={3} style={{ margin: 0 }}>{stat.value}</Title>
              {/* Keep logic for badge, update text check if needed */}
              {stat.value > 0 && stat.label === t('dashboard.stats.dsarRequired') && (
                <Badge status="error" />
              )}
            </div>
          </div>
          <div className="flex flex-col items-end">
            <div
              className="p-2 rounded-full"
              style={{ backgroundColor: `${stat.color}20` }}
            >
              {React.cloneElement(stat.icon as React.ReactElement, { style: { color: stat.color } })}
            </div>
            {stat.onRefresh && (
              <Button
                type="text"
                size="small"
                icon={<ReloadOutlined spin={stat.loading} />}
                onClick={(e) => {
                  e.stopPropagation();
                  stat.onRefresh && stat.onRefresh();
                }}
                className="mt-2"
                aria-label={t('dashboard.refreshStat')} // Add aria-label for accessibility
              />
            )}
          </div>
        </div>
      </Card>
    );
  };

  return (
    <div className="p-6 bg-gray-50 min-h-screen">
      <div className="mb-6">
        <div className="flex justify-between items-center mb-4">
          <div>
            {/* Use t() for title and description */}
            <Title level={4} style={{ margin: 0 }}>{t('dashboard.title')}</Title>
            <Text type="secondary">{t('dashboard.description')}</Text>
          </div>
          <Space>
            <Button
              icon={<ReloadOutlined spin={loading} />}
              onClick={fetchAllData}
              disabled={loading}
            >
              {t('dashboard.refreshAllButton')} {/* Use t() */}
            </Button>
            <Clock />
          </Space>
        </div>
      </div>

      {/* Use t() for loading tip */}
      <Spin spinning={loading} tip={t('dashboard.loadingTip')}>
        <Row gutter={[16, 16]}>
          {stats.map((stat, i) => (
            <FeatureGuard key={i} requiredFeature={stat.requiredFeature}>
              <Col xs={24} sm={12} md={8} key={i}>
                {/* Pass the stat object which now contains the translated label */}
                <StatCard stat={stat} />
              </Col>
            </FeatureGuard>
          ))}
        </Row>

        <Row gutter={[16, 16]} className="mt-6">
          <Col xs={24} sm={16} md={8}> {/* Adjusted Col span for better layout */}
            <Card
              title={
                <div className="flex items-center">
                  <DashboardOutlined className="mr-2" />
                  {/* Use t() for card title */}
                  <span>{t('dashboard.quickActions.title')}</span>
                </div>
              }
              bordered={false}
              className="shadow-md h-full"
            >
              {/* Adjusted grid cols for responsiveness */}
              <div className="grid grid-cols-1 gap-4">
                {quickActions.map((action, i) => (
                  <FeatureGuard key={i} requiredFeature={action.requiredFeature}>
                    <Button
                      key={i}
                      type={action.type}
                      icon={action.icon}
                      onClick={action.onClick}
                      size="large"
                      block
                      className="flex items-center justify-center"
                    >
                      {/* Title is already translated */}
                      {action.title}
                    </Button>
                  </FeatureGuard>
                ))}
              </div>
            </Card>
          </Col>
        </Row>
      </Spin>


    </div>
  );
};

export default MainDashboard;