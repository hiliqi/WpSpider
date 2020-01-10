using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using WpSpider.Pub;
using System.Linq;
using AngleSharp;

namespace WpSpider.Spider
{
    public class Mp : ISpider
    {
        Microsoft.Extensions.Configuration.IConfiguration configuration;
        long author = 0;
        long category = 0;
        string webroot;
        PubHelper pubHelper = new PubHelper();
        WebClient wc = new WebClient();
        public Mp()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Path.Combine(AppContext.BaseDirectory))
              .AddJsonFile("Config/mp.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
            author = long.Parse(configuration.GetSection("author").Value);
            category = long.Parse(configuration.GetSection("category").Value);
            webroot = configuration.GetSection("webroot").Value;
            ServicePointManager.ServerCertificateValidationCallback +=
           (sender, cert, chain, sslPolicyErrors) => true;
        }
        public void Go()
        {
            var urls = configuration.GetSection("urls").GetChildren();
            var filters = configuration.GetSection("filters").GetChildren();           

            foreach (var item in urls)
            {
                Console.WriteLine("开始采集" + item.Value);
                try
                {
                    var html = ReqHelper.GetHtml(item.Value
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
                        img.Source = Path.Combine("/wp-content/uploads/" + date, fileName);//设置img的src为下载路径
                        img.RemoveAttribute("data-src"); //移除data-src属性
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


                    var title = doc.QuerySelector("h2").TextContent.Trim();
                    var style = doc.QuerySelector("style").OuterHtml;
                    var content = doc.QuerySelector("#js_content").OuterHtml;

                    var htmlStr = style + "<p></p>" + content;

                    var addConfig = configuration.GetSection("add").GetChildren();
                    pubHelper.Post(title, htmlStr, category, author);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    File.AppendAllText("err.txt", ex.Message + Environment.NewLine);
                }
            }

        }
    }
}
