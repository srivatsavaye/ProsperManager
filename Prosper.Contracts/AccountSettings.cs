using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prosper.Contracts
{
    public class AccountSetting
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AppSettings
    {
        public string BaseUri { get; set; }
        public string ListingsBaseUri { get; set; }
        public double BidAmount { get; set; }
    }
}
