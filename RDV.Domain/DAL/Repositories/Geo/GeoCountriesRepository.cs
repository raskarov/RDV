using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories.Geo;

namespace RDV.Domain.DAL.Repositories.Geo
{
    /// <summary>
    /// СУБД реализация репозитория стран
    /// </summary>
    public class GeoCountriesRepository: BaseRepository<GeoCountry>, IGeoCountriesRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext"></param>
        public GeoCountriesRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override GeoCountry Load(long id)
        {
            return Find(c => c.Id == id);
        }

        /// <summary>
        /// Возвращает страну по ее наименование. Поиск происходит без учета регистра
        /// </summary>
        /// <param name="countryName">Наименование страны</param>
        /// <returns>Страна или null если не найдено</returns>
        public GeoCountry GetCountryByName(string countryName)
        {
            return Find(c => c.Name.ToLower() == countryName.ToLower());
        }
    }
}