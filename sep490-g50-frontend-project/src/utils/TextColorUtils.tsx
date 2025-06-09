import { ExternalSystemStatus, ExternalSystemStatusText, FormStatus, FormType, HttpMethodType, NumberedUserStatus, PolicyStatus, UserStatus } from "../enum/enum";
import { Tag } from 'antd';
export const getUserStatusText = (status: any, t: (key: string) => string) => {
  switch (status) {
    case NumberedUserStatus.Activated:
      return <Tag color="green">{t("activated")}</Tag>;
    case NumberedUserStatus.Deactivated:
      return <Tag color="red">{t("deactivated")}</Tag>;
    default:
      return <Tag color="default">Unknown</Tag>;
  }
};
export const getUserStatus = (status: any, t: (key: string) => string) => {
  switch (status) {
    case UserStatus.Activated:
      return <Tag color="green">{t("activated")}</Tag>;
    case UserStatus.Deactivated:
      return <Tag color="red">{t("deactivated")}</Tag>;
    default:
      return <Tag color="default">Unknown</Tag>;
  }
};
export const getPolicyStatus = (status: any, t: (key: string) => string) => {
  switch (status) {
    case PolicyStatus.Active:
      return <Tag color="green">{t("active")}</Tag>;
    case PolicyStatus.Deactivated:
      return <Tag color="red">{t("deactivated")}</Tag>;
    case PolicyStatus.Draft:
      return <Tag color="gray">{t("draft")}</Tag>;
    case PolicyStatus.Inactive:
      return <Tag color="red">{t("inactive")}</Tag>;
    default:
      return <Tag color="default">Unknown</Tag>;
  }
};
export const getFormStatusText = (status: any, t: (key: string) => string) => {
  switch (status) {
    case FormStatus.Draft:
      return <Tag color="orange" className="font-bold">{t("draft")}</Tag>;
    case FormStatus.Activated:
      return <Tag color="green" className="font-bold">{t("activated")}</Tag>;
    case FormStatus.Deactivated:
      return <Tag color="red" className="font-bold">{t("deactivated")}</Tag>;
    default:
      return <Tag color="default">Unknown</Tag>;
  }
}
export const getHttpMethodText = (method: any) => {
  switch (method) {
    case HttpMethodType.GET:
      return <Tag color="green" className="font-bold">GET</Tag>;
    case HttpMethodType.POST:
      return <Tag color="blue" className="font-bold">POST</Tag>;
    case HttpMethodType.PUT:
      return <Tag color="yellow" className="font-bold">PUT</Tag>;
    case HttpMethodType.DELETE:
      return <Tag color="red" className="font-bold">DELETE</Tag>;
    case HttpMethodType.PATCH:
      return <Tag color="purple" className="font-bold">PATCH</Tag>;
    default:
      return <Tag color="default">Parent</Tag>;
  }
}
export const getGroupText = (group: string) => {
  switch (group) {
    case "admin_group":
      return <Tag color="blue"  style={{ fontSize: 16 }}>Admin</Tag>;
    case "DPO":
      return <Tag color="blue"  style={{ fontSize: 16 }}>DPO</Tag>;
    case "ProductDeveloper":
      return <Tag color="blue"  style={{ fontSize: 16 }}>Product Developer</Tag>;
    case "Auditor":
      return <Tag color="blue"  style={{ fontSize: 16 }}>Auditor</Tag>;
    case "BusinessOwner":
      return <Tag color="blue"  style={{ fontSize: 16 }}>Business Owner</Tag>;
    case "QA":
      return <Tag color="blue"  style={{ fontSize: 16 }}>QA</Tag>;
    case "QA Manager":
      return <Tag color="blue"  style={{ fontSize: 16 }}>QA Manager</Tag>;
    case "IT Manager":
      return <Tag color="blue"  style={{ fontSize: 16 }}>IT Manager</Tag>;
    case "CTO/CIO":
      return <Tag color="blue"  style={{ fontSize: 16 }}>CTO/CIO</Tag>;
    case "":
      return <Tag color="default" >No group</Tag>;
    case null:
      return <Tag color="default" >No group</Tag>;
    default:
      return <Tag color="blue">{group}</Tag>;
  }
}
export const getExternalSystemStatusText = (status: any, t: (key: string) => string) => {
  switch (status) {
    case ExternalSystemStatusText.Deactivated:
      return <Tag color="red" className="font-bold">{t("deactivated")}</Tag>;
    case ExternalSystemStatusText.WaitingForDPIA:
      return <Tag color="blue" className="font-bold">{t("waitingForDPIA")}</Tag>;
    case ExternalSystemStatusText.WaitingForFIC:
      return <Tag color="cyan" className="font-bold">{t("waitingForFIC")}</Tag>;
    case ExternalSystemStatusText.DPIACreated:
      return <Tag color="" className="font-bold">{t("dpiaCreated")}</Tag>;
    case ExternalSystemStatusText.Activated:
      return <Tag color="green" className="font-bold">{t("activated")}</Tag>;
    default:
      return <Tag color="default">Unknown</Tag>;
  }
}
export const getFormTypeText = (type: any) => {
  switch (type) {
    case FormType.FIC:
      return <Tag color="blue" className="font-bold">FIC</Tag>;
    default:
      return <Tag color="default">Unknown</Tag>;
  }
}
export const getConsentStatus = (status: any, t: (key: string) => string) => {
  switch (status) {
    case "Active":
      return <Tag color="green">{t("active")}</Tag>;
    case "Withdrawn":
      return <Tag color="red">{t("withdrawn")}</Tag>;
    default:
      return <Tag color="default">Unknown</Tag>;
  }
};