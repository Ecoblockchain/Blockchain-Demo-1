using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BlockChainWebApplication.Models;
using System.Threading.Tasks;
using BlockChainWebApplication.Core.Entities.Common;
using BlockChainWebApplicationS.Core.Entities.Common;
using BlockChainWebApplication.Core;
using System.Net.Mime;
using System.Net.Mail;
using BlockChainWebApplication.Core.Entities.SAPI;
using System.ComponentModel.DataAnnotations;
using BlockChainWebApplication.Core.Utilities;

namespace BlockChainWebApplication.Controllers
{
    [AuthorizeAccess]
    public class RequestsController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> List(DataTableRequest request)
        {
            List<Request> _requests = await db.Requests.Where(r => r.IsDataActive && r.HasRejected).ToListAsync();

            foreach (Request _item in _requests)
            {
                if (_item.SenderID == AppManager.User.ID)
                {
                    _item.HasRejected = false;

                    await db.SaveChangesAsync();
                }
            }

            var _query = from r in db.Requests
                         where r.IsDataActive && r.SenderID == AppManager.User.ID || r.Recipient == AppManager.User.Email
                         select new
                         {
                             r.ID,
                             r.Message,
                             r.Status,
                             r.SentOn,
                             r.RespondedOn,
                             r.SenderID,
                             r.Recipient,
                             r.HasDownloaded,
                             r.User,
                             Receiver = db.Users.FirstOrDefault(u => u.IsDataActive && (u.Email == r.Recipient || u.NickName == r.Recipient))

                         };

            if (!string.IsNullOrEmpty(request.sSearch))
            {
                _query = _query.Where(r => r.User.NickName.Contains(request.sSearch) | r.User.Username.Contains(request.sSearch) || r.Recipient.Contains(request.sSearch) || r.SentOn.ToString().Contains(request.sSearch) || r.RespondedOn.ToString().Contains(request.sSearch) || r.Status.Contains(request.sSearch));
            }

            switch (request.iSortCol_0)
            {
                case 0:
                    _query = request.sSortDir_0 == "asc" ? _query.OrderBy(r => (string.IsNullOrEmpty(r.User.NickName) ? r.User.Username : r.User.NickName)) : _query.OrderByDescending(r => (string.IsNullOrEmpty(r.User.NickName) ? r.User.Username : r.User.NickName));
                    break;
                case 1:
                    _query = request.sSortDir_0 == "asc" ? _query.OrderBy(r => (string.IsNullOrEmpty(r.Receiver.NickName) ? r.Receiver.Username : r.Receiver.NickName)) : _query.OrderByDescending(r => (string.IsNullOrEmpty(r.Receiver.NickName) ? r.Receiver.Username : r.Receiver.NickName));
                    break;
                case 2:
                    _query = request.sSortDir_0 == "asc" ? _query.OrderByDescending(r => r.SentOn) : _query.OrderBy(r => r.SentOn);
                    break;
                case 3:
                    _query = request.sSortDir_0 == "asc" ? _query.OrderBy(r => r.RespondedOn) : _query.OrderByDescending(r => r.RespondedOn);
                    break;
                case 4:
                    _query = request.sSortDir_0 == "asc" ? _query.OrderBy(r => r.Status) : _query.OrderByDescending(r => r.Status);
                    break;
                default:
                    _query = _query.OrderByDescending(r => r.ID);
                    break;
            }

            var _count = await _query.CountAsync();

            var _data = await _query.Skip(request.iDisplayStart).Take(request.iDisplayLength).ToListAsync();
            List<DataTableRow> _rows = new List<DataTableRow>();

            foreach (var _rowData in _data)
            {
                DataTableRow _row = new DataTableRow();

                _row.Add((string.IsNullOrEmpty(_rowData.User.NickName) ? "@" + _rowData.User.Username : _rowData.User.NickName));

                if (_rowData.Receiver != null)
                {
                    _row.Add((string.IsNullOrEmpty(_rowData.Receiver.NickName) ? "@" + _rowData.Receiver.Username : _rowData.Receiver.NickName));

                }
                else
                {
                    _row.Add("-");
                }
                _row.Add(_rowData.SentOn.ToString("dd/MM/yyyy hh:mm tt"));

                switch (_rowData.Status)
                {
                    case "Pending":
                        _row.Add(@"<span class='label label-primary'>" + _rowData.Status + "</span>");
                        break;
                    case "Rejected":
                        _row.Add(@"<span class='label label-danger'>" + _rowData.Status + "</span>");
                        break;
                    case "Approved":
                        _row.Add(@"<span class='label label-success'>" + _rowData.Status + "</span>");
                        break;
                }

                if (_rowData.RespondedOn != null)
                {
                    _row.Add(_rowData.RespondedOn.Value.ToString("dd/MM/yyyy hh:mm tt"));
                }
                else
                {
                    _row.Add("-");
                }

                if (_rowData.Status == "Approved" && !_rowData.HasDownloaded && (_rowData.User.Email == AppManager.User.Email || _rowData.User.Username == AppManager.User.Username))
                {
                    _row.Add(@"<a href='/requests/details/" + _rowData.ID + "'><i class='icon-magazine text-primary'></i></a> | <a href='/pv/printpv/?email=" + _rowData.Recipient + "&id=" + _rowData.ID + "'><i class='icon-file-download2 text-primary'></i></a>");
                }
                else
                {
                    _row.Add(@"<a href='/requests/details/" + _rowData.ID + "'><i class='icon-magazine text-primary'></i></a>");
                }

                _rows.Add(_row);
            }

            return Json(new DataTableResponse
            {
                sEcho = request.sEcho,
                iDisplayLength = request.iDisplayLength,
                iTotalRecords = _count,
                iDisplayStart = request.iDisplayStart,
                iTotalDisplayRecords = _count,
                aaData = _rows
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Send()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Send(string receiver, string recipient, string message)
        {
            string _emailaddress = string.Empty;

            if (!string.IsNullOrEmpty(receiver))
            {
                recipient = receiver;
            }

            if (new EmailAddressAttribute().IsValid(recipient))
            {
                _emailaddress = recipient;
            }
            else
            {
                if (db.Users.Any(u => u.IsDataActive && (u.NickName == recipient || u.Username == recipient)))
                {
                    _emailaddress = db.Users.FirstOrDefault(u => u.IsDataActive && (u.NickName == recipient || u.Username == recipient)).Email;
                }
                else
                {
                    TempData["Notification"] = new Notification("Error", "The specified Recipient doesn't have a Seratio Blockchain Account.");
                    return View();
                }

            }

            Request _request = new Request();
            _request.SenderID = AppManager.User.ID;
            _request.Recipient = _emailaddress;
            _request.Message = message;
            _request.Status = "Pending";
            _request.HasDownloaded = false;
            _request.SentOn = DateTime.Now;
            _request.IsDataActive = true;

            db.Requests.Add(_request);
            await db.SaveChangesAsync();

            string _body = $"Hello,<br /><br />Greeting of the day!<br /><br /><b>{(AppManager.User.NickName != null ? AppManager.User.NickName : "@" + AppManager.User.Username)} </b> has sent you a request for your Personal Value Certificate via Seratio Blockchain Platform.<br />Please <a href='{AppManager.AppURL}'>Click Here</a> to generate your Certificate.";
            AppManager.SendEmail("Request for your Personal Value Certificate - Seratio Blockchain", _emailaddress, _body);

            TempData["Notification"] = new Notification("Success", "Your request for the Personal Value Certificate has been sent successfully.");
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Details(int ID)
        {
            if (ID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Request _request = await db.Requests.FindAsync(ID);

            if (_request == null)
            {
                return HttpNotFound();
            }

            ViewBag.receiver = await db.Users.AnyAsync(u => u.IsDataActive && u.Email == _request.Recipient && u.NickName != null) ? db.Users.FirstOrDefault(u => u.IsDataActive && u.Email == _request.Recipient && u.NickName != null).NickName : "@" + db.Users.FirstOrDefault(u => u.IsDataActive && u.Email == _request.Recipient).Username;

            return View(_request);
        }

        public async Task<ActionResult> Reject(int ID)
        {
            if (ID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Request _request = await db.Requests.FirstOrDefaultAsync(r => r.ID == ID && r.Recipient == AppManager.User.Email && r.IsDataActive && r.Status == "Pending");

            if (_request != null)
            {
                _request.Status = "Rejected";
                _request.HasRejected = true;
                _request.RespondedOn = DateTime.Now;

                await db.SaveChangesAsync();

                string _body = $"Hello {(_request.User.NickName != null ? _request.User.NickName : "@" + AppManager.User.Username)},<br /><br /><b>{(AppManager.User.NickName != null ? AppManager.User.NickName : "@" + AppManager.User.Username)} </b> has rejected your request for the Personal Value Certificate.";
                AppManager.SendEmail("Request for the Personal Value Certificate has been rejected - Seratio Blockchain", _request.User.Email, _body);

                TempData["Notification"] = new Notification("Success", "Requested recipient is not found in the record.Please try again.");
            }
            else
            {
                return HttpNotFound();
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Approve(int ID)
        {
            if (ID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Request _request = await db.Requests.FirstOrDefaultAsync(r => r.ID == ID && r.Status == "Pending" && (r.Recipient == AppManager.User.Email || r.Recipient == AppManager.User.NickName) && r.IsDataActive);

            if (_request != null)
            {
                SAPIResponse _response = await AppManager.GetPV(AppManager.User.Email);

                if (_response.data != null)
                {
                    _request.Status = "Approved";
                    _request.RespondedOn = DateTime.Now;

                    await db.SaveChangesAsync();

                    string _body = $"Hello {(_request.User.NickName != null ? _request.User.NickName : "@" + _request.User.Username)} ,<br /><br />Greeting of the day!<br /><br /><b>{(AppManager.User.NickName != null ? AppManager.User.NickName : "@" + AppManager.User.Username)} </b> has approved your request for the Personal Value Certificate.<br />Please <a href='http://localhost:55409'>Click Here</a> to view it.";
                    AppManager.SendEmail("Request for your Personal Value Certificate has been approved - Seratio Blockchain", _request.User.Email, _body);

                    TempData["Notification"] = new Notification("Success", "Request has been Approved and your Certificate has been shared successfully.");
                }
                else
                {
                    TempData["Notification"] = new Notification("Error", "Sorry, you do not have a Personal Value Certificate. Please create one before approving this Request.");
                }

                return RedirectToAction("Index");
            }
            else
            {
                return HttpNotFound();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }

        [HttpPost]
        public async Task<JsonResult> AutoComplete(string q)
        {
            var _users = await (from u in db.Users
                                where u.IsDataActive && u.IsSearchable && (u.NickName.Contains(q) || u.Email.Contains(q) || u.Username.Contains(q))
                                orderby u.NickName
                                select new
                                {
                                    id = u.Email,
                                    text = u.NickName != null ? u.NickName + " " + "(@" + u.Username + ")" : "@" + u.Username
                                }).ToListAsync();

            return Json(_users);
        }
    }
}

