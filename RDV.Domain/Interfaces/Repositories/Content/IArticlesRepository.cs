using System;
using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories.Content
{
    /// <summary>
    /// Абстрактный репозиторий статей
    /// </summary>
    public interface IArticlesRepository: IBaseRepository<Article>
    {
        /// <summary>
        /// Возвращает true если есть запланированные мероприятия на текущий день
        /// </summary>
        /// <param name="date">Дата</param>
        /// <returns></returns>
        bool HasEvents(DateTime date);
    }
}