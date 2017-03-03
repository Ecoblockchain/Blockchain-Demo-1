using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlockChainWebApplication.Core.Entities.GoldPriceSyncer
{
    class GoldPriceResponseObj
    {
        public class Rootobject
        {
            public Market market { get; set; }
            public string updateTimeString { get; set; }
            public bool loggedIn { get; set; }
            public string locale { get; set; }
            public string weightUnit { get; set; }
        }

        public class Market
        {
            public Pitch[] pitches { get; set; }
        }

        public class Pitch
        {
            public object pitchId { get; set; }
            public string securityClassNarrative { get; set; }
            public string securityId { get; set; }
            public string considerationCurrency { get; set; }
            public Price[] prices { get; set; }
            public Buyprice[] buyPrices { get; set; }
            public Sellprice[] sellPrices { get; set; }
            public int size { get; set; }
        }

        public class Price
        {
            public Actionindicator actionIndicator { get; set; }
            public float quantity { get; set; }
            public int limit { get; set; }
            public float ouncesLimit { get; set; }
            public bool sell { get; set; }
            public bool buy { get; set; }
            public float ouncesQuantity { get; set; }
            public float value { get; set; }
        }

        public class Actionindicator
        {
            public string value { get; set; }
            public bool sell { get; set; }
            public bool buy { get; set; }
        }

        public class Buyprice
        {
            public Actionindicator1 actionIndicator { get; set; }
            public float quantity { get; set; }
            public int limit { get; set; }
            public float ouncesLimit { get; set; }
            public bool sell { get; set; }
            public bool buy { get; set; }
            public float ouncesQuantity { get; set; }
            public float value { get; set; }
        }

        public class Actionindicator1
        {
            public string value { get; set; }
            public bool sell { get; set; }
            public bool buy { get; set; }
        }

        public class Sellprice
        {
            public Actionindicator2 actionIndicator { get; set; }
            public float quantity { get; set; }
            public int limit { get; set; }
            public float ouncesLimit { get; set; }
            public bool sell { get; set; }
            public bool buy { get; set; }
            public float ouncesQuantity { get; set; }
            public float value { get; set; }
        }

        public class Actionindicator2
        {
            public string value { get; set; }
            public bool sell { get; set; }
            public bool buy { get; set; }
        }

    }
}