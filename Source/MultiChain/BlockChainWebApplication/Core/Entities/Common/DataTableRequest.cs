using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChainWebApplication.Core.Entities.Common
{
    public class DataTableRequest
    {
        public int sEcho { get; set; }

        public int iDisplayLength { get; set; }

        public int iDisplayStart { get; set; }

        public string sSearch { get; set; }

        public int iSortCol_0 { get; set; }

        public string sSortDir_0 { get; set; }
    }
}
