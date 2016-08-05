﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using MCCA.Models;
using S = System.Data.SqlClient;
using T = System.Threading;
using MCCA.ViewModels.Director;

namespace MCCA.Controllers
{ 
    public class DirectorController : Controller
    {
        //Used for personal account management
        private static int directorID;
        //This method is a simple hello to the user when he or she signs in as well as saving the ID for personal account
        //management
        public IActionResult Index(int ID, String firstName, String lastName)
        {
            directorID = ID;
            ViewData["firstName"] = firstName;
            ViewData["lastName"] = lastName;
            return View();
        }
        public IActionResult ManageAccounts()
        {
            return View();
        }
        public IActionResult AddAccount()
        {
            return View();
        }
        public IActionResult ManageSite()
        {
            return View();
        }
        //This method allows the User to edit personal account information, save the changes to the SQL database and
        //refreshes the page for the User showing the update informatin if successful
        public IActionResult ManagePersonalAccount(ManagePersonalAccountViewModel model)
        {
            Boolean success = false;
            User foundUser = new Models.User();
            User updatedUser = new Models.User();
            //Getting SQL table entry based on User ID
            foundUser = sqlConnectionForUser(directorID);
            //Storing the information in ViewData to be used to pre-fill the Manage Personal Account form
            ViewData["firstName"] = foundUser.FirstName.TrimEnd(' ');
            ViewData["lastName"] = foundUser.LastName.TrimEnd(' ');
            ViewData["accountType"] = foundUser.AccountType.TrimEnd(' ');
            ViewData["center"] = foundUser.Center.TrimEnd(' ');
            ViewData["email"] = foundUser.Email.TrimEnd(' ');
            ViewData["phoneNumber"] = foundUser.PhoneNumber.TrimEnd(' ');
            ViewData["userName"] = foundUser.Username.TrimEnd(' ');
            //Getting ViewModel model information given in the textfields of the Manage Personal Account page
            updatedUser.FirstName = model.FirstName;
            updatedUser.LastName = model.LastName;
            updatedUser.AccountType = model.AccountType;
            updatedUser.Center = model.Center;
            updatedUser.Email = model.Email;
            updatedUser.PhoneNumber = model.PhoneNumber;
            updatedUser.Username = model.Username;
            updatedUser.Password = model.Password;
            //Getting Boolean result of SQL entry information update
            success = sqlConnectionUpdateUser(directorID, updatedUser);
            //If the update was successful, redirect the User to the Manage Personal Account View to refresh the page
            //with the updated information.
            if (success == true)
            {
                return RedirectToAction("ManagePersonalAccount");
            }
            return View();
        }
        //This method simply provides the confirmation page for the deletion of one's account from the database
        public IActionResult DeletePersonalAccount()
        {
            return View();
        }
        //This method is called when the delete confirmation button is clicked on when deleting own account.
        //It deletes the User from the database and sends the User to the Home Page if successful.
        [HttpPost, ActionName("DeletePersonalAccount")]
        public IActionResult DeletePersonalAccountConfirmed()
        {
            Boolean success = false;
            success = sqlConnectionDeleteUser(directorID);
            if (success == true)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            return RedirectToAction("DeletePersonalAccount");
        }
        //This method attempts to connect to the SQL database and returns a User object
        private User sqlConnectionForUser(int ID)
        {
            User foundUser = new Models.User();
            int totalNumberOfTimesToTry = 3;
            int retryIntervalSeconds = 1;

            for (int tries = 1; tries <= totalNumberOfTimesToTry; tries++)
            {
                try
                {
                    if (tries > 1)
                    {
                        T.Thread.Sleep(1000 * retryIntervalSeconds);
                        retryIntervalSeconds = Convert.ToInt32(retryIntervalSeconds * 1.5);
                    }
                    foundUser = accessDatabaseForUser(ID);
                    //Break if an account from the SQL database was found 
                    if (foundUser.AccountType != null)
                    {
                        break;
                    }
                }
                //Break if there is an exception
                catch (Exception Exc)
                {
                    break;
                }
            }
            return foundUser;
        }
        //This method connects to the database, reads the database and finding an entry with the same information
        //as the provided username and password and returns a User object with all information of the User
        private User accessDatabaseForUser(int ID)
        {
            User foundUser = new Models.User();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @"SELECT * FROM Users WHERE ID = @ID";
                    dbCommand.Parameters.AddWithValue("@ID", ID);
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    //Advancing to the next record which is the first and only record in this case
                    dataReader.Read();
                    //Storing information from found sql entry into a User object and returning it
                    foundUser.FirstName = dataReader.GetString(1);
                    foundUser.LastName = dataReader.GetString(2);
                    foundUser.AccountType = dataReader.GetString(3);
                    foundUser.Center = dataReader.GetString(4);
                    foundUser.Email = dataReader.GetString(5);
                    foundUser.PhoneNumber = dataReader.GetString(6);
                    foundUser.Username = dataReader.GetString(7);
                    foundUser.Password = dataReader.GetString(8);
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return foundUser;
            }
        }
        //This method attempts to connect to the SQL database and returns a Boolean value regarding update confirmation
        private Boolean sqlConnectionUpdateUser(int ID, User updatedUser)
        {
            Boolean success = false;
            int totalNumberOfTimesToTry = 3;
            int retryIntervalSeconds = 1;

            for (int tries = 1; tries <= totalNumberOfTimesToTry; tries++)
            {
                try
                {
                    if (tries > 1)
                    {
                        T.Thread.Sleep(1000 * retryIntervalSeconds);
                        retryIntervalSeconds = Convert.ToInt32(retryIntervalSeconds * 1.5);
                    }
                    success = updateUserDatabase(ID, updatedUser);
                    //Break if an account from the SQL database was found 
                    if (success == true)
                    {
                        break;
                    }
                }
                //Break if there is an exception
                catch (Exception Exc)
                {
                    break;
                }
            }
            return success;
        }
        //This method connects to the database, updates the specified SQL entry by the User's ID, collects the 
        //SQL entry for comparison and return a Boolean value based on the comparisons performed.
        private Boolean updateUserDatabase(int ID, User updatedUser)
        {
            Boolean success = false;
            User foundUser = new Models.User();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query that updates the SQL table entry and returns the updated table entry
                    dbCommand.CommandText = @"UPDATE Users 
                                              SET FirstName = @FirstName, LastName = @LastName, Center = @Center, Email = @Email,
                                                  PhoneNumber = @PhoneNumber, Username = @Username, Password = @Password
                                              WHERE ID = @ID
                                              SELECT * FROM Users WHERE ID = @ID";
                    //Updating User information based on comparison with current and new User information
                    //I trim the end of all fields to remove empty spaces
                    dbCommand.Parameters.AddWithValue("@FirstName", updatedUser.FirstName.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@LastName", updatedUser.LastName.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Center", updatedUser.Center.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Email", updatedUser.Email.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@PhoneNumber", updatedUser.PhoneNumber.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@UserName", updatedUser.Username.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Password", updatedUser.Password.TrimEnd(' '));
                    //Specifing update by ID number to ensure correct User's information is updated
                    dbCommand.Parameters.AddWithValue("@ID", ID);
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    dataReader.Read();
                    //Getting the updated SQL entry information for comparison testing to verify the update was successful
                    foundUser.FirstName = dataReader.GetString(1);
                    foundUser.LastName = dataReader.GetString(2);
                    foundUser.AccountType = dataReader.GetString(3);
                    foundUser.Center = dataReader.GetString(4);
                    foundUser.Email = dataReader.GetString(5);
                    foundUser.PhoneNumber = dataReader.GetString(6);
                    foundUser.Username = dataReader.GetString(7);
                    foundUser.Password = dataReader.GetString(8);
                    //Determining if the update was successfully executed by checking if an entry is returned and comparing
                    //all of the returned entry's information with the updated information provided by the user.
                    //I trim all of the found User data because the SQL server seems to add spaces.
                    if (dataReader.HasRows == true && updatedUser.FirstName.Equals(foundUser.FirstName.ToString().TrimEnd(' ')) &&
                        updatedUser.LastName.Equals(foundUser.LastName.ToString().TrimEnd(' ')) &&
                        updatedUser.Center.Equals(foundUser.Center.ToString().TrimEnd(' ')) &&
                        updatedUser.Email.Equals(foundUser.Email.ToString().TrimEnd(' ')) &&
                        updatedUser.PhoneNumber.Equals(foundUser.PhoneNumber.ToString().TrimEnd(' ')) &&
                        updatedUser.Username.Equals(foundUser.Username.ToString().TrimEnd(' ')) &&
                        updatedUser.Password.Equals(foundUser.Password.ToString().TrimEnd(' ')))
                    {
                        success = true;
                    }
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return success;
            }
        }
        //This method attempts to connect to the SQL database and returns a Boolean value regarding deletion confirmation
        private Boolean sqlConnectionDeleteUser(int ID)
        {
            Boolean success = false;
            int totalNumberOfTimesToTry = 3;
            int retryIntervalSeconds = 1;

            for (int tries = 1; tries <= totalNumberOfTimesToTry; tries++)
            {
                try
                {
                    if (tries > 1)
                    {
                        T.Thread.Sleep(1000 * retryIntervalSeconds);
                        retryIntervalSeconds = Convert.ToInt32(retryIntervalSeconds * 1.5);
                    }
                    success = accessDatabaseToDeleteUser(ID);
                    //Break if an account from the SQL database was found to be gone
                    if (success == true)
                    {
                        break;
                    }
                }
                //Break if there is an exception
                catch (Exception Exc)
                {
                    break;
                }
            }
            return success;
        }
        //This method connects to the database, delete the entry with the given ID, connects with the database again
        //to check if the entry is gone and returns the Boolean result of the check.
        private Boolean accessDatabaseToDeleteUser(int ID)
        {
            Boolean success = false;
            User foundUser = new Models.User();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query
                    dbCommand.CommandText = @" DELETE FROM Users WHERE ID = @ID
                                               SELECT * FROM Users WHERE ID = @ID";
                    dbCommand.Parameters.AddWithValue("@ID", ID);
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    dataReader.Read();
                    //If the User can't be found, then the User was successfully deleted 
                    if (dataReader.HasRows == false)
                    {
                        success = true;
                    }
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return success;
            }
        }
        ///This method returns an ADO.NET connection string. 
        private static string GetSqlConnectionString()
        {
            // Preparing the connection string to Azure SQL Database 
            var sqlConnectionSB = new S.SqlConnectionStringBuilder();
            //Connecting to the SQL database with hard-coded strings, but it is not a secure method
            sqlConnectionSB.DataSource = "tcp:mcca.database.windows.net,1433"; //["Server"]  
            sqlConnectionSB.InitialCatalog = "MCCA Database"; //["Database"]  

            sqlConnectionSB.UserID = "whitej";  // "@yourservername"  as suffix sometimes.  
            sqlConnectionSB.Password = "SacMesa416275";

            sqlConnectionSB.IntegratedSecurity = false;

            // Adjust these values if you like. (ADO.NET 4.5.1 or later.)  
            sqlConnectionSB.ConnectRetryCount = 3;
            sqlConnectionSB.ConnectRetryInterval = 1;  // Seconds.  

            // Leave these values as they are.  
            sqlConnectionSB.IntegratedSecurity = false;
            sqlConnectionSB.Encrypt = true;
            sqlConnectionSB.ConnectTimeout = 30;

            return sqlConnectionSB.ToString();
        }
    }
}
