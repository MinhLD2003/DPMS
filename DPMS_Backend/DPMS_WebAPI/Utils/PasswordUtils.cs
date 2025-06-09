using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace DPMS_WebAPI.Utils
{
    public class PasswordUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string HashPassword(string password, string salt)
        {
            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(salt),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            //Console.WriteLine($"Hashed: {hashed}");

            return hashed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saltSize">Length of salt in bytes</param>
        /// <returns></returns>
        public static byte[] GenerateSalt(int saltSize = 128)
        {
            // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
            byte[] salt = RandomNumberGenerator.GetBytes(saltSize);

            return salt;
        }

        /// <summary>
        /// Generate random password
        /// </summary>
        /// <param name="numUppercase"></param>
        /// <param name="numLowercase"></param>
        /// <param name="numNumeric"></param>
        /// <returns>A password contains numUppercase + numLowercase + numNumeric characters</returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GeneratePassword(int numUppercase, int numLowercase, int numNumeric)
        {
            // Some defensive programming and argument checking...
            numUppercase = Math.Max(numUppercase, 0);
            numLowercase = Math.Max(numLowercase, 0);
            numNumeric = Math.Max(numNumeric, 0);
            var totalChars = numUppercase + numLowercase + numNumeric;
            if (totalChars == 0)
            {
                throw new ArgumentException("For at least one character type the number must be at least 1.");
            }
            ReadOnlySpan<char> lowercase = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'];
            ReadOnlySpan<char> uppercase = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'];
            ReadOnlySpan<char> numeric = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
            // stack memory is limited (varies depending on OS), so use heap memory for large buffers
            // Note: .NET will stackalloc abovementioned spans also on the stack as it is the most efficient way to allocate a span in a local function
            Span<char> buffer = totalChars <= 512
                ? stackalloc char[totalChars]
                : new char[totalChars];
            // Split the buffer into three parts for each character type
            var lowercaseChars = buffer[..numLowercase];
            var uppercaseChars = buffer.Slice(numLowercase, numUppercase);
            var numericChars = buffer[^numNumeric..];
            // Fill each part with random characters from the corresponding character set
            RandomNumberGenerator.GetItems(lowercase, lowercaseChars);
            RandomNumberGenerator.GetItems(uppercase, uppercaseChars);
            RandomNumberGenerator.GetItems(numeric, numericChars);

            // Shuffle the buffer in-place (no allocation!) to ensure that the characters are in random order
            RandomNumberGenerator.Shuffle(buffer);
            // Now we allocate the final string on the heap
            return new string(buffer);
        }
    }
}
