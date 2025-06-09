using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper.Features;
using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;

namespace DPMS_WebAPI.Services
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    
    public class AuthService
    {
        private readonly IConfiguration _config;
        private readonly IUserRepository _userRepository;

        public AuthService(IConfiguration config, IUserRepository userRepository)
        {
            _config = config;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Verifies the Google token and returns the payload if the token is valid.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<User?> VerifyGoogleTokenAsync(string token)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(token);
                var existedUser = await _userRepository.GetUserByEmailAsync(payload.Email);

                return existedUser ?? null;

            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Generates a JWT token with the given email and name.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GenerateJwtToken(User user)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("Jwt:Key", "JWT key cannot be null or empty.");
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var features = _userRepository.GetFeaturesByUserEmailAsync(user.Email!);
            // Extract feature claims safely
            var featureClaims = features.Result.Select(f => new Claim("feature", f!.Url + "_" + f.HttpMethod)).ToList();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("name", user.FullName!), // Custom claim
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }.Union(featureClaims);

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Returns the principal from the given token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns> </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("Jwt:Key", "JWT key cannot be null or empty.");
            }
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };

            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);
            return principal;
        }
        public string GeneratePasswordResetToken(string email)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new ArgumentNullException("Jwt:Key", "JWT key cannot be null or empty.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Add only essential claims for password reset
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Email, email),
        new Claim("purpose", "password-reset"), // Mark the token's specific purpose
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N").Substring(0, 8)) // Shorter unique ID
    };

            // Shorter expiration time for security
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.UtcNow.AddHours(1), // 1-hour expiration for reset tokens
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Extracts email from a password reset token
        /// </summary>
        /// <param name="principal">The ClaimsPrincipal from token validation</param>
        /// <returns>The email address or null if not found</returns>
        public string GetEmailFromResetToken(ClaimsPrincipal principal)
        {
            if (principal.HasClaim(c => c.Type == "purpose" && c.Value == "password-reset"))
            {
                return principal.FindFirstValue(ClaimTypes.Email);

            }

            return null;
        }
    }
}