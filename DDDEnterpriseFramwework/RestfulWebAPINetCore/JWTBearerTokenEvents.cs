using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RestfulWebAPINetCore
{
    public class JWTBearerTokenEvents : JwtBearerEvents
    {
        /// <summary>
        /// The default TokenValidated method does only schema validation.
        /// Here we are validating the token against our "token store" as well
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task TokenValidated(TokenValidatedContext context)
        {
            await Task.FromResult(base.TokenValidated(context));
            var jwtToken = context.SecurityToken as JwtSecurityToken;
            var tokenManager = context.HttpContext.RequestServices.GetService<ITokenManager>();
            var nameIdentifierClaim = context.Principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (nameIdentifierClaim.IsNull() || nameIdentifierClaim.Value.IsNullOrEmpty())
            {
                throw new SecurityTokenValidationException("Invalid token.Please re-login to get a new token.");
            }
            else
            {
                var validationsStatus = await tokenManager.ValidateTokenAsync(nameIdentifierClaim.Value, jwtToken.RawData);
                if (!validationsStatus)
                {
                    throw new SecurityTokenValidationException("Invalid token.Please re-login to get a new token.");
                }
            }
            var identity = context.Principal.Identities.First();
            var tokenCacheData = await tokenManager.GetTokenCacheDataAsync(nameIdentifierClaim.Value);
            if(tokenCacheData.ClaimsToConsiderAfterTokenValidation.IsNotNullOrEmpty())
            {
                identity.AddClaims(tokenCacheData.ClaimsToConsiderAfterTokenValidation);
            }
        }
    }
}
