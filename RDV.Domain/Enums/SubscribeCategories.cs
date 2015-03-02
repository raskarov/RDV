using System;
using System.Linq;

namespace RDV.Domain.Enums
{
    /// <summary>
    /// Категории подписки
    /// </summary>
    [Flags]
    public enum SubscribeCategories: short
    {
        /// <summary>
        /// Обновления системы
        /// </summary>
        [EnumDescription("Обновления")]
        Updates = 1,

        /// <summary>
        /// Техническая информация
        /// </summary>
        [EnumDescription("Технические новости")]
        Tech = 2,

        /// <summary>
        /// Новости РДВ
        /// </summary>
        [EnumDescription("Новости РДВ")]
        RDVNews = 4
    }

    public static class SubscribeUtils
    {
        /// <summary>
        /// Возвращает все установленные флажки для рассылок
        /// </summary>
        /// <returns></returns>
        public static short All()
        {
            return (short)Enum.GetValues(typeof (SubscribeCategories)).Cast<short>().Aggregate(0, (current, asShort) => current | asShort);
        }
    }
}