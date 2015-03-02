namespace RDV.Domain.Interfaces.ImportExport
{
    /// <summary>
    /// Объект содержащий результаты импортирования
    /// </summary>
    public class ImportStatistics
    {
        /// <summary>
        /// Количество импортированных элементов
        /// </summary>
        public long ImportedCount { get; set; }

        /// <summary>
        /// Количество не импортированных элементов
        /// </summary>
        public long UnImportedCount { get; set; }

        /// <summary>
        /// Сообщение о результате импорта
        /// </summary>
        public string ResultMessage { get; set; }
    }
}