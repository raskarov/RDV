using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories.Geo
{
    /// <summary>
    /// Абстрактный репозиторий стран
    /// </summary>
    public interface IGeoCountriesRepository: IBaseRepository<GeoCountry>
    {
        /// <summary>
        /// Возвращает страну по ее наименование. Поиск происходит без учета регистра
        /// </summary>
        /// <param name="countryName">Наименование страны</param>
        /// <returns>Страна или null если не найдено</returns>
        GeoCountry GetCountryByName(string countryName);
    }
}