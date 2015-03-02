// ============================================================
// 
// 	RDV
// 	RDV.Domain 
// 	IPartnersRepository.cs
// 
// 	Created by: ykorshev 
// 	 at 18.10.2013 16:52
// 
// ============================================================

using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories.Content
{
    /// <summary>
    /// Абстрактный репозиторий партнеров
    /// </summary>
    public interface IPartnersRepository: IBaseRepository<Partner>
    {
         
    }
}