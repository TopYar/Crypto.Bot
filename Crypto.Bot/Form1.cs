using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Crypto.Bot
{
    public partial class Form1 : Form
    {
        BackgroundWorker bw;
        public Form1()
        {
            InitializeComponent();
            Console.WriteLine("Run");
            this.bw = new BackgroundWorker();
            this.bw.DoWork += this.bw_DoWork; // Метод bw_DoWork будет работать асинхронно
        }

        async void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker; // Получаем ссылку на класс вызвавший событие
            string key = e.Argument as String;
            Console.WriteLine("Key = {0}", key);
            textBox1.Hide();
            button1.Hide();
            label1.Text = "Bot is trying to connect";
            try
            {
                var Bot = new Telegram.Bot.TelegramBotClient(key); // инициализируем API

                await Bot.SetWebhookAsync("");
                
                //Bot.SetWebhook(""); // Обязательно! убираем старую привязку к вебхуку для бота
                int offset = 0; // отступ по сообщениям
                while (true)
                {
                    var updates = await Bot.GetUpdatesAsync(offset); // получаем массив обновлений

                    label1.BackColor = Color.Transparent;
                    label1.Text = "Bot is running";
                    label1.ForeColor = Color.Green;
                    label1.BringToFront();

                    foreach (var update in updates) // Перебираем все обновления
                    {
                        Console.WriteLine("Look updates");
                        var message = update.Message;
                        if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                        {
                            if (message.Text == "/start")
                            {
                                // в ответ на команду /saysomething выводим сообщение
                                await Bot.SendTextMessageAsync(message.Chat.Id, "тест",
                                       replyToMessageId: message.MessageId);
                            }
                        }
                        offset = update.Id + 1;
                    }

                }
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine(ex.Message); // если ключ не подошел - пишем об этом в консоль отладки
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
    }
}
