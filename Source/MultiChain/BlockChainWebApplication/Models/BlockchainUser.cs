using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlockChainWebApplication.Models
{
    public class BlockchainUser
    {
        public string Password { get; set; }

        public string Username { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }
    }
}