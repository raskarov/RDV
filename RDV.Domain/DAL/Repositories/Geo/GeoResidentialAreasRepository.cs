using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories.Geo;

namespace RDV.Domain.DAL.Repositories.Geo
{
    /// <summary>
    /// СУБД реализация жилых массивов
    /// </summary>
    public class GeoResidentialAreasRepository: BaseRepository<GeoResidentialArea>, IGeoResidentialAreasRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext"></param>
        public GeoResidentialAreasRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override GeoResidentialArea Load(long id)
        {
            return Find(a => a.Id == id);
        }
    }
}