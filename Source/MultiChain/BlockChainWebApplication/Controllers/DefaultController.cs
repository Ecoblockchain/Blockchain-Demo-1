using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BlockChainWebApplication.Models;
using BlockChainWebApplication.Core.Utilities;
using BlockChainWebApplication.Core.Entities.Common;
using BlockChainWebApplication.Core;
using System.Net;
using System.Data.Entity;
using System.Net.Mail;
using System.Net.Mime;
using System.IO;
using System.Text;
using BlockChainWebApplicationS.Core.Entities.Common;
using System.Data.Entity.Validation;

namespace BlockChainWebApplication.Controllers
{
    public class DefaultController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Index(string username, string password, bool remindme = false)
        {
            var _loggedInCandidate = await AppManager.AuthenticateUserAsync(username.ToLower(), password);

            if (_loggedInCandidate != null)
            {
                Session["Blockchain_User"] = _loggedInCandidate;

                return RedirectToAction("dashboard", "default");
            }
            else
            {
                TempData["Notification"] = new Notification("Error", "Email and password do not match. Please try again.");
                return View();
            }

        }

        [AllowAnonymous]
        public ActionResult Signup()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Signup(User model)
        {
            model.ConfirmPassword = model.Password;

            ModelState.Clear();
            TryValidateModel(model);

            if (ModelState.IsValid)
            {
                if (!await db.Users.AnyAsync(u => (u.Email == model.Email || u.Username.ToLower() == model.Username.ToLower()) && u.IsDataActive))
                {
                    try
                    {
                        model.AuthString = Rijndael.Encrypt(string.Concat(model.Username.ToLower(), model.Password));
                        model.CreatedOn = model.LastUpdatedOn = DateTime.Now;
                        model.Username = model.Username.ToLower();
                        model.IsDataActive = true;
                        model.IsSearchable = false;

                        db.Users.Add(model);
                        await db.SaveChangesAsync();

                        model.BCHash = await AppManager.CreateAddress();
                        await db.SaveChangesAsync();

                        await Client.IssueMoreAsync(Rijndael.Decrypt(model.BCHash), "Seratio Coin", 1000);

                        string _body = $"Hello {(model.NickName != null ? model.NickName : "@" + model.Username.ToLower())} ,<br /><br />Welcome to Seratio Blockchain.<br />We have added 1000 Seratio Coins to your wallet for you to get started with our Platform.";
                        AppManager.SendEmail("Welcome to Seratio Blockchain", model.Email, _body);

                        TempData["Notification"] = new Notification("Success", "Welcome to Seratio Blockchain, your account has been created successfully.");
                        return RedirectToAction("index");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        string _errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                        TempData["Notification"] = new Notification("Error", _errorMessages);
                    }
                }
                else
                {
                    TempData["Notification"] = new Notification("Error", "There is an existing account associated with this email or Username.");
                }
            }
            else
            {
                TempData["Notification"] = new Notification("Error", "One or more required fields are missing. Please try again later.");
            }

            return View(model);
        }

        public ActionResult Logout()
        {

            if (Session["Blockchain_User"] != null)
            {
                Session.Contents.RemoveAll();
                Session.Clear();
                Session.Abandon();
            }

            return RedirectToAction("index");
        }

        public async Task<ActionResult> Details(int ID)
        {
            if (ID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User _user = await db.Users.FindAsync(AppManager.User.ID);

            if (_user == null)
            {
                return HttpNotFound();
            }

            return View(_user);
        }

        public async Task<ActionResult> Edit(int ID)
        {
            if (ID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User _user = await db.Users.FindAsync(AppManager.User.ID);

            if (_user == null)
            {
                return HttpNotFound();
            }

            return View(_user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(User model)
        {
            model.ConfirmPassword = model.Password;
            ModelState.Clear();
            TryValidateModel(model);

            if (ModelState.IsValid)
            {
                User _user = await db.Users.FindAsync(AppManager.User.ID);

                if (_user != null)
                {
                    if (await db.Users.AnyAsync(u => u.IsDataActive && u.ID != AppManager.User.ID && u.NickName == model.NickName && !string.IsNullOrEmpty(model.NickName)))
                    {
                        TempData["Notification"] = new Notification("Error", "Requested Nickname already exist. Please try another one.");
                        return Redirect("/default/edit/" + AppManager.User.ID);
                    }

                    _user.Phone = model.Phone;
                    _user.Gender = model.Gender;
                    _user.NickName = model.NickName;
                    _user.IsSearchable = model.IsSearchable;
                    _user.LastUpdatedOn = DateTime.Now;
                    _user.Criteria = model.Criteria;

                    if (model.Criteria != null)
                    {
                        _user.CriteriaValue = model.CriteriaValue;
                    }
                    else
                    {
                        _user.CriteriaValue = null;
                    }

                    await db.SaveChangesAsync();

                    TempData["Notification"] = new Notification("Success", "Requested User has been saved successfully.");
                    return RedirectToAction("index", "pv");
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            TempData["Notification"] = new Notification("Error", "One or more fields are missing or contains invalid value. Please try again later.");
            return View(model);
        }

        public ActionResult Error()
        {
            Exception ex = Server.GetLastError();
            ExceptionHandler.Handle(ex);

            return View();
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ChangePassword(User model)
        {
            if (model.Password != null)
            {
                if (model.Password == model.ConfirmPassword)
                {
                    var _user = await db.Users.FirstOrDefaultAsync(c => c.ID == AppManager.User.ID && c.IsDataActive);

                    if (_user != null)
                    {
                        string _authString = Rijndael.Encrypt(string.Concat(_user.Email, model.Password));

                        _user.AuthString = _authString;
                        await db.SaveChangesAsync();

                        TempData["Notification"] = new Notification("Success", "Password has been changed successfully.");
                        return Redirect("/pv");
                    }
                    else
                    {
                        TempData["Notification"] = new Notification("Error", "Sorry, we are unable to process your request. Please try again later.");
                        return RedirectToAction("changepassword");
                    }
                }
                else
                {
                    TempData["Notification"] = new Notification("Error", "Passwords do not match. Please try again later.");
                    return RedirectToAction("changepassword");
                }
            }
            else
            {
                TempData["Notification"] = new Notification("Error", "Sorry, we are unable to process your request. Please try again later.");
                return RedirectToAction("changepassword");
            }


        }

        [AllowAnonymous]
        public ActionResult Forgot()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Forgot(string email)
        {
            var _user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsDataActive);

            if (_user != null)
            {
                string _hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(email));
                string _body = $"Hi there,<br />You or someone else has requested to change the password for your Seratio Blockchain Account.<br />Please <a href='{AppManager.AppURL}/default/resetpassword?email={_hash}'>click here</a> to reset your password.<br /><br />You can ignore this message if you haven't requested to change the password.";
                AppManager.SendEmail("Your Account Password - Seratio Blockchain", email, _body);
            }

            TempData["Notification"] = new Notification("Success", "A link to change your password has been sent to your email account.");
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string email)
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> ResetPassword(User model)
        {
            model.Email = Encoding.UTF8.GetString(Convert.FromBase64String(model.Email));

            var _user = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.IsDataActive);

            if (_user != null)
            {
                _user.AuthString = Rijndael.Encrypt(string.Concat(_user.Username, model.Password));
                await db.SaveChangesAsync();

                TempData["Notification"] = new Notification("Success", "Your password has been changed successfully.");
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Notification"] = new Notification("Error", "Sorry unable to process your request. please try later.");
                return RedirectToAction("Index");
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> GetUsers(string q, int i)
        {
            int _skip = i == 1 ? 0 : i * 20;

            var _query = from u in db.Users

                         where u.IsDataActive && (u.NickName.Contains(q) || u.Email.Contains(q) || u.Username.Contains(q))
                         orderby u.NickName
                         select new
                         {
                             u.NickName,
                             u.Email,
                             u.Username
                         };

            Select2TextPagedResult _result = new Select2TextPagedResult
            {
                Total = await _query.OrderBy(t => t.NickName).CountAsync(),
                Results = (await _query.OrderBy(t => t.NickName).Skip(_skip).Take(20).ToListAsync()).Select(r => new Select2TextItem
                {
                    id = r.Email,
                    text = r.NickName != null ? r.NickName + " " + "(@" + r.Username + ")" : "@" + r.Username
                }).ToList()
            };

            return Json(_result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Dashboard()
        {
            var _response = await Client.GetAddressBalancesAsync(Rijndael.Decrypt(AppManager.User.BCHash));
            ViewBag.Response = _response.Result;
            ViewBag.RecentTransactions = (await Client.ListAddressTransactionsAsync(Rijndael.Decrypt(AppManager.User.BCHash), 100)).Result.Where(i => i.balance.assets != null).OrderByDescending(i => i.time).Take(10).ToList();
            ViewBag.Requests = await db.Requests.Include(r => r.User).Where(r => r.IsDataActive && r.SenderID == AppManager.User.ID || r.Recipient == AppManager.User.Email || r.Recipient == AppManager.User.NickName).OrderByDescending(r => r.ID).Take(10).ToListAsync();

            return View(await db.Users.FirstOrDefaultAsync(u => u.ID == AppManager.User.ID));
        }
    }
}