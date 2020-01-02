using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WpSpider.Model;

namespace WpSpider.Pub
{
    public class PubContext : DbContext
    {
        IConfiguration configuration;
        public PubContext()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Path.Combine(AppContext.BaseDirectory))
              .AddJsonFile("Config/main.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Relationships> Relationships { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connstr = configuration.GetSection("dbConnstr").Value;
            optionsBuilder.UseMySQL(connstr);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var prefix = configuration.GetSection("prefix").Value;
            modelBuilder.Entity<Post>().ToTable(prefix + "posts").HasKey(p => p.Id);
            modelBuilder.Entity<Relationships>().ToTable(prefix + "term_relationships").HasKey(r => r.PostId);        
        }

    }
}
