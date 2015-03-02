namespace RDV.Domain.Enums
{
    /// <summary>
    /// Типы файлов с данными, поддерживающими импорт и экспорт
    /// </summary>
    public enum DataFormats: short
    {
        /// <summary>
        /// Формат электронных таблиц Microsoft Excel
        /// </summary>
        XLS = 1,

        /// <summary>
        /// Формат XML
        /// </summary>
        XML = 2
    }
}