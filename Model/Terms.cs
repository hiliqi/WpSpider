using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpSpider.Model
{
    public class Terms
    {
        [Column("term_id")]
        public long Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("slug")]
        public string Slug { get; set; }

        [Column("term_group")]
        public int TermGroup { get; set; }
    }
}
