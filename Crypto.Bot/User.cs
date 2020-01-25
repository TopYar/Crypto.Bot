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
                Wallet wal = wallet;
                foreach (KeyValuePair<string, string> Currency in wal.balances)
                {
                    if (Currency.Value != "0" || wal.reserved[Currency.Key] != "0" || Currency.Key == "BTC" || Currency.Key == "USD")
                        res += (Currency.Key + ": " + Currency.Value) + (wal.reserved[Currency.Key] != "0" ? " (в ордерах: " + wal.reserved[Currency.Key] + ")" : "") + "\n";
                }

            }
            catch (Exception ex)
            {
                res = "Error";
            }
            return res;
        }

        public Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup GetReplyMarkups()
        {
            dynamic keyboard = null;
            if (this.keyboard == "main")
            {
                if (_key == null || _secret == null)
                    keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                    {
                        Keyboard = new[] {
                                                new[] // row 1
                                                {
                                                    new Telegram.Bot.Types.ReplyMarkups.KeyboardButton("➕ Добавить аккаунт"),

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
            else if (type == "settings") {
                message = "Здесь вы можете изменить секретные ключи или отвязать свой аккаунт.";
            }

            var keyboard = this.GetReplyMarkups();
            await Bot.SendTextMessageAsync(chatId: id, text: message, replyMarkup: keyboard);
        }

    }
}
