namespace RDV.Domain.Enums
{
    /// <summary>
    /// Типы медиа объектов, доступных для загрузки в карточку объектов
    /// </summary>
    public enum ObjectMediaTypes: short
    {
        /// <summary>
        /// Фотография
        /// </summary>
        [EnumDescription("Фото")]
        Photo = 1,

        /// <summary>
        /// Видео
        /// </summary>
        [EnumDescription("Видео")]
        Video = 2
    }
}