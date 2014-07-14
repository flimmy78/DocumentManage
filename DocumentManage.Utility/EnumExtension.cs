using System;
using System.Reflection;

namespace DocumentManage.Utility
{

    [AttributeUsage(AttributeTargets.Field)]
    public class EnumExtension : Attribute
    {
        public string Text { get; set; }

        public EnumExtension(string str)
        {
            Text = str;
        }

        public static string Get(Type tp, string name)
        {
            MemberInfo[] mi = tp.GetMember(name);
            if (mi.Length > 0)
            {
                var attr = GetCustomAttribute(mi[0], typeof(EnumExtension)) as EnumExtension;
                if (attr != null)
                {
                    return attr.Text;
                }
            }
            return string.Empty;
        }

        public static string Get(object enm)
        {
            if (enm != null)
            {
                MemberInfo[] mi = enm.GetType().GetMember(enm.ToString());
                if (mi.Length > 0)
                {
                    var attr = GetCustomAttribute(mi[0], typeof(EnumExtension)) as EnumExtension;
                    if (attr != null)
                    {
                        return attr.Text;
                    }
                }
            }
            return string.Empty;
        }
    }
}
