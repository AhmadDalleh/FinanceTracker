using Domain.Entities;

namespace Application.Common.Security;

public static class RefreshTokenFactory
{
    private const int ValidityDays = 7;

    public static (RefreshToken Token, string RawToken) Create(Guid userId, DateTimeOffset now)
    {
        var rawToken = SecureTokenGenerator.GenerateUrlSafeToken();
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = SecureTokenHasher.Hash(rawToken),
            ExpiresAt = now.AddDays(ValidityDays),
            CreatedAt = now
        };

        return (token, rawToken);
    }
}
