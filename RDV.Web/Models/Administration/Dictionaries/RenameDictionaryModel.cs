namespace RDV.Web.Models.Administration.Dictionaries
{
    /// <summary>
    /// Модель используемая для переименования справочника
    /// </summary>
    public class RenameDictionaryModel
    {
        /// <summary>
        /// Идентификатор переименовываемого справочника
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Системное имя справочника
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Отображаемое имя
        /// </summary>
        public string DisplayName { get; set; } 
    }
}