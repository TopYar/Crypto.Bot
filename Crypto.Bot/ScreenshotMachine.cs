using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Security.Cryptography;

class ScreenshotMachine
{

    private string customerKey;
    private string secretPhrase;
    private string baseApiUrl = "https://api.screenshotmachine.com/?";
    private string basePdfApiUrl = "https://pdfapi.screenshotmachine.com/?";

    public ScreenshotMachine(string customerKey, string secretPhrase)
    {
        this.customerKey = customerKey;
        this.secretPhrase = secretPhrase;
    }

    public string GenerateScreenshotApiUrl(Dictionary<string, string> options)
    {
        return GenerateUrl(baseApiUrl, options);
    }

    public string GeneratePdfApiUrl(Dictionary<string, string> options)
    {
        return GenerateUrl(basePdfApiUrl, options);
    }

    private string GenerateUrl(string baseUrl, Dictionary<string, string> options)
    {
        StringBuilder apiUrl = new StringBuilder(baseUrl);
        apiUrl.Append("key=").Append(customerKey);
        if (secretPhrase != null && secretPhrase.Trim().Length > 0)
        {
            apiUrl.Append("&hash=").Append(CalculateHash(options["url"] + secretPhrase));
        }
        foreach (var pair in options)
        {
            string key = pair.Key;
            string value = pair.Value;
            apiUrl.Append("&").Append(key).Append("=").Append(WebUtility.UrlEncode(value));
        }
        return apiUrl.ToString();
    }

    private string CalculateHash(string text)
    {
        MD5 md5 = MD5.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        byte[] hash = md5.ComputeHash(bytes);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("X2"));
        }
        return sb.ToString();
    }
}