using System.Collections.Generic;
using System.Security.Claims;

namespace RestfulWebAPINetCore
{
    public class TokenCacheData
    {
        public string Token { get; set; }

        public IEnumerable<Claim> ClaimsToConsiderAfterTokenValidation { get; set; }
    }
}
