using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Bot
{
    class Wallet
    {
        public string uid { get; set; }
        public int server_date { get; set; }
        public Dictionary<string, string> balances { get; set; }
        public Dictionary<string, string> reserved { get; set; }
    }
}
