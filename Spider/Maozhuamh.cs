using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using WpSpider.Pub;

namespace WpSpider.Spider
{
    public class Maozhuamh:ISpider
    {
        IConfiguration configuration;
        long author = 0;
        long category = 0;
        string webroot;
        string remoteLink;
        PubHelper pubHelper = new PubHelper();
        WebClient wc = new WebClient();
        PubContext pubContext = new PubContext();

        public Maozhuamh()
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Path.Combine(AppContext.BaseDirectory))
             .AddJsonFile("Config/maozhuamh.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
            author = long.Parse(configuration.GetSection("author").Value);
            category = long.Parse(configuration.GetSection("category").Value);
            webroot = configuration.GetSection("webroot").Value;
            remoteLink = configuration.GetSection("remoteLink").Value;
        }

        public void Go()
        {
            var cate = configuration.GetSection("cate").Value;
            var start = int.Parse(configuration.GetSection("start").Value);
            var last = int.Parse(configuration.GetSection("last").Value);
            for (int i = start; i <= last; i++)
            {
                var url = $"https://www.maozhuamh.com/sort/{cate}/page/{i}";
                var html = ReqHelper.GetHtml(url);
                var parse = new HtmlParser();
                var doc = parse.ParseDocument(html);

                var list = doc.QuerySelectorAll("li.pbox.clr .word a");
                foreach (var item in list)
                {
                    GetContent(item.GetAttribute("href"));
                }
            }
        }

        public void GetContent(string url)
        {
            var html = ReqHelper.GetHtml(url);
            var parse = new HtmlParser();
            var doc = parse.ParseDocument(html);
            var title = doc.QuerySelector("h1").TextContent;
            var post = pubContext.Posts.Where(p => p.PostTitle == title).FirstOrDefault();
            if (post == null)
            {
                var imgs = doc.QuerySelectorAll("#JA_Page img");
                var date = DateTime.Now.ToString("yyyy-MM-dd");
                var downloadDir = Path.Combine(webroot, "wp-content/uploads/" + date); //拼接图片下载目录
                if (!Directory.Exists(downloadDir))
                {
                    Directory.CreateDirectory(downloadDir);
                }
                foreach (IHtmlImageElement img in imgs)
                {                  
                    if (remoteLink == "false")
                    {
                        var imgUrl = img.GetAttribute("src");
                        var fileName = Guid.NewGuid().ToString() + ".jpg";
                        wc.DownloadFile(imgUrl, Path.Combine(downloadDir, fileName));
                        Console.WriteLine("下载" + imgUrl + "到" + downloadDir);
                        img.Source = Path.Combine("/wp-content/uploads/" + date, fileName);//设置img的src为下载路径
                    }
                }
                var content = doc.QuerySelector("#JA_Page").OuterHtml
                    .Replace("点击下载猫爪漫画APP：https://dwz.cn/jN6oZaV8", "前往网站：https://www.2gmx.com/")
                    .Replace("APP底部", "网站");
                pubHelper.Post(title, content, category, author);
            }
            else
            {
                Console.WriteLine("文章" + title + "已经存在");
            }
        }
    }
}
