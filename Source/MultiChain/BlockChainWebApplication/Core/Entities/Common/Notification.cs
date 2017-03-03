using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChainWebApplication.Core.Entities.Common
{
    public class Notification
    {
        public string Type { get; set; }

        public string Text { get; set; }

        public Notification(string Type, string Text)
        {
            this.Text = Text;
            this.Type = Type;
        }
    }
}
