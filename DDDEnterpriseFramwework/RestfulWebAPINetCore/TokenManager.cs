using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulWebAPINetCore
{
    public class TokenManager : ITokenManager
    {
        private readonly IDistributedCache _cache;
        public TokenManager(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<TokenCacheData> GetTokenCacheDataAsync(string key, CancellationToken cancellationToken = default)
        {
            var tokenCacheDataByteArray = await _cache.GetAsync(key, cancellationToken);
            if (tokenCacheDataByteArray.IsNull())
            {
                return null;
            }
            var tokenCacheDataJson = Encoding.ASCII.GetString(tokenCacheDataByteArray);
            return JsonConvert.DeserializeObject<TokenCacheData>(tokenCacheDataJson, new ClaimJsonConverter());
        }

        public async Task<string> CreateTokenAsync(TokenInputData tokenInputData, CancellationToken cancellationToken = default)
        {
            var claims = new List<Claim> { tokenInputData.TokenIdentifierClaim };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(tokenInputData.Secret)
            );
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.Add(tokenInputData.Expiration),
                SigningCredentials = creds,
                Audience = tokenInputData.Audience,
                Issuer = tokenInputData.Issuer
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            var tokenCacheData = new TokenCacheData
            {
                Token = tokenString,
                ClaimsToConsiderAfterTokenValidation = tokenInputData.ClaimsToConsiderAfterTokenValidation
            };
            var tokenCacheDataJson = JsonConvert.SerializeObject(tokenCacheData);
            var tokenCacheDataByteArray = Encoding.ASCII.GetBytes(tokenCacheDataJson);
            await _cache.SetAsync(tokenInputData.Key, tokenCacheDataByteArray, cancellationToken);
            return tokenString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="jwtToken">For Login call it should be null</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ValidateTokenAsync(string key, string jwtToken = null, CancellationToken cancellationToken = default)
        {
            var tokenCacheData = await GetTokenCacheDataAsync(key, cancellationToken);
            if (tokenCacheData.IsNull())
            {
                return false;
            }
            var tokenString = tokenCacheData.Token;
            if (tokenString.IsNotNullOrEmpty())
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = tokenHandler.ReadToken(tokenString);
                //if validity of token is over then remove it from cache
                if (jwtSecurityToken.ValidTo < DateTime.Now)
                {
                    await RemoveTokenAsync(key, cancellationToken);
                    return false;
                }
                return jwtToken.IsNull() || jwtToken == tokenString;
            }
            return false;
        }

        public async Task<bool> RemoveTokenAsync(string key, CancellationToken cancellationToken = default)
        {
            var tokenCacheDataByteArray = await _cache.GetAsync(key, cancellationToken);
            if (tokenCacheDataByteArray.IsNotNull())
            {
                await _cache.RemoveAsync(key, cancellationToken);
            }
            return true;
        }
    }
}
