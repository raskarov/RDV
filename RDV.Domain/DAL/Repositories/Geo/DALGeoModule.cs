using Autofac;
using Autofac.Integration.Mvc;
using RDV.Domain.Interfaces.Repositories.Geo;

namespace RDV.Domain.DAL.Repositories.Geo
{
    /// <summary>
    /// Модуль регистрации зависимостей гео справочника
    /// </summary>
    public class DALGeoModule: Module
    {
        /// <summary>
        /// Загружает зависимости в строительный контейнер
        /// </summary>
        /// <param name="builder">Построитель контейнера</param>
        protected override void Load(ContainerBuilder builder)
        {
            // NOTE: зависимости исключительно InstancePerHttpRequest для использования в гео менеджере
            builder.RegisterType<GeoCountriesRepository>().As<IGeoCountriesRepository>().InstancePerHttpRequest();
            builder.RegisterType<GeoRegionsRepository>().As<IGeoRegionsRepository>().InstancePerHttpRequest();
            builder.RegisterType<GeoRegionDistrictsRepository>().As<IGeoRegionDistrictsRepository>().InstancePerHttpRequest();
            builder.RegisterType<GeoCitiesRepository>().As<IGeoCitiesRepository>().InstancePerHttpRequest();
            builder.RegisterType<GeoDistrictsRepository>().As<IGeoDistrictsRepository>().InstancePerHttpRequest();
            builder.RegisterType<GeoResidentialAreasRepository>().As<IGeoResidentialAreasRepository>().InstancePerHttpRequest();
            builder.RegisterType<GeoStreetsRepository>().As<IGeoStreetsRepository>().InstancePerHttpRequest();
            builder.RegisterType<GeoObjectsRepository>().As<IGeoObjectsRepository>().InstancePerHttpRequest();
        }
    }
}