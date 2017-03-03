using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiChainLib.Model
{

    public class ListStreamsResponse
    {
        public string name { get; set; }
        public string createtxid { get; set; }
        public string streamref { get; set; }
        public bool open { get; set; }
        public Object details { get; set; }
        public bool subscribed { get; set; }
        public bool synchronized { get; set; }
        public int items { get; set; }
        public int confirmed { get; set; }
        public int keys { get; set; }
        public int publishers { get; set; }
    }

}
