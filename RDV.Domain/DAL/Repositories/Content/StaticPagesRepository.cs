using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories.Content;

namespace RDV.Domain.DAL.Repositories.Content
{
    /// <summary>
    /// СУБД реализация репозитория страниц
    /// </summary>
    public class StaticPagesRepository: BaseRepository<StaticPage>, IStaticPagesRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext">Контекст доступа к данным</param>
        public StaticPagesRepository(RDVDataContext dataContext) : base(dataContext)
        {

        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override StaticPage Load(long id)
        {
            return Find(o => o.Id == id);
        }
    }
}