using DPMS_WebAPI.Constants;
using Microsoft.AspNetCore.Authorization;

namespace DPMS_WebAPI.AuthPolicies
{
	public class PolicyRequirement : IAuthorizationRequirement
	{
		private string PolicyName { get; set; } = Policies.Authenticated;

		public PolicyRequirement(string policyName)
		{
			PolicyName = policyName;
		}

		public bool IsAuthenticatedPolicy => PolicyName == Policies.Authenticated;
        public bool IsFeatureRequiredPolicy => PolicyName == Policies.FeatureRequired;
	}
}
