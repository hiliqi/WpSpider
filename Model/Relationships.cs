using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpSpider.Model
{
    public class Relationships
    {
        [SugarColumn(ColumnName = "object_id")]
        public long PostId { get; set; }

        [SugarColumn(ColumnName = "term_taxonomy_id")]
        public long CateId { get; set; }

        [SugarColumn(ColumnName = "term_order")]
        public int TermOrder { get; set; }
    }
}
