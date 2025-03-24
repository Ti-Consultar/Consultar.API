using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace _4_InfraData._5_ConfigEnum
{
    public static class EnumExtensions
    {
        public static string GetDescription<TEnum>(this TEnum enumValue) where TEnum : Enum
        {
            FieldInfo fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo.GetCustomAttribute(typeof(EnumDescriptionAttribute)) is EnumDescriptionAttribute attribute)
            {
                return attribute.Description;
            }

            return enumValue.ToString();
        }
    }
}
