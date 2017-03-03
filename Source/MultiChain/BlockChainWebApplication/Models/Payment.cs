using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlockChainWebApplication.Models
{
    public class Payment
    {
        public string From { get; set; }

        public string To { get; set; }

        public string Asset { get; set; }

        public decimal Quantity { get; set; }
    }
}