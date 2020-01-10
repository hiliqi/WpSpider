using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using WpSpider.Model;

namespace WpSpider.Pub
{
    public class PubHelper
    {
        IConfiguration configuration;
        string webroot;
        WebClient webClient = new WebClient();
        PubContext pubContext = new PubContext();

        public PubHelper()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(AppContext.BaseDirectory))
            .AddJsonFile("Config/main.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
            webroot = configuration.GetSection("webroot").Value;
        }

        public void Post(string title, string html, long category, long author)
        {
            try
            {
                var post = new Post()
                {
                    PostAuthor = author,
                    PostDate = DateTime.Now.ToLocalTime(),
                    PostDateGmt = DateTime.Now.ToLocalTime(),
                    PostContent = html,
                    PostMod = DateTime.Now.ToLocalTime(),
                    PostModGmt = DateTime.Now.ToLocalTime(),
                    PostTitle = title,
                    PostStatus = "publish",
                    PingStatus = "closed",
                    PostExcerpt = string.Empty,
                    ToPing = string.Empty,
                    Pinged = string.Empty,
                    PostName = Guid.NewGuid().ToString(),
                    PostContentFilter = string.Empty
                };
                using (var tran = pubContext.Database.BeginTransaction())
                {
                    try
                    {
                        pubContext.Posts.Add(post);
                        pubContext.SaveChanges();
                        var relationships = new Relationships()
                        {
                            PostId = post.Id,
                            CateId = category
                        };
                        pubContext.Relationships.Add(relationships);
                        pubContext.SaveChanges();
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        File.AppendAllText("err.txt", ex.Message + Environment.NewLine);
                        tran.Rollback();
                    }
                }
                Console.WriteLine("成功发布文章" + title);
            }
            catch (Exception ex)
            {
                File.AppendAllText("err.txt", ex.Message + Environment.NewLine);
                throw ex;
            }
        }
    }
}
