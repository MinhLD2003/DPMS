using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace DPMS_WebAPI.AuthPolicies
{
    public class PolicyAuthorizationHandler : AuthorizationHandler<PolicyRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PolicyAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Check whether user has specified requirement
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement">Requirement object to check agains</param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PolicyRequirement requirement)
        {
            var user = context.User;

            var path = _httpContextAccessor.HttpContext!.Request.Path;
            var method = _httpContextAccessor.HttpContext!.Request.Method;
            var normalizedPath = NormalizePath(path);

            Console.WriteLine("Authorizing: " + user.Identity?.Name);
            Console.WriteLine("Path: " + path);
            Console.WriteLine("Method: " + method);

            if (requirement.IsAuthenticatedPolicy)
            {
                HandleAuthenticationRequirement(context, requirement, user);
            }

            else if (requirement.IsFeatureRequiredPolicy)
            {
                HandleFeatureRequirement(context, requirement, user, normalizedPath, method);
            }

            return Task.CompletedTask;
        }

        private void HandleAuthenticationRequirement(
            AuthorizationHandlerContext context,
            PolicyRequirement requirement,
            ClaimsPrincipal user)
        {
            if (user.Identity?.IsAuthenticated == true)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }

        private void HandleFeatureRequirement(
            AuthorizationHandlerContext context,
            PolicyRequirement requirement,
            ClaimsPrincipal user,
            string normalizedPath,
            string method)
        {
            var featureClaims = user.FindAll("feature").Select(c => c.Value.ToLower()).ToList();
            var requiredFeature = $"{normalizedPath}_{method}".ToLower();

            if (featureClaims.Contains(requiredFeature))
            {
            context.Succeed(requirement);
            }
            else
            {
            context.Fail();
            }
        }

        private static string NormalizePath(string path)
        {
            // Replace any GUID (e.g., 1af08acd-0b96-4082-9053-5217bb6997fc) with {id}
            path = Regex.Replace(path, @"\/[0-9a-fA-F\-]{36}", "/{id}");

            // Replace any numeric ID (e.g., /api/item/123/detail → /api/item/{id}/detail)
            path = Regex.Replace(path, @"\/\d+", "/{id}");

            return path;
        }

    }
}
