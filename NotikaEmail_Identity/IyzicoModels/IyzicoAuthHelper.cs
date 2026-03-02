using System.Security.Cryptography;
using System.Text;

namespace Core_IyzicoPaymentSystem.Methods
{
    public static class IyzicoAuthHelper
    {
        public static string GenerateAuthToke(string apiKey,string secretKey,string uriPath,string requestData="")
        {

            var randomKey = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            // Payload oluşturma
            var payload = string.IsNullOrEmpty(requestData) ? $"{randomKey}{uriPath}" : $"{randomKey}{uriPath}{requestData}";

            // HMAC SHA256 ile imzalama
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var signature = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            // Authorization string oluşturma
            var authorizationString = $"apiKey:{apiKey}&randomKey:{randomKey}&signature:{signature}";

            // Base64 encode etme
            var base64EncodeAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(authorizationString));

            return $"IYZWSv2 {base64EncodeAuth}";


        }

    }
}
