using AngleSharp.Html.Parser;
using Flurl;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using WpSpider.Model;
using WpSpider.Pub;

namespace WpSpider.Spider
{
    public class Jiepai : ISpider
    {
        IConfiguration configuration;
        long category = 0;
        string webroot;
        PubHelper pubHelper = new PubHelper();
        SugarContext sugarContext = new SugarContext();

        public Jiepai()
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Path.Combine(AppContext.BaseDirectory))
             .AddJsonFile("Config/jiepai.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
            category = long.Parse(configuration.GetSection("category").Value);
            webroot = configuration.GetSection("webroot").Value;
            ServicePointManager.ServerCertificateValidationCallback +=
           (sender, cert, chain, sslPolicyErrors) => true;
        }

        public void Go()
        {
            var cate = configuration.GetSection("cate").Value;
            var start = int.Parse(configuration.GetSection("start").Value);
            var url = Url.Combine("https://www.jiepaipu.com/", cate);
            try
            {
                if (start > 1)
                {
                    url = url + $"/page/{start}";
                }
                var html = ReqHelper.GetHtml(url);
                var parse = new HtmlParser();
                var doc = parse.ParseDocument(html);

                var list = doc.QuerySelectorAll("#main article .thumbnail>a");
                Console.WriteLine($"获取到{list.Count()}篇文章");
                foreach (var item in list)
                {
                    GetContent(item.GetAttribute("href"));
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
            Console.WriteLine("开始采集" + url);
            var html = ReqHelper.GetHtml(url);
            var parse = new HtmlParser();
            var doc = parse.ParseDocument(html);
            var title = doc.QuerySelector("h1").TextContent.Trim();
            Console.WriteLine("获取标题" + title);
            var post = sugarContext.Db.Queryable<Post>().Where(p => p.PostTitle == title).First();
            if (post == null)
            {
                var el = doc.QuerySelector(".single-content");
                var ad = el.QuerySelector(".ad-pc");
                ad.Remove();
                var content = $"<p>{title}</p>" + el.InnerHtml.Trim();          
                pubHelper.Post(title, content, category, 1);
            }
            else
            {
                Console.WriteLine("文章" + title + "已经存在");
            }
        }
    }
}
