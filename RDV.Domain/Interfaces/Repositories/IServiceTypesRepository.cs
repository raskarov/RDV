// ============================================================
// 
// 	RDV
// 	RDV.Domain 
// 	IServiceTypesRepository.cs
// 
// 	Created by: ykorshev 
// 	 at 25.07.2013 11:56
// 
// ============================================================

using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий типов услуг
    /// </summary>
    public interface IServiceTypesRepository: IBaseRepository<ServiceType>
    {
         
    }
}