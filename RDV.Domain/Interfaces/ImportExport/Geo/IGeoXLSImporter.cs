namespace RDV.Domain.Interfaces.ImportExport.Geo
{
    /// <summary>
    /// Абстрактный импортер из Excel фората
    /// </summary>
    public interface IGeoXLSImporter: IGeoImporterBase
    {
        /// <summary>
        /// Количество рядов, которое необходимо пропустить от начала таблицы для начала импорта
        /// </summary>
        int SkipRow { get; set; }
    }
}