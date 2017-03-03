using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChainWebApplication.Core.Entities.Common
{
    public class DataTableResponse
    {
        public int sEcho { get; set; }

        public int iDisplayLength { get; set; }

        public int iDisplayStart { get; set; }

        public long iTotalRecords { get; set; }

        public long iTotalDisplayRecords { get; set; }

        public List<DataTableRow> aaData { get; set; }
    }

    public class DataTableRow : List<object>
    {

    }
}
