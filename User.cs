using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        string waitingReply;
        public HashSet<Pair> favouritePairs = new HashSet<Pair>();
        public Dictionary<string, Notification> Notifications = new Dictionary<string, Notification>();
        public int StartHour = 11;
        public int EndHour = 23;
        public int Interval = 15;
        public bool test = false;
        public bool Test
        {
            get => test;
            set
            {
                test = value;
                Save();
            }
        }

        public string WaitingReply
        {
            get
            {
                return waitingReply;
            }

            set
            {
                waitingReply = value;
                this.Save();
            }
        }
        string keyboard;

        public string Keyboard
        {
            get
            {
                return keyboard;
            }

            set
            {
                keyboard = value;
                this.Save();
            }
        }
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
        public void UpdateNotifications()
        {
            foreach (var notify in Notifications)
            {
                notify.Value.StartHour = StartHour;
                notify.Value.EndHour = EndHour;
                notify.Value.Interval = Interval;
                if (notify.Value.isWorking)
                {
                    notify.Value.Stop();
                    notify.Value.Start();
                }
            }
        }
        public void AddNotification(string p, Action task)
        {
            if (!Notifications.ContainsKey(p))
            {
                Notifications.Add(p, new Notification(StartHour, EndHour, Interval, task));
                Save();
            }
        }

        public void StopNotification(string p)
        {
            if (Notifications.ContainsKey(p))
            {
                Notifications[p].Stop();
                Save();
            }
        }

        public void StartNotification(string p)
        {
            if (Notifications.ContainsKey(p))
            {
                Notifications[p].UpdateParams(StartHour, EndHour, Interval);
                Notifications[p].Start();
                Save();
            }
        }

        public void RemoveNotification(string p)
        {
            if (Notifications.ContainsKey(p))
            {
                Notifications[p].Stop();
                Notifications.Remove(p);
                Save();
            }
        }

        public void AddPair(Pair p)
        {
            favouritePairs.Add(p);
            Save();
        }

        public void RemovePair(Pair p)
        {
            if (favouritePairs.Contains(p))
            {
                RemoveNotification(p.pair);
                favouritePairs.Remove(p);
            }
            Save();
        }

        public void Save()
        {
            try
            {
                // serialize JSON to a string and then write string to a file
                File.WriteAllText(this.id.ToString() + ".json", JsonConvert.SerializeObject(this));
            }
            catch
            {
                Console.WriteLine("Error while writing in file. Some data can be lost");
            }
        }

        public Wallet wallet => GetWallet();
        Wallet GetWallet()
        {
            string jsonstring = null;
            Wallet wallet;
            //async query
            try
            {
                if (!Test)
                {
                    api = new ExmoApi(_key, _secret);
                    var task = api.ApiQuery("user_info", new Dictionary<string, string>());
                    jsonstring = task;
                    if (task.Contains("error"))
                        return null;
                }
                else
                {
                    jsonstring = "{\"uid\":120345,\"server_date\":1586796475,\"balances\":{\"EXM\":\"6653.4664\",\"USD\":\"54.36048001\",\"EUR\":\"0\",\"RUB\":\"0.53531085\",\"PLN\":\"0\",\"TRY\":\"0\",\"UAH\":\"0\",\"KZT\":\"0\",\"BTC\":\"0\",\"LTC\":\"0\",\"DOGE\":\"0\",\"DASH\":\"0\",\"ETH\":\"0\",\"WAVES\":\"0\",\"ZEC\":\"0\",\"USDT\":\"0\",\"XMR\":\"0\",\"XRP\":\"52.92751461\",\"KICK\":\"0\",\"ETC\":\"0.15592631\",\"BCH\":\"0\",\"BTG\":\"4.74858518\",\"EOS\":\"0\",\"BTCZ\":\"0\",\"DXT\":\"0\",\"XLM\":\"0\",\"MNX\":\"0\",\"OMG\":\"0\",\"TRX\":\"0\",\"ADA\":\"0\",\"INK\":\"0\",\"NEO\":\"0\",\"GAS\":\"0\",\"ZRX\":\"0\",\"GNT\":\"0\",\"GUSD\":\"0\",\"LSK\":\"0\",\"XEM\":\"0\",\"SMART\":\"0\",\"QTUM\":\"0\",\"HB\":\"0\",\"DAI\":\"0\",\"MKR\":\"0\",\"MNC\":\"0\",\"PTI\":\"0\",\"ATMCASH\":\"0\",\"ETZ\":\"0\",\"USDC\":\"0\",\"ROOBEE\":\"0\",\"DCR\":\"0\",\"XTZ\":\"0\",\"ZAG\":\"0\",\"BTT\":\"0\",\"VLX\":\"0\",\"HP\":\"0\",\"CRON\":\"0\",\"ONT\":\"0\",\"ONG\":\"0\",\"ALGO\":\"0\"},\"reserved\":{\"EXM\":\"0\",\"USD\":\"0\",\"EUR\":\"0\",\"RUB\":\"0\",\"PLN\":\"0\",\"TRY\":\"0\",\"UAH\":\"0\",\"KZT\":\"0\",\"BTC\":\"0\",\"LTC\":\"0\",\"DOGE\":\"0\",\"DASH\":\"0\",\"ETH\":\"0\",\"WAVES\":\"0\",\"ZEC\":\"0\",\"USDT\":\"0\",\"XMR\":\"0\",\"XRP\":\"0\",\"KICK\":\"0\",\"ETC\":\"0\",\"BCH\":\"0\",\"BTG\":\"12.15655334\",\"EOS\":\"0\",\"BTCZ\":\"0\",\"DXT\":\"0\",\"XLM\":\"0\",\"MNX\":\"0\",\"OMG\":\"0\",\"TRX\":\"0\",\"ADA\":\"0\",\"INK\":\"0\",\"NEO\":\"0\",\"GAS\":\"0\",\"ZRX\":\"0\",\"GNT\":\"0\",\"GUSD\":\"0\",\"LSK\":\"0\",\"XEM\":\"0\",\"SMART\":\"0\",\"QTUM\":\"0\",\"HB\":\"0\",\"DAI\":\"0\",\"MKR\":\"0\",\"MNC\":\"0\",\"PTI\":\"0\",\"ATMCASH\":\"0\",\"ETZ\":\"0\",\"USDC\":\"0\",\"ROOBEE\":\"0\",\"DCR\":\"0\",\"XTZ\":\"0\",\"ZAG\":\"0\",\"BTT\":\"0\",\"VLX\":\"0\",\"HP\":\"0\",\"CRON\":\"0\",\"ONT\":\"0\",\"ONG\":\"0\",\"ALGO\":\"0\"}}";
                }

                wallet = JsonConvert.DeserializeObject<Wallet>(jsonstring);
            }
            catch
            {
                wallet = new Wallet();
            }
            return wallet;
        }
        public string[] GetCurrencies(string first = "")
        {
            string[] res = new string[0];
            api = new ExmoApi(_key, _secret);

            string json = api.ApiQuery("ticker", new Dictionary<string, string>());

            Dictionary<string, Dictionary<string, string>> pairs = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);

            foreach (KeyValuePair<string, Dictionary<string, string>> pair in pairs)
            {

                if (first != "")
                {
                    if (pair.Key.Split('_')[0] == first)
                    {
                        if (string.IsNullOrEmpty(Array.Find(res, x => x == pair.Key.Split('_')[1])))
                        {
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = pair.Key.Split('_')[1];
                        }
                    }
                }
                else
                {
                    string[] blacklist = "PTI,ONT,QTUM,LSK,ETZ,XTZ,DAI,BTCZ,ADA,DXT,MKR,MNC,GUSD,MNX,ZRX,ZAG,BTT,ONG,VLX,HB,GNT,INK,USDC".Split(',');
                    if (string.IsNullOrEmpty(Array.Find(res, x => x == pair.Key.Split('_')[0])) && !(blacklist.Contains(pair.Key.Split('_')[0])))
                    {
                        Array.Resize(ref res, res.Length + 1);
                        res[res.Length - 1] = pair.Key.Split('_')[0];
                    }
                }

            }

            return res;
        }
        public string SendOrder(string pair, string type, string price, string quantity)
        {
            Dictionary<string, string>[] res = new Dictionary<string, string>[0];
            api = new ExmoApi(_key, _secret);

            string json = api.ApiQuery("order_create", new Dictionary<string, string>() { { "pair", pair }, { "type", type }, { "price", price }, { "quantity", quantity } });

            Dictionary<string, string> ans = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (ans["result"] != "true")
            {
                return ans["error"];
            }
            return "";
        }
        public string DeleteOpenOrder(string order_id)
        {
            Dictionary<string, string>[] res = new Dictionary<string, string>[0];
            api = new ExmoApi(_key, _secret);

            string json = api.ApiQuery("order_cancel", new Dictionary<string, string>() { { "order_id", order_id } });

            Dictionary<string, string> ans = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (ans["result"] != "true")
            {
                return ans["error"];
            }

            return "";
        }
        public Dictionary<string, string>[] GetUserOpenOrders(string needPair = "")
        {
            
            Dictionary<string, string>[] res = new Dictionary<string, string>[0];
            bool isOpenOrders = false;
            api = new ExmoApi(_key, _secret);

            string json = api.ApiQuery("user_open_orders", new Dictionary<string, string>());
            try
            {
                Dictionary<string, Dictionary<string, string>[]> pairs = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>[]>>(json);
                
                if (pairs.Count == 0) return null;
                foreach (KeyValuePair<string, Dictionary<string, string>[]> pair in pairs)
                {
                    if (pair.Key == needPair)
                    {
                        isOpenOrders = true;
                        return pair.Value;
                        /*
                        foreach (var orders in pair.Value)
                        {
                            foreach (var order in orders)
                            {
                                Console.WriteLine(order.Key + " " + order.Value);
                            }
                        }*/
                    }
                }
            }
            catch (Exception)
            {
                res = null;
            }
            if (!isOpenOrders) return null;
            return res;
        }
        public Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup GetDeleteOpenOrdersInline(string pair)
        {
            var orders = GetUserOpenOrders(pair);
            string add = "delorder:" + pair + ":";

            if (!(orders is null))
            {
                var buttons = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[orders.Length][];
                for (int i = 0; i < orders.Length; i++)
                {
                    Dictionary<string, string> order = orders[i];
                    buttons[i] = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[1];
                    string type = order["type"] == "sell" ? "🔹 Продажа" : "🔸 Покупка";

                    string text = $"{type} {pair.Split('_')[0]}. Цена: {double.Parse(order["price"], CultureInfo.InvariantCulture):F3} {pair.Split('_')[1]}, количество: {double.Parse(order["quantity"], CultureInfo.InvariantCulture):F3} {pair.Split('_')[0]} ❌\n";
                    buttons[i][0] = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(text, add + order["order_id"]);
                    
                    
                }
                var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(buttons);
                return keyboard;
            }
            return null;
        }

        public Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup GetCurrenciesInline(string first = "")
        {
            string[] currencies = GetCurrencies(first);
            int row = 4;
            string add = (first == "") ? "add1:" : "add2:" + first + ":";
            var buttons = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[(currencies.Length + row - 1) / row][];


            for (int i = 0; i < buttons.Length; i++)
            {
                if (currencies.Length % row != 0 && i == buttons.Length - 1)
                {
                    buttons[i] = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[currencies.Length - row * i];

                    for (int j = 0; j < currencies.Length - row * i; j++)
                        buttons[i][j] = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(currencies[row * i + j], add + currencies[row * i + j]);
                }
                else
                {
                    buttons[i] = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[row];

                    for (int j = 0; j < row; j++)
                        buttons[i][j] = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(currencies[row * i + j], add + currencies[row * i + j]);

                }
            }

            if (first != "")
            {
                Array.Resize(ref buttons, buttons.Length + 1);
                buttons[buttons.Length - 1] = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[] {
                            // First column
                            Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("⬅️ Назад", "add2:back"),
                };
            }

            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(buttons);

            return keyboard;
        }
        public static string GetIntervalString(int interval)
        {
            string textInterval = "";
            switch (interval)
            {
                case 1: textInterval = "1 минута"; break;
                case 2: textInterval = " 2 минуты"; break;
                case 5: textInterval = "5 минут"; break;
                case 15: textInterval = "15 минут"; break;
                case 30: textInterval = "30 минут"; break;
                case 60: textInterval = "60 минут"; break;
                case 90: textInterval = "1,5 часа"; break;
                case 120: textInterval = "2 часа"; break;
                case 240: textInterval = "4 часа"; break;
                case 360: textInterval = "6 часов"; break;
                case 720: textInterval = "12 часов"; break;
                case 1440: textInterval = "1 сутки"; break;
            }
            return textInterval;
        }
        public Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup ConfirmationOrderInline(string callback)
        {
            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(

                    new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[][]
                    {
                        new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[] // row 1
                        {
                            Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Да", callback + ":yes"),
                            Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Нет", callback + ":no"),
                        },
                    }
                    );
            return keyboard;
        }
        public Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup NotificationIntervalInline()
        {
            int from = this.StartHour, by = this.EndHour, interval = this.Interval;
            if (from > by)
            {
                int t = from;
                from = by;
                by = t;
            }
            int left, right;
            string textInterval = "минут";

            switch (interval)
            {
                case 1: left = 1; right = 2; textInterval = "1 минута"; break;
                case 2: left = 1; right = 5; textInterval = " 2 минуты"; break;
                case 5: left = 2; right = 15; textInterval = "5 минут"; break;
                case 15: left = 5; right = 30; textInterval = "15 минут"; break;
                case 30: left = 15; right = 60; textInterval = "30 минут"; break;
                case 60: left = 30; right = 90; textInterval = "60 минут"; break;
                case 90: left = 60; right = 120; textInterval = "1,5 часа"; break;
                case 120: left = 90; right = 240; textInterval = "2 часа"; break;
                case 240: left = 120; right = 360; textInterval = "4 часа"; break;
                case 360: left = 240; right = 720; textInterval = "6 часов"; break;
                case 720: left = 360; right = 1440; textInterval = "12 часов"; break;
                case 1440: left = 720; right = 1440; textInterval = "1 сутки"; break;
                default: interval = 60; left = 30; right = 90; break;
            }
            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(

                    new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[][]
                    {
                        new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[] // row 1
                        {
                            Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Начиная с: " + from + ":00","interval:noclick"),

                        },
                        new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[] // row 2
                            {
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("(-3) <<","interval:from:" + ((from - 3 + 24) % 24)),
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("(-1) <","interval:from:" + ((from - 1 + 24) % 24)),
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("> (+1)","interval:from:" + ((from + 1 + 24) % 24)),
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(">> (+3)","interval:from:" + ((from + 3 + 24) % 24)),
                            },
                        new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[] // row 3
                            {
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("Заканчивая в: " + by + ":00","interval:noclick"),
                            },
                        new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[] // row 4
                            {
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("(-3) <<","interval:by:" + ((by - 3 + 24) % 24)),
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("(-1) <","interval:by:" + ((by - 1 + 24) % 24)),
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("> (+1)","interval:by:" + ((by + 1 + 24) % 24)),
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(">> (+3)","interval:by:" + ((by + 3 + 24) % 24)),
                            },
                        new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[] // row 5
                            {
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("С интервалом:","interval:noclick"),
                            },
                        new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[] // row 6
                            {
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("<","interval:interval:" + left),
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(textInterval,"interval:interval:" + interval),
                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(">","interval:interval:" + right),
                            },
                    }
            );

            return keyboard;
        }

        public Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup NotificationFavouriteCurrenciesInline()
        {
            Pair[] arrPairs = favouritePairs.ToArray<Pair>();
            string[] pairs = Array.ConvertAll<Pair, string>(arrPairs, x => x.pair);
            if (pairs.Length == 0)
                return null;

            int row = 2;
            string add = "notification:";
            var buttons = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[(pairs.Length + row - 1) / row][];


            for (int i = 0; i < buttons.Length; i++)
            {
                if (pairs.Length % row != 0 && i == buttons.Length - 1)
                {
                    buttons[i] = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[pairs.Length - row * i];

                    for (int j = 0; j < pairs.Length - row * i; j++)
                    {
                        string pair = pairs[row * i + j];
                        buttons[i][j] = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(pair.Split('_')[0] + "/" + pair.Split('_')[1] + " " + (Notifications[pair].isWorking ? "✅" : "☑️"), add + pair);
                    }
                }
                else
                {
                    buttons[i] = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[row];

                    for (int j = 0; j < row; j++)
                    {
                        string pair = pairs[row * i + j];
                        buttons[i][j] = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(pair.Split('_')[0] + "/" + pair.Split('_')[1] + " " + (Notifications[pair].isWorking ? "✅" : "☑️"), add + pair);
                    }

                }
            }

            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(buttons);

            return keyboard;
        }

        public Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup RemoveFavouriteCurrenciesInline()
        {
            Pair[] arrPairs = favouritePairs.ToArray<Pair>();
            string[] pairs = Array.ConvertAll<Pair, string>(arrPairs, x => x.pair);
            if (pairs.Length == 0)
                return null;

            int row = 3;
            string add = "remove:";
            var buttons = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[(pairs.Length + row - 1) / row][];


            for (int i = 0; i < buttons.Length; i++)
            {
                if (pairs.Length % row != 0 && i == buttons.Length - 1)
                {
                    buttons[i] = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[pairs.Length - row * i];

                    for (int j = 0; j < pairs.Length - row * i; j++)
                    {
                        string pair = pairs[row * i + j];
                        buttons[i][j] = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(pair.Split('_')[0] + "/" + pair.Split('_')[1], add + pair);
                    }
                }
                else
                {
                    buttons[i] = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[row];

                    for (int j = 0; j < row; j++)
                    {
                        string pair = pairs[row * i + j];
                        buttons[i][j] = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(pair.Split('_')[0] + "/" + pair.Split('_')[1], add + pair);
                    }

                }
            }

            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(buttons);

            return keyboard;
        }
        public Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup TradingFavouriteCurrenciesInline()
        {
            Pair[] arrPairs = favouritePairs.ToArray<Pair>();
            string[] pairs = Array.ConvertAll<Pair, string>(arrPairs, x => x.pair);
            if (pairs.Length == 0)
                return null;

            int row = 3;
            string add = "trading:";
            var buttons = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[(pairs.Length + row - 1) / row][];


            for (int i = 0; i < buttons.Length; i++)
            {
                if (pairs.Length % row != 0 && i == buttons.Length - 1)
                {
                    buttons[i] = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[pairs.Length - row * i];

                    for (int j = 0; j < pairs.Length - row * i; j++)
                    {
                        string pair = pairs[row * i + j];
                        buttons[i][j] = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(pair.Split('_')[0] + "/" + pair.Split('_')[1], add + pair);
                    }
                }
                else
                {
                    buttons[i] = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[row];

                    for (int j = 0; j < row; j++)
                    {
                        string pair = pairs[row * i + j];
                        buttons[i][j] = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(pair.Split('_')[0] + "/" + pair.Split('_')[1], add + pair);
                    }

                }
            }

            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(buttons);

            return keyboard;
        }

        public Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup GraphicFavouriteCurrenciesInline()
        {
            Pair[] arrPairs = favouritePairs.ToArray<Pair>();
            string[] pairs = Array.ConvertAll<Pair, string>(arrPairs, x => x.pair);
            if (pairs.Length == 0)
                return null;

            int row = 3;
            string add = "graphic:";
            var buttons = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[(pairs.Length + row - 1) / row][];


            for (int i = 0; i < buttons.Length; i++)
            {
                if (pairs.Length % row != 0 && i == buttons.Length - 1)
                {
                    buttons[i] = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[pairs.Length - row * i];

                    for (int j = 0; j < pairs.Length - row * i; j++)
                    {
                        string pair = pairs[row * i + j];
                        buttons[i][j] = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(pair.Split('_')[0] + "/" + pair.Split('_')[1], add + pair);
                    }
                }
                else
                {
                    buttons[i] = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[row];

                    for (int j = 0; j < row; j++)
                    {
                        string pair = pairs[row * i + j];
                        buttons[i][j] = Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(pair.Split('_')[0] + "/" + pair.Split('_')[1], add + pair);
                    }

                }
            }

            var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(buttons);

            return keyboard;
        }

        public string GetBalance(string pair = "")
        {
            if ((_key == null || _secret == null) && !Test)
                return "Вы пока не добавили свой аккаунт. Для добавления, введите /add";

            string res = "";
            try
            {
                Wallet wal = wallet;
                if (wal.balances != null)
                {
                    foreach (KeyValuePair<string, string> Currency in wal.balances)
                    {
                        if (pair == "")
                        {
                            if (Currency.Value != "0" || wal.reserved[Currency.Key] != "0" || Currency.Key == "BTC" || Currency.Key == "USD")
                                res += (Currency.Key + ": " + Currency.Value) + (wal.reserved[Currency.Key] != "0" ? " (в ордерах: " + wal.reserved[Currency.Key] + ")" : "") + "\n";
                        }
                        else
                        {
                            if (Currency.Key == pair.Split('_')[0] || Currency.Key == pair.Split('_')[1])
                            {
                                res += (Currency.Key + ": " + Currency.Value) + "\n";
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                res = "Error";
            }
            return res;
        }

        public string GetPairInfo(string pr)
        {
            string res = "";
            int flag = 0;
            api = new ExmoApi(_key, _secret);

            string json = api.ApiQuery("ticker", new Dictionary<string, string>());

            Dictionary<string, Dictionary<string, string>> pairs = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);

            foreach (KeyValuePair<string, Dictionary<string, string>> pair in pairs)
            {
                if (pair.Key == pr)
                {
                    flag = 1;
                    res = "Последняя цена продажи " + pr.Split('_')[0] + ": " + pair.Value["last_trade"] + " " + pr.Split('_')[1] + " 💰\n\n" +
                        "Текущая цена продажи: " + pair.Value["sell_price"] + " " + pr.Split('_')[1] + "\n" +
                        "Текущая цена покупки: " + pair.Value["buy_price"] + " " + pr.Split('_')[1] + "\n";
                }
            }

            if (flag == 0)
            {
                res = "К сожалению, данная торговая пара теперь не торгуется на бирже. Она также автоматически удалена из ваших избранных пар";
                RemovePair(new Pair(pr));
            }

            return res;
        }
        public Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup GetReplyMarkups(string pair = "")
        {
            dynamic keyboard = null;
            if (this.keyboard == "main")
            {
                if ((_key == null || _secret == null) && !Test)
                    keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                    {
                        Keyboard = new[] {
                                                new[] // row 1
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("➕ Добавить аккаунт"),

                                                },
                                                new[] // row 2
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("⏳ Тестовый аккаунт"),

                                                },
                                            },
                        ResizeKeyboard = true
                    };
                else
                {
                    keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                    {
                        Keyboard = new[] {
                                                new[] // row 1
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("💰 Баланс"),
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("💵 Торговля"),

                                                },
                                                new[] // row 2
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("📊 Графики"),
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("🔔 Оповещения"),

                                                },
                                                new[] // row 3
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("🌟 Избранные пары"),

                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("🛠 Настройки"),

                                                },
                                            },
                        ResizeKeyboard = true
                    };
                }
            }
            else if (this.keyboard == "settings")
            {
                keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                {
                    Keyboard = new[] {
                                                new[] // row 1
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("✏️ Заменить ключи"),
                                                },
                                                new[] // row 2
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("❌ Отвязать аккаунт"),
                                                },
                                                new[] // row 3
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("⬅️ Назад"),

                                                },
                                            },
                    ResizeKeyboard = true
                };
            }
            else if (this.keyboard == "notification")
            {
                keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                {
                    Keyboard = new[] {
                                                new[] // row 1
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("❓ Включить/выключить оповещения"),
                                                },
                                                new[] // row 2
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("⏱ Изменить периодичность"),

                                                },
                                                new[] // row 3
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("⬅️ Назад"),

                                                },
                                            },
                    ResizeKeyboard = true
                };
            }
            else if (this.keyboard == "favourites")
            {
                keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                {
                    Keyboard = new[] {
                                                new[] // row 2
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("➕ Добавить пару"),
                                                },
                                                new[] // row 2
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("❌ Удалить пару"),
                                                },
                                                new[] // row 3
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("⬅️ Назад"),

                                                },
                                            },
                    ResizeKeyboard = true
                };
            }
            else if (this.keyboard == "trading")
            {
                keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                {
                    Keyboard = new[] {
                                                new[] // row 1
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("🔸 Купить " + pair),
                                                },
                                                new[] // row 2
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("🔹 Продать " + pair),
                                                },
                                                new[] // row 3
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("❌ Удалить заявку " + pair),

                                                },
                                                new[] // row 4
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("⬅️ Назад"),

                                                },
                                            },
                    ResizeKeyboard = true
                };
            }
            return keyboard;
        }

        public async void SendKeyboard(Telegram.Bot.TelegramBotClient Bot, string type, string customMessage = "", string pair = "")
        {
            Keyboard = type;
            string message = null;
            var keyboard = this.GetReplyMarkups();

            if (customMessage != "")
            {
                message = customMessage;
            }
            else if (type == "main")
            {
                message = "🤖 Привет, я Crypto Trading Бот. Рад вас видеть!";
            }
            else if (type == "settings")
            {
                message = "Здесь вы можете изменить секретные ключи или отвязать свой аккаунт";
            }
            else if (type == "favourites")
            {
                message = "Здесь вы можете добавить торговые пары в избранное (или удалить их)";
            }
            else if (type == "notification")
            {
                message = "Здесь вы можете настроить оповещения для избранных торговых пар";
            }
            else if (type == "trading")
            {
                message = "Вы находитесь в режиме торговли для пары " + pair;
            }

            if (type == "trading")
            {
                keyboard = GetReplyMarkups(pair);
            }


            await Bot.SendTextMessageAsync(chatId: id, text: message, replyMarkup: keyboard);
        }

    }
}
