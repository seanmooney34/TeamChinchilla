using System;  // C#  
using G = System.Collections.Generic;
using S = System.Data.SqlClient;
using T = System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Logging;
using MCCA.Models;
using MCCA.Services;
using MCCA.ViewModels.Account;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MCCA.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private User user = new User();

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model, string returnUrl = null)
        {
            User foundUser = new Models.User();
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                foundUser = sqlConnection(model.Username, model.Password);
                //There is a chance that an empty User object may be given due connection issues or incorrect login information
                //is given, so one of the assigned User properties (AccountType) is checked. If the checked User property
                //is null, then the ViewModel is simply sent to the View. 
                if (foundUser.AccountType != null)
                {
                    if (foundUser.AccountType.Contains("Admin"))
                    {
                        return RedirectToAction(nameof(AdminController.Index), "Admin");
                    }
                    else if (foundUser.AccountType.Contains("Director"))
                    {
                        return RedirectToAction(nameof(DirectorController.Index), "Director");
                    }
                    else if (foundUser.AccountType.Contains("Staff"))
                    {
                        return RedirectToAction(nameof(StaffController.Index), "Staff");
                    }
                }
            }
            return View(model);
        }
        //This methold attempts to connect to the SQL database and returns a boolean if it succeeded
        private User sqlConnection(String _username, String _password)
        {
            String username = _username;
            String password = _password;
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
                    foundUser = accessDatabase(username, password);
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
        //as the provided username and password and returns a User object with some information 
        private User accessDatabase(string _username, string _password)
        {
            String username = _username;
            String password = _password;
            User foundUser = new Models.User();
            using (var sqlConnection = new S.SqlConnection(GetSqlConnectionString()))
            {
                using (var dbCommand = sqlConnection.CreateCommand())
                {
                    sqlConnection.Open();
                    dbCommand.CommandText = @"SELECT * FROM Users WHERE Username = @username AND Password = @password";
                    dbCommand.Parameters.AddWithValue("@username", username);
                    dbCommand.Parameters.AddWithValue("@password", password);
                    //Building data reader
                    var dataReader = dbCommand.ExecuteReader();
                    bool count = dataReader.HasRows;

                    //Advancing to the next record which is the first and only record in this case
                    dataReader.Read();
                    //Storing information from found sql entry into a User object and returning it
                    foundUser.ID = dataReader.GetInt32(0);
                    foundUser.FirstName = dataReader.GetString(1);
                    foundUser.LastName = dataReader.GetString(2);
                    foundUser.AccountType = dataReader.GetString(3);
                }
                return foundUser;
            }
        }
         
        ///This method returns an ADO.NET connection string. 
        private static string GetSqlConnectionString()
        {
            // Prepare the connection string to Azure SQL Database.  
            var sqlConnectionSB = new S.SqlConnectionStringBuilder();
            //Connecting to the SQL database with hard-coded strings, but it is not a secure method
            sqlConnectionSB.DataSource = "tcp:mcca.database.windows.net,1433"; //["Server"]  
            sqlConnectionSB.InitialCatalog = "MCCA Database"; //["Database"]  

            sqlConnectionSB.UserID = "whitej";  // "@yourservername"  as suffix sometimes.  
            sqlConnectionSB.Password = "SacMesa416275";
            //Connecting to the SQL database with connection strings accessed by a Configuration Manager through the
            //web.config file, but the latest version of Microsoft Visual no longer allows this so I can't test this.
            /*ConnectionStringSettings dataSource = ConfigurationManager.ConnectionStrings["Data Source"];
            ConnectionStringSettings databaseName = ConfigurationManager.ConnectionStrings["Database Name"];
            ConnectionStringSettings admin = ConfigurationManager.ConnectionStrings["Admin"];
            ConnectionStringSettings password = ConfigurationManager.ConnectionStrings["Password"];
            sqlConnectionSB.DataSource = dataSource.ToString(); //["Server"]  
            sqlConnectionSB.InitialCatalog = databaseName.ToString(); //["Database"] 

            sqlConnectionSB.UserID = admin.ToString();  // "@yourservername"  as suffix sometimes.  
            sqlConnectionSB.Password = password.ToString();*/
            //Connecting to the SQL database with connection strings accessed by a Configuration Builder by grabbing
            //the connection strings through the project.json file, but it does not seem to work
            /*var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("project.json");
            var config = builder.Build();
            String dataSource = config["dataSource"];
            String databaseName = config["databaseName"];
            String admin = config["Admin"];
            String password = config["Password"];
            sqlConnectionSB.DataSource = dataSource; //["Server"]  
            sqlConnectionSB.InitialCatalog = databaseName; //["Database"]  

            sqlConnectionSB.UserID = admin;  // "@yourservername"  as suffix sometimes.  
            sqlConnectionSB.Password = password;*/
            sqlConnectionSB.IntegratedSecurity = false;

            // Adjust these values if you like. (ADO.NET 4.5.1 or later.)  
            sqlConnectionSB.ConnectRetryCount = 3;
            sqlConnectionSB.ConnectRetryInterval = 10;  // Seconds.  

            // Leave these values as they are.  
            sqlConnectionSB.IntegratedSecurity = false;
            sqlConnectionSB.Encrypt = true;
            sqlConnectionSB.ConnectTimeout = 30;

            return sqlConnectionSB.ToString();
        }
        /* public AccountController(
              UserManager<ApplicationUser> userManager,
              SignInManager<ApplicationUser> signInManager,
              IEmailSender emailSender,
              ISmsSender smsSender,
              ILoggerFactory loggerFactory)
          {
              _userManager = userManager;
              _signInManager = signInManager;
              _emailSender = emailSender;
              _smsSender = smsSender;
              _logger = loggerFactory.CreateLogger<AccountController>();
          }

          //
          // GET: /Account/Login
          [HttpGet]
          [AllowAnonymous]
          public IActionResult Login(string returnUrl = null)
          {
              ViewData["ReturnUrl"] = returnUrl;
              return View();
          }

          //
          // POST: /Account/Login
          [HttpPost]
          [AllowAnonymous]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
          {
              ViewData["ReturnUrl"] = returnUrl;
              if (ModelState.IsValid)
              {
                  // This doesn't count login failures towards account lockout
                  // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                  var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                  if (result.Succeeded)
                  {
                      _logger.LogInformation(1, "User logged in.");
                      return RedirectToLocal(returnUrl);
                  }
                  if (result.RequiresTwoFactor)
                  {
                      return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                  }
                  if (result.IsLockedOut)
                  {
                      _logger.LogWarning(2, "User account locked out.");
                      return View("Lockout");
                  }
                  else
                  {
                      ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                      return View(model);
                  }
              }

              // If we got this far, something failed, redisplay form
              return View(model);
          }

          //
          // GET: /Account/Register
          [HttpGet]
          [AllowAnonymous]
          public IActionResult Register()
          {
              return View();
          }

          //
          // POST: /Account/Register
          [HttpPost]
          [AllowAnonymous]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> Register(RegisterViewModel model)
          {
              if (ModelState.IsValid)
              {
                  var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                  var result = await _userManager.CreateAsync(user, model.Password);
                  if (result.Succeeded)
                  {
                      // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                      // Send an email with this link
                      //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                      //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                      //await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
                      //    "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
                      await _signInManager.SignInAsync(user, isPersistent: false);
                      _logger.LogInformation(3, "User created a new account with password.");
                      return RedirectToAction(nameof(HomeController.Index), "Home");
                  }
                  AddErrors(result);
              }

              // If we got this far, something failed, redisplay form
              return View(model);
          }

          //
          // POST: /Account/LogOff
          [HttpPost]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> LogOff()
          {
              await _signInManager.SignOutAsync();
              _logger.LogInformation(4, "User logged out.");
              return RedirectToAction(nameof(HomeController.Index), "Home");
          }

          //
          // POST: /Account/ExternalLogin
          [HttpPost]
          [AllowAnonymous]
          [ValidateAntiForgeryToken]
          public IActionResult ExternalLogin(string provider, string returnUrl = null)
          {
              // Request a redirect to the external login provider.
              var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
              var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
              return new ChallengeResult(provider, properties);
          }

          //
          // GET: /Account/ExternalLoginCallback
          [HttpGet]
          [AllowAnonymous]
          public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
          {
              var info = await _signInManager.GetExternalLoginInfoAsync();
              if (info == null)
              {
                  return RedirectToAction(nameof(Login));
              }

              // Sign in the user with this external login provider if the user already has a login.
              var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
              if (result.Succeeded)
              {
                  _logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);
                  return RedirectToLocal(returnUrl);
              }
              if (result.RequiresTwoFactor)
              {
                  return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
              }
              if (result.IsLockedOut)
              {
                  return View("Lockout");
              }
              else
              {
                  // If the user does not have an account, then ask the user to create an account.
                  ViewData["ReturnUrl"] = returnUrl;
                  ViewData["LoginProvider"] = info.LoginProvider;
                  var email = info.ExternalPrincipal.FindFirstValue(ClaimTypes.Email);
                  return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
              }
          }

          //
          // POST: /Account/ExternalLoginConfirmation
          [HttpPost]
          [AllowAnonymous]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
          {
              if (User.IsSignedIn())
              {
                  return RedirectToAction(nameof(ManageController.Index), "Manage");
              }

              if (ModelState.IsValid)
              {
                  // Get the information about the user from the external login provider
                  var info = await _signInManager.GetExternalLoginInfoAsync();
                  if (info == null)
                  {
                      return View("ExternalLoginFailure");
                  }
                  var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                  var result = await _userManager.CreateAsync(user);
                  if (result.Succeeded)
                  {
                      result = await _userManager.AddLoginAsync(user, info);
                      if (result.Succeeded)
                      {
                          await _signInManager.SignInAsync(user, isPersistent: false);
                          _logger.LogInformation(6, "User created an account using {Name} provider.", info.LoginProvider);
                          return RedirectToLocal(returnUrl);
                      }
                  }
                  AddErrors(result);
              }

              ViewData["ReturnUrl"] = returnUrl;
              return View(model);
          }

          // GET: /Account/ConfirmEmail
          [HttpGet]
          [AllowAnonymous]
          public async Task<IActionResult> ConfirmEmail(string userId, string code)
          {
              if (userId == null || code == null)
              {
                  return View("Error");
              }
              var user = await _userManager.FindByIdAsync(userId);
              if (user == null)
              {
                  return View("Error");
              }
              var result = await _userManager.ConfirmEmailAsync(user, code);
              return View(result.Succeeded ? "ConfirmEmail" : "Error");
          }

          //
          // GET: /Account/ForgotPassword
          [HttpGet]
          [AllowAnonymous]
          public IActionResult ForgotPassword()
          {
              return View();
          }

          //
          // POST: /Account/ForgotPassword
          [HttpPost]
          [AllowAnonymous]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
          {
              if (ModelState.IsValid)
              {
                  var user = await _userManager.FindByNameAsync(model.Email);
                  if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                  {
                      // Don't reveal that the user does not exist or is not confirmed
                      return View("ForgotPasswordConfirmation");
                  }

                  // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
                  // Send an email with this link
                  //var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                  //var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                  //await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                  //   "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
                  //return View("ForgotPasswordConfirmation");
              }

              // If we got this far, something failed, redisplay form
              return View(model);
          }

          //
          // GET: /Account/ForgotPasswordConfirmation
          [HttpGet]
          [AllowAnonymous]
          public IActionResult ForgotPasswordConfirmation()
          {
              return View();
          }

          //
          // GET: /Account/ResetPassword
          [HttpGet]
          [AllowAnonymous]
          public IActionResult ResetPassword(string code = null)
          {
              return code == null ? View("Error") : View();
          }

          //
          // POST: /Account/ResetPassword
          [HttpPost]
          [AllowAnonymous]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
          {
              if (!ModelState.IsValid)
              {
                  return View(model);
              }
              var user = await _userManager.FindByNameAsync(model.Email);
              if (user == null)
              {
                  // Don't reveal that the user does not exist
                  return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
              }
              var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
              if (result.Succeeded)
              {
                  return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
              }
              AddErrors(result);
              return View();
          }

          //
          // GET: /Account/ResetPasswordConfirmation
          [HttpGet]
          [AllowAnonymous]
          public IActionResult ResetPasswordConfirmation()
          {
              return View();
          }

          //
          // GET: /Account/SendCode
          [HttpGet]
          [AllowAnonymous]
          public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
          {
              var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
              if (user == null)
              {
                  return View("Error");
              }
              var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
              var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
              return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
          }

          //
          // POST: /Account/SendCode
          [HttpPost]
          [AllowAnonymous]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> SendCode(SendCodeViewModel model)
          {
              if (!ModelState.IsValid)
              {
                  return View();
              }

              var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
              if (user == null)
              {
                  return View("Error");
              }

              // Generate the token and send it
              var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
              if (string.IsNullOrWhiteSpace(code))
              {
                  return View("Error");
              }

              var message = "Your security code is: " + code;
              if (model.SelectedProvider == "Email")
              {
                  await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
              }
              else if (model.SelectedProvider == "Phone")
              {
                  await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
              }

              return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
          }

          //
          // GET: /Account/VerifyCode
          [HttpGet]
          [AllowAnonymous]
          public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
          {
              // Require that the user has already logged in via username/password or external login
              var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
              if (user == null)
              {
                  return View("Error");
              }
              return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
          }

          //
          // POST: /Account/VerifyCode
          [HttpPost]
          [AllowAnonymous]
          [ValidateAntiForgeryToken]
          public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
          {
              if (!ModelState.IsValid)
              {
                  return View(model);
              }

              // The following code protects for brute force attacks against the two factor codes.
              // If a user enters incorrect codes for a specified amount of time then the user account
              // will be locked out for a specified amount of time.
              var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
              if (result.Succeeded)
              {
                  return RedirectToLocal(model.ReturnUrl);
              }
              if (result.IsLockedOut)
              {
                  _logger.LogWarning(7, "User account locked out.");
                  return View("Lockout");
              }
              else
              {
                  ModelState.AddModelError("", "Invalid code.");
                  return View(model);
              }
          }

          #region Helpers

          private void AddErrors(IdentityResult result)
          {
              foreach (var error in result.Errors)
              {
                  ModelState.AddModelError(string.Empty, error.Description);
              }
          }

          private async Task<ApplicationUser> GetCurrentUserAsync()
          {
              return await _userManager.FindByIdAsync(HttpContext.User.GetUserId());
          }

          private IActionResult RedirectToLocal(string returnUrl)
          {
              if (Url.IsLocalUrl(returnUrl))
              {
                  return Redirect(returnUrl);
              }
              else
              {
                  return RedirectToAction(nameof(HomeController.Index), "Home");
              }
          }

          #endregion*/
    }
}
