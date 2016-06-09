using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordConsole
{
    public class PasswordHasher
    {
        /// <summary>
        /// Salt and SHA-512 hash a user's provided password returning it as a Base64 encoded string.
        /// </summary>
        /// <param name="username">The user's username</param>
        /// <param name="password">The user's entered clartext password</param>
        /// <returns></returns>
        public static string GetBase64PasswordHash(string username, string password)
        {
            if (null == username)
            {
                throw new ArgumentNullException(nameof(username));
            }
            if (null == password)
            {
                throw new ArgumentNullException(nameof(password));
            }

            var salt = username.ToLower();
            var saltedcleartext = password + "{" + salt + "}";

            // C# uses UTF-16 internally but here we use UTF-8 encoding
            byte[] saltedBytes = Encoding.UTF8.GetBytes(saltedcleartext);
            byte[] sha512Hash = System.Security.Cryptography.SHA512.Create().ComputeHash(saltedBytes);
            string base64ShaHash = Convert.ToBase64String(sha512Hash);
            return base64ShaHash;
        }
    }
}
