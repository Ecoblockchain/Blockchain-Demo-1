using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlockChainWebApplication.Models
{
    public class TransactionMetaData
    {
        public decimal? SendersPV { get; set; }

        public decimal? RecipientsPV { get; set; }

        public double GoldPricePerGram { get; set; }

        public double Value { get; set; }

    }
}