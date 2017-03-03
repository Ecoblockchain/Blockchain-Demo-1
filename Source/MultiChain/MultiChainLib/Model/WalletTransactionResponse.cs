using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainLib.Model
{
    public class WalletTransactionResponse
    {
        public Balance balance { get; set; }
        public string[] myaddresses { get; set; }
        public string[] addresses { get; set; }
        public Permission[] permissions { get; set; }
        public Issue issue { get; set; }
        public string[] data { get; set; }
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

    public class Balance
    {
        public decimal amount { get; set; }
        public AssetResponse[] assets { get; set; }
    }

    public class Asset
    {
        public string name { get; set; }
        public string assetref { get; set; }
        public int qty { get; set; }
    }

    public class Issue
    {
        public string name { get; set; }
        public string assetref { get; set; }
        public int multiple { get; set; }
        public float units { get; set; }
        public bool open { get; set; }
        public Details details { get; set; }
        public decimal qty { get; set; }
        public int raw { get; set; }
        public string[] addresses { get; set; }
    }

    public class Details
    {

    }

    public class Permission
    {
        public bool connect { get; set; }
        public bool send { get; set; }
        public bool receive { get; set; }
        public bool write { get; set; }
        public bool create { get; set; }
        public bool issue { get; set; }
        public bool mine { get; set; }
        public bool admin { get; set; }
        public bool activate { get; set; }
        public int startblock { get; set; }
        public long endblock { get; set; }
        public int timestamp { get; set; }
        public string[] addresses { get; set; }
    }

}
