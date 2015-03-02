using System.IO;
using RDV.Domain.Interfaces.Infrastructure;

namespace RDV.Domain.Interfaces.ImportExport.Geo
{
    /// <summary>
    /// Абстрактный интерфейс импортера гео объектов из XLS файла
    /// </summary>
    public interface IGeoImporterBase : IImporter
    {
        /// <summary>
        /// Гео менеджер
        /// </summary>
        IGeoManager GeoManager { get; set; }
    }
}
