using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using BWF.Membership.Adaptor.Interfaces;
using BWF.Membership.Adaptor.Models;
using System.Threading.Tasks;
using log4net;

namespace Brady.ScrapRunner.Host.Membership
{

    /// <summary>
    /// The Scrap Runner specific implementation of Membership 
    /// </summary>
    public class ScrapRunMembershipProvider : IMembershipAdaptor
    {

        private static readonly ILog Log = LogManager.GetLogger(typeof (ScrapRunMembershipProvider));

        // TODO: Remove or disable hardcoded admin prior to customer deployment
        private bool enableHardcodedAdmin = true;

        // TODO:  Flesh out roles and user assignments.
        // For now only drivers use the service, so currently we assign them the administrator role 
        // for set-up simplicity.  Note we could use the exploroer as an alternative password reset 
        // mechanism.
        //
        //   private const string RoleDispatcher = "Dispatcher";
        //   private const string RoleDriver = "Driver";
        private const string RoleAdministrator = "Administrator";
        private List<string> allRoles = new List<string>();
        private List<string> userRoles = new List<string>();

        /// <summary>
        /// The non-argument constructor
        /// </summary>
        public ScrapRunMembershipProvider()
        {
            allRoles.Add(RoleAdministrator);
            // allRoles.Add(RoleDriver);
            // allRoles.Add(RoleDispatcher);

            userRoles.Add(RoleAdministrator);
            // userRoles.Add(RoleDriver);
            // userRoles.Add(RoleDispatcher);
        }

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
        public bool SupportsChangePassword => true;

        /// <summary>
        /// Indicate to the BWF if this optional functionality is implemented.
        /// </summary>
        public bool SupportsPasswordReset => true;

        /// <summary>
        /// Indicate to the BWF if this optional functionality is implemented.
        /// </summary>
        public bool SupportsUnlockUser => false;

        /// <summary>
        /// Indicate to the BWF if this optional functionality is implemented.
        /// </summary>
        public bool RequiresExternalAuthenticationUrl => false;

        /// <summary>
        /// Indicate to the BWF if this optional functionality is implemented.
        /// </summary>
        public bool RequiresExternalSignOutUrl => false;

        /// <summary>
        /// Return existing users 
        /// </summary>
        public Task<IEnumerable<ExternalUser>> GetUsersAsync()
        {
            Log.Debug("Enter GetUsersAsync()");
            var externalUsers = new List<ExternalUser>();

            if (enableHardcodedAdmin)
            {
                externalUsers.Add(
                    new ExternalUser()
                    {
                        Username = "admin",
                        EmailAddress = "",
                        IsApproved = true
                    });
            }

            var connectionString = ConfigurationManager.ConnectionStrings["ScrapRunner"].ConnectionString;
            var queryString = "SELECT em.EmployeeId AS UserName, " +
                              "       '' AS EmailAddress, " +
                              "       CAST(CASE WHEN em.InactiveDate IS NULL THEN 1 ELSE 0 END AS bit) as IsApproved " +
                              "  FROM dbo.EmployeeMaster AS em " +
                              " WHERE em.SecurityLevel = 'DR' " +
                              " ORDER BY em.EmployeeId ";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    var command = new SqlCommand(queryString, con);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        externalUsers.Add(new ExternalUser()
                        {
                            Username = reader.GetString(0),
                            EmailAddress = reader.GetString(1),
                            IsApproved = reader.GetBoolean(2)
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception in GetUsersAsync", e);
            }
            return Task.FromResult((IEnumerable<ExternalUser>) externalUsers);
        }

        /// <summary>
        /// Is the user locked?
        /// </summary>
        /// <param name="username"></param>
        /// <returns>true of user is locked, false otherwise</returns>
        public Task<bool> IsUserLockedAsync(string username)
        {
            // Locking not supported
            return Task.FromResult(false);
        }

        /// <summary>
        /// What Role(s) does this user have?
        /// </summary>
        /// <param name="username"></param>
        /// <returns>A list of zero to many roles</returns>
        public Task<IEnumerable<string>> GetRolesForUserAsync(string username)
        {
            // TODO: UserRole details TBD.
            return Task.FromResult((IEnumerable<string>) userRoles);
        }

        /// <summary>
        /// Get the User's Key.  In our system this is the same as username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>the username</returns>
        public Task<object> GetUserKeyAsync(string username)
        {
            return Task.FromResult<object>(username);
        }

        /// <summary>
        /// Authenticate the user.
        /// </summary>
        /// <param name="credentials">a UsernameAndPassword tuple</param>
        /// <returns>true if password is valid and user is not restricted/locked</returns>
        public Task<string> AuthenticateUserAsync(object credentials)
        {
            Log.Debug("Enter AuthenticateUserAsync(object credentials)");
            bool isAuthenticated = false;
            var usernameAndPassword = credentials as UsernameAndPassword;
            if (null != usernameAndPassword)
            {

                if (enableHardcodedAdmin && "admin".Equals(usernameAndPassword.Username))
                {
                    isAuthenticated = "mem_2014".Equals(usernameAndPassword.Password);
                }
                else
                {
                    var hashIncoming = PasswordHasher.GetBase64PasswordHash(usernameAndPassword.Username,
                        usernameAndPassword.Password);
                    var oldUsernameAndPassword = SelectPersistedUserAndPasswordHash(usernameAndPassword.Username);
                    if (!string.IsNullOrEmpty(oldUsernameAndPassword?.Password))
                    {
                        isAuthenticated = hashIncoming.Equals(oldUsernameAndPassword.Password);
                    }
                }
            }
            return Task.FromResult(isAuthenticated ? usernameAndPassword.Username : null);
        }

        /// <summary>
        /// Let the user change an existing password.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns>true if successful</returns>
        public Task<bool> ChangePasswordAsync(string username, string oldPassword, string newPassword)
        {
            Log.Debug("Enter ChangePasswordAsync(username, oldPassword, newPassword)");
            bool wasSuccessful = false;

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Must not be null or empty", nameof(username));
            }
            if (string.IsNullOrEmpty(oldPassword))
            {
                throw new ArgumentException("Must not be null or empty", nameof(oldPassword));
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("Must not be null or empty", nameof(newPassword));
            }
            if (oldPassword.Equals(newPassword))
            {
                throw new ArgumentException("New password must differ from old password", nameof(newPassword));
            }

            if (enableHardcodedAdmin && "admin".Equals(username))
            {
                //Option:  throw new NotImplementedException();
                // for now: wasSuccessful = false;
            }
            else
            {
                var oldHash = PasswordHasher.GetBase64PasswordHash(username, oldPassword);
                var newHash = PasswordHasher.GetBase64PasswordHash(username, newPassword);
                var oldUsernameAndPassword = SelectPersistedUserAndPasswordHash(username);
                if (!string.IsNullOrEmpty(oldUsernameAndPassword?.Password))
                {
                    if (oldHash.Equals(oldUsernameAndPassword.Password))
                    {
                        var connectionString = ConfigurationManager.ConnectionStrings["ScrapRunner"].ConnectionString;
                        string sql = "UPDATE dbo.EmployeeMaster " +
                                     "   SET PasswordEncrypted = @PasswordEncrypted " +
                                     " WHERE EmployeeId = @EmployeeId ";

                        using (SqlConnection con = new SqlConnection(connectionString))
                        {
                            con.Open();
                            var command = con.CreateCommand();
                            var transaction = con.BeginTransaction();

                            try
                            {
                                // Must assign both transaction object and connection
                                // to Command object for a pending local transaction
                                command.Connection = con;
                                command.Transaction = transaction;
                                command.CommandText = sql;
                                command.Parameters.Add("@PasswordEncrypted", SqlDbType.VarChar);
                                command.Parameters.Add("@EmployeeId", SqlDbType.VarChar);
                                command.Parameters["@PasswordEncrypted"].Value = newHash;
                                command.Parameters["@EmployeeId"].Value = username;
                                var numrows = command.ExecuteNonQuery();

                                if (numrows == 1)
                                {
                                    wasSuccessful = true;
                                }

                                if (numrows > 1)
                                {
                                    throw new Exception("numrows greater than 1 when updating password!");
                                }

                                transaction.Commit();
                            }
                            catch (Exception e)
                            {
                                transaction.Rollback();
                                Log.Error("Exception in SelectPersistedUserAndPasswordHash", e);
                            }
                        }
                    }
                }
            }
            return Task.FromResult(wasSuccessful);
        }

        /// <summary>
        /// Let an administrator hard reset a password.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public Task<bool> ResetPasswordAsync(string username, string newPassword)
        {
            Log.Debug("Enter ResetPasswordAsync(username, newPassword)");
            bool wasSuccessful = false;

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Must not be null or empty", nameof(username));
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentException("Must not be null or empty", nameof(newPassword));
            }

            if (enableHardcodedAdmin && "admin".Equals(username))
            {
                //Option:  throw new NotImplementedException();
                // for now: wasSuccessful = false;
            }
            else
            {
                var newHash = PasswordHasher.GetBase64PasswordHash(username, newPassword);
                var connectionString = ConfigurationManager.ConnectionStrings["ScrapRunner"].ConnectionString;
                string sql = "UPDATE dbo.EmployeeMaster " +
                             "   SET PasswordEncrypted = @PasswordEncrypted " +
                             " WHERE EmployeeId = @EmployeeId ";

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    var command = con.CreateCommand();
                    var transaction = con.BeginTransaction();

                    try
                    {
                        // Must assign both transaction object and connection
                        // to Command object for a pending local transaction
                        command.Connection = con;
                        command.Transaction = transaction;
                        command.CommandText = sql;
                        command.Parameters.Add("@PasswordEncrypted", SqlDbType.VarChar);
                        command.Parameters.Add("@EmployeeId", SqlDbType.VarChar);
                        command.Parameters["@PasswordEncrypted"].Value = newHash;
                        command.Parameters["@EmployeeId"].Value = username;
                        var numrows = command.ExecuteNonQuery();
                        if (numrows == 1)
                        {
                            wasSuccessful = true;
                        }
                        if (numrows > 1)
                        {
                            throw new Exception("numrows greater than 1 when resetting password!");
                        }
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        Log.Error("Exception in SelectPersistedUserAndPasswordHash", e);
                    }
                }
            }
            return Task.FromResult(wasSuccessful);
        }

        public Task<bool> CreateUserAsync(ExternalUser user, string password)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateUserAsync(ExternalUser user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteUserAsync(string username)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateRoleAsync(string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteRoleAsync(string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AssignRoleToUserAsync(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveRoleFromUserAsync(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetRolesAsync()
        {
            // TODO: AllRole details TBD.
            return Task.FromResult((IEnumerable<string>)allRoles);
        }
        
        public Task<bool> UnlockUserAsync(string username)
        {
            throw new NotImplementedException();
        }

        public string GetExternalAuthenticationUrl(string redirectUrl)
        {
            throw new NotImplementedException();
        }

        public string GetExternalSignOutUrl(string redirectUrl, string reason)
        {
            throw new NotImplementedException();
        }

        public string GetAuthenticationCodeForAuthenticatedQuery(dynamic query)
        {
            throw new NotImplementedException();
        }

        public string GetRedirectUrlForAuthenticatedQuery(dynamic query)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fetch an existing username and password hash from the database.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>null if username not found</returns>
        private UsernameAndPassword SelectPersistedUserAndPasswordHash(string username)
        {
            Log.Debug("Enter SelectPersistedUserAndPasswordHash(username)");
            UsernameAndPassword usernameAndPassword = null;
            var connectionString = ConfigurationManager.ConnectionStrings["ScrapRunner"].ConnectionString;
            var queryString = "SELECT em.EmployeeId, em.PasswordEncrypted " +
                              "  FROM dbo.EmployeeMaster AS em " +
                              " WHERE em.EmployeeId = @EmployeeId ";
            if (!string.IsNullOrEmpty(username))
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        var command = new SqlCommand(queryString, con);
                        command.Parameters.Add("@EmployeeId", SqlDbType.VarChar);
                        command.Parameters["@EmployeeId"].Value = username;
                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            // There is at most one result, possibly null
                            usernameAndPassword = new UsernameAndPassword(reader.GetString(0), reader.GetString(1));
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Exception in SelectPersistedUserAndPasswordHash", e);
                }
            }
            return usernameAndPassword;
        }
    }
}
