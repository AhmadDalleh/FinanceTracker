using System.Security.Cryptography;
using System.Text;

namespace Application.Common.Security;

public static class PasswordResetTokenHasher
{
    public static string Hash(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
