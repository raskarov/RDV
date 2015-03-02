// ============================================================
// 
// 	RDV
// 	RDV.Domain 
// 	ISystemStatsRepository.cs
// 
// 	Created by: ykorshev 
// 	 at 25.10.2013 10:34
// 
// ============================================================

using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий статистических показателей
    /// </summary>
    public interface ISystemStatsRepository: IBaseRepository<SystemStat>
    {
         
    }
}