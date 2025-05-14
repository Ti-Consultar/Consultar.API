using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace _4_InfraData._5_ConfigEnum
{
    public static class EnumExtensions
    {
        public static string GetDescription<TEnum>(this TEnum enumValue) where TEnum : Enum
        {
            FieldInfo fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null &&
                fieldInfo.GetCustomAttribute(typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                return attribute.Description;
            }

            return enumValue.ToString();
        }

        public static List<KeyValuePair<int, string>> ToDropdown<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                       .Cast<TEnum>()
                       .Select(value => new KeyValuePair<int, string>(
                           Convert.ToInt32(value),
                           value.GetDescription()))
                       .ToList();
        }
    }
}
