using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System.Net;
using Telegram.Bot.Types.InputFiles;
using System.Drawing.Imaging;

namespace Crypto.Bot
{
    public class Program
    {
        static void Main()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Directory.SetCurrentDirectory("../../../res");
            }
            else
            {
                Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + "/res");
            }
            CryptoBot bot = new CryptoBot();

        }
    }
    public class CryptoBot
    {
        BackgroundWorker bw;
        string key = "969989580:AAHsTi5XrDxeUhbKCWDNssCPO9rF2ca04xY";
        bool isBotActive = false;
        Dictionary<long, User> users = new Dictionary<long, User>();
        Dictionary<string, bool> cur = new Dictionary<string, bool>();
        string dir = Directory.GetCurrentDirectory();
        Telegram.Bot.TelegramBotClient Bot;

        public CryptoBot()
        {
            /*DataConnection.DefaultSettings = new MySettings();
            using (var db = new DbCrypto())
            {
                var query = from User in db.User select User.id;
                Console.WriteLine(query.ToList<long>());
            }
            */
            Console.WriteLine(dir);
            DirectoryInfo directoryInfo = new DirectoryInfo(dir);
            foreach (var file in directoryInfo.GetFiles()) //проходим по файлам
            {
                string[] name = file.Name.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                //получаем расширение файла и проверяем подходит ли оно нам 
                if (name[1] == "json")
                {
                    long id = long.Parse(name[0]);

                    User user = JsonConvert.DeserializeObject<User>(File.ReadAllText(file.FullName));
                    Console.WriteLine("Added info " + user.id);
                    users.Add(id, user);
                }

            }

            cur.Add("BTC", false);
            cur.Add("BTG", false);
            cur.Add("ETH", false);
            cur.Add("LTC", false);

            Console.WriteLine("Run");

            bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork; // Метод bw_DoWork будет работать асинхронно

            bw.RunWorkerAsync();
            Console.ReadLine();

        }


        async void bw_DoWork(object sender, DoWorkEventArgs e)
        {

            var worker = sender as BackgroundWorker; // Получаем ссылку на класс вызвавший событие
            Console.WriteLine("Key = {0}", key);

            try
            {
                Bot = new Telegram.Bot.TelegramBotClient(key); // инициализируем API

                await Bot.SetWebhookAsync(""); // Убираем старую привязку к вебхуку для бота
                int offset = 0; // отступ по сообщениям

                isBotActive = true;

                while (true)
                {
                    if (!isBotActive)
                        return;

                    var updates = await Bot.GetUpdatesAsync(offset); // получаем массив обновлений

                    foreach (var update in updates) // Перебираем все обновления
                    {

                        // Callback для кнопок
                        if (update.CallbackQuery != null)
                        {

                            var message = update.CallbackQuery.Message;

                            User user;
                            if (users.ContainsKey(message.Chat.Id))
                            {
                                user = users[message.Chat.Id];
                            }
                            else
                            {
                                user = new User(message.Chat.Id);

                                users.Add(user.id, user);

                                // Отправляем стартовую клавиатуру
                                user.SendKeyboard(Bot, "main");
                            }

                            string[] choose = update.CallbackQuery.Data.Split(':');

                            if (choose[0] == "add1")
                            {
                                try
                                {
                                    await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "You have choosen " + choose[1]);

                                    var keyboard = user.GetCurrenciesInline(choose[1]);

                                    await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId, text: "Выбрано: " + choose[1] + "/...\n" +
                                        "Выберите вторую валюту или вернитесь назад", replyMarkup: keyboard);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Answer on too old query");
                                    Console.WriteLine(ex);
                                }
                            }
                            else if (choose[0] == "add2")
                            {
                                try
                                {
                                    if (choose[1] == "back")
                                    {
                                        var keyboard = user.GetCurrenciesInline();
                                        await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                                        await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId, text: "Выберите первую валюту:", replyMarkup: keyboard);
                                    }
                                    else
                                    {
                                        await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "You have choosen " + choose[2]);
                                        Pair pair = new Pair(choose[1] + "_" + choose[2]);
                                        if (user.favouritePairs.Contains(pair))
                                            await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId, text: "Торговая пара " + choose[1] + "/" + choose[2] +
                                             " уже находится в избранном!");
                                        else
                                        {
                                            user.AddPair(pair);
                                            await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId, text: "Новая торговая пара " + choose[1] + "/" + choose[2] +
                                             " успешно добавлена в избранное!");
                                        }
                                        /*Capture(choose[1] + "_" + choose[2], message.Chat.Id);
                                        await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId, text: user.GetPairInfo(choose[1] + "_" + choose[2]));*/

                                    }
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("Answer for too old query 2");
                                }
                            }
                            else if (choose[0] == "cur")
                            {
                                cur[choose[1]] = !cur[choose[1]]; // Changing to opposite bool

                                var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(
                                                   new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[][]
                                                   {
                                                            // First row
                                                            new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[] {
                                                                // First column
                                                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("BTC " + (cur["BTC"] ? "✅" : "☑️"),  "cur:BTC"),

                                                                // Second column
                                                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("BTG " + (cur["BTG"] ? "✅" : "☑️"),  "cur:BTG"),
                                                            },
                                                            // Second row
                                                            new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[] {
                                                                // First column
                                                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("ETH " + (cur["ETH"] ? "✅" : "☑️"),  "cur:ETH"),

                                                                // Second column
                                                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("LTC " + (cur["LTC"] ? "✅" : "☑️"),  "cur:LTC"),
                                                            },
                                                   }
                                               );
                                try
                                {
                                    await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                                    await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId, text: "Choose cryptocurrency:", replyMarkup: keyboard);
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("Answer for too old query");
                                }
                            }
                            else if (choose[0] == "remove")
                            {
                                user.RemovePair(new Pair(choose[1]));
                                var keyboard = user.RemoveFavouriteCurrenciesInline();

                                try
                                {
                                    await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                                    if (keyboard != null)
                                        await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId, text: "Пара успешно удалена!\n\nВыберите торговую пару для удаления:", replyMarkup: keyboard);
                                    else
                                        await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId, text: "Пара успешно удалена!\n\nУ вас больше нет избранных пар. Добавьте их в соответствующей вкладке!");
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("Answer for too old query");
                                }
                            }
                            if (choose[0] == "graphic")
                            {
                                try
                                {
                                    await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                                    await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId, text: user.GetPairInfo(choose[1]));
                                    Capture(choose[1], message.Chat.Id);
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("Answer for too old query");
                                }
                            }
                        }
                        //Console.WriteLine("Look updates: " + updates.Length);
                        if (update.Type.ToString() == "Message")
                        {
                            var message = update.Message;

                            if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                            {
                                User user;
                                if (users.ContainsKey(message.Chat.Id))
                                {
                                    user = users[message.Chat.Id];
                                }
                                else
                                {
                                    user = new User(message.Chat.Id);

                                    users.Add(user.id, user);

                                    // Отправляем стартовую клавиатуру
                                    //user.SendKeyboard(Bot, "main");
                                }


                                if (message.Text == "/start")
                                {

                                    // Отправляем стартовую клавиатуру
                                    user.SendKeyboard(Bot, "main", "🤖 Привет, я Crypto Trading Бот! \n\nВот, что я умею:\n" +
                                        "• управлять вашим аккаунтом на бирже Exmo\n• выводить график курсов криптовалют\n• оповещать об изменениях цены");
                                }
                                else if (message.Text == "/add" || message.Text == "➕ Добавить аккаунт" || message.Text == "✏️ Заменить ключи")
                                {
                                    user.WaitingReply = "addAccount";
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Введите публичный и секретный ключ в следующем виде: \n\n"
                                        + "K-f5c61f526446102b3c7af11909cfffa72ad8b4e6\nS-5272b9cb568506c205e2e5a056586c116fe5d1e0",
                                           replyToMessageId: message.MessageId);
                                }
                                else if (message.Text == "/test" || message.Text == "⏳ Тестовый аккаунт")
                                {
                                    user.test = true;
                                    user.SendKeyboard(Bot, "main", "Ваш тестовый аккаунт успешно активирован!\n\nДля проверки доступны все функции, кроме покупки и продажи криптовалюты.");
                                }
                                else if (message.Text == "/keys")
                                {
                                    if (user._key != null && user._secret != null)
                                        await Bot.SendTextMessageAsync(message.Chat.Id, "Ваш публичный ключ: \n" + users[message.Chat.Id]._key,
                                               replyToMessageId: message.MessageId);
                                    else
                                        await Bot.SendTextMessageAsync(message.Chat.Id, "Вы пока не добавили свой аккаунт. Для добавления, введите /add",
                                              replyToMessageId: message.MessageId);
                                }
                                else if (message.Text == "/balance" || message.Text == "💰 Баланс")
                                {
                                    await Bot.SendTextMessageAsync(message.Chat.Id, users[message.Chat.Id].GetBalance(),
                                              replyToMessageId: message.MessageId);
                                }
                                else if (message.Text == "/charts" || message.Text == "📊 Графики")
                                {
                                    var keyboard = user.GraphicFavouriteCurrenciesInline();

                                    if (keyboard != null)
                                        await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Выберите торговую пару, график которой вы хотите увидеть:", replyMarkup: keyboard);
                                    else
                                        await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "У вас пока нет избранных пар. Добавьте их в соответствующей вкладке!");

                                }
                                else if (message.Text == "/notifications" || message.Text == "🔔 Оповещения")
                                {
                                    /*
                                    var keyboard = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(
                                                    new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[][]
                                                    {
                                                            // First row
                                                            new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[] {
                                                                // First column
                                                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("BTC " + (cur["BTC"] ? "✅" : "☑️"), "cur:BTC"),

                                                                // Second column
                                                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("BTG " + (cur["BTG"] ? "✅" : "☑️"), "cur:BTG"),
                                                            },
                                                            // Second row
                                                            new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton[] {
                                                                // First column
                                                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("ETH " + (cur["ETH"] ? "✅" : "☑️"), "cur:ETH"),

                                                                // Second column
                                                                Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData("LTC " + (cur["LTC"] ? "✅" : "☑️"), "cur:LTC"),
                                                            },
                                                    }
                                                );
                                    await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Choose cryptocurrency:", replyMarkup: keyboard);*/
                                    await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Данная функция находится в разработке");
                                }
                                else if (message.Text == "/settings" || message.Text == "🛠 Настройки")
                                {
                                    user.SendKeyboard(Bot, "settings");
                                }
                                else if (message.Text == "/favourites" || message.Text == "🌟 Избранные пары")
                                {
                                    user.SendKeyboard(Bot, "favourites");
                                }
                                else if (message.Text == "/add_pair" || message.Text == "➕ Добавить пару")
                                {
                                    var keyboard = user.GetCurrenciesInline();

                                    await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Выберите первую валюту:", replyMarkup: keyboard);
                                }
                                else if (message.Text == "/remove_pair" || message.Text == "❌ Удалить пару")
                                {
                                    var keyboard = user.RemoveFavouriteCurrenciesInline();

                                    if (keyboard != null)
                                        await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Выберите торговую пару для удаления:", replyMarkup: keyboard);
                                    else
                                        await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "У вас пока нет избранных пар. Добавьте их в соответствующей вкладке!");
                                }
                                else if (message.Text == "/remove_acc" || message.Text == "❌ Отвязать аккаунт")
                                {
                                    user._key = null;
                                    user._secret = null;
                                    user.test = false;
                                    user.Save();
                                    user.SendKeyboard(Bot, "main", "Аккаунт успешно отвязан!");
                                }
                                else if (message.Text == "/back" || message.Text == "⬅️ Назад")
                                {
                                    switch (user.Keyboard)
                                    {
                                        case "settings":
                                        case "notifications":
                                        case "charts":
                                        case "favourites":
                                            {
                                                user.SendKeyboard(Bot, "main");
                                                break;
                                            }
                                        default:
                                            {
                                                user.SendKeyboard(Bot, "main");
                                                break;
                                            }
                                    }

                                }
                                else
                                {
                                    // Добавляем аккаунт биржи
                                    try
                                    {
                                        string[] keys = message.Text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (user.WaitingReply == "addAccount")
                                        {
                                            user.WaitingReply = "";
                                            if (keys.Length != 2 || !keys[0].StartsWith("K-") || !keys[1].StartsWith("S-"))
                                            {
                                                throw new ArgumentException("Пожалуйста, введите две строчки в указанном формате\n");
                                            }
                                        }
                                        if (keys.Length == 2 && keys[0].StartsWith("K-") && keys[1].StartsWith("S-"))
                                        {
                                            user._key = keys[0];
                                            user._secret = keys[1];

                                            try
                                            {
                                                string check = user.GetBalance();
                                                if (check == "Error")
                                                    throw new ArgumentException("Похоже вы ввели неверные ключи");
                                                user.Save();

                                                user.SendKeyboard(Bot, "main", "Ваш аккаунт успешно добавлен!");

                                            }
                                            catch (Exception ex)
                                            {
                                                await Bot.SendTextMessageAsync(message.Chat.Id, ex.Message,
                                                          replyToMessageId: message.MessageId);
                                            }


                                        }
                                    }
                                    catch (ArgumentException ex)
                                    {
                                        await Bot.SendTextMessageAsync(message.Chat.Id, ex.Message,
                                               replyToMessageId: message.MessageId);
                                    }
                                }


                            }
                        }
                        offset = update.Id + 1;
                    }
                }
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid format. A valid token looks like \"1234567:4TT8bAc8GHUspu3ERYn - KGcvsvGB9u_n4ddy\".");
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



        async void Capture(string pair, long id, string message = "")
        {
            var mes = await Bot.SendTextMessageAsync(id, message + "Пожалуйста, ожидайте графика...");
            string output = "output.png";
            try
            {
                string[] keys = File.ReadAllLines(dir + "/screenkeys.txt");
                string customerKey;
                string[] key = new string[0];
                int index = 0;
                for (int i = 0; i < keys.Length; i++)
                {
                    key = keys[i].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (key[1] != "0")
                    {
                        index = i;
                        break;
                    }
                }

                customerKey = key[0];
                string secretPhrase = ""; //leave secret phrase empty, if not needed

                var options = new Dictionary<string, string>();
                // mandatory parameter
                //options.Add("url", "https://ru.tradingview.com/symbols/BTCUSD/");
                options.Add("url", "https://exmo.me/ru/trade/" + pair);
                // all next parameters are optional, see our webtite screenshot API guide for more details
                options.Add("dimension", "1020x600"); // or "1366xfull" for full length screenshot
                options.Add("device", "desktop");
                options.Add("format", "png");
                options.Add("cacheLimit", "0");
                options.Add("delay", "2000");
                options.Add("zoom", "100");

                ScreenshotMachine sm = new ScreenshotMachine(customerKey, secretPhrase);

                string apiUrl = sm.GenerateScreenshotApiUrl(options);
                //use final apiUrl where needed
                Console.WriteLine(apiUrl);

                //or save screenshot directly

                await Task.Run(() => new WebClient().DownloadFile(apiUrl, output));
                Image img = Image.FromFile(output);

                output = "output2.png";
                //Bitmap crop = (Bitmap)Crop(img, new Rectangle(30, 540, 976, 530));
                Bitmap crop = (Bitmap)Crop(img, new Rectangle(66, 200, 849, 330));
                crop.Save(output, ImageFormat.Png);
                using (FileStream fs = System.IO.File.OpenRead(output))
                {
                    InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, pair.Split('_')[0] + "/" + pair.Split('_')[1] + ".png");
                    await Bot.DeleteMessageAsync(id, mes.MessageId);
                    await Bot.SendPhotoAsync(id, inputOnlineFile, pair.Split('_')[0] + "/" + pair.Split('_')[1] + " - график торговой пары");

                }

                key[1] = (int.Parse(key[1]) - 1).ToString();
                keys[index] = String.Join(":", key);

                File.WriteAllLines(dir + "/screenkeys.txt", keys);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Screenshot saved as " + output);

        }
        public Image Crop(Image image, Rectangle selection)
        {
            Bitmap bmp = image as Bitmap;

            // Check if it is a bitmap:
            if (bmp == null)
                throw new ArgumentException("No valid bitmap");

            // Crop the image:
            Bitmap cropBmp = bmp.Clone(selection, bmp.PixelFormat);

            // Release the resources:
            image.Dispose();

            return cropBmp;
        }
    }
}

/*using System;

namespace Crypto.Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Form1 form = new Form1();
            form.Start();
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

namespace Telegram.Bot.Examples.WebHook
{
    public static class Bot
    {
        public static readonly TelegramBotClient Api = new TelegramBotClient("969989580:AAHsTi5XrDxeUhbKCWDNssCPO9rF2ca04xY");
    }

    public static class Program
    {
        public static void Main()
        {
            // Endpoint must be configured with netsh:
            // netsh http add urlacl url=https://+:8443/ user=<username>
            // netsh http add sslcert ipport=0.0.0.0:8443 certhash=<cert thumbprint> appid=<random guid>

            using (WebApp.Start<Startup>("https://+:8442"))
            {
                // Register WebHook
                // You should replace {YourHostname} with your Internet accessible hosname
                Bot.Api.SetWebhookAsync("https://89.46.65.56:8442/WebHook").Wait();

                Console.WriteLine("Server Started");

                // Stop Server after <Enter>
                Console.ReadLine();

                // Unregister WebHook
                Bot.Api.DeleteWebhookAsync().Wait();
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var configuration = new HttpConfiguration();

            configuration.Routes.MapHttpRoute("WebHook", "{controller}");

            //app.UseWebApi(configuration);
            app.Use(configuration);
        }
    }

    public class WebHookController : ApiController
    {
        public async Task<IHttpActionResult> Post(Update update)
        {
            var message = update.Message;

            Console.WriteLine("Received Message from {0}", message.Chat.Id);

            if (message.Type == MessageType.Text)
            {
                // Echo each Message
                await Bot.Api.SendTextMessageAsync(message.Chat.Id, message.Text);
            }
            else if (message.Type == MessageType.Photo)
            {
                // Download Photo
                var file = await Bot.Api.GetFileAsync(message.Photo.LastOrDefault()?.FileId);

                var filename = file.FileId + "." + file.FilePath.Split('.').Last();

                using (var saveImageStream = File.Open(filename, FileMode.Create))
                {
                    await Bot.Api.DownloadFileAsync(file.FilePath, saveImageStream);
                }

                await Bot.Api.SendTextMessageAsync(message.Chat.Id, "Thx for the Pics");
            }

            return Ok();
        }
    }
}*/
