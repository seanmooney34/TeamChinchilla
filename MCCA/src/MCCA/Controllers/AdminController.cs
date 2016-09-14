﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using MCCA.Models;
using S = System.Data.SqlClient;
using T = System.Threading;
using MCCA.ViewModels.Admin;
using Microsoft.Extensions.Configuration;
using System.Web;
namespace MCCA.Controllers
{
    //[Authorize(Roles = "Admin")]
    [Authorize]
    public class AdminController : Controller
    {
        //Used for personal account management
        private static int adminID;
        //This method is a simple hello to the user when he or she signs in as well as saving the ID for personal account
        //management
        public IActionResult Index(int ID, String firstName, String lastName)
        {
            adminID = ID;
            ViewData["ID"] = ID;
            ViewData["firstName"] = firstName;
            ViewData["lastName"] = lastName;
            return View();
        }
        //This method collects all User accounts and passes them into the View to be displayed in Manage Accounts
        public IActionResult ManageAccounts(String name, String sortOrder)
        {
            List<User> userList = new List<User>();
            List<User> searchList = new List<User>();
            //Storing the List object returned which contains all Users
            userList = sqlConnectionForUsersList();
            //If the search text bar passes a value, then a list is created and passed into the view containing
            //users whose names contain the given search input
            if (String.IsNullOrEmpty(name) == false)
            {
                foreach (User member in userList)
                {
                    String username = member.FirstName + " " + member.LastName;
                    if (username.Contains(name))
                    {
                        searchList.Add(member);
                    }
                }
                userList = searchList;
            }
            //If there is no value passed in the search text bar, sorting the list of Users is based on given sort 
            //button clicks with the list being ordered by account type by default 
            if (String.IsNullOrEmpty(name) == true)
            {
                switch (sortOrder)
                {
                    case "First Name":
                        {
                            userList.Sort(delegate (User x, User y)
                            {
                                return x.FirstName.CompareTo(y.FirstName);
                            });
                            break;
                        }
                    case "Last Name":
                        {
                            userList.Sort(delegate (User x, User y)
                            {
                                return x.LastName.CompareTo(y.LastName);
                            });
                            break;
                        }
                    default:
                        {
                            userList.Sort(delegate (User x, User y)
                            {
                                return x.AccountType.CompareTo(y.AccountType);
                            });
                            break;
                        }
                }
            }
            return View(userList);
        }
        //This method returns the AddAccount View
        [HttpGet]
        public IActionResult AddAccount()
        {
            return View();
        }
        //This method adds an account with provided information to the SQL database and redirects user to ManageAccounts
        //if successful
        [HttpPost]
        public IActionResult AddAccount(AddAccountViewModel model)
        {
            Boolean success = false;
            User newUser = new Models.User();
            //ID initialized for comparison
            int ID = 1;
            List<User> userList = new List<User>();
            //Storing the SortedList object returned which contains all Users
            userList = sqlConnectionForUsersList();
            //ID is compared with the ID value of all Users and is incremented by 1 in each loop. If ID doesn't match
            //a User ID then break the loop and use the new ID value for the new User account ID.
            //This means if a User is deleted, then a new User will get the old ID
            foreach (var item in userList)
            {
                if (ID != item.ID)
                {
                    break;
                }
                ID += 1;
            }
            newUser.FirstName = model.FirstName;
            newUser.LastName = model.LastName;
            newUser.AccountType = model.AccountType;
            newUser.Center = model.Center;
            newUser.Email = model.Email;
            newUser.PhoneNumber = model.PhoneNumber;
            newUser.Username = model.Username;
            newUser.Password = model.Password;
            success = sqlConnectionAddUser(ID, newUser);
            if (success == true)
            {
                return RedirectToAction("ManageAccounts");
            }
            return View();
        }
        //This method returns the Edit View with the EditViewModel passed in to display account information
        [HttpGet]
        public IActionResult Edit(int ID)
        {
            User foundUser = new Models.User();
            EditViewModel model = new EditViewModel();
            //Getting User information based on User ID
            foundUser = sqlConnectionForUser(ID);
            //Storing the information in ViewData to be used to fill in the Edit form
            model.ID = foundUser.ID;
            model.FirstName = foundUser.FirstName;
            model.LastName = foundUser.LastName;
            model.AccountType = foundUser.AccountType;
            model.Center = foundUser.Center;
            model.Email = foundUser.Email;
            model.PhoneNumber = foundUser.PhoneNumber;
            model.Username = foundUser.Username;
            return View(model);
        }
        //This method allows the Admin to edit accounts displayed in Manage Accounts
        public IActionResult Edit(EditViewModel model)
        {
            Boolean success = false;
            User updatedUser = new Models.User();
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
            success = sqlConnectionUpdateUser(model.ID, updatedUser);
            //If the update was successful, redirect the User to the Manage Accounts page
            if (success == true)
            {
                return RedirectToAction("ManageAccounts");
            }
            return View(model);
        }
        //This method sends an entry's information from Manage Accounts into the View when the Delete link is clicked on
        public IActionResult Delete(int ID)
        {
            User foundUser = new Models.User();
            foundUser = sqlConnectionForUser(ID);
            return View(foundUser);
        }
        //This method deletes the user from the system if the delete button is clicked on and sends the User
        //to Manage Accounts
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(User model)
        {
            Boolean success = false;
            //The model's first name is checked to verify if the model object is null to prevent unnecessary SQL database access
            if (String.IsNullOrEmpty(model.FirstName))
            {
                success = sqlConnectionDeleteUser(model.ID);
            }
            if (success == true)
            {
                return RedirectToAction("ManageAccounts");
            }
            return RedirectToAction("Edit", new { ID = model.ID });
        }
        public IActionResult ManageCenters()
        {
            return View();
        }
        [HttpGet]
        
        //This method returns the AddCenter View
        public IActionResult AddCenter()
        {
            return View();
        }
        
        //[HttpPost, ActionName("AddCenter")]
        [HttpPost]
        public IActionResult AddCenter(AddCenterViewModel model)
        {
            /*if(String.IsNullOrEmpty(model.Name) == false)
            {
                return RedirectToAction("ManageAccounts");
            }*/
            /*if (model.Picture.File.ContentLength > 0)
            {
                return RedirectToAction("ManageAccounts");
            }*/
            return RedirectToAction("ManageCenters");
        }

        [HttpGet]
        public IActionResult PictureTest()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PictureTest(HttpPostedFileBase uploadFile)
        {
            if(uploadFile.ContentLength > 0)
            {
                return RedirectToAction("ManageAccounts");
            }
            return RedirectToAction("ManagePersonalAccount");
        }

        public IActionResult ManageSite()
        {
            return View();
        }
        //This method returns the ManagePersonalAccount View with the ManagePersonalAccountViewModel passed in to 
        //display account information
        [HttpGet]
        public IActionResult ManagePersonalAccount()
        {
            User foundUser = new Models.User();
            ManagePersonalAccountViewModel model = new ManagePersonalAccountViewModel();
            //Getting SQL table entry based on User ID
            foundUser = sqlConnectionForUser(adminID);
            model.FirstName = foundUser.FirstName;
            model.LastName = foundUser.LastName;
            model.AccountType = foundUser.AccountType;
            model.Center = foundUser.Center;
            model.Email = foundUser.Email;
            model.PhoneNumber = foundUser.PhoneNumber;
            model.Username = foundUser.Username;
            return View(model);
        }
        //This method allows the User to edit personal account information, save the changes to the SQL database and
        //refreshes the page for the User showing the update informatin if successful
        [HttpPost]
        public IActionResult ManagePersonalAccount(ManagePersonalAccountViewModel model)
        {
            Boolean success = false;
            User updatedUser = new Models.User();
            //Getting ViewModel model information given in the textfields of the Manage Personal Account page that
            //an Admin is allowed to change
            updatedUser.FirstName = model.FirstName.TrimEnd(' ');
            updatedUser.LastName = model.LastName.TrimEnd(' ');
            updatedUser.Center = model.Center.TrimEnd(' ');
            updatedUser.Email = model.Email.TrimEnd(' ');
            updatedUser.PhoneNumber = model.PhoneNumber.TrimEnd(' ');
            updatedUser.Username = model.Username.TrimEnd(' ');
            updatedUser.Password = model.Password.TrimEnd(' ');
            //Getting Boolean result of SQL entry information update
            success = sqlConnectionUpdateUser(adminID, updatedUser);
            //If the update was successful, redirect the User to the Manage Personal Account View to refresh the page
            //with the updated information.
            if (success == true)
            {
                return RedirectToAction("ManagePersonalAccount");
            }
            return View(model);
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
                    foundUser.ID = dataReader.GetInt32(0);
                    foundUser.FirstName = dataReader.GetString(1).TrimEnd(' ');
                    foundUser.LastName = dataReader.GetString(2).TrimEnd(' ');
                    foundUser.AccountType = dataReader.GetString(3).TrimEnd(' ');
                    foundUser.Center = dataReader.GetString(4).TrimEnd(' ');
                    foundUser.Email = dataReader.GetString(5).TrimEnd(' ');
                    foundUser.PhoneNumber = dataReader.GetString(6).TrimEnd(' ');
                    foundUser.Username = dataReader.GetString(7).TrimEnd(' ');
                    foundUser.Password = dataReader.GetString(8).TrimEnd(' ');
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
                    //I trim all of the found User data because the SQL server seems to add spaces.
                    foundUser.FirstName = dataReader.GetString(1).TrimEnd(' ');
                    foundUser.LastName = dataReader.GetString(2).TrimEnd(' ');
                    foundUser.Center = dataReader.GetString(4).TrimEnd(' ');
                    foundUser.Email = dataReader.GetString(5).TrimEnd(' ');
                    foundUser.PhoneNumber = dataReader.GetString(6).TrimEnd(' ');
                    foundUser.Username = dataReader.GetString(7).TrimEnd(' ');
                    foundUser.Password = dataReader.GetString(8).TrimEnd(' ');
                    //Determining if the update was successfully executed by checking if an entry is returned and comparing
                    //all of the returned entry's information with the updated information provided by the user.
                    if (dataReader.HasRows == true && updatedUser.FirstName.Equals(foundUser.FirstName) &&
                        updatedUser.LastName.Equals(foundUser.LastName) &&
                        updatedUser.Center.Equals(foundUser.Center) &&
                        updatedUser.Email.Equals(foundUser.Email) &&
                        updatedUser.PhoneNumber.Equals(foundUser.PhoneNumber) &&
                        updatedUser.Username.Equals(foundUser.Username) &&
                        updatedUser.Password.Equals(foundUser.Password))
                    {
                        success = true;
                    }
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return success;
            }
        }
        //This method attempts to connect to the SQL database and returns a List object containing all Users
        private List<User> sqlConnectionForUsersList()
        {
            //SortedList<String, User> userList = new SortedList<String, User>();
            List<User> userList = new List<User>();
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
                    userList = accessDatabaseForUsers();
                    //Break if the List object is not empty
                    if (userList.Count > 0)
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
            return userList;
        }
        //This method connects to the database, collects all the entries in the Users table into a list
        //based on Users' account type and returns the list.
        private List<User> accessDatabaseForUsers()
        {
            //SortedList<String, User> userList = new SortedList<String, User>();
            List<User> userList = new List<User>();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    //Opening SQL connection
                    sqlConnection.Open();
                    //Creating SQL query that updates the SQL table entry and returns the updated table entry
                    dbCommand.CommandText = @"SELECT * FROM Users";
                    var dataReader = dbCommand.ExecuteReader();
                    var iterator = dataReader.GetEnumerator();
                    while (iterator.MoveNext())
                    {
                        User foundUser = new Models.User();
                        //Getting the SQL entry information 
                        //I trim all of the found User data because the SQL server seems to add spaces.
                        foundUser.ID = dataReader.GetInt32(0);
                        foundUser.FirstName = dataReader.GetString(1).TrimEnd(' ');
                        foundUser.LastName = dataReader.GetString(2).TrimEnd(' ');
                        foundUser.AccountType = dataReader.GetString(3).TrimEnd(' ');
                        foundUser.Center = dataReader.GetString(4).TrimEnd(' ');
                        foundUser.Email = dataReader.GetString(5).TrimEnd(' ');
                        foundUser.PhoneNumber = dataReader.GetString(6).TrimEnd(' ');
                        foundUser.Username = dataReader.GetString(7).TrimEnd(' ');
                        foundUser.Password = dataReader.GetString(8).TrimEnd(' ');
                        //Adding each User object to the sorted list using Account Type as the key
                        userList.Add(foundUser);
                    }
                    //Closing SQL connection
                    sqlConnection.Close();
                }
                return userList;
            }
        }
        //This method attempts to connect to the SQL database and a Boolean value regarding account creation confirmation
        private Boolean sqlConnectionAddUser(int newID, User newUser)
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
                    success = accessDatabaseToAddUser(newID, newUser);
                    //Break if an account added to the SQL database 
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
        //This method connects to the database, adds to the Users table, collects Users from the table for comparison,
        //and returns Boolean value regarding success
        private Boolean accessDatabaseToAddUser(int newID, User newUser)
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
                    dbCommand.CommandText = @"INSERT INTO Users (ID, FirstName, LastName, AccountType, Center, Email, PhoneNumber, Username, Password)
                                              Values (@ID, @FirstName, @LastName, @AccountType, @Center, @Email, @PhoneNumber, @Username, @Password)
                                              Select * FROM Users WHERE ID = @ID";
                    dbCommand.Parameters.AddWithValue("@ID", newID);
                    //I trim the ends of empty spaces
                    dbCommand.Parameters.AddWithValue("@FirstName", newUser.FirstName.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@LastName", newUser.LastName.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@AccountType", newUser.AccountType.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Center", newUser.Center.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Email", newUser.Email.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@PhoneNumber", newUser.PhoneNumber.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Username", newUser.Username.TrimEnd(' '));
                    dbCommand.Parameters.AddWithValue("@Password", newUser.Password.TrimEnd(' '));
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    //Advancing to the next record which is the first and only record in this case
                    dataReader.Read();
                    //Storing information from found sql entry into a User object 
                    //I trim all of the found User data because the SQL server seems to add spaces.
                    foundUser.ID = dataReader.GetInt32(0);
                    foundUser.FirstName = dataReader.GetString(1).TrimEnd(' ');
                    foundUser.LastName = dataReader.GetString(2).TrimEnd(' ');
                    foundUser.AccountType = dataReader.GetString(3).TrimEnd(' ');
                    foundUser.Center = dataReader.GetString(4).TrimEnd(' ');
                    foundUser.Email = dataReader.GetString(5).TrimEnd(' ');
                    foundUser.PhoneNumber = dataReader.GetString(6).TrimEnd(' ');
                    foundUser.Username = dataReader.GetString(7).TrimEnd(' ');
                    foundUser.Password = dataReader.GetString(8).TrimEnd(' ');
                    //Determining if the table entry was successfully executed by checking if an entry is returned and comparing
                    //all of the returned entry's information with the new User's information.
                    if (dataReader.HasRows == true && newUser.FirstName.Equals(foundUser.FirstName) &&
                        newUser.LastName.Equals(foundUser.LastName) &&
                        newUser.Center.Equals(foundUser.Center) &&
                        newUser.Email.Equals(foundUser.Email) &&
                        newUser.PhoneNumber.Equals(foundUser.PhoneNumber) &&
                        newUser.Username.Equals(foundUser.Username) &&
                        newUser.Password.Equals(foundUser.Password))
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
            //Connecting to the SQL database with connection strings accessed by a Configuration Builder by grabbing
            //the connection strings through the appsettings.json file
            var builder = new ConfigurationBuilder();
            builder.AddJsonFile("appsettings.json");
            var connectionStringConfig = builder.Build();
            connectionStringConfig.GetSection("ConnectionStrings");

            String dataSource = connectionStringConfig["ConnectionStrings:dataSource"];
            String databaseName = connectionStringConfig["ConnectionStrings:databaseName"];
            String admin = connectionStringConfig["ConnectionStrings:Admin"];
            String password = connectionStringConfig["ConnectionStrings:Password"];
            sqlConnectionSB.DataSource = dataSource; //["Server"]  
            sqlConnectionSB.InitialCatalog = databaseName; //["Database"]  

            sqlConnectionSB.UserID = admin;
            sqlConnectionSB.Password = password;

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