using BlockChainWebApplication.Models;
using MultiChainLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BlockChainWebApplication.Core.Utilities;

namespace BlockChainWebApplication.Controllers
{
    [AuthorizeAccess]
    public class BaseController : Controller
    {
        protected DatabaseEntities db = DatabaseContextManager.Current;
        //protected MultiChainClient Client = new MultiChainClient("43.224.34.42", 4350, false, "multichainrpc", "F6bF2AzoWbaUQLSJzpRqxY7sUnQnUGXTv2FvsN6HRkJW", "chain1");
        protected MultiChainClient Client = new MultiChainClient("45.63.97.114", 4350, false, "multichainrpc", "EQTBzc6H3idrNB2Z8uxySGCj2uxNLKeMJK5tc4iWM8ti", "seratio_chain");
    }
}