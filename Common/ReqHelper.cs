using CsharpHttpHelper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace WpSpider
{
    public class ReqHelper
    {     
        public static string GetHtml(string url, string ugent = "pc",  string encoding = "UTF-8",
            string referer = "", string cookie = "", params KeyValuePair<string, string>[] pairs)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = url,
                UserAgent = ugent == "pc"?
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36" : 
                "Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1",
                Encoding = Encoding.GetEncoding(encoding),
                Cookie = cookie
            };
            foreach (var pair in pairs)
            {
                item.Header.Add(pair.Key, pair.Value);
            }
            item.Header.Add("Cache-Control", "no-cache");
            HttpResult result = http.GetHtml(item);
            return result.Html;
        }

        public static string Post(string url, string postData = "", string cookie = "", string host="", params KeyValuePair<string, string>[] pairs)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = url,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36",
                Cookie = cookie,
                Method = "POST",
                Postdata = postData,
                Host = host
            };
            foreach (var pair in pairs)
            {
                item.Header.Add(pair.Key, pair.Value);
            }
            item.Header.Add("Cache-Control", "no-cache");
            HttpResult result = http.GetHtml(item);
            return result.Html;
        }
    }
}
