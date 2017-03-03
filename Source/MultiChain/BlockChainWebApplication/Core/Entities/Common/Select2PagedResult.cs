using BlockChainWebApplicationS.Core.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlockChainWebApplication.Core.Entities.Common
{
    public class Select2PagedResult
    {
        public int Total { get; set; }

        public List<Select2Item> Results { get; set; }
    }

    public class Select2TextPagedResult
    {
        public int Total { get; set; }

        public List<Select2TextItem> Results { get; set; }
    }
}