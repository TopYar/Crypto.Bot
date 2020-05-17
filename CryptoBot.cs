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
using Telegram.Bot.Types;
using ZXing.QrCode;
using ZXing;
using ZXing.Common;
using System.Globalization;

namespace Crypto.Bot
{
    public class CryptoBot
    {
        BackgroundWorker bw;
        string key;
        bool isBotActive = false;
        Dictionary<long, User> users = new Dictionary<long, User>();
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

            DirectoryInfo directoryInfo = new DirectoryInfo(dir);
            try
            {
                key = System.IO.File.ReadAllText("config.txt");
                foreach (var file in directoryInfo.GetFiles()) //проходим по файлам
                {
                    string[] name = file.Name.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    //получаем расширение файла и проверяем подходит ли оно нам 
                    if (name[1] == "json")
                    {
                        long id = long.Parse(name[0]);

                        User user = JsonConvert.DeserializeObject<User>(System.IO.File.ReadAllText(file.FullName));
                        //Console.WriteLine("Added info " + user.id);
                        users.Add(id, user);
                    }

                }
            }
            catch (IOException)
            {
                Console.WriteLine("Ошибка чтения файла");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }



            bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork; // Метод bw_DoWork будет работать асинхронно

            bw.RunWorkerAsync();
            Console.ReadLine();

        }


        async void bw_DoWork(object sender, DoWorkEventArgs e)
        {

            var worker = sender as BackgroundWorker; // Получаем ссылку на класс вызвавший событие

            tryagain:

            try
            {
                Bot = new Telegram.Bot.TelegramBotClient(key); // инициализируем API

                await Bot.SetWebhookAsync(""); // Убираем старую привязку к вебхуку для бота
                int offset = 0; // отступ по сообщениям
                Console.WriteLine("Successful Run");
                Console.WriteLine("Key = {0}", key);
                isBotActive = true;

                foreach (var u in users)
                {
                    var user = u.Value;
                    foreach (var n in user.Notifications)
                    {
                        var notification = n.Value;
                        var pair = n.Key;
                        notification.UpdateParams(user.StartHour, user.EndHour, user.Interval);
                        notification.SetTask(async () =>
                        {
                            await Bot.SendTextMessageAsync(chatId: user.id, text: user.GetPairInfo(pair));
                            //Capture("BTC_USD", message.Chat.Id);
                        });
                    }
                }

                while (true)
                {
                    if (!isBotActive)
                        return;

                    var updates = await Bot.GetUpdatesAsync(offset); // получаем массив обновлений

                    foreach (var update in updates) // Перебираем все обновления
                    {
                        try
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
                                        await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Вы выбрали " + choose[1]);

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
                                            await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Вы выбрали " + choose[2]);
                                            Pair pair = new Pair(choose[1] + "_" + choose[2]);
                                            if (user.favouritePairs.Contains(pair))
                                                await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId, text: "Торговая пара " + choose[1] + "/" + choose[2] +
                                                 " уже находится в избранном!");
                                            else
                                            {
                                                user.AddPair(pair);

                                                // AddNotify

                                                user.AddNotification(pair.pair,
                                                    async () =>
                                                    {
                                                        await Bot.SendTextMessageAsync(chatId: user.id, text: user.GetPairInfo(pair.pair));
                                                        //Capture("BTC_USD", message.Chat.Id);
                                                    });


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
                                else if (choose[0] == "notification")
                                {
                                    try
                                    {

                                        user.Notifications[choose[1]].isWorking = !user.Notifications[choose[1]].isWorking; // Changing to opposite bool
                                        if (!user.Notifications[choose[1]].isWorking)
                                        {
                                            await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Оповещения выключены для пары " + choose[1].Split('_')[0] + "/" + choose[1].Split('_')[1]);
                                            user.StopNotification(choose[1]);
                                        }
                                        else
                                        {
                                            await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Оповещения включены для пары " + choose[1].Split('_')[0] + "/" + choose[1].Split('_')[1]);
                                            user.StartNotification(choose[1]);
                                        }

                                        var keyboard = user.NotificationFavouriteCurrenciesInline();

                                        await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                                            text: "Выберите включение/выключение оповещений для следующих торговых пар:", replyMarkup: keyboard);

                                    }
                                    catch (Exception)
                                    {
                                        await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                                            text: "Данное сообщение устарело! Попробуйте заново открыть вкладку с оповещениями");
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
                                else if (choose[0] == "graphic")
                                {
                                    try
                                    {
                                        await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                                        string info = user.GetPairInfo(choose[1]);
                                        await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId, text: info);
                                        if (!info.StartsWith("К сожалению"))
                                            Capture(choose[1], message.Chat.Id);
                                    }
                                    catch (Exception)
                                    {
                                        Console.WriteLine("Answer for too old query");
                                    }
                                }
                                else if (choose[0] == "interval")
                                {
                                    await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
                                    int sh = user.StartHour, eh = user.EndHour, intr = user.Interval;
                                    if (choose[1] == "noclick") { }
                                    else if (choose[1] == "from")
                                    {
                                        user.StartHour = int.Parse(choose[2]);
                                    }
                                    else if (choose[1] == "by")
                                    {
                                        user.EndHour = int.Parse(choose[2]);
                                    }
                                    else if (choose[1] == "interval")
                                    {
                                        user.Interval = int.Parse(choose[2]);
                                    }
                                    if (user.StartHour > user.EndHour && user.StartHour * user.EndHour != 0)
                                    {
                                        int t = user.StartHour;
                                        user.StartHour = user.EndHour;
                                        user.EndHour = t;
                                    }
                                    if (sh != user.StartHour || eh != user.EndHour || intr != user.Interval)
                                    {
                                        user.UpdateNotifications();
                                        user.Save();
                                        var keyboard = user.NotificationIntervalInline();
                                        await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                                            text: String.Format($"Текущая периодичность: \n• интервал: {User.GetIntervalString(user.Interval)}, начиная с {user.StartHour}:00 до {user.EndHour}:00\n\nИзменить:"), replyMarkup: keyboard);
                                    }

                                }
                                else if (choose[0] == "trading")
                                {
                                    try
                                    {
                                        await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id);

                                        string pair = choose[1];
                                        string text = "";

                                        var orders = user.GetUserOpenOrders(pair);
                                        if (orders != null)
                                            foreach (var order in orders)
                                            {
                                                string type = order["type"] == "sell" ? "🔹 Продажа" : "🔸 Покупка";
                                                text += $"{type} {pair.Split('_')[0]}. Цена: {order["price"]:F5} {pair.Split('_')[1]}, количество: {order["quantity"]:F7} {pair.Split('_')[0]}\n";
                                            }


                                        await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                                            text: String.Format($"Для торговли выбрана пара {String.Join('/', choose[1].Split("_"))}"));
                                        user.SendKeyboard(Bot, "trading", text == "" ? text : $"Список активных заявок для пары {String.Join('/', choose[1].Split("_"))}:\n\n{text}",
                                            pair: String.Join('/', choose[1].Split("_")));
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                    }
                                }
                                else if (choose[0] == "exmo")
                                {
                                    try
                                    {
                                        await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id);

                                        string pair = choose[1]; // Торговая пара
                                        string type = choose[2]; // Тип заявки
                                        string price = choose[3]; // Цена
                                        string amount = choose[4]; // Количество
                                        string confirmation = choose[5]; // Подтверждение (да/нет)

                                        if (confirmation == "yes")
                                        {
                                            // Отправка заявки на биржу и получение результата
                                            string res = user.SendOrder(pair, type, price, amount);
                                            if (res == "")
                                            {
                                                await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                                                text: String.Format($"Заявка на " + (type == "buy" ? "покупку" : "продажу") + $" {amount} {pair.Split('_')[0]} по цене {price} {pair.Split('_')[1]} принята в обработку!"));

                                                string text = "";

                                                var orders = user.GetUserOpenOrders(pair);
                                                foreach (var order in orders)
                                                {
                                                    text += (order["type"] == "sell" ? "🔹 Продажа" : "🔸 Покупка") + $" {pair.Split('_')[0]}. Цена: {order["price"]:F5} {pair.Split('_')[1]}, количество: {order["quantity"]:F7} {pair.Split('_')[0]}\n";
                                                }
                                                user.SendKeyboard(Bot, "trading", text == "" ? text : $"Список активных заявок для пары {String.Join('/', pair.Split("_"))}:\n\n{text}",
                                                                    pair: String.Join('/', pair.Split("_")));
                                            }
                                            else
                                            {
                                                string text = "Ошибка: ";
                                                switch (res.Split(' ')[1])
                                                {
                                                    case "50052:":
                                                    case "50054:": text += "недостаток средств"; break;
                                                    case "50319:": text += "цена по заявке меньше допустимого минимума для данной пары"; break;
                                                    default: text += res; break;
                                                }
                                                await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                                                text: String.Format(text));
                                            }
                                        }
                                        else
                                            await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId,
                                            text: String.Format($"Заявка на " + (type == "buy" ? "покупку" : "продажу") + $" {amount} {pair.Split('_')[0]} по цене {price} {pair.Split('_')[1]} была отменена!"));

                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                                else if (choose[0] == "delorder")
                                {
                                    try
                                    {
                                        await Bot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Заявка " + String.Join('/', choose[1].Split("_")) + " успешно удалена!");

                                        string pair = choose[1];
                                        string order_id = choose[2];
                                        user.DeleteOpenOrder(order_id);

                                        var keyboard = user.GetDeleteOpenOrdersInline(pair);

                                        if (!(keyboard is null))
                                        {
                                            await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId, text: "Выберете активную заявку для удаления:", replyMarkup: keyboard);
                                        }
                                        else
                                        {
                                            await Bot.EditMessageTextAsync(chatId: message.Chat.Id, messageId: message.MessageId, text: $"У вас пока нет активных заявок для пары {message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries)[3]}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
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
                                        user.WaitingReply = "";

                                        // Отправляем стартовую клавиатуру
                                        user.SendKeyboard(Bot, "main", "🤖 Привет, я Crypto Trading Бот! \n\nВот, что я умею:\n" +
                                            "• управлять вашим аккаунтом на бирже Exmo\n• выводить график курсов криптовалют\n• оповещать об изменениях цены");
                                    }
                                    else if (message.Text == "/add" || message.Text == "➕ Добавить аккаунт" || message.Text == "✏️ Заменить ключи")
                                    {
                                        user.WaitingReply = "addAccount";
                                        user.Test = false;

                                        string[] instructionPhotos = { "AgACAgIAAxkBAAIRJl6tpvDGU8LZ4bJeyQzvxYoMAek-AAL0rTEbVUlpSc9D6b_292nzdgABwQ4ABAEAAwIAA20AA6rDBQABGQQ",
                                                                    "AgACAgIAAxkBAAIRKV6tpxgknt3-kqPF36i9-r1CUeD5AAL1rTEbVUlpSdH0mzgw_Dqmofzski4AAwEAAwIAA20AA2DhAAIZBA",
                                                                    "AgACAgIAAxkBAAIRLF6tpymPg__V6hzQDZer7SiICcn3AAL2rTEbVUlpSd8Mh9O8vqDLCSvukS4AAwEAAwIAA20AA05oAQABGQQ",
                                                                    "AgACAgIAAxkBAAIRL16tpzxI3c3_biI1S573FcdeyKAHAAL3rTEbVUlpSRnmunE2vSjM9uZKkS4AAwEAAwIAA20AAygDAwABGQQ"};
                                        string[] captions = { "1. Перейдите в настройки вашего аккаунта",
                                                            "2. Перейдите во вкладку API и нажмите \"Сгенерировать API ключ\"",
                                                            "3. Введите любое название для ключа и подтвердите создание",
                                                            "4. Активируйте API ключ по ссылке, пришедшей на ваш E-mail.\n\n5. Отправьте QR-код, полученный на сайте или введите публичный и секретный ключ в следующем виде:\n\n"+
                                                            "K-f5c61f526446102b3c7af11909cfffa72ad8b4e6\nS-5272b9cb568506c205e2e5a056586c116fe5d1e0"};
                                        List<InputMediaPhoto> input = new List<InputMediaPhoto>();


                                        for (int i = 0; i < instructionPhotos.Length; i++)
                                        {
                                            string photo = (string)instructionPhotos[i];
                                            InputOnlineFile inputMedia = new InputOnlineFile(photo);
                                            await Bot.SendPhotoAsync(update.Message.Chat.Id, inputMedia, captions[i]);
                                        }
                                    }
                                    else if (message.Text == "/test" || message.Text == "⏳ Тестовый аккаунт")
                                    {
                                        user.WaitingReply = "";
                                        user.Test = true;
                                        user.SendKeyboard(Bot, "main", "Ваш тестовый аккаунт успешно активирован!\n\nДля проверки доступны все функции, кроме покупки и продажи криптовалюты.");
                                    }
                                    else if (message.Text == "/keys")
                                    {
                                        user.WaitingReply = "";
                                        if (user._key != null && user._secret != null)
                                            await Bot.SendTextMessageAsync(message.Chat.Id, "Ваш публичный ключ: \n" + users[message.Chat.Id]._key,
                                                   replyToMessageId: message.MessageId);
                                        else
                                            await Bot.SendTextMessageAsync(message.Chat.Id, "Вы пока не добавили свой аккаунт. Для добавления, введите /add",
                                                  replyToMessageId: message.MessageId);
                                    }
                                    else if (message.Text == "/balance" || message.Text == "💰 Баланс")
                                    {
                                        user.WaitingReply = "";
                                        await Bot.SendTextMessageAsync(message.Chat.Id, users[message.Chat.Id].GetBalance(),
                                                  replyToMessageId: message.MessageId);
                                    }
                                    else if (message.Text == "/trading" || message.Text == "💵 Торговля")
                                    {
                                        user.WaitingReply = "";
                                        var keyboard = user.TradingFavouriteCurrenciesInline();
                                        if (user.Test)
                                            await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Данная функция недоступна на тестовом аккаунте. Отправьте QR-код, полученный на сайте или введите публичный и секретный ключ");
                                        else if (keyboard != null)
                                            await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Выберите торговую пару, на которой будете производить покупку/продажу:", replyMarkup: keyboard);
                                        else
                                            await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "У вас пока нет избранных пар. Добавьте их в соответствующей вкладке!");
                                    }
                                    else if (message.Text.Contains("🔸 Купить"))
                                    {

                                        string pair = String.Join('_', message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries)[2].Split('/'));
                                        string info = user.GetPairInfo(pair);
                                        if (!info.StartsWith("К сожалению"))
                                        {
                                            await Bot.SendTextMessageAsync(chatId: user.id, text: info);
                                            user.WaitingReply = "exmo:" + pair + ":buy";
                                            await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: $"Ваш свободный баланс для пары {pair.Split('_')[0] + "/" + pair.Split('_')[1]}: \n" + user.GetBalance(pair) +
                                                "\n" + $"Введите цену ({ pair.Split('_')[1]}), по которой хотите купить { pair.Split('_')[0]}:");
                                        }
                                        else
                                        {
                                            user.SendKeyboard(Bot, "main", info);
                                        }
                                    }
                                    else if (message.Text.Contains("🔹 Продать"))
                                    {
                                        string pair = String.Join('_', message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries)[2].Split('/'));
                                        string info = user.GetPairInfo(pair);

                                        if (!info.StartsWith("К сожалению"))
                                        {
                                            await Bot.SendTextMessageAsync(chatId: user.id, text: info);
                                            user.WaitingReply = "exmo:" + pair + ":sell";
                                            await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: $"Ваш свободный баланс для пары {pair.Split('_')[0] + "/" + pair.Split('_')[1]}: \n" + user.GetBalance(pair) +
                                                "\n" + $"Введите цену ({ pair.Split('_')[1]}), по которой хотите продать { pair.Split('_')[0]}:");
                                        }
                                        else
                                        {
                                            user.SendKeyboard(Bot, "main", info);
                                        }
                                    }
                                    else if (message.Text.Contains("❌ Удалить заявку"))
                                    {
                                        string pair = String.Join('_', message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries)[3].Split('/'));
                                        var keyboard = user.GetDeleteOpenOrdersInline(pair);

                                        if (!(keyboard is null))
                                        {
                                            await Bot.SendTextMessageAsync(chatId: user.id, text: "Выберете активную заявку для удаления:", replyMarkup: keyboard);
                                        }
                                        else
                                        {
                                            await Bot.SendTextMessageAsync(chatId: user.id, text: $"У вас пока нет активных заявок для пары {message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries)[3]}");
                                        }
                                    }
                                    else if (message.Text == "/charts" || message.Text == "📊 Графики")
                                    {
                                        user.WaitingReply = "";
                                        var keyboard = user.GraphicFavouriteCurrenciesInline();

                                        if (keyboard != null)
                                            await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Выберите торговую пару, график которой вы хотите увидеть:", replyMarkup: keyboard);
                                        else
                                            await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "У вас пока нет избранных пар. Добавьте их в соответствующей вкладке!");

                                    }
                                    else if (message.Text == "/notification" || message.Text == "🔔 Оповещения")
                                    {
                                        user.WaitingReply = "";
                                        user.SendKeyboard(Bot, "notification");
                                    }
                                    else if (message.Text == "/toggle_notification" || message.Text == "❓ Включить/выключить оповещения")
                                    {
                                        user.WaitingReply = "";
                                        var keyboard = user.NotificationFavouriteCurrenciesInline();
                                        if (keyboard != null)
                                            await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Выберите включение/выключение оповещений для следующих торговых пар:", replyMarkup: keyboard);
                                        else
                                            await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "У вас пока нет избранных пар. Добавьте их в соответствующей вкладке!");
                                    }
                                    else if (message.Text == "/set_interval" || message.Text == "⏱ Изменить периодичность")
                                    {
                                        user.WaitingReply = "";
                                        var keyboard = user.NotificationIntervalInline();
                                        await Bot.SendTextMessageAsync(chatId: message.Chat.Id,
                                            text: String.Format($"Текущая периодичность: \n• интервал: {User.GetIntervalString(user.Interval)}, начиная с {user.StartHour}:00 до {user.EndHour}:00\n\nИзменить:"), replyMarkup: keyboard);


                                    }
                                    else if (message.Text == "/settings" || message.Text == "🛠 Настройки")
                                    {
                                        user.WaitingReply = "";
                                        user.SendKeyboard(Bot, "settings");
                                    }
                                    else if (message.Text == "/favourites" || message.Text == "🌟 Избранные пары")
                                    {
                                        user.WaitingReply = "";
                                        user.SendKeyboard(Bot, "favourites");
                                    }
                                    else if (message.Text == "/add_pair" || message.Text == "➕ Добавить пару")
                                    {
                                        user.WaitingReply = "";
                                        var keyboard = user.GetCurrenciesInline();

                                        await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Выберите первую валюту:", replyMarkup: keyboard);
                                    }
                                    else if (message.Text == "/remove_pair" || message.Text == "❌ Удалить пару")
                                    {
                                        user.WaitingReply = "";
                                        var keyboard = user.RemoveFavouriteCurrenciesInline();

                                        if (keyboard != null)
                                            await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "Выберите торговую пару для удаления:", replyMarkup: keyboard);
                                        else
                                            await Bot.SendTextMessageAsync(chatId: message.Chat.Id, text: "У вас пока нет избранных пар. Добавьте их в соответствующей вкладке!");
                                    }
                                    else if (message.Text == "/remove_acc" || message.Text == "❌ Отвязать аккаунт")
                                    {
                                        user.WaitingReply = "";
                                        user._key = null;
                                        user._secret = null;
                                        user.Test = false;
                                        user.SendKeyboard(Bot, "main", "Аккаунт успешно отвязан!");
                                    }
                                    else if (message.Text == "/back" || message.Text == "⬅️ Назад")
                                    {
                                        user.WaitingReply = "";
                                        switch (user.Keyboard)
                                        {
                                            case "settings":
                                            case "notification":
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
                                            // Разделяем сообщение на массив строк
                                            string[] keys = message.Text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (user.WaitingReply == "addAccount")
                                            {
                                                user.WaitingReply = "";
                                                if (keys.Length != 2 || !keys[0].StartsWith("K-") || !keys[1].StartsWith("S-"))
                                                {
                                                    throw new ArgumentException("Пожалуйста, введите две строчки в указанном формате\n");
                                                }
                                            }
                                            else if (user.WaitingReply.StartsWith("exmo"))
                                            {
                                                try
                                                {
                                                    if (user.WaitingReply.Split(':').Length == 3)
                                                    {
                                                        double price = 0;
                                                        if (!double.TryParse(message.Text.Replace('.', ','), out price) || price <= 0)
                                                        {
                                                            throw new ArgumentException($"Пожалуйста, введите число (цену в {user.WaitingReply.Split(':')[1].Split('_')[1]}), по которой вы хотите " +
                                                                (user.WaitingReply.Split(':')[2] == "buy" ? "купить " : "продать ") + user.WaitingReply.Split(':')[1].Split('_')[0] + "\n");
                                                        }
                                                        user.WaitingReply += ":" + message.Text.Replace(',', '.');
                                                        await Bot.SendTextMessageAsync(message.Chat.Id, $"Пожалуйста, введите число (количество {user.WaitingReply.Split(':')[1].Split('_')[0]}), которое вы хотите " + (user.WaitingReply.Split(':')[2] == "buy" ? "купить" : "продать") + ":\n");
                                                    }
                                                    else if (user.WaitingReply.Split(':').Length == 4)
                                                    {
                                                        double amount = 0;
                                                        if (!double.TryParse(message.Text.Replace('.', ','), out amount) || amount <= 0)
                                                        {
                                                            throw new ArgumentException($"Пожалуйста, введите число (количество {user.WaitingReply.Split(':')[1].Split('_')[0]}), которое вы хотите " + (user.WaitingReply.Split(':')[2] == "buy" ? "купить" : "продать") + "\n");
                                                        }
                                                        user.WaitingReply += ":" + message.Text.Replace(',', '.');
                                                        var keyboard = user.ConfirmationOrderInline(user.WaitingReply);


                                                        string text = "Подтвердите " + (user.WaitingReply.Split(':')[2] == "buy" ? "покупку" : "продажу") + $" {user.WaitingReply.Split(':')[4]} {user.WaitingReply.Split(':')[1].Split('_')[0]} по цене {user.WaitingReply.Split(':')[3]} {user.WaitingReply.Split(':')[1].Split('_')[1]}:";
                                                        await Bot.SendTextMessageAsync(message.Chat.Id, text, replyMarkup: keyboard);
                                                        user.WaitingReply = "";
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    await Bot.SendTextMessageAsync(message.Chat.Id, ex.Message,
                                                              replyToMessageId: message.MessageId);
                                                }
                                            }
                                            // Проверяем, что строчки начинаются также, как и ключи аутентификации
                                            if (keys.Length == 2 && keys[0].StartsWith("K-") && keys[1].StartsWith("S-"))
                                            {
                                                user._key = keys[0];
                                                user._secret = keys[1];

                                                try
                                                {
                                                    string check = user.GetBalance(); // Используем ключи для получения баланса пользователя
                                                    if (check == "Error")
                                                        throw new ArgumentException("Похоже вы ввели неверные ключи. Сначала активируйте их в письме, пришедшем на ваш email, или создайте новую пару ключей");
                                                    // Сохраняем данные пользователя (сериализация в .json файл)
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
                                if (update.Message.Photo != null)
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
                                        user.SendKeyboard(Bot, "main");
                                    }

                                    Telegram.Bot.Types.File file = await Bot.GetFileAsync(message.Photo[message.Photo.Count() - 1].FileId);
                                    using (FileStream photoStream = new FileStream("temp.png", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                                    {

                                        await Bot.GetInfoAndDownloadFileAsync(message.Photo[message.Photo.Count() - 1].FileId, photoStream);
                                        var image = Bitmap.FromStream(photoStream) as Bitmap;

                                        try
                                        {
                                            MultiFormatReader reader = new MultiFormatReader();
                                            BitmapLuminanceSource ls = new BitmapLuminanceSource(image);
                                            var binarizer = new HybridBinarizer(ls);
                                            BinaryBitmap binaryBitmap = new BinaryBitmap(binarizer);
                                            var result = reader.decode(binaryBitmap);

                                            string[] keys = result.Text.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (keys.Length == 3 && keys[1].StartsWith("K-") && keys[2].StartsWith("S-"))
                                            {
                                                user.WaitingReply = "";
                                                user.Test = false;

                                                user._key = keys[1];
                                                user._secret = keys[2];

                                                try
                                                {
                                                    string check = user.GetBalance();
                                                    if (check == "Error")
                                                        throw new ArgumentException("Похоже вы ввели неверные ключи. Сначала активируйте их в письме, пришедшем на ваш email, или создайте новую пару ключей");
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
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex);
                                            if (user.WaitingReply == "addAccount")
                                            {
                                                await Bot.SendTextMessageAsync(message.Chat.Id, "Не удалось распознать QR code");
                                                user.WaitingReply = "";
                                            }
                                        }

                                    }
                                }
                            }
                            offset = update.Id + 1;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid format. A valid token looks like \"1234567:4TT8bAc8GHUspu3ERYn-KGcvsvGB9u_n4ddy\".");
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine(ex.Message);
                if (isBotActive)
                    goto tryagain;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (isBotActive)
                    goto tryagain;
            }
        }


        /// <summary>
        /// Выводит скриншот графика курса
        /// </summary>
        /// <param name="pair"></param>
        /// <param name="id">Id пользователя</param>
        /// <param name="message">Сообщение, которое необходимо отправить</param>
        async void Capture(string pair, long id, string message = "")
        {
            var mes = await Bot.SendTextMessageAsync(id, message + "Пожалуйста, ожидайте графика...");
            try
            {
                string[] keys = System.IO.File.ReadAllLines(dir + "/screenkeys.txt");
                string customerKey;
                string key;
                int index = -1;
                for (int i = 0; i < keys.Length; i++)
                {
                    key = keys[i];
                    string checkQuotaUrl = "https://api.apiflash.com/v1/urltoimage/quota?access_key=" + key + "&fresh=true";
                    Dictionary<string, long> dc = new Dictionary<string, long>();
                    await Task.Run(() =>
                    {
                        using (var wc = new WebClient())
                        {
                            string s = wc.DownloadString(checkQuotaUrl);
                            dc = JsonConvert.DeserializeObject<Dictionary<string, long>>(s);

                        }
                    });

                    if (dc["remaining"] > 0)
                    {
                        index = i;
                        break;
                    }
                }
                if (index >= 0)
                {
                    customerKey = keys[index];

                    // Save screenshot directly
                    string newApiUrl = "https://api.apiflash.com/v1/urltoimage?access_key=" + customerKey + "&delay=3&format=png&height=600&quality=100&ttl=5&url=" + "https://exmo.me/ru/trade/" + pair + "&width=1020";

                    await Task.Run(async () =>
                    {
                        using (var wc = new WebClient())
                        {
                            using (MemoryStream stream = new MemoryStream(wc.DownloadData(newApiUrl)))
                            {
                                Image img = Image.FromStream(stream);
                                Bitmap crop = (Bitmap)Crop(img, new Rectangle(66, 200, 849, 330));

                                crop.Save("output.png", ImageFormat.Png);
                                using (FileStream fs = new FileStream("output.png", FileMode.OpenOrCreate))
                                {
                                    InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, pair.Split('_')[0] + "/" + pair.Split('_')[1] + ".png");

                                    await Bot.DeleteMessageAsync(id, mes.MessageId);
                                    await Bot.SendPhotoAsync(id, inputOnlineFile, pair.Split('_')[0] + "/" + pair.Split('_')[1] + " - график торговой пары");
                                }
                            }
                        }
                    });

                }
                else
                {
                    await Bot.EditMessageTextAsync(id, mes.MessageId, "К сожалению, невозможно сделать скриншот графика. Пожалуйста, введите новые ключи в файл screenkeys.txt");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

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
