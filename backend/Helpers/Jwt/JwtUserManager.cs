using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using CorPool.Mongo.DatabaseModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using AuthenticationOptions = CorPool.BackEnd.Options.AuthenticationOptions;

namespace CorPool.BackEnd.Helpers.Jwt {
    /**
     * This class is the endpoint for all user-related matters. It is an extension
     * of the default, framework-provided UserManager class: it modifies the JWT
     * generation logic to include a claim for the current Tenant.
     */
    public class JwtUserManager : UserManager<User> {
        private readonly AuthenticationOptions _authOptions;

        public JwtUserManager(IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators, IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger, IOptions<AuthenticationOptions> authOptions) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) {
            _authOptions = authOptions.Value;
        }

        /**
         * This mostly mimics the framework default implementation, and it adds our own custom claim
         */
        public Task<(string, DateTime)> GenerateJwtToken(User user) {
            var claims = new List<Claim> {
                new Claim("Tenant", user.TenantId),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64)
            };

            var expiry = DateTime.Now.AddDays(1);

            var jwt = new JwtSecurityToken(
                issuer: _authOptions.Authority,
                audience: _authOptions.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiry,
                signingCredentials: new SigningCredentials(new JwtSigningKey(_authOptions.SigningKey), SecurityAlgorithms.HmacSha256)
            );

            return Task.FromResult((new JwtSecurityTokenHandler().WriteToken(jwt), expiry));
        }
    }
}
