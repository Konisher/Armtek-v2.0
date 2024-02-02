using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ApiKeys
{
    public class ApiKeys
    {
        private static List<string> validApiKeys = new List<string>
        {
            "a144e13e-59e5-4c9e-a40a-717782010126",
            "1234-5678"
        };

        public static bool IsValidApiKey(string apiKey)
        {
            return validApiKeys.Contains(apiKey);
        }

        public static async Task AddKeysAsync()
        {
            var api = await GenerateApiKeyAsync();
            validApiKeys.Add(api);
            Console.WriteLine(api);
        }

        public static async Task<string> GenerateApiKeyAsync(int length = 32)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] chars = new char[length];

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                byte[] data = new byte[length];
                await Task.Run(() =>
                {
                    crypto.GetBytes(data);
                });

                for (int i = 0; i < length; i++)
                {
                    chars[i] = validChars[data[i] % validChars.Length];
                }
            }

            string apiKey = new string(chars);
            Console.WriteLine(apiKey); // Output the generated API key to the console
            return apiKey;
        }

    }
}
