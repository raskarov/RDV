using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Domain.Core;

namespace RDV.Domain.Infrastructure
{
    /// <summary>
    /// Менеджер аудита
    /// </summary>
    public class AuditManager: IAuditManager
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
        public void PushEvent(User user, AuditEventTypes eventType, string message, string ip = null, string browserInfo = null, string additionalInfo = null)
        {
            // Создаем вложенный скоуп
            using (var httpRequestScope = Locator.BeginNestedHttpRequestScope())
            {
                // Репозиторий
                var auditRep = Locator.GetService<IAuditEventsRepository>();

                // Сохраняет новое событие аудита
                auditRep.Add(new AuditEvent()
                                 {
                                     UserId = user.Id,
                                     EventDate = DateTimeZone.Now,
                                     EventType = eventType,
                                     Message = message,
                                     IP = ip,
                                     BrowserInfo = browserInfo,
                                     AdditionalInformation = additionalInfo
                                 });
                //auditRep.SubmitChanges();
            }
        }

        /// <summary>
        /// Помещает событие аудта в стек, дополнительно заполняя его данными из объекта HTTP запроса
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="eventType">Тип события</param>
        /// <param name="message">Сообщение</param>
        /// <param name="httpRequest">Объект HTTP запроса, содержащий данные связанные с запросом</param>
        public void PushEvent(User user, AuditEventTypes eventType, string message, HttpRequest httpRequest)
        {
            // Подгатавливаем данные для извлечения
            var ip = httpRequest.UserHostAddress;
            var browserInfo = httpRequest.UserAgent;
            var additionalInfo = GetJsonRequestInfo(httpRequest);

            // Отправляем их
            PushEvent(user,eventType,message,ip,browserInfo,additionalInfo);
        }

        /// <summary>
        /// Возвращает список всех событий аудита, отсортированных по дате
        /// </summary>
        /// <returns>Коллекция всех событий аудита из системы, является IQueryable, так что поддерживает дополнительные LINQ операции</returns>
        public IEnumerable<AuditEvent> GetAllEvents()
        {
            // NOTE: тут используется скоуп основного HTTP потока
            return Locator.GetService<IAuditEventsRepository>().FindAll().OrderByDescending(e => e.EventDate);
        }

        /// <summary>
        /// Возвращает список событий аудита произошедших у указанного пользователя. Список отсортирован по дате
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns>Список событий аудита</returns>
        public IEnumerable<AuditEvent> GetEventsForUser(User user)
        {
            // NOTE: тут используется скоуп основного HTTP потока
            return Locator.GetService<IAuditEventsRepository>().GetEventsForUser(user);
        }

        /// <summary>
        /// Возвращает список событий для указанной компании. список отсортирован по дате
        /// </summary>
        /// <param name="company">Компания, события которой нужно получить</param>
        /// <returns>Список</returns>
        public IEnumerable<AuditEvent> GetEventsForCompany(Company company)
        {
            // NOTE: тут используется скоуп основного HTTP потока
            return Locator.GetService<IAuditEventsRepository>().GetEventsForCompany(company);
        }

        /// <summary>
        /// Ищем событие с указанным идентификатором
        /// </summary>
        /// <param name="id">Идентификатор события</param>
        /// <returns>Событие или null если не найдено</returns>
        public AuditEvent FindEvent(long id)
        {
            return Locator.GetService<IAuditEventsRepository>().Load(id);
        }

        /// <summary>
        /// Формирует данные HTTP запроса в виде JSON объекта и возвращает его представление в виде строки
        /// </summary>
        /// <param name="httpRequest">Объект HTTP запроса</param>
        /// <returns>JSON строка</returns>
        private string GetJsonRequestInfo(HttpRequest httpRequest)
        {
            if (httpRequest == null)
            {
                return null;
            }
            // Добавляем основные данные
            var resultObj = new JObject
                                {
                                    new JProperty("ip", new JValue(httpRequest.UserHostAddress)),
                                    new JProperty("host", new JValue(httpRequest.UserHostName)),
                                    new JProperty("language",
                                                  new JValue(String.Join(",", httpRequest.UserLanguages ?? new string[] {}))),
                                    new JProperty("agent", new JValue(httpRequest.UserAgent)),
                                    new JProperty("method", new JValue(httpRequest.HttpMethod))
                                };
            if (httpRequest.QueryString.Count > 0)
            {
                // Добавляем данные из GET запроса
                var getObject = new JObject();
                foreach (var key in httpRequest.QueryString.AllKeys)
                {
                    getObject.Add(new JProperty(key,new JValue(httpRequest.QueryString[key])));
                }
                resultObj.Add(new JProperty("query",getObject));
            }
            if (httpRequest.Form.Count > 0)
            {
                // Добавляем данные из POST запроса
                var postObject = new JObject();
                foreach (var key in httpRequest.Form.AllKeys)
                {
                    postObject.Add(new JProperty(key,new JValue(httpRequest.Form[key])));
                }
                resultObj.Add(new JProperty("form",postObject));
            }
            return resultObj.ToString();
        }
    }
}