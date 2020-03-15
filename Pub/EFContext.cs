using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WpSpider.Model;

namespace WpSpider.Pub
{
    public class EFContext : DbContext
    {
        IConfiguration configuration;
        public EFContext()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Path.Combine(AppContext.BaseDirectory))
              .AddJsonFile("Config/Main.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Relationships> Relationships { get; set; }
        public DbSet<Terms> Terms { get; set; }

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
            modelBuilder.Entity<Terms>().ToTable(prefix + "terms").HasKey(t => t.Id);
        }

    }
}
