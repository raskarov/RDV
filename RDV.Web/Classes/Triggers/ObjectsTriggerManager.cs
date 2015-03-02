using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Mailing.Templates;
using RDV.Domain.Infrastructure.Misc;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Classes.Search.Interfaces;
using RDV.Web.Models.Search;
using RDV.Domain.Core;

namespace RDV.Web.Classes.Triggers
{
    /// <summary>
    /// Реализация менеджера тригеров
    /// </summary>
    public class ObjectsTriggerManager: IObjectsTriggerManager
    {
        /// <summary>
        /// Менеджер поисковых запросов
        /// </summary>
        public ISearchRequestsRepository SearchRequestsRepository { get; set; }

        /// <summary>
        /// Менеджер поиска
        /// </summary>
        public IObjectSearchManager SearchManager { get; set; }

        /// <summary>
        /// Менеджер уведомлений по почте
        /// </summary>
        public IMailNotificationManager MailNotificationManager { get; set; }

        /// <summary>
        /// Менеджер уведомлений по смс
        /// </summary>
        public ISMSNotificationManager SMSNotificationManager { get; set; }

        /// <summary>
        /// Менеджер платежей
        /// </summary>
        public IPaymentsRepository PaymentsRepository { get; set; }

        /// <summary>
        /// Инициилизирует менеджер и заполняет его объектами
        /// </summary>
        /// <param name="searchRequestsRepository">Менеджер поисковых запросов</param>
        /// <param name="searchManager">Менеджер поиска объектов</param>
        /// <param name="mailNotificationManager">Менеджер уведомлений по почте</param>
        /// <param name="smsNotificationManager">Менеджер уведомлений по смс</param>
        /// <param name="paymentsRepository">Менеджер платежей</param>
        public ObjectsTriggerManager(ISearchRequestsRepository searchRequestsRepository, IObjectSearchManager searchManager, IMailNotificationManager mailNotificationManager, ISMSNotificationManager smsNotificationManager, IPaymentsRepository paymentsRepository)
        {
            SearchRequestsRepository = searchRequestsRepository;
            SearchManager = searchManager;
            MailNotificationManager = mailNotificationManager;
            SMSNotificationManager = smsNotificationManager;
            PaymentsRepository = paymentsRepository;
        }

        /// <summary>
        /// Вызывают инвокацию события о том, что указанный объект был активирован т.е. переведен в статус активный
        /// </summary>
        /// <param name="obj"></param>
        public void ObjectActivated(EstateObject obj)
        {
            ProcessObject(obj, SearchRequestTriggerEvent.Activation);
        }

        /// <summary>
        /// Вызывает инвокацию события о том, что у указанного объекта изменилась цена
        /// </summary>
        /// <param name="obj">Объект недвижимости</param>
        /// <param name="newPrice">Новая цена</param>
        public void ObjectPriceChanged(EstateObject obj, double? newPrice)
        {
            ProcessObject(obj, SearchRequestTriggerEvent.PriceChanging,newPrice);
        }

        /// <summary>
        /// Обрабатывает триггерное событие по объекту
        /// </summary>
        /// <param name="estateObject">Объект недвижимости</param>
        /// <param name="searchRequestTriggerEvent">Сработавшее событие</param>
        /// <param name="newPrice">Новая цена на объект</param>
        private void ProcessObject(EstateObject estateObject, SearchRequestTriggerEvent searchRequestTriggerEvent, double? newPrice = null)
        {
            // Ищем список всех подходящих запросов
            var matchedRequests = GetMatchedRequests(estateObject);
            if (matchedRequests.Count > 0)
            {
                // Проверяем нет ли уже объекта в этих запросах
                foreach (var matchedRequest in matchedRequests)
                {
                    // Ув
                    var so = matchedRequest.SearchRequestObjects.FirstOrDefault(ro => ro.EstateObjectId == estateObject.Id);
                    var exists = so != null;
                    if (!exists)
                    {
                        var searchRequestObj = new SearchRequestObject()
                            {
                                DateCreated = DateTimeZone.Now,
                                SearchRequest = matchedRequest,
                                EstateObject = estateObject,
                                Status = (short) SearchRequestObjectStatus.New,
                                New = true,
                                TriggerEvent = (short) searchRequestTriggerEvent
                            };
                        matchedRequest.SearchRequestObjects.Add(searchRequestObj);

                        // Уведомляем пользователя по email о новом объекте
                        var model = new
                            {
                                Name =
                                    String.Format("{0} {1}", matchedRequest.User.FirstName, matchedRequest.User.SurName),
                                RequestName = matchedRequest.Title,
                                ObjectId = estateObject.Id,
                                Address = estateObject.Address.ToShortAddressString()
                            };
                        var template =
                            new ParametrizedFileTemplate(
                                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "TriggerObject.htm"), model)
                                .ToString();
                        MailNotificationManager.Notify(matchedRequest.User,"Новый объект в системе",template);
                    }
                    else
                    {
                        // Проверяем находится ли объект в отлоненных и его отклонение было вызвано ценой
                        if (so.Status == (short) SearchRequestObjectStatus.Declined && so.DeclineReasonPrice && newPrice.HasValue && so.OldPrice.HasValue && newPrice.Value < so.OldPrice.Value)
                        {
                            // Переносим объект в новые
                            so.Status = (short) SearchRequestObjectStatus.New;
                            so.DateCreated = DateTimeZone.Now;

                            // Уведомляем пользователя о новом объекте
                            var model = new
                                {
                                    Name =
                                        String.Format("{0} {1}", matchedRequest.User.FirstName,
                                                      matchedRequest.User.SurName),
                                    RequestName = matchedRequest.Title,
                                    ObjectId = estateObject.Id,
                                    Address = estateObject.Address.ToShortAddressString(),
                                    NewPrice = newPrice.FormatPrice(),
                                    OldPrice = so.OldPrice.FormatPrice()
                                };
                            var template =
                            new ParametrizedFileTemplate(
                                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "TriggerObjectPrice.htm"), model)
                                .ToString();
                            MailNotificationManager.Notify(matchedRequest.User, "Изменение цены объекта", template);

							so.SearchRequestObjectComments.Add(new SearchRequestObjectComment()
							{
								DateCreated = DateTimeZone.Now,
								SearchRequestObject = so,
								Text = "Объект перенесен в \"Новые\" поскольку по объекту снизилась стоимость",
								UserId = -1
							});
							Locator.GetService<ISearchRequestsRepository>().SubmitChanges();
                        }
                    }
                }
            }

            // Сохраняем
            SearchRequestsRepository.SubmitChanges();
        }

        /// <summary>
        /// Возвращает список всех поисковых запросов, под которые попадает указанный объект
        /// </summary>
        /// <param name="estateObject"></param>
        /// <returns></returns>
        public IList<SearchRequest> GetMatchedRequests(EstateObject estateObject)
        {
            var resultList = new List<SearchRequest>();
            
            // Перебираем поисковые запросы только у активных пользователей
            foreach (var searchRequest in SearchRequestsRepository.Search(r => r.User.Status == (short)UserStatuses.Active))
            {
                var searchData = new SearchFormModel();

                // Преобразуем кверю запроса в коллекцию ключ значение используя объект HTTP запроса
                var request = new HttpRequest("index.html", "http://new.nprdv.ru", searchRequest.SearchUrl);
                try
                {
                    searchData.ReadFromFormCollection(request);
                }
                catch (Exception)
                {
                    continue; // Не смогли прочитать - уходим дальше
                }

                // Проверяем подходит ли нам объект
                if (SearchManager.CheckObject(searchData, estateObject))
                {
                    resultList.Add(searchRequest);
                }
            }

            // Отдаем список
            return resultList;
        }

        /// <summary>
        /// Возвращает список поисковых запросов, под которые попадает указанный объект исключая определенные поисковые запросы
        /// </summary>
        /// <param name="estateObject">Объект недвижимости</param>
        /// <param name="skipIds">Перечисление, содержащее идентификаторы элементов, который нужно пропустить</param>
        /// <returns></returns>
        public IList<SearchRequest> GetMatchedRequests(EstateObject estateObject, IEnumerable<long> skipIds)
        {
            var resultList = new List<SearchRequest>();

            // Перебираем поисковые запросы только у активных пользователей
            foreach (var searchRequest in SearchRequestsRepository.Search(r => r.User.Status == (short)UserStatuses.Active && !skipIds.Contains(r.Id)))
            {
                var searchData = new SearchFormModel();

                // Преобразуем кверю запроса в коллекцию ключ значение используя объект HTTP запроса
                var request = new HttpRequest("index.html", "http://new.nprdv.ru", searchRequest.SearchUrl);
                try
                {
                    searchData.ReadFromFormCollection(request);
                }
                catch (Exception)
                {
                    continue; // Не смогли прочитать - уходим дальше
                }

                // Проверяем подходит ли нам объект
                if (SearchManager.CheckObject(searchData, estateObject))
                {
                    resultList.Add(searchRequest);
                }
            }

            // Отдаем список
            return resultList;
        }
    }
}