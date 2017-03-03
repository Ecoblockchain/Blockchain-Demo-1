using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlockChainWebApplication.Models
{
    public class Asset
    {
        public string Name { get; set; }

        public int Quantity { get; set; }

        public string Address { get; set; }

        public decimal Units { get; set; }
    }
}