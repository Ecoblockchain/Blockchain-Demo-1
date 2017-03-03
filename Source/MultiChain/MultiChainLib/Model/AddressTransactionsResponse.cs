using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainLib.Model
{
    public class AddressTransactionsResponse
    {
        public BalanceSummary balance { get; set; }
        public string[] myaddresses { get; set; }
        public string[] addresses { get; set; }
        public object[] permissions { get; set; }
        public object[] data { get; set; }
        public int confirmations { get; set; }
        public string blockhash { get; set; }
        public int blockindex { get; set; }
        public int blocktime { get; set; }
        public string txid { get; set; }
        public bool valid { get; set; }
        public int time { get; set; }
        public int timereceived { get; set; }
        public string comment { get; set; }
    }


    public class BalanceSummary
    {
        public decimal amount { get; set; }
        public AssetSummary[] assets { get; set; }
    }

    public class AssetSummary
    {
        public string name { get; set; }
        public string assetref { get; set; }
        public decimal qty { get; set; }
    }
}
