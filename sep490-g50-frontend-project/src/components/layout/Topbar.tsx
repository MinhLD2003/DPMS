import { Layout, Dropdown, Avatar, Typography, Image, Space, Menu } from "antd";
import { UserOutlined, SettingOutlined, LogoutOutlined, ProfileOutlined, UnorderedListOutlined, MenuOutlined } from "@ant-design/icons";
import { useContext, useEffect, useState, useMemo } from "react";
import { AuthContext } from "../../contexts/AuthContext";
import { useNavigate } from "react-router-dom";
import { ItemType } from "antd/es/menu/interface";
import { jwtDecode } from "jwt-decode";
import LanguageSwitcher from "../language/LanguageSwitch";
import { useTranslation } from "react-i18next";
import { useConsentModal } from "../../contexts/ConsentModalContext";
import AxiosClient from "../../configs/axiosConfig";
import { useMediaQuery } from "react-responsive";
import { PurposeValue } from "../../pages/Consent/models/ConsentPostModel";

const { Header } = Layout;
const { Text } = Typography;

const Topbar: React.FC = () => {
  const authContext = useContext(AuthContext);
  const { t } = useTranslation();
  const { showConsentModal } = useConsentModal();
  const isMobile = useMediaQuery({ maxWidth: 768 });

  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const [email, setEmail] = useState<string | null>(null);
  const [username, setUsername] = useState<string | null>(null);
  const [uid, setUid] = useState<string | null>(null);

  // Early return pattern with proper UI feedback
  if (!authContext) {
    return (
      <Header style={{ background: 'white', display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
        <Text type="secondary">Loading...</Text>
      </Header>
    );
  }

  const { user, consentChecked, setConsentChecked, logout } = authContext;
  const handleMyConsentClick = async () => {
    if (!email) return;

    try {
      const response = await AxiosClient.get(`/Consent/dpms-consent/${email}`);
      const fullUrl = response.data.consentLink;
      if (fullUrl) {
        window.open(`${fullUrl}?fromDPMS=true`);
      }
    } catch (error) {
      console.error("Error fetching consent URL:", error);
    }
  };
  // Extract user info from token
  useEffect(() => {
    if (user?.token) {
      try {
        const decoded: { email: string, sub: string } = jwtDecode(user.token);
        setEmail(decoded.email);

        // Extract username from email (before the @ symbol)
        const username = decoded.email.split('@')[0];
        setUsername(username);
        setUid(decoded.sub);
      } catch (error) {
        console.error("Failed to decode token:", error);
      }
    }
  }, [user]);

  useEffect(() => {
    const checkConsent = async (email: string) => {
      try {
        const response = await AxiosClient.get(`/Consent/dpms-consent/${email}`);
        const hasConsent = response.data.consented;
        let hasNewConsent = false;

        if (hasConsent) {
          // Check if any purpose has null status
          hasNewConsent = response.data.purposes.some(
            (consent: PurposeValue) =>
              consent.status === null ||
              String(consent.status).toLowerCase() === 'null'
          );
        }
        // Show modal if either no consent or new consent needed
        if (!hasConsent && !consentChecked) {
          showConsentModal(response.data.consentLink, 1);
          setConsentChecked(true);
        }
        if (hasNewConsent && !consentChecked) {
          showConsentModal(response.data.consentLink, 2);
          setConsentChecked(true);
        }
      } catch (error) {
        console.error("Error checking consent:", error);
      }
    };

    if (user?.token && email) {
      checkConsent(email);
    }
  }, [user, email, consentChecked, setConsentChecked, showConsentModal, navigate]);

  // Memoize menu items to prevent unnecessary re-renders
  const profileMenu = useMemo<ItemType[]>(() => [
    {
      key: "1",
      label: t("viewProfile"),
      icon: <ProfileOutlined />,
      onClick: () => navigate(`profile/${uid}`),
    },
    {
      key: "4",
      label: t("myConsent"),
      icon: <UnorderedListOutlined />,
      onClick: handleMyConsentClick,
    },
    {
      key: "3",
      label: t("logout"),
      icon: <LogoutOutlined />,
      onClick: () => {
        logout();
        navigate('/login');
      },
      danger: true,
    },
  ], [navigate, logout, uid,t]);

  const headerBackgroundColor = '#001529';
  const headerTextColor = 'white';
  const accentColor = '#FF7300';

  return (
    <Header
      style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        backgroundColor: headerBackgroundColor,
        background: 'linear-gradient(90deg, #001529, rgb(0, 61, 82), rgb(2, 67, 95))',
        color: headerTextColor,
        padding: '0 16px',
        //boxShadow: '0 0 8px 4px rgba(228, 149, 2, 0.93)',
        position: 'sticky',
        top: 0,
        zIndex: 1000,
        width: '100%',
        height: 'auto',
        minHeight: 64,
      }}
    >
      {/* Logo Section */}
      <div
        className="brand-section"
        style={{
          display: 'flex',
          alignItems: 'center',
          maxWidth: isMobile ? '100px' : '150px',
          cursor: 'pointer',
        }}>
        <Image
          src="/FPTEduLogo.png"
          alt="DPMS Logo"
          preview={false}
          style={{
            maxHeight: '50px',
            objectFit: 'contain',
            borderRadius: 8,
          }}
          onClick={() => navigate('/dashboard')} // Add navigation
        />
      </div>

      {/* Center Section - can be used for navigation or search */}
      <div className="center-section" style={{ flexGrow: 1, display: 'flex', justifyContent: 'center' }}>
        {/* Uncomment if you want to add navigation items
        <Menu
          mode="horizontal"
          selectedKeys={[location.pathname]}
          style={{ borderBottom: 'none', background: 'transparent' }}
          items={[
            { key: '/', label: 'Dashboard' },
            { key: '/reports', label: 'Reports' },
            { key: '/docs', label: 'Documentation' },
          ]}
        />
        */}
      </div>
      {/* Spacer */}
      <div style={{ flexGrow: 1 }} />

      {/* User Controls */}
      <Space size="small" align="center">
        {!isMobile && username && (
          <Text style={{
            marginRight: 12,
            color: headerTextColor,
            display: 'inline-block',
          }}>
            {t("welcome")}, {username}
          </Text>
        )}

        <Dropdown
          menu={{ items: profileMenu }}
          placement="bottomRight"
          trigger={["click"]}
          open={open}
          onOpenChange={setOpen}
        >
          <Avatar
            style={{
              cursor: 'pointer',
              backgroundColor: accentColor,
            }}
            icon={<UserOutlined />}
          />
        </Dropdown>

        <LanguageSwitcher />
      </Space>
    </Header>
  );
};

export default Topbar;