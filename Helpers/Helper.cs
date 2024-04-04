using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;

namespace UserServiceTemplate.Helpers;

public static class Helper {
    public static string GetHashPassword(string password) {
        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!,
            salt: Encoding.UTF8.GetBytes("salt"),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
    }
}
