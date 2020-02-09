using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WpSpider
{
    public class BaiduFanyi
    {
        public static List<string> Fanyi(string fromLang, string toLang, string content)
        {
            try
            {
                // 原文
                string q = content;
                // 源语言
                string from = fromLang;
                // 目标语言
                string to = toLang;
                // 改成您的APP ID
                string appId = "20200202000379663";
                Random rd = new Random();
                string salt = rd.Next(100000).ToString();
                // 改成您的密钥
                string secretKey = "GxmJpRe5qqOKhjSqjxmU";
                string sign = EncryptString(appId + q + salt + secretKey);
                string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
                url += "q=" + HttpUtility.UrlEncode(q);
                url += "&from=" + from;
                url += "&to=" + to;
                url += "&appid=" + appId;
                url += "&salt=" + salt;
                url += "&sign=" + sign;
                var json = ReqHelper.GetHtml(url);
                List<string> list = new List<string>();
                JObject obj = JObject.Parse(json);
                foreach (var item in obj["trans_result"])
                {
                    list.Add(item["dst"].ToString());
                }
                return list;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }

        // 计算MD5值
        private static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }
    }  
}
