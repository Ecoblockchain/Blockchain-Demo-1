using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using MultiChainLib;
using BlockChainWebApplication.Core.Entities.Common;
using BlockChainWebApplication.Core.Utilities;

namespace BlockChainWebApplication.Controllers
{
    [AuthorizeAccess(RequireSuperAdmin = true)]
    public class AddressesController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            var _response = await Client.GetAddressesAsync();
            return View(_response.Result);
        }

        public async Task<ActionResult> Create()
        {
            var _response = await Client.GetNewAddressAsync();

            await Client.GrantAsync(new string[] { _response.Result }, BlockchainPermissions.Send);
            await Client.GrantAsync(new string[] { _response.Result }, BlockchainPermissions.Receive);

            TempData["Notification"] = new Notification("Success", "New Address has been created successfully");

            return RedirectToAction("Index");
        }
    }
}