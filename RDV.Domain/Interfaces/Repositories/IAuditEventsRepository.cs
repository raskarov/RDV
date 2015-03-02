using System.Collections.Generic;
using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий для хранения событий аудита
    /// </summary>
    public interface IAuditEventsRepository: IBaseRepository<AuditEvent>
    {
        /// <summary>
        /// Возвращает коллекцию событий аудита для указанного пользователя, отсортированных по дате
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns>Коллекция событий, произошедших у указанного пользователя</returns>
        IEnumerable<AuditEvent> GetEventsForUser(User user);
            
        /// <summary>
        /// Возвращает список событий произошедших в указанной компании, отсортированных по дате
        /// </summary>
        /// <param name="company">Компания</param>
        /// <returns>Коллекция событий произошедших в указанной компании</returns>
        IEnumerable<AuditEvent> GetEventsForCompany(Company company);
    }
}