namespace RDV.Web.Models.Administration.Dictionaries
{
    /// <summary>
    /// Модель используемая для создания нового справочника
    /// </summary>
    public class NewDictionaryModel
    {
        /// <summary>
        /// Системное имя справочника
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Отображаемое имя
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Значения по умолчанию, разделенные запятыми
        /// </summary>
        public string DefaultValues { get; set; }
    }
}