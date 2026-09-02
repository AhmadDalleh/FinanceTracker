using System.Security.Cryptography;

namespace Application.Common.Security;

public static class SecureTokenGenerator
{
    public static string GenerateUrlSafeToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}
