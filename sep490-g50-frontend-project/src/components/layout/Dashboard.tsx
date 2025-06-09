import React, { useContext } from 'react';
import { Layout, theme } from 'antd';
import { Outlet, useLocation } from 'react-router-dom';
import Navbar from './Navbar';
import Topbar from './Topbar';
import { AuthContext } from '../../contexts/AuthContext';
import MainDashboard from '../../pages/MainScreen';
import NewChat from '../AIChatbot/ChatPrompt';

const { Content, Footer } = Layout;

const Dashboard: React.FC = () => {
  const location = useLocation();
  const {
    token: { borderRadiusLG },
  } = theme.useToken();

  const authContext = useContext(AuthContext);

  if (!authContext) return null;

  return (
    <Layout style={{ minHeight: '100vh', width: '99vw' }}>
      <Topbar />
      <Layout>
        <Navbar />
        <Layout style={{ background: 'rgba(173, 169, 166, 0.4)' }}>
          <Content
            style={{
              margin: '0px',
              padding: 24,
              minHeight: 280,
              borderRadius: borderRadiusLG,
              overflowX: 'hidden'
            }}
          >
            {location.pathname === "/dashboard" || location.pathname === "/dashboard/" ? (
              <MainDashboard />
            ) : (
              <Outlet />
            )}
          <NewChat/>
          </Content>
          <Footer style={{ textAlign: 'center', height: '12px' }}>
            Ant Design ©{new Date().getFullYear()} Created by Ant UED
          </Footer>
        </Layout>
      </Layout>
    </Layout>
  );
};

export default Dashboard;