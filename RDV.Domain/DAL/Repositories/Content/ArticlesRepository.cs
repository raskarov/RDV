using System;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Repositories.Content;

namespace RDV.Domain.DAL.Repositories.Content
{
    /// <summary>
    /// СУБД реализация репозитория статей
    /// </summary>
    public class ArticlesRepository: BaseRepository<Article>, IArticlesRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext">Контекст доступа к данным</param>
        public ArticlesRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override Article Load(long id)
        {
            return Find(a => a.Id == id);
        }

        /// <summary>
        /// Возвращает true если есть запланированные мероприятия на текущий день
        /// </summary>
        /// <param name="date">Дата</param>
        /// <returns></returns>
        public bool HasEvents(DateTime date)
        {
            return Search(e => e.ArticleType == ArticleTypes.CalendarEvent && e.PublicationDate.Date == date.Date).Any();
        }
    }
}