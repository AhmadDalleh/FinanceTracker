using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetValidTokenByHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken);
}
