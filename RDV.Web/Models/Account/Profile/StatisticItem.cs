namespace RDV.Web.Models.Account.Profile
{
    /// <summary>
    /// Элемент используемый для отображения статистики в профиле
    /// </summary>
    public class StatisticItem
    {
        /// <summary>
        /// Наименование элемента
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// Значение элемента
        /// </summary>
        public string ItemValue { get; set; }

        /// <summary>
        /// Группа статистических элементов
        /// </summary>
        public string ItemsGroup { get; set; }

        /// <summary>
        /// Инициализирует статистический элемент с указанными параметрами
        /// </summary>
        /// <param name="itemName">Наименование показателя</param>
        /// <param name="itemValue">Значение показателя</param>
        public StatisticItem(string itemName, string itemValue)
        {
            ItemName = itemName;
            ItemValue = itemValue;
        }
    }
}