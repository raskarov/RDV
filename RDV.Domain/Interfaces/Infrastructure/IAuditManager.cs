using System.Collections.Generic;
using System.Web;
using RDV.Domain.Entities;
using RDV.Domain.Enums;

namespace RDV.Domain.Interfaces.Infrastructure
{
    /// <summary>
    /// Менеджер управляющий событиями аудита
    /// </summary>
    public interface IAuditManager
    {
        /// <summary>
        /// Помещает событие аудита в стек событий аудита
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="eventType">Тип события</param>
        /// <param name="message">Сообщение</param>
        /// <param name="ip">IP адрес пользователя</param>
        /// <param name="browserInfo">Сведения о браузере пользователя</param>
        /// <param name="additionalInfo">Дополнительные сведения</param>
        void PushEvent(User user, AuditEventTypes eventType, string message, string ip = null, string browserInfo = null, string additionalInfo = null);

        /// <summary>
        /// Помещает событие аудта в стек, дополнительно заполняя его данными из объекта HTTP запроса
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="eventType">Тип события</param>
        /// <param name="message">Сообщение</param>
        /// <param name="httpRequest">Объект HTTP запроса, содержащий данные связанные с запросом</param>
        void PushEvent(User user, AuditEventTypes eventType, string message, HttpRequest httpRequest);

        /// <summary>
        /// Возвращает список всех событий аудита, отсортированных по дате
        /// </summary>
        /// <returns>Коллекция всех событий аудита из системы, является IQueryable, так что поддерживает дополнительные LINQ операции</returns>
        IEnumerable<AuditEvent> GetAllEvents();

        /// <summary>
        /// Возвращает список событий аудита произошедших у указанного пользователя. Список отсортирован по дате
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns>Список событий аудита</returns>
        IEnumerable<AuditEvent> GetEventsForUser(User user);

        /// <summary>
        /// Возвращает список событий для указанной компании. список отсортирован по дате
        /// </summary>
        /// <param name="company">Компания, события которой нужно получить</param>
        /// <returns>Список</returns>
        IEnumerable<AuditEvent> GetEventsForCompany(Company company);

        /// <summary>
        /// Ищем событие с указанным идентификатором
        /// </summary>
        /// <param name="id">Идентификатор события</param>
        /// <returns>Событие или null если не найдено</returns>
        AuditEvent FindEvent(long id);
    }
}