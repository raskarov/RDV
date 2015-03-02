// ============================================================
// 
// 	RDV
// 	RDV.Domain 
// 	BooksRepository.cs
// 
// 	Created by: ykorshev 
// 	 at 26.08.2013 10:51
// 
// ============================================================

using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories.Content;

namespace RDV.Domain.DAL.Repositories.Content
{
    /// <summary>
    /// СУБД реализация репозитория книг
    /// </summary>
    public class BooksRepository: BaseRepository<Book>, IBooksRepository
    {
        /// <summary>
        /// Инициализирует новый инстанс абстрактного репозитория для указанного типа
        /// </summary>
        /// <param name="dataContext"></param>
        public BooksRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override Book Load(long id)
        {
            return Find(b => b.Id == id);
        }
    }
}