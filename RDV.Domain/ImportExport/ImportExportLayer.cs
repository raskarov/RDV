using Autofac;
using RDV.Domain.ImportExport.Geo;
using RDV.Domain.Interfaces.ImportExport.Geo;

namespace RDV.Domain.ImportExport
{
    /// <summary>
    /// Модуль регистрации зависимостей импорта-экспорта
    /// </summary>
    public class ImportExportLayer: Module
    {
        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        /// <param name="builder">The builder through which components can be
        ///             registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GeoXLSImporter>().As<IGeoXLSImporter>();
            builder.RegisterType<GeoXMLImporter>().As<IGeoXMLImporter>();
        }
    }
}