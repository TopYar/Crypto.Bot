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
    public class Program
    {
        static void Main()
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}