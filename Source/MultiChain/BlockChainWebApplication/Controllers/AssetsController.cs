using BlockChainWebApplication.Core;
using BlockChainWebApplication.Core.Entities.Common;
using BlockChainWebApplication.Core.Utilities;
using BlockChainWebApplication.Models;
using MultiChainLib;
using MultiChainLib.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BlockChainWebApplication.Controllers
{
    [AuthorizeAccess(RequireSuperAdmin = true)]
    public class AssetsController : BaseController
    {
        // GET: Assets
        public async Task<ActionResult> Index()
        {
            return View((await Client.ListAssetsAsync()).Result);
        }

        public ActionResult Issue()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Issue(Asset model)
        {
            if (ModelState.IsValid)
            {
                var _recipient = await db.Users.FirstOrDefaultAsync(u => u.Email == model.Address && u.IsDataActive);

                if (_recipient != null && !string.IsNullOrEmpty(_recipient.BCHash))
                {
                    if (!string.IsNullOrEmpty(Request.QueryString["asset_name"]))
                    {
                        var _duplicateAssets = (await Client.ListAssetsAsync(model.Name)).Result;
                        var _response = await Client.IssueMoreAsync(Rijndael.Decrypt(_recipient.BCHash), model.Name, model.Quantity);

                        if (string.IsNullOrEmpty(_response.Error))
                        {
                            TempData["Notification"] = new Notification("Success", $"More quantity of {model.Name} has been issued successfully");
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
                            name = model.Name,
                            open = true
                        };

                        var _response = await Client.IssueAsync(Rijndael.Decrypt(_recipient.BCHash), _asset, model.Quantity, model.Units);

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
                    TempData["Notification"] = new Notification("Error", "Invalid Recipient. Please try again later.");
                }
            }
            else
            {
                TempData["Notification"] = new Notification("Error", "One or more required fields are missing. Please try again later.");
            }

            return Redirect("/Assets/Index");
        }
    }
}