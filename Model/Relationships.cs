using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpSpider.Model
{
    public class Relationships
    {
        [Column("object_id")]
        public long PostId { get; set; }

        [Column("term_taxonomy_id")]
        public long CateId { get; set; }
    }
}
