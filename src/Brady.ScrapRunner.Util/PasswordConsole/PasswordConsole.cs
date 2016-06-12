using PasswordConsole.Util;
using System;
using System.Text;

namespace PasswordConsole
{
    /// <summary>
    /// A simple console application to generate SHA-512 hashed Passwords.
    /// </summary>
    class PasswordConsole
    {
        private static void Main(string[] args)
        {

            // Do we want full debugging?
            bool debug = false;
            if (args != null && args.Length > 0)
            {
                if ("-d".Equals(args[0]) || "--debug".Equals(args[0]))
                {
                    debug = true;
                }
                else
                {
                    Console.Write("\nUsage: PasswordConsole [-d | --debug]");
                    Console.WriteLine("\n\nPress Enter key to continue...");
                    Console.In.ReadLine();
                    return;
                }
            }

            Console.WriteLine("Note: username and password input are trimmed of all whitespace.");
            do
            {
                Console.Write("\nUsername (or exit): ");
                var username = Console.In.ReadLine() ?? "";
                username = username.Trim();

                if (username.Trim() == "")
                {
                    Console.WriteLine("Trival username not allowed.");
                    continue;
                }
                if ("exit".Equals(username))
                {
                    break;
                }

                Console.Write("Password: ");
                var password = Console.In.ReadLine() ?? "";
                password = password.Trim();

                if (password.Trim() == "")
                {
                    Console.WriteLine("Trival password not allowed.");
                    continue;
                }


                if (debug)
                {
                    // The general approach
                    // C# uses UTF-16 internally but here we use UTF-8 encoding
                    var salt = username.ToLower();
                    var cleartext = password + "{" + salt + "}";
                    byte[] saltedcleartext = Encoding.UTF8.GetBytes(cleartext);

                    Console.WriteLine("\nclearText length:" + cleartext.Length);
                    Console.WriteLine("bytearray length: " + saltedcleartext.Length);
                    Console.WriteLine("bytearray: " + BitConverter.ToString(saltedcleartext));

                    // Different hashes and representations
                    byte[] hash = System.Security.Cryptography.SHA1.Create().ComputeHash(saltedcleartext);
                    Console.WriteLine("SHA1: " + BitConverter.ToString(hash));
                    Console.WriteLine("SHA1: " + BitConverter.ToString(hash).Replace("-", ""));
                    Console.WriteLine("SHA1: " + Convert.ToBase64String(hash));

                    byte[] hash2 = System.Security.Cryptography.SHA256.Create().ComputeHash(saltedcleartext);
                    Console.WriteLine("SHA256: " + BitConverter.ToString(hash2));
                    Console.WriteLine("SHA256: " + BitConverter.ToString(hash2).Replace("-", ""));
                    Console.WriteLine("SHA256: " + Convert.ToBase64String(hash2));

                    byte[] hash3 = System.Security.Cryptography.SHA512.Create().ComputeHash(saltedcleartext);
                    Console.WriteLine("SHA512: " + BitConverter.ToString(hash3));
                    Console.WriteLine("SHA512: " + BitConverter.ToString(hash3).Replace("-", ""));
                    Console.WriteLine("SHA512: " + Convert.ToBase64String(hash3));
                }

                string base64Hash = PasswordHasher.GetBase64PasswordHash(username, password);
                Console.WriteLine("");
                //Console.WriteLine("User: {0}, Password: {1}, SHA-512: {2}", username, password, base64Hash);
                Console.WriteLine("Username: {0}", username);
                Console.WriteLine("Password: {0}", password);
                Console.WriteLine("SHA-512 : {0}", base64Hash);

            } while (true);

            Pause();
        }

        private static void Pause()
        {
            Console.WriteLine("\n\nPress Enter key to continue...");
            Console.In.ReadLine();
        }

    }
}
