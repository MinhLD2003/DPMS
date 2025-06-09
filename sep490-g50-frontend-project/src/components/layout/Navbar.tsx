import { Layout, Menu, theme, Typography, ConfigProvider } from "antd";
import {
  UserOutlined,
  FileTextOutlined,
  SettingOutlined,
  TeamOutlined,
  FormOutlined,
  ProfileOutlined,
  DesktopOutlined,
  TagOutlined,
  DashboardOutlined,
  SafetyOutlined,
  BookOutlined,
  FireOutlined
} from "@ant-design/icons";
import React, { useState, useEffect } from "react";
import { Link, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useMediaQuery } from "react-responsive";
import { getUserFeatures } from "../../utils/jwtDecodeUtils";

const { Sider } = Layout;
const { Title } = Typography;

const Navbar: React.FC = () => {
  const { t } = useTranslation();
  const location = useLocation();
  const isMobile = useMediaQuery({ maxWidth: 768 });
  const [collapsed, setCollapsed] = useState(isMobile);
  const userFeatures = getUserFeatures();
  // Handle responsive collapse
  useEffect(() => {
    setCollapsed(isMobile);
  }, [isMobile]);

  // Improved path matching to handle nested routes
  const getCurrentKey = () => {
    const pathSegments = location.pathname.split('/').filter(Boolean);
    return pathSegments[pathSegments.length - 1] || 'dashboard';
  };

  // Get open keys based on current path
  const getOpenKeys = () => {
    const path = location.pathname;
    const openKeys = [];

    if (['forms', 'fic-templates', 'fic-submissions'].some(route => path.includes(route))) {
      openKeys.push('forms');
    }
    if (['groups', 'features', 'accounts'].some(route => path.includes(route))) {
      openKeys.push('admin-actions');
    }
    if (['purposes', 'consent', 'templates', 'submissions', 'policies'].some(route => path.includes(route))) {
      openKeys.push('dpo-actions');
    }
    if (['purposes', 'consent-management', 'consent-logs'].some(route => path.includes(route))) {
      openKeys.push('consents');
    }
    if (['risk-management'].some(route => path.includes(route))) {
      openKeys.push('risks');
    }
    // if (['dashboard'].some(route => path.includes(route))) {
    //   openKeys.push('/');
    // }

    return openKeys;
  };

  const [openKeys, setOpenKeys] = useState(getOpenKeys());

  useEffect(() => {
    //console.log(userFeatures);
    setOpenKeys(isMobile ? [] : getOpenKeys());
  }, [location.pathname, isMobile]);

  // Menu styling
  const menuItemStyle = {
    borderRadius: '8px',
    transition: 'all 0.3s ease',
    fontSize: '14px',
    color: '#ffffff',
  };

  const selectedItemStyle = {
    ...menuItemStyle,
    background: 'linear-gradient(90deg, rgb(255, 102, 0), #FF8C00)',
    color: 'white',
  };

  const submenuTitleStyle = {
    fontWeight: 600,
    color: '#ffffff',
  };

  const iconStyle = {
    color: '#FF7300',
    fontSize: '20px',
    strokeWidth: 20,
    stroke: '#FF7300',
  };

  // Check if any admin action feature exists
  const hasAdminAccess = userFeatures.some(feature =>
    ['/api/Account_GET', '/api/Group_GET', '/api/Feature_GET'].includes(feature)
  );
  const hasDPOAccess = userFeatures.some(feature =>
    ['/api/PrivacyPolicy_POST'].includes(feature)
  );

  return (
    <ConfigProvider>
      <Sider
        width={240}
        collapsible
        collapsed={collapsed}
        onCollapse={(value) => setCollapsed(value)}
        breakpoint="lg"
        collapsedWidth={isMobile ? 0 : 80}
        zeroWidthTriggerStyle={{ top: 64 }}
        theme="dark"
        style={{
          backgroundColor: '#001529',
          overflow: 'auto',
          height: '100vh',
          position: 'sticky',
          left: 0,
          top: 0,
          borderRight: '2px solid rgb(255, 123, 0)',
        }}
      >
        {/* Logo/Header Section */}
        <div
          style={{
            padding: collapsed ? '24px 0' : '24px 10px',
            textAlign: 'center',
            borderBottom: '1px solid rgb(250, 163, 2)',
            marginBottom: '16px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: collapsed ? 'center' : 'flex-start',
            height: '80px',
            backgroundColor: '#000C17',
          }}
        >
          <FireOutlined style={iconStyle} />
          {!collapsed && (
            <Title level={4} style={{ color: '#FF7300', margin: 0, fontWeight: 600 }}>
              DPMS Portal
            </Title>
          )}
        </div>

        <Menu
          mode="inline"
          theme="dark"
          style={{
            backgroundColor: '#001529',
            border: 'none',
            overflowY: 'auto',
            overflowX: 'hidden',
            paddingTop: '12px',
          }}
          selectedKeys={[getCurrentKey()]}
          openKeys={openKeys}
          onOpenChange={setOpenKeys}
          items={[
            ...(hasAdminAccess ? [{
              key: 'admin-actions',
              icon: <UserOutlined style={iconStyle} />,
              label: <span style={submenuTitleStyle}>{t('navbar.adminManagement')}</span>,
              style: { marginBottom: '8px' },
              children: [
                {
                  key: "group-management",
                  icon: <TeamOutlined style={{ fontSize: "16px" }} />,
                  label: <Link to="groups">{t("navbar.groups")}</Link>,
                  style: location.pathname.includes("groups") ? selectedItemStyle : menuItemStyle,
                },
                {
                  key: 'feature-management',
                  icon: <TagOutlined style={{ fontSize: '16px' }} />,
                  label: <Link to="features">{t('navbar.features')}</Link>,
                  style: location.pathname.includes('features') ? selectedItemStyle : menuItemStyle,
                },
                {
                  key: 'account-management',
                  icon: <UserOutlined style={{ fontSize: '16px' }} />,
                  label: <Link to="accounts">{t('navbar.accounts')}</Link>,
                  style: location.pathname.includes('accounts') ? selectedItemStyle : menuItemStyle,
                },
              ]
            }] : []),
            // ...(hasDPOAccess ? [{
            //   key: 'dpo-actions',
            //   icon: <UserOutlined style={iconStyle} />,
            //   label: <span style={submenuTitleStyle}>{t('navbar.dpoManagement')}</span>,
            //   style: { marginBottom: '8px' },
            //   children: [
                ...(userFeatures.includes('/api/Form/get-templates_GET') ? [{
                  key: 'templates',
                  icon: <DesktopOutlined style={{ fontSize: '16px' }} />,
                  label: <Link to="forms/templates">{t('navbar.ficTemplates')}</Link>,
                  style: location.pathname.includes('templates') ? selectedItemStyle : menuItemStyle
                }] : []),
                ...(userFeatures.includes('/api/Form/get-all-submissions_GET') ? [{
                  key: 'submissions',
                  icon: <DesktopOutlined style={{ fontSize: '16px' }} />,
                  label: <Link to="forms/submissions">{t('navbar.ficSubmissions')}</Link>,
                  style: location.pathname.includes('submissions') ? selectedItemStyle : menuItemStyle
                }] : []),
                ...(userFeatures.includes('/api/PrivacyPolicy_GET') ? [{
                  key: 'policies',
                  icon: <BookOutlined style={{ fontSize: '16px' }} />,
                  label: <Link to="policies">{t('navbar.policyList')}</Link>,
                  style: location.pathname.includes('policies') ? selectedItemStyle : menuItemStyle
                }] : []),
                ...(userFeatures.includes('/api/Purpose_GET') ? [{
                  key: 'purposes',
                  icon: <FileTextOutlined style={{ fontSize: '16px' }} />,
                  label: <Link to="purposes">{t('navbar.purposeList')}</Link>,
                  style: location.pathname.includes('purposes') ? selectedItemStyle : menuItemStyle
                }] : []),
                ...(userFeatures.includes('/api/Consent/consent-log_GET') ? [{
                  key: 'consent-logs',
                  icon: <ProfileOutlined style={{ fontSize: '16px' }} />,
                  label: <Link to="consent-management/logs">{t('navbar.consentLogs')}</Link>,
                  style: location.pathname.includes('consent-management/logs') ? selectedItemStyle : menuItemStyle
                }] : []),
            //   ]
            // }] : []),





            ...(userFeatures.includes('/api/ExternalSystem_GET') ? [{
              key: 'system-management',
              icon: <DesktopOutlined style={{ fontSize: '16px' }} />,
              label: <Link to="systems">{t('navbar.systems')}</Link>,
              style: location.pathname.includes('systems') ? selectedItemStyle : menuItemStyle,
            }] : []),
            ...(userFeatures.includes('/api/DPIA_GET') ? [{
              key: 'DPIA-Management',
              icon: <ProfileOutlined style={{ fontSize: '16px' }} />,
              label: <Link to="dpias">DPIA</Link>,
              style: location.pathname.includes('dpias') ? selectedItemStyle : menuItemStyle
            }] : []),
            ...(userFeatures.includes('/api/IssueTicket_GET') ? [{

              key: 'ticket-management',
              icon: <ProfileOutlined style={{ fontSize: '16px' }} />,
              label: <Link to="tickets">{t('navbar.tickets')}</Link>,
              style: location.pathname.includes('tickets') ? selectedItemStyle : menuItemStyle
            }] : []),
            ...(userFeatures.includes('/api/Risk_GET') ? [{
              key: 'risks',
              icon: <SafetyOutlined style={{ fontSize: '16px' }} />,
              label: <Link to="risk-management">{t('navbar.riskList')}</Link>,
              style: location.pathname.includes('risk') ? selectedItemStyle : menuItemStyle
            }] : []),
          ]}
        />
      </Sider>
    </ConfigProvider>
  );
};

export default Navbar;