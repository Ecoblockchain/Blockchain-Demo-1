using BlockChainWebApplication.Core;
using BlockChainWebApplication.Core.Entities.Common;
using BlockChainWebApplication.Core.Entities.SAPI;
using BlockChainWebApplication.Core.Utilities;
using BlockChainWebApplication.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BlockChainWebApplication.Controllers
{
    [AuthorizeAccess]
    public class WalletController : BaseController
    {
        // GET: Wallet
        public async Task<ActionResult> Index()
        {
            var _response = await Client.GetAddressBalancesAsync(Rijndael.Decrypt(AppManager.User.BCHash));
            return View(_response.Result);
        }

        public async Task<ActionResult> Send()
        {
            ViewBag.Asset = (await Client.GetAddressBalancesAsync(Rijndael.Decrypt(AppManager.User.BCHash))).Result.Select(i => new SelectListItem
            {
                Text = $"{i.Name} ({i.Qty} available in wallet)",
                Value = i.AssetRef
            });

            return View();
        }

        private static bool IsCriteriaMatches(string criteria, decimal? value, decimal? pv)
        {
            if (string.IsNullOrEmpty(criteria) || value == null)
            {
                return true;
            }
            else if (pv == null)
            {
                return false;
            }
            else
            {
                switch (criteria)
                {
                    case ">=":
                        return pv >= value;
                    case "<=":
                        return pv <= value;
                    case "=":
                        return pv == value;
                    default:
                        return false;
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> Send(Payment model, string receiver)
        {
            User _sender = await db.Users.FirstOrDefaultAsync(u => u.ID == AppManager.User.ID);

            if (_sender != null && _sender.PV != null)
            {
                var _asset = (await Client.GetAddressBalancesAsync(Rijndael.Decrypt(AppManager.User.BCHash))).Result.FirstOrDefault(a => a.AssetRef == model.Asset);

                if (_asset != null)
                {
                    if (_asset.Qty < model.Quantity)
                    {
                        TempData["Notification"] = new Notification("Error", "Sorry, you don't have sufficient balance in your wallet.");
                        return RedirectToAction("Send");
                    }
                }

                string _emailAddress = string.Empty;

                if (!string.IsNullOrEmpty(receiver))
                {
                    model.To = receiver;
                }

                if (new EmailAddressAttribute().IsValid(model.To))
                {
                    _emailAddress = model.To;
                }
                else
                {
                    if (await db.Users.AnyAsync(u => u.IsDataActive && (u.NickName == model.To || u.Username == model.To)))
                    {
                        _emailAddress = await db.Users.Where(u => u.IsDataActive && (u.NickName == model.To || u.Username == model.To)).Select(u => u.Email).FirstOrDefaultAsync();
                    }
                    else
                    {
                        TempData["Notification"] = new Notification("Error", "The specified Recipient doesn't have a Seratio Blockchain Account.");
                        return RedirectToAction("Send");
                    }
                }

                User _recipient = await db.Users.FirstOrDefaultAsync(u => u.IsDataActive && u.Email == _emailAddress);

                if (_recipient != null && !string.IsNullOrEmpty(_recipient.BCHash) && !string.IsNullOrEmpty(AppManager.User.BCHash))
                {
                    if (IsCriteriaMatches(_recipient.Criteria, _recipient.CriteriaValue, _sender.PV))
                    {
                        if (IsCriteriaMatches(_sender.Criteria, _sender.CriteriaValue, _recipient.PV))
                        {
                            model.To = Rijndael.Decrypt(_recipient.BCHash);
                            model.From = Rijndael.Decrypt(AppManager.User.BCHash);

                            var _response = await Client.SendAssetFromAsync(model.From, model.To, model.Asset, model.Quantity, 0, JsonConvert.SerializeObject(new TransactionMetaData
                            {
                                GoldPricePerGram = AppManager.CurrentGoldPricePerGram,
                                Value = (double)model.Quantity * AppManager.CurrentGoldPricePerGram,
                                RecipientsPV = _recipient.PV,
                                SendersPV = _sender.PV
                            }));

                            TempData["Notification"] = new Notification("Success", "Your transaction has been processed successfully");
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            TempData["Notification"] = new Notification("Error", "Sorry, your Transaction cannot be processed because the Recipient's PV Score doesn't meet the criteria set by you.");
                            return RedirectToAction("Send");
                        }
                    }
                    else
                    {
                        TempData["Notification"] = new Notification("Error", "Sorry, your Transaction cannot be processed because your PV Score doesn't meet the criteria set by the Recipient.");
                        return RedirectToAction("Send");
                    }
                }
                else
                {
                    TempData["Notification"] = new Notification("Error", "The specified Recipient doesn't have a Seratio Blockchain Account.");
                }
            }
            else
            {
                TempData["Notification"] = new Notification("Error", "You need to have your Personal Value Score calculated in order to make a transaction.");
            }

            return RedirectToAction("Send");
        }

        public async Task<ActionResult> Request()
        {
            ViewBag.Asset = (await Client.GetAddressBalancesAsync(Rijndael.Decrypt(AppManager.User.BCHash))).Result.Select(i => new SelectListItem
            {
                Text = $"{i.Name} ({i.Qty} available in wallet)",
                Value = i.Name
            });

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Request(Payment model)
        {
            SAPIResponse _pv = await AppManager.GetPV(AppManager.User.Email);

            if (_pv != null && _pv.data != null)
            {
                AssestsRequest _request = new AssestsRequest();
                _request.UserID = AppManager.User.ID;
                _request.Assest = "Seratio Coin";
                _request.Quantity = model.Quantity;
                _request.CreatedOn = DateTime.Now;
                _request.Status = "Pending";
                _request.HasRejected = false;
                _request.HasApproved = false;

                db.AssestsRequests.Add(_request);
                await db.SaveChangesAsync();

                TempData["Notification"] = new Notification("Success", "Your request has been processed successfully");
                return RedirectToAction("requests");
            }

            else
            {
                TempData["Notification"] = new Notification("Error", "You need to have your Personal Value Score calculated in order to make a transaction.");
            }

            return RedirectToAction("request");
        }

        public ActionResult Requests()
        {
            return View();
        }

        public async Task<JsonResult> List(DataTableRequest request)
        {
            List<AssestsRequest> _requests = await db.AssestsRequests.Where(r => (r.HasApproved || r.HasRejected)).ToListAsync();

            foreach (AssestsRequest _item in _requests)
            {
                if (_item.UserID == AppManager.User.ID)
                {
                    _item.HasRejected = false;
                    _item.HasApproved = false;

                    await db.SaveChangesAsync();
                }
            }

            var _query = from r in db.AssestsRequests
                         select new
                         {
                             r.ID,
                             r.UserID,
                             r.Assest,
                             r.Quantity,
                             r.CreatedOn,
                             r.StatusChangedOn,
                             r.User,
                             r.Status
                         };

            if (!string.IsNullOrEmpty(request.sSearch))
            {
                _query = _query.Where(r => r.User.NickName.Contains(request.sSearch) || r.User.Username.Contains(request.sSearch) || r.CreatedOn.ToString().Contains(request.sSearch) || r.StatusChangedOn.ToString().Contains(request.sSearch) || r.Assest.Contains(request.sSearch));
            }

            if (!AppManager.User.IsSuperAdmin)
            {
                _query = _query.Where(i => i.UserID == AppManager.User.ID);
            }

            switch (request.iSortCol_0)
            {
                case 0:
                    _query = request.sSortDir_0 == "asc" ? _query.OrderBy(r => (string.IsNullOrEmpty(r.User.NickName) ? r.User.Username : r.User.NickName)) : _query.OrderBy(r => (string.IsNullOrEmpty(r.User.NickName) ? r.User.Username : r.User.NickName));
                    break;
                case 1:
                    _query = request.sSortDir_0 == "asc" ? _query.OrderBy(r => r.Assest) : _query.OrderByDescending(r => r.Assest);
                    break;
                case 2:
                    _query = request.sSortDir_0 == "asc" ? _query.OrderByDescending(r => r.Quantity) : _query.OrderBy(r => r.Quantity);
                    break;
                case 3:
                    _query = request.sSortDir_0 == "asc" ? _query.OrderByDescending(r => r.Status) : _query.OrderBy(r => r.Status);
                    break;
                case 4:
                    _query = request.sSortDir_0 == "asc" ? _query.OrderByDescending(r => r.CreatedOn) : _query.OrderBy(r => r.CreatedOn);
                    break;
                case 5:
                    _query = request.sSortDir_0 == "asc" ? _query.OrderBy(r => r.StatusChangedOn) : _query.OrderByDescending(r => r.StatusChangedOn);
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

                _row.Add(_rowData.User.NickName == null ? "@" + _rowData.User.Username : _rowData.User.NickName);
                _row.Add(_rowData.Assest);
                _row.Add(String.Format("{0:0.00000000}", _rowData.Quantity));

                switch (_rowData.Status)
                {
                    case "Pending":
                        if (!AppManager.User.IsSuperAdmin)
                        {
                            _row.Add(@"<span class='label label-primary'>" + _rowData.Status + "</span>");
                        }
                        else
                        {
                            _row.Add(@"<span class='label label-primary'>" + _rowData.Status + "</span><div><span>");

                        }
                        break;
                    case "Rejected":
                        _row.Add(@"<span class='label label-danger'>" + _rowData.Status + "</span>");
                        break;
                    case "Approved":
                        _row.Add(@"<span class='label label-success'>" + _rowData.Status + "</span>");
                        break;
                }

                _row.Add(_rowData.CreatedOn.ToString("dd/MM/yyyy hh:mm tt"));
                _row.Add(_rowData.StatusChangedOn != null ? _rowData.CreatedOn.ToString("dd/MM/yyyy hh:mm tt") : "-");

                if (_rowData.Status == "Pending" && AppManager.User.IsSuperAdmin)
                {
                    _row.Add(@"<a href='/wallet/approve/" + _rowData.ID + "'><i class='confirm mr5' title='Approve' data-confirm='Are you sure you want to approve this Request?'><i class='icon-checkmark4 text-primary'></i></i></a>  <a href='/wallet/reject/" + _rowData.ID + "'><i class='confirm mr5' title='Reject' data-confirm='Are you sure you want to reject this Request?'><i class='icon-cross2 text-primary'></i></i></a></span><div>");
                }
                else
                {
                    _row.Add("N/A");
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

        public async Task<ActionResult> Approve(int ID)
        {
            if (ID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AssestsRequest _request = await db.AssestsRequests.FirstOrDefaultAsync(r => r.ID == ID);

            if (_request != null)
            {

                _request.StatusChangedOn = DateTime.Now;
                _request.Status = "Approved";
                _request.HasApproved = true;
                _request.HasRejected = false;

                await db.SaveChangesAsync();

                var _recipient = db.Users.FirstOrDefault(u => u.Email == _request.User.Email && u.IsDataActive);


                if (_recipient != null && !string.IsNullOrEmpty(_recipient.BCHash))
                {
                    if (!string.IsNullOrEmpty(_request.Assest))
                    {
                        var _duplicateAssets = (await Client.ListAssetsAsync(_request.Assest)).Result;

                        var _response = await Client.IssueMoreAsync(Rijndael.Decrypt(_recipient.BCHash), _request.Assest, (decimal)_request.Quantity);

                        if (string.IsNullOrEmpty(_response.Error))
                        {
                            TempData["Notification"] = new Notification("Success", $"More quantity of {_request.Assest} has been issued successfully");
                        }
                        else
                        {
                            TempData["Notification"] = new Notification("Error", _response.Error);
                        }
                    }
                    else
                    {
                        object _asset = new
                        {
                            name = _request.Assest,
                            open = true
                        };

                        var _response = await Client.IssueAsync(Rijndael.Decrypt(_recipient.BCHash), _asset, (int)_request.Quantity, 5);

                        if (string.IsNullOrEmpty(_response.Error))
                        {
                            TempData["Notification"] = new Notification("Success", "New Asset has been created successfully");
                        }
                        else
                        {
                            TempData["Notification"] = new Notification("Error", _response.Error);
                        }
                    }

                }
                else
                {
                    TempData["Notification"] = new Notification("Error", "Invalid Requst. Please try again later.");
                }

            }
            return RedirectToAction("requests");
        }

        public async Task<ActionResult> Reject(int ID)
        {
            if (ID == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AssestsRequest _request = await db.AssestsRequests.FirstOrDefaultAsync(r => r.ID == ID);

            if (_request != null)
            {
                _request.StatusChangedOn = DateTime.Now;
                _request.Status = "Rejected";
                _request.HasRejected = true;
                _request.HasApproved = false;

                await db.SaveChangesAsync();

                TempData["Notification"] = new Notification("Success", "New requsts has been rejected successfully");
            }
            else
            {
                TempData["Notification"] = new Notification("Error", "Invalid Requst. Please try again later.");
            }

            return RedirectToAction("requests");

        }


        public async Task<ActionResult> History()
        {
            var _response = await Client.ListAddressTransactionsAsync(Rijndael.Decrypt(AppManager.User.BCHash), 100);
            return View(_response.Result);
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