using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Bot
{
    class User
    {
        public long id;
        ExmoApi api;
        public string _key;
        public string _secret;
        public string waitingReply;
        public User()
        {

        }

        public User(long id)
        {
            this.id = id;
        }

        public User(long id, string key, string secret)
        {
            this.id = id;
            _key = key;
            _secret = secret;
        }

        public void Save()
        {
            // serialize JSON to a string and then write string to a file
            File.WriteAllText(this.id.ToString() + ".json", JsonConvert.SerializeObject(this));
        }

        public Wallet wallet => GetWallet();
        Wallet GetWallet()
        {
            api = new ExmoApi(_key, _secret);
            Wallet wallet;
            //async query
            try
            {
                var task = api.ApiQuery("user_info", new Dictionary<string, string>());
                string jsonstring = task;

                wallet = JsonConvert.DeserializeObject<Wallet>(jsonstring);
            }
            catch
            {
                wallet = new Wallet();
            }
            return wallet;
        }

        public string GetBalance()
        {
            if (_key == null || _secret == null)
                return "Вы пока не добавили свой аккаунт. Для добавления, введите /add";

            string res = "";
            try
            {
                foreach (KeyValuePair<string, string> Currency in wallet.balances)
                {
                    if (Currency.Value != "0")
                        res += (Currency.Key + ": " + Currency.Value) + "\n";
                }

            }
            catch (Exception ex)
            {
                res = "Error";
            }
            return res;
        }

    }
}
