using System.Threading;
using System.Threading.Tasks;

namespace RestfulWebAPINetCore
{
    public interface ITokenManager
    {
        Task<TokenCacheData> GetTokenCacheDataAsync(string key, CancellationToken cancellationToken = default);

        Task<string> CreateTokenAsync(TokenInputData tokenInputData, CancellationToken cancellationToken = default);

        Task<bool> ValidateTokenAsync(string key, string jwtToken = null, CancellationToken cancellationToken = default);

        Task<bool> RemoveTokenAsync(string key, CancellationToken cancellationToken = default);
    }
}
