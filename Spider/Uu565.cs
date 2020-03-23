using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
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
    public class Uu565 : ISpider
    {
        private string cookie;
        private string domain;
        private string bookSrcId;
        long category = 0;
        PubHelper pubHelper = new PubHelper();
        WebClient wc = new WebClient();
        Microsoft.Extensions.Configuration.IConfiguration configuration;
        string webroot;
        SugarContext sugarContext = new SugarContext();
        public Uu565()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Path.Combine(AppContext.BaseDirectory))
               .AddJsonFile("Config/Main.json", optional: true, reloadOnChange: true)
              .AddJsonFile("Config/Uu565.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
            category = long.Parse(configuration.GetSection("category").Value);
            webroot = configuration.GetSection("webroot").Value;
            cookie = configuration.GetSection("cookie").Value;
            domain = configuration.GetSection("domain").Value;
            ServicePointManager.ServerCertificateValidationCallback +=
           (sender, cert, chain, sslPolicyErrors) => true;
        }
        public void Go()
        {
            int start = int.Parse(configuration.GetSection("start").Value);
            int last = int.Parse(configuration.GetSection("last").Value);
            for (int i = start; i <= last; i++)
            {
                var html = ReqHelper.GetHtml(Flurl.Url.Combine(domain, "index.php?m=&c=commic&a=cates&p=" + i), cookie: cookie);
                var parser = new HtmlParser();
                var doc = parser.ParseDocument(html);
                var lis = doc.QuerySelectorAll(".tab_box_list li");
                foreach (var li in lis)
                {
                    var href = li.GetAttribute("onclick");
                    bookSrcId = href.Replace("location.href='/index.php?m=&c=commic&a=detail&id=", string.Empty);
                    GetDetail(Flurl.Url.Combine(domain, href.Replace("location.href=", string.Empty).Replace("'", string.Empty)));
                }
            }
        }

        private void GetDetail(string url)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<div id=\"js_content\">");
                var html = ReqHelper.GetHtml(url, cookie: cookie);
                var parse = new HtmlParser();
                var doc = parse.ParseDocument(html);
                var bookName = doc.QuerySelector(".details_top_left_top h3").TextContent;
                var count = sugarContext.Db.Queryable<Post>().Where(p => p.PostTitle == bookName).Count();
                if (count <= 0)
                {                    
                    var desc = doc.QuerySelector("p.details_p_active").TextContent.Trim();
                    sb.Append($"<p>{desc}</p>");
                    var tags = new List<string>();
                    var plist = doc.QuerySelectorAll(".details_top_left_btm > p").ToList();
                    if (plist.Count > 1)
                    {
                        var arr = plist[1].QuerySelectorAll("span.tag");
                        foreach (var item in arr)
                        {
                            tags.Add(item.TextContent.Trim());
                        }
                    }
                    var json = ReqHelper.Post($"{domain}index.php?m=&c=commic&a=fetch_chapter&commic_id={bookSrcId}&spread_id=",
                            "index=1", cookie: cookie);
                    var obj = JObject.Parse(json);
                    if (obj["status"].ToString() == "1")
                    {
                        var body = obj["info"]["body"].ToString();
                        doc = parse.ParseDocument(body);
                        var imgs = doc.QuerySelectorAll("img").ToList();
                        var date = DateTime.Now.ToString("yyyy-MM-dd");
                        var downloadDir = Path.Combine(webroot, "wp-content/uploads/" + date); //拼接图片下载目录
                        if (!Directory.Exists(downloadDir))
                        {
                            Directory.CreateDirectory(downloadDir);
                        }
                        foreach (IHtmlImageElement img in imgs)
                        {
                            var imgUrl = img.GetAttribute("data-original");
                            var fileName = Guid.NewGuid().ToString() + ".jpg";
                            wc.DownloadFile(imgUrl, Path.Combine(downloadDir, fileName));
                            Console.WriteLine("下载" + imgUrl + "到" + downloadDir);
                            img.Source = Path.Combine("/wp-content/uploads/" + date, fileName);//设置img的src为下载路径
                            img.RemoveAttribute("srcset"); //移除data-src属性
                            sb.Append($"<p>{img.OuterHtml}</p>");
                        }
                        var addConfigs = configuration.GetSection("add").GetChildren();
                        if (addConfigs.Count() > 0)
                        {
                            foreach (var config in addConfigs)
                            {
                                sb.Append(config.Value);
                            }
                        }
                        sb.Append("</div>");
                        pubHelper.Post(bookName, sb.ToString(), category, 1, tags);
                    }
                    
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
