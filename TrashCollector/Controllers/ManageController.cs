using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using TrashCollector.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Web.Security;
using System.Globalization;

namespace TrashCollector.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Phone number was removed."
                : message == ManageMessageId.UpdateProfileSuccess ? "Profile name and phone number has been updated."
                : message == ManageMessageId.UpdateDatesSuccess ? "Pickup date preferences have been updated."
                : message == ManageMessageId.UpdateAddressSuccess ? "Address has been updated."
                : message == ManageMessageId.AddedChargeSuccess ? "Charge successfully added to customer."
                : message == ManageMessageId.AddEmployeeSuccess ? "Successfully added employee."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };

            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ViewBag.Name = user.Name;

                ViewBag.displayAdmin = "No";
                ViewBag.displayCollector = "No";
                ViewBag.displayCustomer = "No";

                if (IsAdminUser())
                {
                    ViewBag.displayAdmin = "Yes";
                }
                else if (IsCollectorUser())
                {
                    ViewBag.displayCollector = "Yes";
                }
                else if (IsCustomerUser())
                {
                    ViewBag.displayCustomer = "Yes";
                }
                return View(model);
            }
            else
            {
                ViewBag.Name = "Not Logged In";
            }

            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }
        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            UpdateProfileSuccess,
            UpdateDatesSuccess,
            UpdateAddressSuccess,
            AddedChargeSuccess,
            AddEmployeeSuccess,
            Error
        }
        ///////////////////////////////////////////


        #endregion
        /// ////////////////////////////////////////////////////////////////////

        //
        // GET: /Manage/UpdateProfile
        public ActionResult UpdateProfile()
        {
            ApplicationUser u = UserManager.FindById(User.Identity.GetUserId());
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ViewBag.Name = user.Name;

                ViewBag.displayAdmin = "No";

                if (IsAdminUser())
                {
                    ViewBag.displayAdmin = "Yes";
                }
                return View(u);
            }
            else
            {
                ViewBag.Name = "Not Logged In";
            }
            return View(u);
        }
        public ActionResult UpdateUserProfile(string id)
        {
            ApplicationUser u = UserManager.FindById(id);
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ViewBag.Name = user.Name;

                ViewBag.displayAdmin = "No";

                if (IsAdminUser())
                {
                    ViewBag.displayAdmin = "Yes";
                }
                return View("UpdateProfile", u);
            }
            else
            {
                ViewBag.Name = "Not Logged In";
            }
            return View("UpdateProfile", u);
        }
        //
        // POST: /Manage/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProfile(UpdateProfileViewModel model)
        {
            ApplicationUser m = UserManager.FindById(User.Identity.GetUserId());
            if (!ModelState.IsValid)
            {
                return View("UpdateProfile", m);
            }
            //Get current user
            //Update name and phone
            m.FirstName = model.FirstName;
            m.LastName = model.LastName;
            m.PhoneNumber = model.PhoneNumber;

            IdentityResult result = await UserManager.UpdateAsync(m);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.UpdateProfileSuccess });
            }
            //AddErrors(result);
            return View("UpdateProfile", m.Id);
        }
        //
        // GET: /Manage/AddEmployee
        public ActionResult AddEmployee()
        {
            return View();
        }
        //
        // GET: /Manage/CollectionList
        public ActionResult CollectionList()
        {
            ApplicationDbContext db = new ApplicationDbContext();

            var date = DateTime.Now;
            var users = UserManager.Users.Where(w => w.CustomerDates != null).ToList();
            var collector = UserManager.FindById(User.Identity.GetUserId());
            var collectorZip = db.Collectors.Where(w => w.UserId == collector.Id).Select(s => s.ZipCode).First();

            var pickupList = users.Where(w => CheckDates(date, w) == true).Select(s => s)
                .Where(w => w.CustomerAddress.Zip == collectorZip).Select(s => s);
            List<CollectorPickupsViewModel> modelList = new List<CollectorPickupsViewModel>();

            foreach (var u in pickupList)
            {
                var modelObject = new CollectorPickupsViewModel(u);
                modelList.Add(modelObject);
            }
            return View(modelList);
        }

        public bool CheckDates(DateTime date, ApplicationUser user)
        {
            var alternateDay = user.CustomerDates.AlternatePickup;
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar calendar = dfi.Calendar;

            if (date >= user.CustomerDates.VacationStart && date < user.CustomerDates.VacationEnd)
            {
                return false;
            }
            else if(alternateDay != null && calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday) == calendar.GetWeekOfYear(alternateDay.Value, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
            {
                if (date.Day == alternateDay.Value.Day) return true;
                else return false;
            }
            else if(date.DayOfWeek == user.CustomerDates.DefaultDay)
            {
                return true;
            }
            return false;
        }

        private bool IsAdminUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ApplicationDbContext db = new ApplicationDbContext();
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                var s = UserManager.GetRoles(user.GetUserId());
                if (s[0].ToString() == "Admin")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        private bool IsCollectorUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ApplicationDbContext db = new ApplicationDbContext();
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                var s = UserManager.GetRoles(user.GetUserId());
                if (s[0].ToString() == "Collector")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        private bool IsCustomerUser()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ApplicationDbContext db = new ApplicationDbContext();
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                var s = UserManager.GetRoles(user.GetUserId());
                if (s[0].ToString() == "Customer")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        //
        // POST: /Manage/AddEmployee
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddEmployee(AddEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, PhoneNumber = model.PhoneNumber };
                var result = await UserManager.CreateAsync(user, model.Password);

                var db = new ApplicationDbContext();
                var roleStore = new RoleStore<IdentityRole>(db);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                var userStore = new UserStore<ApplicationUser>(db);
                var userManager = new UserManager<ApplicationUser>(userStore);
                userManager.AddToRole(user.Id, "Collector");

                var newCollector = new Collectors();
                newCollector.UserId = user.Id;
                newCollector.ZipCode = model.ZipCode;
                var result2 = db.Collectors.Add(newCollector);
                db.SaveChanges();

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Manage", new { Message = ManageMessageId.AddEmployeeSuccess });
                }

                AddErrors(result);

            }
                // If we got this far, something failed, redisplay form
                return View(model);
        }
        //
        // GET: /Manage/ListUsers
        public ActionResult ListUsers()
        {
            var userList = UserManager.Users.ToList();
            List<ListUserViewModel> modelList = new List<ListUserViewModel>();

            foreach(var u in userList)
            {
                var modelObject = new ListUserViewModel(u);
                modelList.Add(modelObject);
            }
            return View(modelList.OrderBy(o => o.UserRoles));
        }

        //
        //GET: /Manage/Delete/32432
        public ActionResult DeleteUser(string id)
        {
            var db = new ApplicationDbContext();
            var user = db.Users.First(s => s.Id == id);
            var model = new DeleteUserViewModel(user);
            
            return View(model);
        }
        //
        //Post: /Manage/Delete/32432
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            var db = new ApplicationDbContext();
            try
            {
            var user = db.Users.First(u => u.Id == id);
            db.Users.Remove(user);
            db.SaveChanges();
            }
            catch
            {
                return RedirectToAction("ListUsers");
            }
            return RedirectToAction("ListUsers");
        }
        //
        //GET: /Manage/ViewDetails/32432
        public ActionResult ViewDetails(string id)
        {
            var db = new ApplicationDbContext();
            try
            {
                var user = db.Users.First(u => u.Id == id);
                var model = new ListUserViewModel(user);
                return View(model);
            }
            catch
            {
                return RedirectToAction("ListUsers");
            }
        }
        //
        //GET: /Manage/GoToCharges/32146543
        public ActionResult GoToCharges(string id)
        {
            return RedirectToAction( "Create", "CustomerCharges", new { id = id });
        }
        //
        // GET: /Manage/BillList
        public ActionResult BillList()
        {
            ApplicationDbContext db = new ApplicationDbContext();

            var date = DateTime.Now;
            var userStore = new UserStore<ApplicationUser>(db);
            var userManager = new UserManager<ApplicationUser>(userStore);
            var user = userManager.FindById(User.Identity.GetUserId());

            var billsList = db.CustomerCharges.Where(w => w.ApplicationUserId == user.Id).Select(s => s).ToList();
            var monthList = from c in billsList
                            group c by new
                            {
                                c.Date.Value.Month,
                                c.Date.Value.Year
                            } into grp
                            select new MonthlyBill()
                            {
                                monthGroup = grp.Key.Month,
                                month = new DateTime(grp.Key.Year, grp.Key.Month, 1).ToString("MMMM"),
                                sum = grp.Sum(s => s.Charge)
                            };
            List<BillViewModel> modelList = new List<BillViewModel>();
            foreach(var m in monthList)
            {
                var modelItem = new BillViewModel() { Month = m.month, Sum = m.sum, MonthInt = m.monthGroup};
                modelList.Add(modelItem);
            }
            return View("MonthlyBill", modelList.OrderBy(o => o.MonthInt));
        }
        private class MonthlyBill
        {
            public int monthGroup;
            public double sum;
            public string month;
        }
    }
}