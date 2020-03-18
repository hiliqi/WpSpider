using AngleSharp;
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

namespace WpSpider
{
    public class MpHelper
    {
        Microsoft.Extensions.Configuration.IConfiguration configuration;
        long author = 0;
        long category = 0;
        string webroot;
        PubHelper pubHelper = new PubHelper();
        WebClient wc = new WebClient();

        public MpHelper()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Path.Combine(AppContext.BaseDirectory))
              .AddJsonFile("Config/Main.json", optional: true, reloadOnChange: true)
              .AddJsonFile("Config/Mp.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
            author = long.Parse(configuration.GetSection("author").Value);
            category = long.Parse(configuration.GetSection("category").Value);
            webroot = configuration.GetSection("webroot").Value;
            ServicePointManager.ServerCertificateValidationCallback +=
           (sender, cert, chain, sslPolicyErrors) => true;
        }

        public void ComeOn(string item)
        {
            var filters = configuration.GetSection("filters").GetChildren();
            var domain = configuration.GetSection("domain").Value;
            Console.WriteLine("开始采集" + item);
            try
            {
                var html = ReqHelper.GetHtml(item
                           , ugent: "Mozilla/5.0 (iPhone; CPU iPhone OS 8_0 like Mac OS X) AppleWebKit/600.1.4 (KHTML, like Gecko) Mobile/12A365 MicroMessenger/5.4.1 NetType/WIFI");
                var parse = new HtmlParser();
                var doc = parse.ParseDocument(html);

                //开始进行元素过滤
                foreach (var filter in filters)
                {
                    var eles = doc.QuerySelectorAll(filter.Value); //根据写的规则进行过滤
                    if (eles.Length > 0)
                    {
                        foreach (var ele in eles)
                        {
                            ele.Remove();
                        }
                    }

                }

                var imgs = doc.QuerySelectorAll("#js_content img");
                var date = DateTime.Now.ToString("yyyy-MM-dd");
                var downloadDir = Path.Combine(webroot, "wp-content/uploads/" + date); //拼接图片下载目录
                if (!Directory.Exists(downloadDir))
                {
                    Directory.CreateDirectory(downloadDir);
                }
                foreach (IHtmlImageElement img in imgs)
                {
                    var imgUrl = img.GetAttribute("data-src");
                    var fileName = Guid.NewGuid().ToString() + ".jpg";
                    wc.DownloadFile(imgUrl, Path.Combine(downloadDir, fileName));
                    Console.WriteLine("下载" + imgUrl + "到" + downloadDir);
                    img.SetAttribute("src", Flurl.Url.Combine(domain, "/wp-content/uploads/" + date + "/" + fileName)); //设置img的src为下载路径 
                    img.RemoveAttribute("data-src");
                    img.RemoveAttribute("data-ratio");
                    img.RemoveAttribute("data-type");
                    img.RemoveAttribute("style");
                    img.RemoveAttribute("class");
                    img.RemoveAttribute("data-w");
                }


                var addConfigs = configuration.GetSection("add").GetChildren();
                if (addConfigs.Count() > 0)
                {
                    foreach (var config in addConfigs)
                    {
                        var node = doc.QuerySelector(config["parent"]); //拿到父节点  
                        var context = BrowsingContext.New();
                        var document = context.OpenAsync(m => m.Content(config["content"])).Result;
                        var element = document.QuerySelector(config["tag"]);
                        node.AppendChild(element); //插入节点

                    }
                }

                var el = doc.QuerySelector("#js_content");
                el.RemoveAttribute("style");
                var title = doc.QuerySelector("h2").TextContent.Trim();
                //var style = doc.QuerySelector("style").InnerHtml;
                var content = el.OuterHtml;
                var replaces = configuration.GetSection("replace").GetChildren();
                foreach (var replace in replaces)
                {
                    if (!string.IsNullOrEmpty(replace["old"].ToString()) && !string.IsNullOrEmpty(replace["new"].ToString()))
                    {
                        content = content.Replace(replace["old"].ToString(), replace["new"].ToString());
                    }

                }
                //StringBuilder sb = new StringBuilder();
                //sb.AppendLine("<style>");
                //sb.AppendLine(style);
                //sb.AppendLine("</style>");
                //sb.AppendLine(content);
                //var htmlStr = sb.ToString();

                var addConfig = configuration.GetSection("add").GetChildren();
                pubHelper.Post(title, content, category, author, new List<string>());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace.ToString());
                File.AppendAllText("err.txt", ex.Message + Environment.NewLine);
            }
        }
    }
}
