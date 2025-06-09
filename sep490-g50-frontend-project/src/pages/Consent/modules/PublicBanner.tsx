import React, { useState } from "react";
import { Modal, Typography, Image, Tabs } from "antd";
import SecondaryLayout from "../../../components/layout/SecondaryLayout";
import PrivacyPolicyTab from "./PrivacyPolicyTab";
import ConsentManagementTab from "./ConsentTab";
import DSARTab from "./DsarTab";
import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router-dom";

const { Paragraph } = Typography;

const PublicBanner: React.FC = () => {
    const { t } = useTranslation();
    const [isModalVisible, setIsModalVisible] = useState(true);
    const [activeTab, setActiveTab] = useState("consent");
    const { uid, token } = useParams<{ uid: string; token: string }>();
    const navigate = useNavigate();

    const handleCloseModal = () => {
        setIsModalVisible(false);
    };
    if (!uid || !token) return (<><text>need login</text></>);

    return (
        <SecondaryLayout>
            <Modal
                open={isModalVisible}
                onCancel={handleCloseModal}
                footer={null}
                width={900}
                destroyOnClose
                closable={false}
                maskClosable={false}
            >
                <Image
                    src="/FPTEduLogo.png"
                    alt="DPMS Logo"
                    preview={false}
                    style={{
                        width: 120,
                        borderRadius: 8,
                    }}
                />

                <Paragraph>
                        {t("publicBanner.paragraph")}
                </Paragraph>

                <Tabs activeKey={activeTab} onChange={setActiveTab} centered>
                    <Tabs.TabPane key="consent" tab={t('publicBanner.consentManagementTab')}>
                        <div className="h-[550px] overflow-y-auto"> {/* Fixed height and scroll */}
                            <ConsentManagementTab uid={uid} token={token}
                            />
                        </div>
                    </Tabs.TabPane>
                    <Tabs.TabPane key="privacy" tab={t('publicBanner.privacyPolicyTab')}>
                        <div className="h-[550px] overflow-x-hidden overflow-y-auto px-12"> {/* Ensuring same height */}
                            <PrivacyPolicyTab />
                        </div>
                    </Tabs.TabPane>
                    <Tabs.TabPane key="dsar" tab={t('publicBanner.requestDataTab')}>
                        <div className="h-[550px] overflow-y-auto"> {/* Ensuring same height */}
                            <DSARTab />
                        </div>
                    </Tabs.TabPane>
                </Tabs>


                <div className="text-center mt-4">
                    <span className="text-sm font-semibold italic text-gray-600 tracking-wide">
                        {t('publicBanner.poweredBy')}
                        <span className="font-semibold bg-gradient-to-r from-orange-400 to-yellow-600 bg-clip-text text-transparent text-underlined">
                            DPMS
                        </span>
                    </span>

                </div>
            </Modal>
        </SecondaryLayout>
    );
};

export default PublicBanner;