using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using TinyPinyin.Core;
using WpSpider.Model;

namespace WpSpider.Pub
{
    public class PubHelper
    {
        IConfiguration configuration;
        string webroot;
        WebClient webClient = new WebClient();
        EFContext pubContext = new EFContext();
        SugarContext sugarContext = new SugarContext();

        public PubHelper()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(AppContext.BaseDirectory))
            .AddJsonFile("Config/Main.json", optional: true, reloadOnChange: true);
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
                var id = sugarContext.Db.Insertable(post).ExecuteReturnIdentity();
                Console.WriteLine($"成功发布文章{title}，id为{id}");
                var c = sugarContext.Db.Queryable<TermTaxonomy>().Where(t => t.TermId == category).First();
                long termTaxonomyId = c == null ? 1 : c.Id; //查出该分类对应的TermTaxonomy的ID
                var relationships = new Relationships()
                {
                    PostId = id,
                    CateId = termTaxonomyId
                };
                sugarContext.Db.Insertable(relationships).ExecuteCommand();
                //foreach (var tag in tags)
                //{
                //    var term = sugarContext.Db.Queryable<Terms>().Where(t => t.Name == tag).First();
                //    if (term == null) //如果该tag不存在，新建标签以及对应的TermTaxonomy
                //    {
                //        term = new Terms()
                //        {
                //            Name = tag,
                //            Slug = Common.CommonHelper.GetPinyin(tag),
                //            TermGroup = 0
                //        };
                //        term.Id = sugarContext.Db.Insertable(term).ExecuteReturnIdentity();
                //        var termTaxonomy = new TermTaxonomy()
                //        {
                //            TermId = term.Id,
                //            Taxonomy = "post_tag",
                //            Parent = 0,
                //            Count = 0,
                //            Description = "post_tag"
                //        };
                //        termTaxonomyId = sugarContext.Db.Insertable(termTaxonomy).ExecuteReturnIdentity();
                //    }
                //    else //如果tag已存在，则查出它在termTaxonomy表中的termTaxonomyId
                //    {
                //        termTaxonomyId = sugarContext.Db.Queryable<TermTaxonomy>().Where(t => t.TermId == term.Id).First().Id;
                //    }
                //    relationships = new Relationships()
                //    {
                //        PostId = id,
                //        CateId = termTaxonomyId
                //    };
                //    sugarContext.Db.Insertable(relationships).ExecuteCommand();
                //}

            }
            catch (Exception ex)
            {
                File.AppendAllText("err.txt", ex.Message + Environment.NewLine);
                throw ex;
            }
        }
    }
}
