using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories.Geo;

namespace RDV.Domain.Infrastructure.Geo
{
    /// <summary>
    /// Реализация географического менеджера для работы с геосправочником
    /// </summary>
    public class GeoManager: IGeoManager
    {
        /// <summary>
        /// Репозиторий стран
        /// </summary>
        public IGeoCountriesRepository CountriesRepository { get; set; }

        /// <summary>
        /// Репозиторий регионов
        /// </summary>
        public IGeoRegionsRepository RegionsRepository { get; set; }

        /// <summary>
        /// Репозиторий районов регионов
        /// </summary>
        public IGeoRegionDistrictsRepository RegionsDistrictsRepository { get; set; }

        /// <summary>
        /// Репозиторий городов
        /// </summary>
        public IGeoCitiesRepository CitiesRepository { get; set; }

        /// <summary>
        /// Репозиторий районов
        /// </summary>
        public IGeoDistrictsRepository DistrictsRepository { get; set; }

        /// <summary>
        /// Репозиторий жилых массивов
        /// </summary>
        public IGeoResidentialAreasRepository ResidentialAreasRepository { get; set; }

        /// <summary>
        /// Репозиторий улиц
        /// </summary>
        public IGeoStreetsRepository StreetsRepository { get; set; }

        /// <summary>
        /// Репозиторий объектов
        /// </summary>
        public IGeoObjectsRepository ObjectsRepository { get; set; }

        /// <summary>
        /// Стандартный конструктор для инъекции зависимостей репозиториев
        /// </summary>
        /// <param name="countriesRepository">Репозиторий стран</param>
        /// <param name="regionsRepository">Репозиторий регионов</param>
        /// <param name="geoRegionDistricts">Репозиторий регионов у районов</param>
        /// <param name="citiesRepository">Репозиторий городов</param>
        /// <param name="districtsRepository">Репозиторий районов</param>
        /// <param name="residentialAreasRepository">Репозиторий жилых массивов</param>
        /// <param name="streetsRepository">Репозиторий улиц</param>
        /// <param name="objectsRepository">Репозиторий объектов на улицах</param>
        public GeoManager(IGeoCountriesRepository countriesRepository, IGeoRegionsRepository regionsRepository, IGeoRegionDistrictsRepository geoRegionDistricts, IGeoCitiesRepository citiesRepository, IGeoDistrictsRepository districtsRepository, IGeoResidentialAreasRepository residentialAreasRepository, IGeoStreetsRepository streetsRepository, IGeoObjectsRepository objectsRepository)
        {
            CountriesRepository = countriesRepository;
            RegionsRepository = regionsRepository;
            RegionsDistrictsRepository = geoRegionDistricts;
            CitiesRepository = citiesRepository;
            DistrictsRepository = districtsRepository;
            ResidentialAreasRepository = residentialAreasRepository;
            StreetsRepository = streetsRepository;
            ObjectsRepository = objectsRepository;
        }
    }
}