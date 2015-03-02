namespace RDV.Web.Models.MainPage
{
    /// <summary>
    /// Модель, используемая для рапорта об ошибке
    /// </summary>
    public class BugReportModel
    {
        /// <summary>
        /// Имя автора сообщенич
        /// </summary>
        public string ReporterName { get; set; }

        /// <summary>
        /// Email автора
        /// </summary>
        public string ReporterEmail { get; set; }

        /// <summary>
        /// Адрес страницы где ошибка
        /// </summary>
        public string ErrorLocation { get; set; }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        public string ReportMessage { get; set; }

        /// <summary>
        /// Тип сообщения
        /// </summary>
        public string Subject { get; set; }
    }
}