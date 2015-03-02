namespace RDV.Domain.Enums
{
    /// <summary>
    /// Типы статей
    /// </summary>
    public enum ArticleTypes: short
    {
        /// <summary>
        /// Новость
        /// </summary>
        [EnumDescription("Новость")]
        News = 1,

        /// <summary>
        /// Видеорепортаж = новость
        /// </summary>
        [EnumDescription("Видеорепортаж - новость")]
        VideoNews = 2,

        /// <summary>
        /// Видеорепортаж - интервью
        /// </summary>
        [EnumDescription("Видеорепортаж - интервью")]
        VideoInterview = 3,

        /// <summary>
        /// Видеорепортаж - презентация
        /// </summary>
        [EnumDescription("Видеорепортаж - презентация")]
        VideoPresentation = 4,

        /// <summary>
        /// Событие календаря
        /// </summary>
        [EnumDescription("Событие календаря")]
        CalendarEvent = 5,

        /// <summary>
        /// Событие календаря
        /// </summary>
        [EnumDescription("Новость Учебного Центра")]
        TraningCenterNews = 6,
    }
}