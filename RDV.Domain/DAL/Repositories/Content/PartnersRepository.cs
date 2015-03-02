// ============================================================
// 
// 	RDV
// 	RDV.Domain 
// 	PartnersRepository.cs
// 
// 	Created by: ykorshev 
// 	 at 18.10.2013 16:52
// 
// ============================================================

using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories.Content;

namespace RDV.Domain.DAL.Repositories.Content
{
    /// <summary>
    /// СУБД реализация репозитория партнеров
    /// </summary>
    public class PartnersRepository: BaseRepository<Partner>, IPartnersRepository
    {
        /// <summary>
        /// Инициализирует новый инстанс абстрактного репозитория для указанного типа
        /// </summary>
        /// <param name="dataContext"></param>
        public PartnersRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override Partner Load(long id)
        {
            return Find(p => p.Id == id);
        }
    }
}