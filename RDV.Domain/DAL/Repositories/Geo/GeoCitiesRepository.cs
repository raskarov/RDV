using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories.Geo;

namespace RDV.Domain.DAL.Repositories.Geo
{
    /// <summary>
    /// СУБД реализация репозитория городов
    /// </summary>
    public class GeoCitiesRepository: BaseRepository<GeoCity>, IGeoCitiesRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext"></param>
        public GeoCitiesRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override GeoCity Load(long id)
        {
            return Find(c => c.Id == id);
        }
    }
}