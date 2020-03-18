using AngleSharp.Html.Parser;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using WpSpider.Pub;

namespace WpSpider.Spider
{
    public class Blfuman : ISpider
    {
        IConfiguration configuration;
        MpHelper mpHelper = new MpHelper();

        public Blfuman()
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Path.Combine(AppContext.BaseDirectory))
             .AddJsonFile("Config/Blfuman.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
        }
        public void Go()
        {
            var urls = configuration.GetSection("urls").GetChildren();
            foreach (var url in urls)
            {
                mpHelper.ComeOn(url.Value.Replace("http:", "https:"));               
            }          
        }      
    }
}
