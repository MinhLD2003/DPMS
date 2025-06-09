using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace DPMS_WebAPI.Utils
{
    public static class ConsentUtils
    {
        //private static byte[] Key;
        //private static byte[] IV;

        //// Initialize the encryption parameters from configuration
        //public static void Initialize(IConfiguration configuration)
        //{
        //    // Read from configuration
        //    string keyString = configuration["Encryption:Key"];
        //    string ivString = configuration["Encryption:IV"];

        //    if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(ivString))
        //    {
        //        throw new InvalidOperationException("Encryption keys not found in configuration.");
        //    }

        //    // Convert from Base64 strings to byte arrays
        //    Key = Convert.FromBase64String(keyString);
        //    IV = Convert.FromBase64String(ivString);
        //}

        ///// <summary>
        ///// Encrypts the provided email and returns a lowercase hexadecimal string.
        ///// </summary>
        //public static string EncryptEmail(string email)
        //{
        //    if (string.IsNullOrEmpty(email))
        //        throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Key = Key;
        //        aes.IV = IV;
        //        aes.Mode = CipherMode.CBC;
        //        aes.Padding = PaddingMode.PKCS7;

        //        using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        //            using (StreamWriter writer = new StreamWriter(cs))
        //            {
        //                writer.Write(email);
        //            }

        //            // Convert to lowercase hex string
        //            byte[] encryptedBytes = ms.ToArray();
        //            StringBuilder result = new StringBuilder(encryptedBytes.Length * 2);
        //            foreach (byte b in encryptedBytes)
        //            {
        //                result.Append(b.ToString("x2")); // "x2" formats as lowercase hex
        //            }
        //            return result.ToString();
        //        }
        //    }
        //}

        ///// <summary>
        ///// Decrypts the hex encoded string back to the original email.
        ///// </summary>
        //public static string DecryptEmail(string encryptedEmail)
        //{
        //    if (string.IsNullOrEmpty(encryptedEmail))
        //        throw new ArgumentException("Encrypted email cannot be null or empty.", nameof(encryptedEmail));

        //    // Convert hex string back to bytes (case insensitive)
        //    byte[] cipherText = new byte[encryptedEmail.Length / 2];
        //    for (int i = 0; i < cipherText.Length; i++)
        //    {
        //        cipherText[i] = Convert.ToByte(encryptedEmail.Substring(i * 2, 2), 16);
        //    }

        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Key = Key;
        //        aes.IV = IV;
        //        aes.Mode = CipherMode.CBC;
        //        aes.Padding = PaddingMode.PKCS7;

        //        using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
        //        using (MemoryStream ms = new MemoryStream(cipherText))
        //        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
        //        using (StreamReader reader = new StreamReader(cs))
        //        {
        //            return reader.ReadToEnd();
        //        }
        //    }
        //}
        private static byte[] Key;

        // Initialize the encryption parameters from configuration
        public static void Initialize(IConfiguration configuration)
        {
            // Read from configuration
            string keyString = configuration["Encryption:Key"];

            if (string.IsNullOrEmpty(keyString))
            {
                throw new InvalidOperationException("Encryption key not found in configuration.");
            }

            // Convert from Base64 string to byte array
            Key = Convert.FromBase64String(keyString);

            // Validate key size (AES-256 requires 32-byte key)
            if (Key.Length != 32)
            {
                throw new InvalidOperationException("Encryption key must be 32 bytes (256 bits) long.");
            }
        }

        /// <summary>
        /// Encrypts the provided email using AES-GCM and returns a lowercase hexadecimal string.
        /// </summary>
        public static string EncryptEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            // Get email bytes
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(email);

            // Generate a new nonce for each encryption (CRITICAL for security)
            byte[] nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
            RandomNumberGenerator.Fill(nonce);

            // Create cipher text buffer and authentication tag buffer
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

            // Encrypt with AES-GCM
            using (var aesGcm = new AesGcm(Key))
            {
                aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);
            }

            // Combine nonce + ciphertext + tag for storage
            byte[] result = new byte[nonce.Length + ciphertext.Length + tag.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, nonce.Length + ciphertext.Length, tag.Length);

            // Convert to lowercase hex string
            StringBuilder hexString = new StringBuilder(result.Length * 2);
            foreach (byte b in result)
            {
                hexString.Append(b.ToString("x2"));
            }

            return hexString.ToString();
        }

        /// <summary>
        /// Decrypts the hex encoded string back to the original email.
        /// </summary>
        public static string DecryptEmail(string encryptedEmail)
        {
            if (string.IsNullOrEmpty(encryptedEmail))
                throw new ArgumentException("Encrypted email cannot be null or empty.", nameof(encryptedEmail));

            // Convert hex string to bytes
            byte[] encryptedBytes = new byte[encryptedEmail.Length / 2];
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                encryptedBytes[i] = Convert.ToByte(encryptedEmail.Substring(i * 2, 2), 16);
            }

            // Extract nonce, ciphertext and tag
            int nonceLength = AesGcm.NonceByteSizes.MaxSize; // 12 bytes
            int tagLength = AesGcm.TagByteSizes.MaxSize; // 16 bytes
            int ciphertextLength = encryptedBytes.Length - nonceLength - tagLength;

            if (ciphertextLength < 0)
                throw new ArgumentException("Invalid encrypted data format.");

            byte[] nonce = new byte[nonceLength];
            byte[] ciphertext = new byte[ciphertextLength];
            byte[] tag = new byte[tagLength];

            Buffer.BlockCopy(encryptedBytes, 0, nonce, 0, nonceLength);
            Buffer.BlockCopy(encryptedBytes, nonceLength, ciphertext, 0, ciphertextLength);
            Buffer.BlockCopy(encryptedBytes, nonceLength + ciphertextLength, tag, 0, tagLength);

            // Decrypt with AES-GCM
            byte[] plaintextBytes = new byte[ciphertextLength];
            using (var aesGcm = new AesGcm(Key))
            {
                try
                {
                    aesGcm.Decrypt(nonce, ciphertext, tag, plaintextBytes);
                }
                catch (CryptographicException)
                {
                    throw new CryptographicException("Authentication failed. The encrypted data may have been tampered with.");
                }
            }

            return Encoding.UTF8.GetString(plaintextBytes);
        }

        // The remaining utility methods can stay unchanged
        // GenerateApiKey(), HashApiKey(), GenerateTokenString()
        /// <summary>
        /// Generates a secure API key with only alphanumeric characters.
        /// </summary>
        /// <param name="length">Length of the API key in characters (default: 32 characters).</param>
        /// <returns>An alphanumeric API key.</returns>
        public static string GenerateApiKey(int length = 32)
        {
            // Define allowed characters (alphanumeric only)
            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            
            using (var rng = new RNGCryptoServiceProvider())
            {
                var result = new StringBuilder(length);
                byte[] randomByte = new byte[1];

                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(randomByte);
                    // Use modulo to get an index within the allowed characters range
                    var index = randomByte[0] % allowedChars.Length;
                    result.Append(allowedChars[index]);
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// Hash API key and return a lowercase hexadecimal string
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns>Lowercase hexadecimal representation of the hash</returns>
        public static string HashApiKey(string apiKey)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
                // Convert to lowercase hexadecimal string
                StringBuilder result = new StringBuilder(hashedBytes.Length * 2);
                foreach (byte b in hashedBytes)
                {
                    result.Append(b.ToString("x2")); // "x2" formats as lowercase hex
                }
                return result.ToString();
            }
        }

        /// <summary>
        /// Generates a random token string using only alphanumeric characters
        /// </summary>
        /// <returns>Alphanumeric token string</returns>
        public static string GenerateTokenString(int length = 64)
        {
            // Define allowed characters (alphanumeric only)
            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            
            using (var rng = new RNGCryptoServiceProvider())
            {
                var result = new StringBuilder(length);
                byte[] randomByte = new byte[1];

                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(randomByte);
                    var index = randomByte[0] % allowedChars.Length;
                    result.Append(allowedChars[index]);
                }

                return result.ToString();
            }
        }
    }
}