namespace RDV.CAN.Special.Models
{
    /// <summary>
    /// Модель элемента формы обратной связи
    /// </summary>
    public class FeedbackModel
    {
        /// <summary>
        /// Имя отправителя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Его Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Телефон
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Сообщение
        /// </summary>
        public string Content { get; set; }
    }
}