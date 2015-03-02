using System.IO;
using RDV.Domain.Interfaces.ImportExport;
using RDV.Domain.Interfaces.ImportExport.Geo;
using RDV.Domain.Interfaces.Infrastructure;

namespace RDV.Domain.ImportExport.Geo
{
    /// <summary>
    /// Базовый гео импортер
    /// </summary>
    public abstract class GeoImporterBase: IGeoImporterBase
    {
        /// <summary>
        /// Гео менеджер
        /// </summary>
        public IGeoManager GeoManager { get; set; }

        /// <summary>
        /// Инъекция менеджера в конструкторе
        /// </summary>
        /// <param name="geoManager">Гео менеджер</param>
        protected GeoImporterBase(IGeoManager geoManager)
        {
            GeoManager = geoManager;
        }

        /// <summary>
        /// Импортирует данные из указанного потока
        /// </summary>
        /// <param name="stream">Поток с данными</param>
        /// <returns>Статистика импорта</returns>
        public abstract ImportStatistics ImportStream(Stream stream);

        /// <summary>
        /// Импортирует данные из указанного файла
        /// </summary>
        /// <param name="filename">Имя файла для импорта</param>
        /// <returns>Статистика импорта</returns>
        public abstract ImportStatistics ImportFile(string filename);
    }
}