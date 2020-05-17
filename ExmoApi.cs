﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Crypto.Bot
{
    public class ExmoApi
    {
        private static long _nounce;
        // API settings
        private string _key;
        private string _secret;
        private string _url = "https://api.exmo.com/v1/{0}";

        static ExmoApi()
        {
            _nounce = Helpers.GetTimestamp();
        }

        public ExmoApi(string key, string secret)
        {
            if (key == null || secret == null)
            {
                key = "null";
                secret = "null";
            }
            _key = key;
            _secret = secret;
        }

        public async Task<string> ApiQueryAsync(string apiName, IDictionary<string, string> req)
        {
            using (var client = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                var n = Interlocked.Increment(ref _nounce);
                req.Add("nonce", Convert.ToString(n));
                var message = ToQueryString(req);

                var sign = Sign(_secret, message);

                var content = new FormUrlEncodedContent(req);
                content.Headers.Add("Sign", sign);
                content.Headers.Add("Key", _key);

                var response = await client.PostAsync(string.Format(_url, apiName), content);

                return await response.Content.ReadAsStringAsync();
            }
        }

        /// <summary>
        /// Метод позволяет составит запрос к бирже и получить ответ
        /// </summary>
        /// <param name="apiName">Вызываемый метод API</param>
        /// <param name="req">Запрос</param>
        /// <returns>Строка, ответ биржи на запрос</returns>
        public string ApiQuery(string apiName, IDictionary<string, string> req)
        {
            using (var wb = new WebClient())
            {
                req.Add("nonce", Convert.ToString(_nounce++));
                // Строковое представление запроса к бирже
                var message = ToQueryString(req);
                // Вычисление подписи запроса
                var sign = Sign(_secret, message);

                // Добавление HTTP-заголовков: подпись и публичный ключ
                wb.Headers.Add("Sign", sign);
                wb.Headers.Add("Key", _key);

                var data = req.ToNameValueCollection();

                var response = wb.UploadValues(string.Format(_url, apiName), "POST", data);
                return Encoding.UTF8.GetString(response);
            }
        }

        private string ToQueryString(IDictionary<string, string> dic)
        {
            var array = (from key in dic.Keys
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(dic[key])))
                .ToArray();
            return string.Join("&", array);
        }

        public static string Sign(string key, string message)
        {
            using (HMACSHA512 hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key)))
            {
                byte[] b = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                return ByteToString(b);
            }
        }

        public static string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2"); // hex format
            }
            return (sbinary).ToLowerInvariant();
        }

    }

    public static class Helpers
    {
        public static NameValueCollection ToNameValueCollection<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            var nameValueCollection = new NameValueCollection();

            foreach (var kvp in dict)
            {
                string value = string.Empty;
                if (kvp.Value != null)
                    value = kvp.Value.ToString();

                nameValueCollection.Add(kvp.Key.ToString(), value);
            }

            return nameValueCollection;
        }

        public static long GetTimestamp()
        {
            var d = (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
            return (long)d;
        }
    }
}
