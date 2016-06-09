using System;
using System.Text;

namespace PasswordConsole
{
    /// <summary>
    /// A simple console application to generate SHA-512 hashed Passwords.
    /// </summary>
    class PasswordConsole
    {
        private static bool _debug = false;

        static void Main(string[] args)
        {

            // Do we want full debugging?
            if (args != null && args.Length > 0)
            {
                if ("-d".Equals(args[0]) || "--debug".Equals(args[0]))
                {
                    _debug = true;
                }
                else
                {
                    Console.Write("\nUsage: PasswordConsole [-d | --debug]");
                    Console.WriteLine("\n\nPress Enter key to continue...");
                    return;
                }
            }

            do
            {
                Console.Write("\nUsername (or exit): ");
                var username = Console.In.ReadLine() ?? "";

                if (username == "")
                {
                    continue;
                }
                if ("exit".Equals(username))
                {
                    break;
                }

                Console.Write("Password: ");
                var password = Console.In.ReadLine() ?? "";

                if (_debug)
                {
                    var salt = username.ToLower();
                    var cleartext = password + "{" + salt + "}";
                    // C# uses UTF-16 internally but here we use UTF-8 encoding
                    byte[] saltedcleartext = Encoding.UTF8.GetBytes(cleartext);

                    Console.WriteLine("clearText length:" + cleartext.Length);
                    Console.WriteLine("bytearray length: " + saltedcleartext.Length);
                    Console.WriteLine("bytearray: " + BitConverter.ToString(saltedcleartext));

                    byte[] hash = System.Security.Cryptography.SHA1.Create().ComputeHash(saltedcleartext);
                    Console.WriteLine("SHA1: " + Convert.ToBase64String(hash));
                    Console.WriteLine("SHA1: " + BitConverter.ToString(hash).Replace("-", ""));

                    byte[] hash2 = System.Security.Cryptography.SHA256.Create().ComputeHash(saltedcleartext);
                    Console.WriteLine("SHA256: " + Convert.ToBase64String(hash2));
                    Console.WriteLine("SHA256: " + BitConverter.ToString(hash2).Replace("-", ""));

                    byte[] hash3 = System.Security.Cryptography.SHA512.Create().ComputeHash(saltedcleartext);
                    Console.WriteLine("SHA512: " + Convert.ToBase64String(hash3));
                    Console.WriteLine("SHA512 (bytes): " + BitConverter.ToString(hash3));
                    Console.WriteLine("SHA512 (bytes): " + BitConverter.ToString(hash3).Replace("-", ""));
                }

                string base64Hash = PasswordHasher.GetBase64PasswordHash(username, password);
                Console.WriteLine("");
                //Console.WriteLine("User: {0}, Password: {1}, SHA-512: {2}", username, password, base64Hash);
                Console.WriteLine("Username: {0}", username);
                Console.WriteLine("Password: {0}", password);
                Console.WriteLine("SHA-512 : {0}", base64Hash);

            } while (true);

            Console.WriteLine("\n\nPress Enter key to continue...");
        }
    }
}
