using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace RestfulWebAPINetCore
{
    public class TokenInputData
    {
        public string Key { get; set; }

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public string Secret { get; set; }

        public TimeSpan Expiration { get; set; }

        public Claim TokenIdentifierClaim { get; set; }

        public IEnumerable<Claim> ClaimsToConsiderAfterTokenValidation { get; set; }
    }
}
