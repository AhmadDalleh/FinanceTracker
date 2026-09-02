using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetValidTokenByHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken);
}
