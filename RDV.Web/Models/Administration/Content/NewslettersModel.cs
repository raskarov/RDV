namespace RDV.Web.Models.Administration.Content
{
    /// <summary>
    /// Модель используемая при создании рассылок
    /// </summary>
    public class NewslettersModel
    {
        /// <summary>
        /// Заголовок рассылки
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Содержимое
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Категория получателей
        /// </summary>
        public int Recipients { get; set; }
    }
}