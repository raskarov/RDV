using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория поисковых запросов
    /// </summary>
    public class SearchRequestsRepository: BaseRepository<SearchRequest>, ISearchRequestsRepository
    {
        /// <summary>
        /// Инициализирует новый инстанс абстрактного репозитория для указанного типа
        /// </summary>
        /// <param name="dataContext"></param>
        public SearchRequestsRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override SearchRequest Load(long id)
        {
            return Find(r => r.Id == id);
        }
    }
}