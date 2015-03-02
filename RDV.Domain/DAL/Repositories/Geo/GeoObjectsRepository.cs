using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories.Geo;

namespace RDV.Domain.DAL.Repositories.Geo
{
    /// <summary>
    /// СУБД реализация репозитория объектов
    /// </summary>
    public class GeoObjectsRepository: BaseRepository<GeoObject>, IGeoObjectsRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext"></param>
        public GeoObjectsRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override GeoObject Load(long id)
        {
            return Find(o => o.Id == id);
        }
    }
}