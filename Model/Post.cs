using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WpSpider.Model
{
    public class Post
    {
        public long Id { get; set; }

        [Column("post_author")] //作者ID
        public long PostAuthor { get; set; }

        [Column("post_date")] //发布日期
        public DateTime PostDate { get; set; }

        [Column("post_date_gmt")] //发布日期GMT
        public DateTime PostDateGmt { get; set; }

        [Column("post_modified")] //修改日期
        public DateTime PostMod { get; set; }

        [Column("post_modified_gmt")] //修改日期GMT
        public DateTime PostModGmt { get; set; }

        private string postContent;
        [Column("post_content")] //发布内容
        public string PostContent
        {
            get
            {
                return postContent;
            }
            set
            {
                postContent = String.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string postTitle;
        [Column("post_title")] //标题
        public string PostTitle
        {
            get
            {
                return postTitle;
            }
            set
            {
                postTitle = String.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string postExcerpt;
        [Column("post_excerpt")] //摘录
        public string PostExcerpt
        {
            get
            {
                return postExcerpt;
            }
            set
            {
                postExcerpt = String.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        [Column("post_status")]
        public string PostStatus { get; set; }   

        [Column("post_name")]
        public string PostName { get; set; }

        [Column("ping_status")]
        public string PingStatus { get; set; }     

        [Column("to_ping")]
        public string ToPing { get; set; }

        [Column("pinged")]
        public string Pinged { get; set; }

        [Column("post_content_filtered")]
        public string PostContentFilter { get; set; }
    }
}
