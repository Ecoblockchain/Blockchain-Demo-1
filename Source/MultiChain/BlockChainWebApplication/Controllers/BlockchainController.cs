using BlockChainWebApplication.Core;
using BlockChainWebApplication.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BlockChainWebApplication.Controllers
{
    public class BlockchainController : BaseController
    {
        public async Task<ActionResult> Info()
        {
            return View((await Client.GetBlockchainInfoAsync()).Result);
        }

        public async Task<ActionResult> Log()
        {
            var _response = await Client.ListWalletTransactionsAsync(1000);
            return View(_response.Result);
        }

        public async Task<ActionResult> Details(string txid)
        {
            var _response = await Client.GetWalletTransaction(txid);
            return View(_response.Result);
        }

        public async Task<ActionResult> Transactions()
        {
            var _users = await db.Users.Where(u => u.IsDataActive && u.BCHash != null).ToListAsync();

            List<MultiChainLib.Model.AddressTransactionsResponse> _items = new List<MultiChainLib.Model.AddressTransactionsResponse>();

            foreach (var _user in _users)
            {
                var _response = await Client.ListAddressTransactionsAsync(Rijndael.Decrypt(_user.BCHash), 100);

                if (string.IsNullOrEmpty(_response.Error))
                {
                    _items.AddRange(_response.Result);
                }
            }

            return View(_items.Where(t => t.balance.assets != null).OrderByDescending(i => i.time).GroupBy(t => t.txid).Select(g=>g.FirstOrDefault()).ToList());
        }

        public async Task<ActionResult> TransactionsForUser()
        {
            var _users = await db.Users.Where(u => u.IsDataActive && u.BCHash != null).ToListAsync();

            List<MultiChainLib.Model.AddressTransactionsResponse> _items = new List<MultiChainLib.Model.AddressTransactionsResponse>();

            foreach (var _user in _users)
            {
                var _response = await Client.ListAddressTransactionsAsync(Rijndael.Decrypt(_user.BCHash), 100);

                if (string.IsNullOrEmpty(_response.Error))
                {
                    _items.AddRange(_response.Result);
                }
            }

            return View(_items.Where(t => t.balance.assets != null).OrderByDescending(i => i.time).GroupBy(t => t.txid).Select(g => g.FirstOrDefault()).ToList());
        }
    }
}