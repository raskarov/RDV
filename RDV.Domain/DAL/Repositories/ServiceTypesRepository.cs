// ============================================================
// 
// 	RDV
// 	RDV.Domain 
// 	ServiceTypesRepository.cs
// 
// 	Created by: ykorshev 
// 	 at 25.07.2013 12:00
// 
// ============================================================

using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория типов услуг
    /// </summary>
    public class ServiceTypesRepository: BaseRepository<ServiceType>, IServiceTypesRepository
    {
        /// <summary>
        /// Инициализирует новый инстанс абстрактного репозитория для указанного типа
        /// </summary>
        /// <param name="dataContext"></param>
        public ServiceTypesRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override ServiceType Load(long id)
        {
            return Find(st => st.Id == id);
        }
    }
}