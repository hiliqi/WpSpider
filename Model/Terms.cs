using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpSpider.Model
{
    public class Terms
    {
        [SugarColumn(ColumnName = "term_id", IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(ColumnName = "name")]
        public string Name { get; set; }

        [SugarColumn(ColumnName = "slug")]
        public string Slug { get; set; }

        [SugarColumn(ColumnName = "term_group")]
        public int TermGroup { get; set; }
    }
}
