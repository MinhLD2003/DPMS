import React, { useCallback, useEffect, useState } from 'react';
import { Card, Avatar, Button, Tag, Descriptions, Row, Col, Divider, Typography, Tooltip, Spin, Layout } from 'antd';
import {
    EditOutlined,
    LockOutlined,
    MailOutlined,
    UserOutlined,
    PhoneOutlined,
    HomeOutlined,
    CalendarOutlined,
    CheckCircleOutlined,
    ClockCircleOutlined,
    TeamOutlined,
    SettingOutlined
} from '@ant-design/icons';

// Import Material UI icons we want to use
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import PersonAddIcon from '@mui/icons-material/PersonAdd';
import UpdateIcon from '@mui/icons-material/Update';
import BadgeIcon from '@mui/icons-material/Badge';
import DomainIcon from '@mui/icons-material/Domain';
import GroupsIcon from '@mui/icons-material/Groups';
import PasswordChangeModal from './ChangePasswordModal';
import { ProfilePostModel, UserModel } from '../models/ProfileModel';
import { getUserStatus, getUserStatusText } from '../../../utils/TextColorUtils';
import { useParams } from 'react-router-dom';
import AxiosClient from '../../../configs/axiosConfig';
import moment from 'moment';
import { useTranslation } from 'react-i18next';
import PageHeader from '../../../components/common/PageHeader';
import UpdateProfileModal from './UpdateProfile';

const { Title, Text, Paragraph } = Typography;



const ProfilePage = () => {
    const { t } = useTranslation();
    const { id } = useParams<{ id: string }>();
    const [modalVisible, setModalVisible] = useState(false);
    const [user, setUser] = useState<UserModel | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [updatePayload, setUpdatePayload] = useState<ProfilePostModel>();
    const fetchUser = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            const result = await AxiosClient.get(`/Account/profile/${id}`);
            setUser(result.data);
            setUpdatePayload(result.data);
            console.log(result.data);
        } catch (err: any) {
            console.error("Error fetching user:", err);
            setError(err.message || "Failed to fetch user profile.");
        } finally {
            setLoading(false);
        }
    }, [id]);

    useEffect(() => {
        fetchUser();
    }, [fetchUser]);

    if (loading) {
        return (
            <div className="flex justify-center items-center h-screen">
                <Spin size="large" tip={t("loadingProfile")} />
            </div>
        );
    }

    if (error) {
        return (
            <div className="flex justify-center items-center h-screen text-red-500">
                {t("errorLoadingProfile")}: {error}
            </div>
        );
    }

    if (!user) {
        return (
            <div className="flex justify-center items-center h-screen">
                {t("noProfileFound")}
            </div>
        );
    }

    return (
        <Layout style={{ minHeight: '100vh', backgroundColor: '#D3D3D3', borderRadius: '12px' }}>
            <PageHeader title="Profile"/>
            <Row gutter={[24, 24]} className='p-8'>
                {/* Left column - Personal information */}
                <Col xs={24} lg={8}>
                    <Card 
                        className="shadow-xl"
                        bordered={false}
                        cover={
                            <div className="bg-blue-50 p-6 flex text-center">
                                <Avatar
                                    size={150}
                                    src="https://cdn-icons-png.flaticon.com/512/149/149071.png"
                                    className="border-2 border-blue-400"
                                />
                            </div>
                        }
                    >
                        <div className="text-center mb-4">
                            <Title level={3}>{user.fullName}</Title>
                            <Text type="secondary">@{user.userName}</Text>
                            <br />
                            {getUserStatus(user.status, t)}
                        </div>

                        <Divider className="my-3" />

                        <div className="flex justify-center space-x-2 mb-4">
                            {updatePayload && <UpdateProfileModal profile={updatePayload} onUpdateSuccess={fetchUser} />}

                            <Button icon={<LockOutlined />} onClick={() => setModalVisible(true)}>
                                {t("password")}
                            </Button>
                            <PasswordChangeModal
                                visible={modalVisible}
                                onCancel={() => setModalVisible(false)}
                            />
                        </div>

                        <Descriptions
                            layout="horizontal"
                            column={1}
                            className="mt-4"
                            colon={false}
                            labelStyle={{ fontWeight: 'bold' }}
                            contentStyle={{ whiteSpace: 'normal', wordBreak: 'break-word' }}
                        >
                            <Descriptions.Item label={<><MailOutlined className="mr-2" /> {t("email")}</>}>
                                {user.email}
                            </Descriptions.Item>

                            {/* <Descriptions.Item label={<><PhoneOutlined className="mr-2" /> {t("phone")}</>}>
                                {user.phone || "null"}
                            </Descriptions.Item> */}

                            <Descriptions.Item label={<><CalendarOutlined className="mr-2" /> {t("dob")}</>}>
                                {moment(user.dob).format('DD/MM/YYYY')}
                            </Descriptions.Item>

                            {/* <Descriptions.Item label={<><HomeOutlined className="mr-2" /> {t("address")}</>}>
                                <Paragraph ellipsis={{ rows: 2, expandable: true, symbol: t("more") }}>
                                    {user.address || "null"}
                                </Paragraph>
                            </Descriptions.Item> */}
                        </Descriptions>
                    </Card>
                </Col>


                {/* Right column - System information */}
                <Col xs={24} lg={16}>
                    <Card
                        title={
                            <div className="flex items-center">
                                <SettingOutlined className="mr-2" />
                                <span>{t("accountDetails")}</span>
                            </div>
                        }
                        className="shadow-md"
                        bordered={false}
                    >
                        <Descriptions
                            bordered
                            column={1}
                            labelStyle={{ fontWeight: 'bold', width: '20%', minWidth: '50px' }}
                            contentStyle={{ whiteSpace: 'normal', wordBreak: 'break-word' }}
                            size="middle"
                        >
                            <Descriptions.Item label={<><DomainIcon fontSize="small" className="mr-2" />{t("system.name")}</>}>
                                <Tag color="cyan">{user.systems[0] || t("noSystems")}</Tag>
                            </Descriptions.Item>

                            <Descriptions.Item label={<><GroupsIcon fontSize="small" className="mr-2" />{t("groups")}</>}>
                                {user.groups.length > 0 ? (
                                    user.groups.map((group, index) => (
                                        <Tag key={index} color="blue" className="mx-1 my-1">
                                            <TeamOutlined /> {group}
                                        </Tag>
                                    ))
                                ) : (
                                    <Tag>{t("noGroupsAssigned")}</Tag>
                                )}
                            </Descriptions.Item>

                            <Descriptions.Item label={<><BadgeIcon fontSize="small" className="mr-2" />{t("userId")}</>}>
                                <Text code copyable>{user.id}</Text>
                            </Descriptions.Item>

                            <Descriptions.Item label={<><AccessTimeIcon fontSize="small" className="mr-2" />{t("lastActive")}</>}>
                                <Tooltip title={new Date().toLocaleString()}>
                                    <span><ClockCircleOutlined className="mr-1" />
                                        {moment(user.lastTimeLogin).format('DD/MM/YYYY HH:mm:ss')}
                                    </span>
                                </Tooltip>
                            </Descriptions.Item>

                            <Descriptions.Item label={<><PersonAddIcon fontSize="small" className="mr-2" />{t("createdBy")}</>}>
                                {user.createdBy || "null"}
                            </Descriptions.Item>

                            <Descriptions.Item label={<><CalendarOutlined className="mr-2" />{t("createdDate")}</>}>
                                {moment(user.createdAt).format('DD/MM/YYYY HH:mm:ss')}
                            </Descriptions.Item>

                            <Descriptions.Item label={<><UpdateIcon fontSize="small" className="mr-2" /> {t("lastModifiedBy")}</>}>
                                {user.lastModifiedBy}
                            </Descriptions.Item>

                            <Descriptions.Item label={<><CalendarOutlined className="mr-2" />{t("lastModifiedAt")}</>}>
                                {moment(user.lastModifiedAt).format('DD/MM/YYYY HH:mm:ss')}
                            </Descriptions.Item>
                        </Descriptions>
{/* 
                        <div className="mt-6">
                            <Divider orientation="left">{t("activityHistory")}</Divider>
                            <Card size="small" className="bg-gray-50">
                                <div className="text-center py-4">
                                    <Button type="dashed">{t("viewFullActivityHistory")}</Button>
                                </div>
                            </Card>
                        </div> */}
                    </Card>
                </Col>
            </Row>
        </Layout>
    );
};

export default ProfilePage;