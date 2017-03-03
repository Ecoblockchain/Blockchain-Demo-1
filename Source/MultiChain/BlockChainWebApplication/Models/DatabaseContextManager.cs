using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace BlockChainWebApplication.Models
{
    public class DatabaseContextManager
    {
        public static DatabaseEntities Current
        {
            get
            {
                try
                {
                    string _key = "DATABSE@" + HttpContext.Current.GetHashCode().ToString("x") + Thread.CurrentContext.ContextID.ToString();
                    DatabaseEntities _context = HttpContext.Current.Items[_key] as DatabaseEntities;

                    if (_context == null)
                    {
                        _context = new DatabaseEntities();
                        HttpContext.Current.Items[_key] = _context;
                    }

                    return _context;
                }
                catch
                {
                    return new DatabaseEntities();
                }
            }
        }
    }
}