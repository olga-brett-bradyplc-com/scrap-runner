using BulkPasswordConsole.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace BulkPasswordConsole
{
    /// <summary>
    /// A rudimentary console application to read in a two column, tab delimited, input file 
    /// and bulk generate hashed passwords (SHA-512 in Base64) into an output file.
    /// 
    /// Input is a simple text file.  The expected format is tab separated usernames and passwords:
    /// <code>
    /// Username\tPassword\n    (This header row is optional)
    /// 1000\tdrv1000\n
    /// 121\tdrv121\n
    /// 124\tdrv124\n
    /// ...
    /// </code>
    /// The usernames and passwords are trimmed after splitting on the tab.
    /// Optional arguments are the input file or the input file and output file in that order.
    /// </summary>
    class BulkPasswordConsole
    {
        private static void Main(string[] args)
        {

            //
            // Determine and read the input file
            //

            string inputFileName;
            if (args != null && args.Length > 0)
            {
                inputFileName = args[0];
            }
            else
            {
                Console.Write("\nInput file: ");
                inputFileName = Console.In.ReadLine() ?? "";
            }
            inputFileName = inputFileName.Trim();

            if (inputFileName.Equals(""))
            {
                Console.WriteLine("No trivial input filenames!");
                Pause();
                return;
            }

            if (!File.Exists(inputFileName))
            {
                Console.WriteLine("Can't find file: {0}", inputFileName);
                Pause();
                return;
            }

            var inputLines = File.ReadAllLines(inputFileName);
            Console.WriteLine("{0} input lines read from: {1}", inputLines.Length, inputFileName);

            //
            // Determine and set up the output file stream 
            //

            string outputFileName;
            if (args != null && args.Length > 1)
            {
                outputFileName = args[1];
            }
            else
            {
                Console.Write("\nOutput file: ");
                outputFileName = Console.In.ReadLine() ?? "";
            }
            outputFileName = outputFileName.Trim();

            if (outputFileName.Equals(""))
            {
                Console.WriteLine("No trivial output filenames!");
                Pause();
                return;
            }

            if (File.Exists(outputFileName))
            {
                Console.WriteLine("File exists!: {0}", outputFileName);
                Pause();
                return;
            }

            var lineNo = 0;
            var writeLineNo = 0;
            var updateStmtList = new List<string>();
            using (var streamWriter = new StreamWriter(File.Create(outputFileName)))
            {
                foreach (var line in inputLines)
                {
                    lineNo++;
                    var trimLine = line.Trim();
                    if (trimLine.ToLower().StartsWith("username"))
                    {
                        Console.WriteLine("Skipping header line {0}: {1}", lineNo, line);
                        continue;
                    }

                    var pair = trimLine.Split('\t');
                    if (pair.Length != 2)
                    {
                        Console.WriteLine("Skipping bad line {0}: {1}", lineNo, line);
                        continue;
                    }

                    var username = pair[0].Trim();
                    var password = pair[1].Trim();
                    if (username.Length == 0 || password.Length == 0)
                    {
                        Console.WriteLine("Skipping trivial/blank username or password on line {0}: {1}", lineNo, line);
                        continue;
                    }

                    var hash = PasswordHasher.GetBase64PasswordHash(username, password);
                    var updateStmt = string.Format(
                        "UPDATE dbo.EmployeeMaster SET PasswordEncrypted = '{0}' WHERE EmployeeId = '{1}' AND PasswordEncrypted IS NULL ",
                        hash, username);
                    updateStmtList.Add(updateStmt);

                    writeLineNo++;
                    if (writeLineNo == 1)
                    {
                        streamWriter.WriteLine("Username\tPassword\tHash");
                    }
                    streamWriter.WriteLine("{0}\t{1}\t{2}", username, password, hash);
                }

                if (writeLineNo > 0)
                {
                    streamWriter.WriteLine("");
                    streamWriter.WriteLine("SQL Update Statements");
                    foreach (var line in updateStmtList)
                    {
                        streamWriter.WriteLine("{0}", line);
                    }
                }

                streamWriter.Flush();
            }
            Console.WriteLine("{0} Lines processed and written to outputfile: {1}", writeLineNo, outputFileName);
            Pause();
        }

        private static void Pause()
        {
            Console.WriteLine("\n\nPress Enter key to continue...");
            Console.In.ReadLine();
        }
    }
}
