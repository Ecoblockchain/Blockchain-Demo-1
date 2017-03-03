using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using static BlockChainWebApplication.Core.Entities.GoldPriceSyncer.GoldPriceResponseObj;

namespace BlockChainWebApplication.Core.Utilities
{
    public class GoldPriceSyncer : IJob
    {
        public async void Execute(IJobExecutionContext context)
        {
            var _responseString = await new HttpClient().GetStringAsync("https://www.bullionvault.com/view_market_json.do?marketWidth=1");
            Rootobject _responseObject = JsonConvert.DeserializeObject<Rootobject>(_responseString);
            double _averageGoldPriceinKG = (_responseObject != null ? (_responseObject.market != null ? (_responseObject.market.pitches != null ? _responseObject.market.pitches.Where(p => p.securityClassNarrative == "GOLD" && p.considerationCurrency == "GBP").Average(p => p.buyPrices[0].limit) : 0) : 0) : 0);
            double _averageGoldPriceinGram = _averageGoldPriceinKG / 1000;

            AppManager.CurrentGoldPricePerGram = _averageGoldPriceinGram;

            //  Console.WriteLine("Average Gold Price per Gram at {0} is {1} USD.", DateTime.Now.ToShortTimeString(), _averageGoldPriceinGram);
        }
    }
}