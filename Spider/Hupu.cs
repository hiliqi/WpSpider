using AngleSharp.Html.Parser;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using WpSpider.Pub;

namespace WpSpider.Spider
{
    public class Hupu : ISpider
    {
        Microsoft.Extensions.Configuration.IConfiguration configuration;
        long author = 0;
        long category = 0;
        string webroot;
        PubHelper pubHelper = new PubHelper();
        PubContext pubContext = new PubContext();
        WebClient wc = new WebClient();

        public Hupu()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Path.Combine(AppContext.BaseDirectory))
              .AddJsonFile("Config/Main.json", optional: true, reloadOnChange: true)
              .AddJsonFile("Config/Hupu.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
            category = long.Parse(configuration.GetSection("category").Value);
            webroot = configuration.GetSection("webroot").Value;
            ServicePointManager.ServerCertificateValidationCallback +=
           (sender, cert, chain, sslPolicyErrors) => true;
        }
        public void Go()
        {
            var html = ReqHelper.GetHtml("https://voice.hupu.com/soccer");
            var parser = new HtmlParser();
            var doc = parser.ParseDocument(html);
            var alist = doc.QuerySelectorAll("h4 > a");
            foreach (var a in alist)
            {
                GetDetail(a.GetAttribute("href"));
            }
        }

        public void GetDetail(string url)
        {
            try
            {
                var html = ReqHelper.GetHtml(url);
                var parser = new HtmlParser();
                var doc = parser.ParseDocument(html);
                var title = BaiduFanyi.Fanyi("zh", "vie", doc.QuerySelector("h1").TextContent)[0];
                Thread.Sleep(1000);
                var post = pubContext.Posts.Where(p => p.PostTitle == title).FirstOrDefault();
                if (post == null)
                {
                    var imgUrl = doc.QuerySelector(".artical-importantPic img").GetAttribute("src");
                    var date = DateTime.Now.ToString("yyyy-MM-dd");
                    var downloadDir = Path.Combine(webroot, "wp-content/uploads/" + date); //拼接图片下载目录
                    if (!Directory.Exists(downloadDir))
                    {
                        Directory.CreateDirectory(downloadDir);
                    }
                    var fileName = Guid.NewGuid().ToString() + ".jpg";
                    wc.DownloadFile(imgUrl, Path.Combine(downloadDir, fileName));
                    Console.WriteLine("下载" + imgUrl + "到" + downloadDir);
                    imgUrl = Path.Combine("/wp-content/uploads/", date, fileName);
                    var img = $"<p><img src=\"{imgUrl}\"></p>";

                    var content = doc.QuerySelector(".artical-main-content").TextContent;
                    var list = BaiduFanyi.Fanyi("zh", "vie", content);
                    StringBuilder sb = new StringBuilder();
                    list.ForEach((l) => sb.Append($"<p>{l}</p>"));
                    string result = title + img + sb.ToString();
                    pubHelper.Post(title, result, category, 1);
                }
                else
                {
                    Console.WriteLine(title + "已存在，跳过");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace.ToString());
            }
        }
    }
}
