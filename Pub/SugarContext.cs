using Microsoft.Extensions.Configuration;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WpSpider.Model;

namespace WpSpider.Pub
{
    public class SugarContext
    {
        public SqlSugarClient Db;
        public SugarContext()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(AppContext.BaseDirectory))
            .AddJsonFile("Config/Main.json", optional: true, reloadOnChange: true);
            IConfiguration configuration = builder.Build();
            Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = configuration.GetSection("dbConnstr").Value,
                DbType = DbType.MySql,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
            var prefix = configuration.GetSection("prefix").Value;
            Db.MappingTables.Add("Post", prefix + "posts");
            Db.MappingTables.Add("Relationships", prefix + "term_relationships");
            Db.MappingTables.Add("Terms", prefix + "terms");
            Db.MappingTables.Add("TermTaxonomy", prefix + "term_taxonomy");
        }
      
        public SimpleClient<Post> Post => new SimpleClient<Post>(Db);
        public SimpleClient<Relationships> Relationships => new SimpleClient<Relationships>(Db);
        public SimpleClient<Terms> Terms => new SimpleClient<Terms>(Db);
        public SimpleClient<TermTaxonomy> TermTaxonomy => new SimpleClient<TermTaxonomy>(Db);
    }
}
