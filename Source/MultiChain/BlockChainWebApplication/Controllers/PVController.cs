using BlockChainWebApplication.Core;
using BlockChainWebApplication.Core.Entities.Common;
using BlockChainWebApplication.Core.Entities.SAPI;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BlockChainWebApplication.Controllers
{
    public class PVController : BaseController
    {
        // GET: PV
        public async Task<ActionResult> Index()
        {
            SAPIResponse _response = await AppManager.GetPV(AppManager.User.Email);

            if (_response != null && _response.data != null)
            {
                ViewBag.User = await db.Users.FirstOrDefaultAsync(u => u.IsDataActive && u.ID == AppManager.User.ID);

                if (ViewBag.User != null)
                {
                    ViewBag.PV = ViewBag.User.PV;
                }

                return View(_response.data);
            }
            else
            {
                return View();
            }
        }

        public ActionResult Calculate()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Calculate(Data model)
        {
            if (model.country != null)
            {
                model.email = AppManager.User.Email;
                model.currency = model.country;

                SAPIResponse _response = await AppManager.CalculatePV(model);

                var _user = await db.Users.FirstOrDefaultAsync(i => i.IsDataActive && i.ID == AppManager.User.ID);

                if (_user != null)
                {
                    _user.PV = (decimal)_response.data.pv;
                    await db.SaveChangesAsync();
                }

                if (_response.status.ToLower() == "success")
                {
                    TempData["Notification"] = new Notification("Success", "Your request for the Personal Value Certificate has been processed successfully.");
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Notification"] = new Notification("Error", "Your request for the Personal Value Certificate cannot be processed now. Please try again later.");
                    return View();
                }
            }
            else
            {
                TempData["Notification"] = new Notification("Error", "One or more fields are missing or contains invalid value. Please try again later.");
                return View(model);
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> PDF(string email, int id)
        {
            SAPIResponse _result = await AppManager.GetPV(email);

            if (_result.data == null)
            {
                return HttpNotFound();
            }

            var _request = await db.Requests.FirstOrDefaultAsync(r => r.ID == id);

            ViewBag.User = (await db.Users.FirstOrDefaultAsync(u => u.Email == email));

            ViewBag.Nickname = string.IsNullOrEmpty(ViewBag.User.NickName) ? "@" + ViewBag.User.Username : ViewBag.User.NickName;

            if (_request != null)
            {
                _request.HasDownloaded = true;

                await db.SaveChangesAsync();
            }

            return View(_result);
        }

        [AllowAnonymous]
        public async Task<ActionResult> PrintPV(string email, int id)
        {
            var _user = await db.Users.FirstOrDefaultAsync(u => u.IsDataActive && u.Email == email);

            string _url = AppManager.AppURL + "/pv/PDF/?email=" + email + "&id=" + id;
            string _file = await AppManager.SavePDF(_url, Server.MapPath("~//Print//PrintPV-" + (string.IsNullOrEmpty(_user.NickName) ? "@" + _user.Username : _user.NickName) + ".pdf"));
            return Redirect("/print/" + _file);
        }

    }
}