using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace BlockChainWebApplication.Core.Entities.Common
{
    public class LoggedInUser
    {
        public int ID { get; set; }
  
        public string NickName { get; set; }

        public string Email { get; set; }

        public string Gender { get; set; }

        public string Username { get; set; }

        public string Phone { get; set; }

        public string BCHash { get; set; }

        public bool IsSearchable { get; set; }

        public bool IsSuperAdmin { get; set; }
    }
}