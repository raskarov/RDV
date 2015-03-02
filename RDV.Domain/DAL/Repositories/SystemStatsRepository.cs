// ============================================================
// 
// 	RDV
// 	RDV.Domain 
// 	SystemStatsRepository.cs
// 
// 	Created by: ykorshev 
// 	 at 25.10.2013 10:35
// 
// ============================================================

using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория показателей
    /// </summary>
    public class SystemStatsRepository: BaseRepository<SystemStat>, ISystemStatsRepository
    {
        /// <summary>
        /// Инициализирует новый инстанс абстрактного репозитория для указанного типа
        /// </summary>
        /// <param name="dataContext"></param>
        public SystemStatsRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override SystemStat Load(long id)
        {
            return Find(s => s.Id == id);
        }
    }
}