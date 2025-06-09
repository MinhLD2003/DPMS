namespace DPMS_WebAPI.Builders
{
    public interface IEventMessageBuilder
    {
        string BuildDPIACreatedEvent(string userName);
        string BuildDPIAStatusChangeEvent(string userName, string role, string status);
        string BuildDPIAResponsibilityStatusChangeEvent(string userName, string role, string responsibility, string status);
        string BuildDPIAReviewRequestEvent(string userName, string role);
        string BuildDPIAApprovalEvent(string userName, string status);
    }

    public class DPIAEventMessageBuilder : IEventMessageBuilder
    {
        public string BuildDPIACreatedEvent(string userName)
        {
            return $"{userName} created the DPIA";
        }

        public string BuildDPIAStatusChangeEvent(string userName, string role, string status)
        {
            return $"{userName} in charge of {role} changed status to {status}";
        }

        public string BuildDPIAResponsibilityStatusChangeEvent(string userName, string role, string responsibility, string status)
        {
            return $"{userName} in charge of {role} changed status of {responsibility} to {status}";
        }

        public string BuildDPIAReviewRequestEvent(string userName, string role)
        {
            return $"{userName} with role {role} made a request for DPO review";
        }

        public string BuildDPIAApprovalEvent(string userName, string status)
        {
            return $"{userName} reviewed the DPIA and it is {status}";
        }
    }
}
