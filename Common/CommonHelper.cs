using RuPeng.Common.NetCore;
using System;
using System.Collections.Generic;
using System.Text;
using TinyPinyin.Core;

namespace WpSpider.Common
{
    public class CommonHelper
    {
        public static long GetTimeStamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }

        public static string GetPinyin(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in str)
            {
                if (!char.IsWhiteSpace(item))
                {
                    if (PinyinHelper.IsChinese(item))
                    {
                        sb.Append(PinyinHelper.GetPinyin(item));
                    }
                }

            }
            return sb.ToString().ToLower();
        }

        public static string GetKey()
        {
            return MD5Helper.ComputeMd5(GetMachineCodeString() + "raccoon").ToUpper();
        }

        public static string GetMachineCodeString()
        {
            var id = Environment.MachineName + Environment.UserName + Environment.UserDomainName;
            return MD5Helper.ComputeMd5(id);
        }
    }
}
