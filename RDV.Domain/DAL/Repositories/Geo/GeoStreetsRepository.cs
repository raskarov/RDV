using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories.Geo;

namespace RDV.Domain.DAL.Repositories.Geo
{
    /// <summary>
    /// СУБД реализация репозитория улиц
    /// </summary>
    public class GeoStreetsRepository: BaseRepository<GeoStreet>, IGeoStreetsRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext"></param>
        public GeoStreetsRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override GeoStreet Load(long id)
        {
            return Find(s => s.Id == id);
        }
    }
}