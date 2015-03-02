using System;

namespace RDV.Web.Classes.Extensions
{
    /// <summary>
    /// Статический класс, содержащий расширения, для выполнения конвертаций и преобразований
    /// </summary>
    public static class ConversionExtensions
    {
        /// <summary>
        /// Выполняет конвертацию данных с плавающей запятой в строку, с фиксированным форматом
        /// </summary>
        /// <param name="value">Значение</param>
        /// <returns></returns>
        public static string FormatString(this double? value)
        {
            return value.HasValue ? value.Value.ToString("0.0") : string.Empty;
        }

        /// <summary>
        /// Выполняет конвертацию данных строки в число которое может быть null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int? ConvertToNullableInt(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new int?();
            }
            else
            {
                return Convert.ToInt32(value);
            }
        }

        /// <summary>
        /// Выполняет конвертацию данных строки в число которое может быть null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long? ConvertToNullableLong(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new long?();
            }
            else
            {
                return Convert.ToInt64(value);
            }
        }

        /// <summary>
        /// Выполняет конвертацию данных строки в булевый тип которое может быть null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool? ConvertToNullableBool(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new bool?();
            }
            else
            {
                return Convert.ToBoolean(value);
            }
        }

        /// <summary>
        /// Выполняет конвертацию данных строки в число с плавающей запятой тип которое может быть null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double? ConvertToNullableDouble(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new double?();
            }
            else
            {
                return Convert.ToDouble(value);
            }
        }

        /// <summary>
        /// Выполняет конвертацию данных строки в дату и время которое может быть null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime? ConvertToNullableDateTime(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new DateTime?();
            }
            else
            {
                return Convert.ToDateTime(value);
            }
        }

        /// <summary>
        /// Выполняет конвертацию данных строки идентификатор географического объекта
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long ConvertToGeoId(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return -1;
            }
            else
            {
                return Convert.ToInt64(value);
            }
        }

        /// <summary>
        /// Выполняет преобразование количества байт в размер, пригодный для понимая и оценки
        /// </summary>
        /// <param name="value">Размер данных в байтах</param>
        /// <returns>Строка размера</returns>
        public static string ToFileSize(this long value)
        {
            var prefix = "байт";
            double multipler = 1.0;
            if (value > 1024 * 1024 * 1024)
            {
                prefix = "гигабайт";
                multipler = 1024.0 * 1024.0 * 1024;
            } else
            if (value > 1024 * 1024)
            {
                prefix = "мегабайт";
                multipler = 1024.0 * 1024.0;
            } else
            if (value > 1024)
            {
                prefix = "килобайт";
                multipler = 1024.0;
            }
            return string.Format("{0:0.00} {1}", Convert.ToDouble(value) / multipler, prefix);
        }

        
    }
}