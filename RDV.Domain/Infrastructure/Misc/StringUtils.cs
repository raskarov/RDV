using System.Text;
using Newtonsoft.Json.Linq;

namespace RDV.Domain.Infrastructure.Misc
{
    /// <summary>
    /// Статический класс содержащий утилитарные методы для работ со строками
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Генерирует строку, состояющую из указанного символа в указанном количестве
        /// </summary>
        /// <param name="character">Символ</param>
        /// <param name="count">Количество</param>
        /// <returns>Цельная строка</returns>
        public static string GenerateString(char character, int count)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < count; i++)
            {
                sb.Append(character);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Нормализует строку, содержащую номер телефона
        /// </summary>
        /// <param name="phone">Номер телефона</param>
        /// <returns>Нормализованный номер телефона</returns>
        public static string NormalizePhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return "";
            }
            var str = new StringBuilder(phone);
            str.Replace("-", string.Empty).Replace(")", string.Empty).Replace("(", string.Empty).Replace("+7", "7").Replace(" ","");
            if (phone.StartsWith("8"))
            {
                str[0] = '7';
            }
            return str.ToString();
        }

        /// <summary>
        /// Форматирует номер телефона для удобного отображения на сайте
        /// </summary>
        /// <param name="phone">Номер телефона</param>
        /// <returns></returns>
        public static string FormatPhoneNumber(this string phone)
        {
            var ph = NormalizePhoneNumber(phone);
            var sb = new StringBuilder(ph);
            if (sb.Length == 11)
            {
                return sb.Insert(0, '+').Insert(2, '-').Insert(6, '-').Insert(10, '-').Insert(13, '-').ToString();
            }
            else if (sb.Length == 6)
            {
                return sb.Insert(2, "-").Insert(5, "-").ToString();
            }
            else return ph;
        }

        /// <summary>
        /// Форматирует число как цену, разбивая его на разряды
        /// </summary>
        /// <param name="price">Цена</param>
        /// <returns></returns>
        public static string FormatPrice(this double? price)
        {
            if (price == null)
            {
                return string.Empty;
            }
            return string.Format("{0:N}", (long)price.Value).Replace(",00","");
        }

        /// <summary>
        /// Обрезает меньше указанного количества символов и возвращает строку с троеточием
        /// </summary>
        /// <param name="str">Строка</param>
        /// <param name="length">Длина</param>
        /// <returns></returns>
        public static string TrimEllipsis(this string str, int length)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            if (str.Length <= length)
            {
                return str;
            }
            else
            {
                return string.Format("{0}...", str.Substring(0, length));
            }
        }
    }
}