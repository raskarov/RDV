using System.IO;
using RDV.Domain.Interfaces.ImportExport;
using RDV.Domain.Interfaces.ImportExport.Geo;
using RDV.Domain.Interfaces.Infrastructure;

namespace RDV.Domain.ImportExport.Geo
{
    /// <summary>
    /// Реализация импортера из XML данных
    /// </summary>
    public class GeoXMLImporter: GeoImporterBase, IGeoXMLImporter
    {
        /// <summary>
        /// Инъекция зависимости в конструкторе
        /// </summary>
        /// <param name="geoManager">Гео менеджер</param>
        public GeoXMLImporter(IGeoManager geoManager) : base(geoManager)
        {
        }

        /// <summary>
        /// Импортирует данные из указанного потока
        /// </summary>
        /// <param name="stream">Поток с данными</param>
        /// <returns>Статистика импорта</returns>
        public override ImportStatistics ImportStream(Stream stream)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Импортирует данные из указанного файла
        /// </summary>
        /// <param name="filename">Имя файла для импорта</param>
        /// <returns>Статистика импорта</returns>
        public override ImportStatistics ImportFile(string filename)
        {
            throw new System.NotImplementedException();
        }
    }
}