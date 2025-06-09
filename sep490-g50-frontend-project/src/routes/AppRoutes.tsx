// /routes/AppRoutes.tsx
import { Navigate, createBrowserRouter } from "react-router-dom";
import Dashboard from "../components/layout/Dashboard";
import Login from "../pages/Auth/modules/Login";
import AuthGuard from "./AuthGuard"; // Optionally for protected routes
import LoginGuard from "./LoginGuard";
import NotFound from "../pages/NotFound";
import ViewFeatures from "../pages/Feature/ViewFeatures";
import AddUsersToGroup from "../pages/Group/modules/AddUsersTo";
import ViewAccounts from "../pages/Account/modules/ViewAccounts";
import ViewGroups from "../pages/Group/modules/ViewGroups";
import TicketList from "../pages/Ticket/TicketList";
import ForgotPassword from "../pages/Auth/modules/ForgotPassword";
import Verify from "../pages/Auth/modules/Verify";
import ResetPassword from "../pages/Auth/modules/ResetPassword";
import AddTicket from "../pages/Ticket/AddTicket";
import MyProfilePage from "../pages/Profile/modules/MyProfile";
import FICTemplateReadOnly from "../pages/FIC/modules/FICTemplateReadOnly";
import SubmissionDetail from "../pages/FIC/modules/FICSubmissionDetail";
import TicketDetail from "../pages/Ticket/TicketDetail";
import PurposeList from "../pages/Consent/modules/PurposeList";
import SystemDetail from "../pages/System/SystemDetails/SystemDetail";
import AddFeatureToGroup from "../pages/Group/modules/AddFeatureToGroup";
import DPIAList from "../pages/DPIA/DPIAList";
import CreateDPIA from "../pages/DPIA/DPIACreate";
import DPIADetailsScreen from "../pages/DPIA/DPIADetail";
import FICTemplateList from "../pages/FIC/modules/FICTemplateList";
import FICSubmissionList from "../pages/FIC/modules/FICSubmissionList";
import FICTemplateConfig from "../pages/FIC/modules/FICTemplateConfig";
import SystemList from "../pages/System/ViewSystem";
import CreateSystemFIC from "../pages/FIC/modules/FICDemo";
import CreateSystemConsentForm from "../pages/Consent/modules/ConsentConfig";
import ConsentTable from "../pages/Consent/modules/ViewConsentLogs";
import PublicBanner from "../pages/Consent/modules/PublicBanner";
import PolicyList from "../pages/Policy/PolicyList";
import AddPolicy from "../pages/Policy/AddPolicy";
import PolicyUpdate from "../pages/Policy/PolicyUpdate";
import DSARList from "../pages/System/SystemDetails/DsarTab";
import Custom403 from "../pages/AccessDenied";
import ResponsibilityDetailScreen from "../pages/DPIA/ResponsibilityDetailScreen";
import RiskForm from "../pages/Risk/modules/AddRisk";
import FetchRiskList from "../pages/Risk/modules/ViewRisks";
import RemoveUsersFromGroup from "../pages/Group/modules/RemoveUsersFrom";
import FICTemplateUpdate from "../pages/FIC/modules/FICTemplateUpdate";
import FeatureRouteGuard from "./FeatureRouteGuard";
const router = createBrowserRouter([
  {
    path: "/",
    element: <Navigate to="/login" />
  },
  {
    path: "/forgot-password",
    element: <ForgotPassword />
  },
  {
    path: "/verify",
    element: <Verify />
  },
  {
    path: "/reset-password",
    element: <ResetPassword />
  },
  {
    path: "/login",
    element: <LoginGuard><Login /></LoginGuard>
  },
  {
    path: "/public-banner/:uid/:token",
    element: <PublicBanner />
  },
  {
    path: "/dashboard",
    element:
      <AuthGuard><Dashboard /></AuthGuard>,
    children: [
      {
        path: "access-denied",
        element: <Custom403 />
      },
      // Account Management
      {
        path: "accounts", element: (
          <FeatureRouteGuard requiredFeature="/api/Account_GET">
            <ViewAccounts />
          </FeatureRouteGuard>
        )
      },
      {
        path: "accounts/:id", element:
          <FeatureRouteGuard requiredFeature="/api/User/{id}_GET">
            <MyProfilePage />
          </FeatureRouteGuard>
      },

      { path: "profile/:id", element: <MyProfilePage /> },

      // Group Management
      {
        path: "groups",
        element: (
          <FeatureRouteGuard requiredFeature="/api/Group_GET">
            <ViewGroups />
          </FeatureRouteGuard>
        )
      },
      {
        path: "groups/:groupId/add-users",
        element: (
          <FeatureRouteGuard requiredFeature="/api/Group/add-user-to-group_POST">
            <AddUsersToGroup />
          </FeatureRouteGuard>
        )
      },
      {
        path: "groups/:groupId/:groupName/remove-users",
        element: (
          <FeatureRouteGuard requiredFeature="/api/Group/remove-user-from-group_DELETE">
            <RemoveUsersFromGroup />
          </FeatureRouteGuard>
        )
      },


      // Feature Management
      {
        path: "features",
        element: (
          <FeatureRouteGuard requiredFeature="/api/Feature_GET">
            <ViewFeatures />
          </FeatureRouteGuard>
        )
      },
      {
        path: "features/add-feature-to-group",
        element:
          <FeatureRouteGuard requiredFeature="/api/Feature/add-feature-to-group_POST">
            <AddFeatureToGroup />
          </FeatureRouteGuard>
      },
      //System Management
      {
        path: "systems",
        element:
          <FeatureRouteGuard requiredFeature="/api/ExternalSystem_GET">
            <SystemList />
          </FeatureRouteGuard>
      },
      {
        path: "systems/detail/:id", element:
          <FeatureRouteGuard requiredFeature="/api/ExternalSystem/{id}/get-system-details_GET">
            <SystemDetail />
          </FeatureRouteGuard>
      },
      {
        path: "systems/detail/:id/consent-config",
        element:
          <FeatureRouteGuard requiredFeature="/api/ExternalSystem/bulk-add-purposes_POST">
            <CreateSystemConsentForm />
          </FeatureRouteGuard>
      },
      {
        path: "systems/detail/:id/create-system-fic", element:
          <FeatureRouteGuard requiredFeature="/api/Form/submit_POST">
            <CreateSystemFIC />
          </FeatureRouteGuard>
      },
      {
        path: "systems/detail/:id/dsar-management", element:
          <FeatureRouteGuard requiredFeature="/api/Dsar/get-list/{id}_GET">
            <DSARList />
          </FeatureRouteGuard>
      },

      {
        path: "purposes", element:
          <FeatureRouteGuard requiredFeature="/api/Purpose_GET">
            <PurposeList />
          </FeatureRouteGuard>
      },
      {
        path: "consent-management/logs", element:
          <FeatureRouteGuard requiredFeature="/api/Consent/consent-log_GET">
            <ConsentTable />
          </FeatureRouteGuard>
      },

      {
        path: "forms/templates",
        element:
          <FeatureRouteGuard requiredFeature="/api/Form/get-templates_GET">
            <FICTemplateList />
          </FeatureRouteGuard>
      },
      {
        path: "forms/templates/:id",
        element:
          <FeatureRouteGuard requiredFeature="/api/Form/{id}_GET">
            <FICTemplateReadOnly />
          </FeatureRouteGuard>
      },
      {
        path: "forms/templates/config", element:
          <FeatureRouteGuard requiredFeature="/api/Form/save_POST">
            <FICTemplateConfig />
          </FeatureRouteGuard>
      },
      {
        path: "forms/submissions", element:
          <FeatureRouteGuard requiredFeature="/api/Form/get-submissions_GET">
            <FICSubmissionList />
          </FeatureRouteGuard>
      },
      {
        path: "forms/submissions/:id",
        element:
          <FeatureRouteGuard requiredFeature="/api/Form/submission/{id}_GET">
            <SubmissionDetail />
          </FeatureRouteGuard>
      },
      {
        path: "forms/templates/:id/edit", element:
          <FeatureRouteGuard requiredFeature="/api/Form/update_POST">
            <FICTemplateUpdate />
          </FeatureRouteGuard>
      },

      //Ticket Management
      {
        path: "tickets", element:
          <FeatureRouteGuard requiredFeature="/api/IssueTicket_GET">
            <TicketList />
          </FeatureRouteGuard>
      },
      {
        path: "tickets/new", element:
          <FeatureRouteGuard requiredFeature="/api/IssueTicket_POST">
            <AddTicket />
          </FeatureRouteGuard>
      },
      {
        path: "tickets/detail/:id", element:
          <FeatureRouteGuard requiredFeature="/api/IssueTicket/{id}_GET">
            <TicketDetail />
          </FeatureRouteGuard>
      },
      //DPIA
      {
        path: "dpias", element:
          <FeatureRouteGuard requiredFeature="/api/DPIA_GET">
            <DPIAList />
          </FeatureRouteGuard>
      },
      {
        path: "dpias/create", element:
          <FeatureRouteGuard requiredFeature="/api/DPIA_POST">
            <CreateDPIA />
          </FeatureRouteGuard>
      },
      {
        path: "dpias/detail/:id", element:
          <FeatureRouteGuard requiredFeature="/api/DPIA/dpia-detail/{id}_GET">
            <DPIADetailsScreen />
          </FeatureRouteGuard>
      },
      {
        path: "dpias/detail/:dpiaId/responsibility/:responsibilityId", element:
          <ResponsibilityDetailScreen />
      },
      //Profile
      //{ path: "profile", element: <MyProfilePage /> },

      {
        path: "policies", element:
          <FeatureRouteGuard requiredFeature="/api/PrivacyPolicy_GET">
            <PolicyList />
          </FeatureRouteGuard>
      },
      {
        path: "policies/create", element:
          <FeatureRouteGuard requiredFeature="/api/PrivacyPolicy_POST">
            <AddPolicy />
          </FeatureRouteGuard>
      },
      {
        path: "policies/update/:id", element:
          <FeatureRouteGuard requiredFeature="/api/PrivacyPolicy/{id}_PUT">
            <PolicyUpdate />
          </FeatureRouteGuard>

      },

      {
        path: "risk-management/new", element:
          <FeatureRouteGuard requiredFeature="/api/Risk_POST">
            <RiskForm />
          </FeatureRouteGuard>
      },
      {
        path: "risk-management", element:
          <FeatureRouteGuard requiredFeature="/api/Risk_GET">
            <FetchRiskList />
          </FeatureRouteGuard>
      },



    ]
  },
  {
    path: "*",
    element: <NotFound />
  }
]);

export default router;