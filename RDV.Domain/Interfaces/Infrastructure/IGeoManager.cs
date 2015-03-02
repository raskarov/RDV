using RDV.Domain.Interfaces.Repositories.Geo;

namespace RDV.Domain.Interfaces.Infrastructure
{
    /// <summary>
    /// Географический менеджер для работы с гео справочником
    /// </summary>
    public interface IGeoManager
    {
        /// <summary>
        /// Репозиторий стран
        /// </summary>
        IGeoCountriesRepository CountriesRepository { get; set; }

        /// <summary>
        /// Репозиторий регионов
        /// </summary>
        IGeoRegionsRepository RegionsRepository { get; set; }

        /// <summary>
        /// Репозиторий районов регионов
        /// </summary>
        IGeoRegionDistrictsRepository RegionsDistrictsRepository { get; set; }

        /// <summary>
        /// Репозиторий городов
        /// </summary>
        IGeoCitiesRepository CitiesRepository { get; set; }

        /// <summary>
        /// Репозиторий районов
        /// </summary>
        IGeoDistrictsRepository DistrictsRepository { get; set; }

        /// <summary>
        /// Репозиторий жилых массивов
        /// </summary>
        IGeoResidentialAreasRepository ResidentialAreasRepository { get; set; }

        /// <summary>
        /// Репозиторий улиц
        /// </summary>
        IGeoStreetsRepository StreetsRepository { get; set; }

        /// <summary>
        /// Репозиторий объектов
        /// </summary>
        IGeoObjectsRepository ObjectsRepository { get; set; }
    }
}