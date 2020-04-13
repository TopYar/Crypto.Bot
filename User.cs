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
        string waitingReply;
        public HashSet<Pair> favouritePairs = new HashSet<Pair>();
        public bool test = false;

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

        public void AddPair(Pair p)
        {
            favouritePairs.Add(p);
            Save();
        }

        public void RemovePair(Pair p)
        {
            favouritePairs.Remove(p);
            Save();
        }

        public void Save()
        {
            // serialize JSON to a string and then write string to a file
            File.WriteAllText(this.id.ToString() + ".json", JsonConvert.SerializeObject(this));
        }

        public Wallet wallet => GetWallet();
        Wallet GetWallet()
        {
            string jsonstring = null;
            Wallet wallet;
            //async query
            try
            {
                if (!test)
                {
                    api = new ExmoApi(_key, _secret);
                    var task = api.ApiQuery("user_info", new Dictionary<string, string>());
                    jsonstring = task;
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
                    
                    if (string.IsNullOrEmpty(Array.Find(res, x => x == pair.Key.Split('_')[0])))
                    {
                        Array.Resize(ref res, res.Length + 1);
                        res[res.Length - 1] = pair.Key.Split('_')[0];
                    }
                }

            }

            return res;
        }

        public Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup GetCurrenciesInline(string first = "")
        {
            string[] currencies = GetCurrencies(first);
            int row = 5;
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

        public string GetBalance()
        {
            if ((_key == null || _secret == null) && !test)
                return "Вы пока не добавили свой аккаунт. Для добавления, введите /add";

            string res = "";
            try
            {
                Wallet wal = wallet;
                if (wal.balances != null)
                {
                    foreach (KeyValuePair<string, string> Currency in wal.balances)
                    {
                        if (Currency.Value != "0" || wal.reserved[Currency.Key] != "0" || Currency.Key == "BTC" || Currency.Key == "USD")
                            res += (Currency.Key + ": " + Currency.Value) + (wal.reserved[Currency.Key] != "0" ? " (в ордерах: " + wal.reserved[Currency.Key] + ")" : "") + "\n";
                    }
                }
            }
            catch (Exception)
            {
                res = "Something went wrong";
            }
            return res;
        }

        public string GetPairInfo(string pr)
        {
            string res = "";
            api = new ExmoApi(_key, _secret);

            string json = api.ApiQuery("ticker", new Dictionary<string, string>());

            Dictionary<string, Dictionary<string, string>> pairs = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);

            foreach (KeyValuePair<string, Dictionary<string, string>> pair in pairs)
            {
                if (pair.Key == pr)
                    res = "Последняя цена продажи " + pr.Split('_')[0] + ": " + pair.Value["last_trade"] + " " + pr.Split('_')[1] + " 💰\n\n" +
                        "Текущая цена продажи: " + pair.Value["sell_price"] + " " + pr.Split('_')[1] + "\n" +
                        "Текущая цена покупки: " + pair.Value["buy_price"] + " " + pr.Split('_')[1] + "\n";
            }

            return res;
        }

        public Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup GetReplyMarkups()
        {
            dynamic keyboard = null;
            if (this.keyboard == "main")
            {
                if ((_key == null || _secret == null) && !test)
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

                                                },
                                                new[] // row 2
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("📊 Графики"),

                                                },
                                                new[] // row 3
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("🌟 Избранные пары"),
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("🔔 Оповещения"),

                                                },
                                                new[] // row 4
                                                {
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
                                                new[] // row 2
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
            return keyboard;
        }

        public async void SendKeyboard(Telegram.Bot.TelegramBotClient Bot, string type, string customMessage = "")
        {
            Keyboard = type;
            string message = null;

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

            var keyboard = this.GetReplyMarkups();
            await Bot.SendTextMessageAsync(chatId: id, text: message, replyMarkup: keyboard);
        }

    }
}
