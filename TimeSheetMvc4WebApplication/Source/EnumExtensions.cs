using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;


namespace CommonBase
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Получает строку с описанием для элемента enum
        /// </summary>
        /// <param name="enumObj">enum</param>
        /// <returns>Описание</returns>
        public static string Description(this Enum enumObj)
        {
            FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());
            object[] attribArray = fieldInfo.GetCustomAttributes(false);
            if (attribArray.Length == 0)
                return enumObj.ToString();

            DescriptionAttribute attrib = attribArray[0] as DescriptionAttribute;

            if (attrib != null)
                return attrib.Description;

            return enumObj.ToString();
        }
    }

}
