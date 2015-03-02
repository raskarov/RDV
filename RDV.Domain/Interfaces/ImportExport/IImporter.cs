using System.IO;

namespace RDV.Domain.Interfaces.ImportExport
{
    /// <summary>
    /// Базовый интерфейс для всех импортеров
    /// </summary>
    public interface IImporter
    {
        /// <summary>
        /// Импортирует данные из указанного потока
        /// </summary>
        /// <param name="stream">Поток с данными</param>
        /// <returns>Статистика импорта</returns>
        ImportStatistics ImportStream(Stream stream);

        /// <summary>
        /// Импортирует данные из указанного файла
        /// </summary>
        /// <param name="filename">Имя файла для импорта</param>
        /// <returns>Статистика импорта</returns>
        ImportStatistics ImportFile(string filename);
    }
}