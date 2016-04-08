using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using BWF.Membership.Adaptor.Interfaces;
using BWF.Membership.Adaptor.Models;

namespace Brady.ScrapRunner.Host.Membership
{

    /// <summary>
    /// The Scrap Runner specific implementation of Membership 
    /// </summary>
    public class ScrapRunMembershipProvider : IMembershipAdaptor
    {

        /// <summary>
        /// Allow selection of adaptor by framework 
        /// </summary>
        public string AdaptorName => "ScrapRunMembershipProvider";

        /// <summary>
        /// Indicate to the BWF if this optional functionality is implemented.
        /// </summary>
        public bool SupportsCreateUpdateAndDeleteUser => false;

        /// <summary>
        /// Indicate to the BWF if this optional functionality is implemented.
        /// </summary>
        public bool SupportsCreateAndDeleteRole => false;

        /// <summary>
        /// Indicate to the BWF if this optional functionality is implemented.
        /// </summary>
        public bool SupportsAssignRoleToUserAndRemoveRoleFromUser => false;

        /// <summary>
        /// Indicate to the BWF if this optional functionality is implemented.
        /// </summary>
        public bool SupportsChangePassword => false;

        /// <summary>
        /// Indicate to the BWF if this optional functionality is implemented.
        /// </summary>
        public bool SupportsPasswordReset => false;

        /// <summary>
        /// Indicate to the BWF if this optional functionality is implemented.
        /// </summary>
        public bool SupportsUnlockUser => false;

        /// <summary>
        /// Return existing users 
        /// </summary>
        public IEnumerable<ExternalUser> Users
        {
            get
            {
                var externalUsers = new List<ExternalUser>
                {
                    new ExternalUser()
                    {
                        Username = "admin",
                        EmailAddress = "steve.maniak@bradyplc.com",
                        IsApproved = true
                    },
                    new ExternalUser()
                    {
                        Username = "steve",
                        EmailAddress = "steve.maniak@bradyplc.com",
                        IsApproved = true
                    },
                    new ExternalUser()
                    {
                        Username = "smaniak",
                        EmailAddress = "steve.maniak@bradyplc.com",
                        IsApproved = true
                    },
                    new ExternalUser()
                    {
                        Username = "sparky",
                        EmailAddress = "steve.maniak@bradyplc.com",
                        IsApproved = true
                    }
                };

                var connectionString = ConfigurationManager.ConnectionStrings["ScrapRunner"].ConnectionString;
                var queryString = "SELECT EmployeeId AS UserName, " + 
                                  "       '' AS EmailAddress, " +
                                  "       CAST(CASE WHEN InactiveDate IS NULL THEN 1 ELSE 0 END AS bit) as IsApproved " +
                                  "  FROM dbo.EmployeeMaster ";

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (var command = new SqlCommand(queryString, con))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                externalUsers.Add(new ExternalUser() { Username = reader.GetString(0), EmailAddress = reader.GetString(1), IsApproved = reader.GetBoolean(2) });
                            }
                        }
                    }
                }
                return externalUsers;
            }
        }

        /// <summary>
        /// Return all recognized roles.
        /// </summary>
        public IEnumerable<string> Roles
        {
            get
            {
                // Perhaps something like: SELECT DISTINCT SecurityLevel from dbo.EmployeeMaster
                return new[] { "Administrator", "Dispatcher", "Driver" };
            }
        }

        /// <summary>
        /// Is the user locked?
        /// </summary>
        /// <param name="username"></param>
        /// <returns>true of user is locked, false otherwise</returns>
        public bool IsUserLocked(string username)
        {
            // Details TBD.  If even supported, perhaps SELECT EmployeeStatus FROM dbo.EmployeeMaster WHERE EmployeeId = '{0}'
            return false;
        }

        /// <summary>
        /// What Role(s) does this user have?
        /// </summary>
        /// <param name="username"></param>
        /// <returns>A list of zero to many roles</returns>
        public IEnumerable<string> GetRolesForUser(string username)
        {
            // Details TBD.  Probably simple like: SELECT SecurityLevel from dbo.EmployeeMaster WHERE EmployeeId = '{0}'
            return new[] { "Administrator" };
        }

        /// <summary>
        /// Get the User's Key.  In our system this is the same as username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>the username</returns>
        public object GetUserKey(string username)
        {
            return username;
        }

        /// <summary>
        /// Authenticate the user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>true if password is valid and user is not restricted/locked</returns>
        public bool AuthenticateUser(string username, string password)
        {
            // Details TBD.  Perhaps something like hash and compare: 
            // SELECT Password from dbo.EmployeeMaster WHERE EmployeeId = '{0}'
            bool authenticated = null != username && null != password && "mem_2014".Equals(password);
            return authenticated;
        }

        /// <summary>
        /// Let the user change an existing password.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns>true if successful</returns>
        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            // Details TBD.  Perhaps somethign like hash old and compare: 
            // SELECT Password from dbo.EmployeeMaster WHERE EmployeeId = '{0}'
            // Then hash new and insert: 
            // UPDATE dbo.EmployeeMaster SET Password = '{0}' from  WHERE EmployeeId = '{1}'
            throw new NotImplementedException();
        }

        /// <summary>
        /// Let an administrator hard reset a password.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public bool ResetPassword(string username, string newPassword)
        {
            // Details TBD:  Perhaps simply hash new and insert: 
            // UPDATE dbo.EmployeeMaster SET Password = '{0}' from  WHERE EmployeeId = '{1}'
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unlock a user
        /// </summary>
        /// <param name="username"></param>
        /// <returns>true if user is currently or now unlocked</returns>
        public bool UnlockUser(string username)
        {
            // Details TBD:  Any support?
            throw new NotImplementedException();
        }

        public bool CreateUser(ExternalUser user, string password)
        {
            throw new NotImplementedException();
        }

        public bool UpdateUser(ExternalUser user)
        {
            throw new NotImplementedException();
        }

        public bool DeleteUser(string username)
        {
            throw new NotImplementedException();
        }

        public bool CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public bool DeleteRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public bool AssignRoleToUser(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public bool RemoveRoleFromUser(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Hash a user's password.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Base64 representaion of the user's salted and hashed password.</returns>
        private string hashPassword(string username, string password)
        {
            // This is a sample SHA512 example.  We may need to retrofit whatever hashing or encodidng the
            // legacy SR code is using.
            if (null == username)
            {
                username = "";
            }
            if (null == password)
            {
                password = "";
            }
            var salt = username.ToLower();
            byte[] saltedcleartext = System.Text.Encoding.UTF8.GetBytes(password + "{" + salt + "}");
            byte[] hash = System.Security.Cryptography.SHA512.Create().ComputeHash(saltedcleartext);
            return Convert.ToBase64String(hash);
        }

    }
}
