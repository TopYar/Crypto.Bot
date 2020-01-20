using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Crypto.Bot
{
    public partial class Form1 : Form
    {
        BackgroundWorker bw, crypt;
        Button tryagain = new Button();
        string key;
        delegate void del();
        bool isBotActive = false;
        Dictionary<long, User> users = new Dictionary<long, User>();

        Telegram.Bot.TelegramBotClient Bot;
        public Form1()
        {
            InitializeComponent();

            DirectoryInfo directoryInfo = new DirectoryInfo("../Debug");

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

            Console.WriteLine("Run");

            tryagain = Clone(button1);
            EventHandler tryClick = new EventHandler(Tryagain_Click);
            tryagain.Click += tryClick;
            tryagain.Text = "Try again";


            bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork; // Метод bw_DoWork будет работать асинхронно

            crypt = new BackgroundWorker();
            crypt.DoWork += crypt_DoWork; // Метод crypt_DoWork будет работать асинхронно
            crypt.RunWorkerAsync();

            FirstScreen();
        }

        async void crypt_DoWork(object sender, DoWorkEventArgs e)
        {



        }

        async void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var hideAll = new del(HideAll);
            var label1_Show = new del(label1.Show);
            var tryagain_Show = new del(tryagain.Show);

            Invoke(hideAll);
            Invoke(label1_Show);

            label1.Text = "Bot is trying to connect";

            var worker = sender as BackgroundWorker; // Получаем ссылку на класс вызвавший событие
            key = e.Argument as String;
            Console.WriteLine("Key = {0}", key);

            try
            {
                Bot = new Telegram.Bot.TelegramBotClient(key); // инициализируем API

                await Bot.SetWebhookAsync(""); // Убираем старую привязку к вебхуку для бота
                int offset = 0; // отступ по сообщениям


                label1.Text = "Bot is running";
                label1.ForeColor = Color.Green;
                isBotActive = true;

                tryagain.Text = "Stop";
                Invoke(tryagain_Show);

                while (true)
                {
                    if (!isBotActive)
                        return;

                    var updates = await Bot.GetUpdatesAsync(offset); // получаем массив обновлений

                    foreach (var update in updates) // Перебираем все обновления
                    {
                        Console.WriteLine("Look updates");
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
                                    user.Save();
                                    users.Add(user.id, user);
                                }

                                if (user.waitingReply == "addAccount")
                                {
                                    try
                                    {
                                        string[] keys = message.Text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (keys.Length != 2)
                                        {
                                            throw new ArgumentException("Пожалуйста, введите две строчки в указанном формате");
                                        }
                                        else
                                        {
                                            if (!keys[0].StartsWith("K-") || !keys[1].StartsWith("S-"))
                                            {
                                                user.waitingReply = "";
                                                user.Save();
                                                throw new ArgumentException("Пожалуйста, введите две строчки в указанном формате\nПопробовать снова /add");
                                            }
                                            user._key = keys[0];
                                            user._secret = keys[1];

                                            try
                                            {
                                                string check = user.GetBalance();
                                                if (check == "Error")
                                                    throw new ArgumentException("Похоже вы ввели неверные ключи");
                                                user.Save();

                                                await Bot.SendTextMessageAsync(message.Chat.Id, "Your key: " + keys[0] + "\nYour secret: " + keys[1],
                                                           replyToMessageId: message.MessageId);

                                                user.waitingReply = "";
                                                user.Save();
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
                                else
                                {
                                    if (message.Text == "/start")
                                    {

                                        await Bot.SendTextMessageAsync(message.Chat.Id, "тест",
                                               replyToMessageId: message.MessageId);
                                    }
                                    else if (message.Text == "/add")
                                    {
                                        user.waitingReply = "addAccount";
                                        await Bot.SendTextMessageAsync(message.Chat.Id, @"Введите публичный и секретный ключ в следующем виде:

K-f5c61f526446102b3c7af11909cfffa72ad8b4e7
S-5272b9cb568506c205e2e5a056586c116fe5d1e1",
                                               replyToMessageId: message.MessageId);
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
                                    else if (message.Text == "/balance")
                                    {
                                        Console.WriteLine(users[message.Chat.Id].GetBalance());

                                        await Bot.SendTextMessageAsync(message.Chat.Id, users[message.Chat.Id].GetBalance(),
                                                  replyToMessageId: message.MessageId);
                                    }
                                }
                            }
                            offset = update.Id + 1;
                        }
                    }
                }
            }
            catch (ArgumentException)
            {
                label1.Text = "Invalid format. A valid token looks like \n\"1234567:4TT8bAc8GHUspu3ERYn - KGcvsvGB9u_n4ddy\".";
                label1.ForeColor = Color.Red;

                tryagain.Text = "Try again";
                Invoke(tryagain_Show);

                Console.WriteLine("Invalid format. A valid token looks like \"1234567:4TT8bAc8GHUspu3ERYn - KGcvsvGB9u_n4ddy\".");
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                label1.Text = "Exception occured: " + ex.Message + "\nYour token maybe invalid";
                label1.ForeColor = Color.Red;

                tryagain.Text = "Try again";
                Invoke(tryagain_Show);
                Console.WriteLine(ex.Message);
            }
            /*catch (Exception ex)
            {
                label1.Text = "Something went wrong (check your internet connection)";
                label1.ForeColor = Color.Red;

                tryagain.Text = "Try again";
                Invoke(tryagain_Show);
                Console.WriteLine(ex.Message);
            }*/
        }
        void HideAll()
        {
            tryagain.Hide();
            textBox1.Hide();
            button1.Hide();
            label1.Hide();
        }
        void FirstScreen()
        {
            HideAll();
            textBox1.Show();
            button1.Show();
            label1.Show();

            label1.ForeColor = Color.Black;
            label1.Text = "Type Telegram token";
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            var text = textBox1.Text; // Получаем содержимое текстового поля textBox1 в переменную text
            if (text != "" && this.bw.IsBusy != true) // Если не запущен - запускаем
            {
                Console.WriteLine("Run Async");
                this.bw.RunWorkerAsync(text); // Передаем эту переменную в виде аргумента методу bw_DoWork
            }

        }

        async private void Tryagain_Click(object sender, EventArgs e)
        {
            if (isBotActive)
            {
                isBotActive = false;
                label1.Text = "Trying to stop Bot";
                label1.ForeColor = Color.Black;
                tryagain.Hide();
                await Task.Delay(2000);
            }

            FirstScreen();
        }

        public static T Clone<T>(T controlToClone) where T : Control
        {
            T instance = Activator.CreateInstance<T>();

            Type control = controlToClone.GetType();
            PropertyInfo[] info = control.GetProperties();
            object p = control.InvokeMember("", System.Reflection.BindingFlags.CreateInstance, null, controlToClone, null);
            foreach (PropertyInfo pi in info)
            {
                if ((pi.CanWrite) && !(pi.Name == "WindowTarget") && !(pi.Name == "Capture"))
                {
                    pi.SetValue(instance, pi.GetValue(controlToClone, null), null);
                }
            }
            return instance;
        }
    }
}
