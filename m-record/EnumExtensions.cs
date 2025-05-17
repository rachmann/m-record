using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.ComponentModel;
using System.Reflection;

namespace m_record
{
    public static class EnumExtensions
    {
        public static string ToDescription(this Enum value)
        {
            if (value == null)
                return string.Empty;

            FieldInfo? fi = value.GetType().GetField(value.ToString());
            if (fi != null)
            {
                var attr = fi.GetCustomAttribute<DescriptionAttribute>();
                if (attr != null)
                    return attr.Description;
            }
            return value.ToString();
        }

        public static TEnum FromDescription<TEnum>(string description) where TEnum : struct, Enum
        {
            foreach (var field in typeof(TEnum).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                {
                    if (attr.Description == description)
                        return (TEnum)field.GetValue(null)!;
                }
                else
                {
                    if (field.Name == description)
                        return (TEnum)field.GetValue(null)!;
                }
            }
            // Return default (0) if no match found
            return default;
        }
    }
}