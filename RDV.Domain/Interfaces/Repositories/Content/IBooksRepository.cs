// ============================================================
// 
// 	RDV
// 	RDV.Domain 
// 	IBooksRepository.cs
// 
// 	Created by: ykorshev 
// 	 at 26.08.2013 10:50
// 
// ============================================================

using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories.Content
{
    /// <summary>
    /// Абстрактный репозиторий книг
    /// </summary>
    public interface IBooksRepository: IBaseRepository<Book>
    {
         
    }
}