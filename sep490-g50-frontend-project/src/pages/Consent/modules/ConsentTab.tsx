import React, { useState, useEffect } from "react";
import { Typography, List, Switch, message, Button, Spin } from "antd";
import { CheckCircleOutlined } from "@ant-design/icons";
import consentService from "../apis/ConsentAPIs";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { ConsentItem } from "../models/ConsentBannerViewModel";
import { PurposeValue } from "../models/ConsentPostModel";
import AxiosClient from "../../../configs/axiosConfig";
import { getEmailFromToken } from "../../../utils/jwtDecodeUtils";

const { Text, Paragraph } = Typography;

type ConsentManagementTabProps = {
  uid?: string;
  token?: string;
  onSubmit?: () => void;
};

const ConsentManagementTab: React.FC<ConsentManagementTabProps> = ({
  uid,
  token,
  onSubmit
}) => {
  const { t } = useTranslation();
  const [consentPurposes, setConsentPurposes] = useState<PurposeValue[]>([]);
  const [purposesList, setPurposesList] = useState<ConsentItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSubmitted, setIsSubmitted] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    if (uid && token) {
      fetchPurposes(uid, token);
    }
  }, [uid, token, t]);
  const getConsentData = async (apiUrl: string): Promise<Record<string, string>> => {
    try {
      const response = await AxiosClient.get(apiUrl);
      if (response.data.consented && response.data.purposes) {
        const consentData: Record<string, string> = {};
        response.data.purposes.forEach((purpose: PurposeValue) => {
          consentData[purpose.purposeId] = String(purpose.status);
        });
        return consentData;
      }
      return {};
    } catch (error) {
      console.error('Error fetching consent data from API:', error);
      return {};
    }
  };

  const fetchPurposes = async (uniqueIdentifier: string, token: string) => {
    setLoading(true);
    try {
      const result = await consentService.getConsentBanner(uniqueIdentifier, token);
      if (result.success) {
        setPurposesList(result.objectList);
        const urlParams = new URLSearchParams(window.location.search);
        const fromDPMS = urlParams.get('fromDPMS') === 'true';

        let consentData: Record<string, string> = {};

        if (fromDPMS) {
          // Fetch consent data from API when coming from DPMS
          const userEmail = getEmailFromToken();
          consentData = await getConsentData(`/Consent/dpms-consent/${userEmail}`);
        }
        else{
          consentData = await getConsentData(`/Consent/get-banner/${uniqueIdentifier}/${token}`);
        }

        const initialConsent: PurposeValue[] = result.objectList.map(item => {
          const purposeId = String(item.purposeId);
          // Check if this purposeId exists in consentData
          const hasPresetValue = purposeId in consentData;
          // If it exists, set the status based on consentData (convert "True"/"False" to boolean)
          const status = hasPresetValue ? consentData[purposeId] === "True" : false;

          return {
            purposeId: purposeId,
            status: status
          };
        });

        setConsentPurposes(initialConsent);
      } else {
        message.error(t('consentManagementTab.loadBannerFailed'));// BUG
      }
    } catch (error) {
      message.error(t('consentManagementTab.fetchError'));
    } finally {
      setLoading(false);
      setIsSubmitted(false);
    }
  };

  const handleToggle = (purposeId: string, checked: boolean) => {
    setConsentPurposes(prevConsent =>
      prevConsent.map(purpose =>
        purpose.purposeId === purposeId ? { ...purpose, status: checked } : purpose
      )
    );
  };

  const handleToggleAll = (checked: boolean) => {
    setConsentPurposes(prevConsent =>
      prevConsent.map(purpose => ({ ...purpose, status: checked }))
    );
  };

  const getConsentStatus = (purposeId: number): boolean => {
    const purpose = consentPurposes.find(p => p.purposeId === String(purposeId));
    return purpose ? purpose.status : false;
  };

  const areAllConsentsAccepted = () => {
    return consentPurposes.length > 0 && consentPurposes.every(purpose => purpose.status);
  };

  const handleSubmit = async () => {
    if (!uid || !token) return;

    setIsSubmitting(true);
    const payload = {
      uniqueIdentifier: uid,
      tokenString: token,
      consentPurposes: consentPurposes
    };

    try {
      const result = await consentService.postConsent(payload);
      setIsSubmitting(false);
      setIsSubmitted(true);
      message.success(t('consentManagementTab.submitSuccess'));

      // Call optional onSubmit callback if provided
      onSubmit && onSubmit();
    } catch (error) {
      message.error(t('consentManagementTab.submitFailed'));
      setIsSubmitting(false);
    }
  };

  const handleReset = () => {
    setConsentPurposes(prevConsent =>
      prevConsent.map(purpose => ({ ...purpose, status: false }))
    );
    setIsSubmitted(false);
  };
  const handleBackToDashboard = () => {
    navigate("/dashboard");
  }

  const renderConfirmationView = () => (
    <div>
      <div style={{ display: "flex", alignItems: "center", marginBottom: "1rem" }}>
        <CheckCircleOutlined style={{ fontSize: 24, color: "#52c41a", marginRight: 8 }} />
        <Text strong>{t('consentManagementTab.thankYou')}</Text>
      </div>
      {purposesList.map((item) => (
        <div
          key={item.purposeId}
          style={{
            marginBottom: "1rem",
            padding: "0.5rem",
            border: "1px solid #f0f0f0",
            borderRadius: "4px",
            backgroundColor: "#fafafa",
          }}
        >
          <Text>
            <strong>{item.purposeName}:</strong> <br />
            {item.purposeDescription}
          </Text>
          <div style={{ textAlign: "right" }}>
            <Text
              style={{
                backgroundColor: getConsentStatus(item.purposeId) ? "#f6ffed" : "#fff1f0",
                color: getConsentStatus(item.purposeId) ? "#52c41a" : "#ff4d4f",
                padding: "2px 8px",
                borderRadius: "2px",
                fontWeight: 500,
              }}
            >
              {getConsentStatus(item.purposeId) ? t('consentManagementTab.accepted') : t('consentManagementTab.declined')}
            </Text>
          </div>
        </div>
      ))}
      <Button onClick={handleReset} block style={{
        backgroundColor: "#FA8C16",
        borderColor: "#FA8C16",
        color: "#fff",
        marginTop: "1rem",
      }}
      >
        {t('consentManagementTab.modifyConsent')}
      </Button>
      <Button onClick={handleBackToDashboard} block style={{ marginTop: "1rem" }}>
        {t("consentManagementTab.backToDashboard")}
      </Button>

    </div>
  );

  const renderFormView = () => (
    <>
      <List
        bordered
        dataSource={[
          { isMasterToggle: true },
          ...purposesList.map(item => ({ ...item, isMasterToggle: false }))
        ]}
        renderItem={(item: any) => {
          if (item.isMasterToggle) {
            return (
              <List.Item
                style={{
                  display: "flex",
                  justifyContent: "space-between",
                  flexWrap: "wrap",
                  padding: "12px 16px",
                  backgroundColor: "#f5f5f5",
                  borderBottom: "2px solid #e8e8e8"
                }}
              >
                <div style={{ flex: 1, paddingRight: "1rem" }}>
                  <Text strong style={{ fontSize: "16px" }}>
                    {t('consentManagementTab.toggleAllConsents')}
                  </Text>
                  <Paragraph>
                    {t('consentManagementTab.toggleAllDescription')}
                  </Paragraph>
                </div>
                <Switch
                  checked={areAllConsentsAccepted()}
                  onChange={handleToggleAll}
                  size="default"
                  style={{
                    backgroundColor: areAllConsentsAccepted() ? "#FA8C16" : "#d9d9d9",
                  }}
                />
              </List.Item>
            );
          }

          return (
            <List.Item
              style={{
                display: "flex",
                justifyContent: "space-between",
                flexWrap: "wrap",
                padding: "12px 16px",
              }}
            >
              <div style={{ flex: 1, paddingRight: "1rem", marginBottom: "8px" }}>
                <Text strong>{item.purposeName}</Text>
                <br />
                {item.purposeDescription}
              </div>
              <Switch
                checked={getConsentStatus(item.purposeId)}
                onChange={(checked) => handleToggle(String(item.purposeId), checked)}
                style={{
                  backgroundColor: getConsentStatus(item.purposeId) ? "#FA8C16" : "#d9d9d9",
                }}
              />
            </List.Item>
          );
        }}
        style={{ marginBottom: "1.5rem" }}
      />

      <Button
        type="primary"
        block
        onClick={handleSubmit}
        loading={isSubmitting}
        style={{
          backgroundColor: "#FA8C16",
          borderColor: "#FA8C16",
          color: "#fff",
          marginBottom: "1rem",
        }}
      >
        {t('consentManagementTab.submitConsent')}
      </Button>
    </>
  );

  if (!uid || !token) return null;

  return loading ? <Spin tip="Loading..." /> : isSubmitted ? renderConfirmationView() : renderFormView();
};

export default ConsentManagementTab;