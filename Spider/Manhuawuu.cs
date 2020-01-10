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
    public class Manhuawuu : ISpider
    {
        IConfiguration configuration;
        long author = 0;
        long category = 0;
        string webroot;
        string remoteLink;
        PubHelper pubHelper = new PubHelper();
        WebClient wc = new WebClient();
        PubContext pubContext = new PubContext();

        public Manhuawuu()
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Path.Combine(AppContext.BaseDirectory))
             .AddJsonFile("Config/manhuawuu.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
            author = long.Parse(configuration.GetSection("author").Value);
            category = long.Parse(configuration.GetSection("category").Value);
            webroot = configuration.GetSection("webroot").Value;
            remoteLink = configuration.GetSection("remoteLink").Value;
        }

        public void Go()
        {
            try
            {
                var cate = configuration.GetSection("cate").Value;
                var start = int.Parse(configuration.GetSection("start").Value);
                var last = int.Parse(configuration.GetSection("last").Value);
                var url = "http://www.manhuawuu.com/" + cate;
                for (int i = start; i <= last; i++)
                {
                    if (i > 1)
                    {
                        url = url + $"/page/{i}";
                    }
                    var html = ReqHelper.GetHtml(url);
                    var parse = new HtmlParser();
                    var doc = parse.ParseDocument(html);

                    var list = doc.QuerySelectorAll(".post-style-card > a");
                    foreach (var item in list)
                    {
                        GetContent(item.GetAttribute("href"));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void GetContent(string url)
        {
            var html = ReqHelper.GetHtml(url);
            var parse = new HtmlParser();
            var doc = parse.ParseDocument(html);
            var title = doc.QuerySelector("div.article-title a").TextContent;
            var post = pubContext.Posts.Where(p => p.PostTitle == title).FirstOrDefault();
            if (post == null)
            {
                var imgs = doc.QuerySelectorAll(".article-body p img");
                var date = DateTime.Now.ToString("yyyy-MM-dd");
                var downloadDir = Path.Combine(webroot, "wp-content/uploads/" + date); //拼接图片下载目录
                if (!Directory.Exists(downloadDir))
                {
                    Directory.CreateDirectory(downloadDir);
                }
                foreach (IHtmlImageElement img in imgs)
                {
                    if (img.GetAttribute("src") == "http://www.manhuawuu.com/wp-content/uploads/2018/10/微信公众号.jpg")
                    {
                        img.Remove();
                    }
                    if(remoteLink == "false")
                    {
                        var imgUrl = img.GetAttribute("src");
                        var fileName = Guid.NewGuid().ToString() + ".jpg";
                        wc.DownloadFile(imgUrl, Path.Combine(downloadDir, fileName));
                        Console.WriteLine("下载" + imgUrl + "到" + downloadDir);
                        img.Source = Path.Combine("/wp-content/uploads/" + date, fileName);//设置img的src为下载路径
                    }                  
                    img.RemoveAttribute("srcset"); //移除data-src属性
                }
                var content = doc.QuerySelector(".article-body").OuterHtml.Replace("一本正经的漫画", "拿发阅读");
                pubHelper.Post(title, content, category, author);
            }
            else
            {
                Console.WriteLine("文章" + title + "已经存在");
            }
            
        }
    }
}
