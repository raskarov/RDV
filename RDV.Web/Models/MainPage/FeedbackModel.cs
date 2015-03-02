namespace RDV.Web.Models.MainPage
{
    /// <summary>
    /// Модель формы обратной связи
    /// </summary>
    public class FeedbackModel
    {
        /// <summary>
        /// Имя отправителя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Телефон
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Самосообщение
        /// </summary>
        public string Message { get; set; }
    }
}