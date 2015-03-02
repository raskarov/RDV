using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Routing;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Classes.Search.Interfaces;
using RDV.Web.Classes.Security;
using RDV.Web.Models.Objects;
using RDV.Web.Models.SearchRequests;
using RDV.Domain.Core;

namespace RDV.Web.Controllers
{
    /// <summary>
    /// Контроллер управления поисковыми запросами
    /// </summary>
    public class SearchRequestsController : BaseController
    {
        #region Поисковые запросы

        /// <summary>
        /// Отображает главную страницу интерфейса поисковых запросов
        /// </summary>
        /// <returns></returns>
        [Route("search-requests")]
        [AuthorizationCheck]
        public ActionResult Index(RequestsListSection section = RequestsListSection.MyRequests)
        {
            PushNavigationItem("Личный кабинет", "Корневая страница личного кабинета", "/account/");
            PushNavigationItem("Запросы", "Запросы на поиски объектов", "/search-requests/", true);

            var rep = Locator.GetService<ISearchRequestsRepository>();

            switch (section)
            {
                case RequestsListSection.MyRequests:
                    if (!CurrentUser.HasPermission(Permission.EditOwnObjects))
                    {
                        Locator.GetService<IUINotificationManager>().Error("Доступ к этому разделу запрещен");
                        return RedirectToAction("Index", "Main");
                    }
                    break;
                case RequestsListSection.CompanyRequests:
                    if (!CurrentUser.HasPermission(Permission.EditCompanyObjects))
                    {
                        Locator.GetService<IUINotificationManager>().Error("Доступ к этому разделу запрещен");
                        return RedirectToAction("Index", "Main");
                    }
                    break;
                case RequestsListSection.AllRequests:
                    if (!CurrentUser.HasPermission(Permission.EditAllObjects))
                    {
                        Locator.GetService<IUINotificationManager>().Error("Доступ к этому разделу запрещен");
                        return RedirectToAction("Index", "Main");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("section");
            }

            IEnumerable<SearchRequest> requests;
            switch (section)
            {
                case RequestsListSection.MyRequests:
                    requests = rep.Search(r => r.UserId == CurrentUser.Id);
                    break;
                case RequestsListSection.CompanyRequests:
                    requests = rep.Search(r => r.User.CompanyId == CurrentUser.CompanyId);
                    break;
                case RequestsListSection.AllRequests:
                    requests = rep.FindAll();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("section");
            }

            IEnumerable list = requests.Select(r => new RequestItemModel(r));
            ViewBag.section = section;

            return View(list);
        }

        /// <summary>
        /// Обрабатывает частичный вид грида списка запросов
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        [AuthorizationCheck]
        public ActionResult RequestsListPartial(RequestsListSection section)
        {
            var rep = Locator.GetService<ISearchRequestsRepository>();

            IEnumerable<SearchRequest> requests;
            switch (section)
            {
                case RequestsListSection.MyRequests:
                    requests = rep.Search(r => r.UserId == CurrentUser.Id);
                    break;
                case RequestsListSection.CompanyRequests:
                    requests = rep.Search(r => r.User.CompanyId == CurrentUser.CompanyId);
                    break;
                case RequestsListSection.AllRequests:
                    requests = rep.FindAll();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("section");
            }

            IEnumerable list = requests.Select(r => new RequestItemModel(r));

            return PartialView(new RequestsListModel()
            {
                Section = section,
                Data = list
            });
        }

        /// <summary>
        /// Удаляет указанный поисковый запрос
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("search-requests/delete/{id}")]
        public ActionResult DeleteSearchRequest(long id, RequestsListSection section = RequestsListSection.MyRequests)
        {
            // Ищем запрос
            var request = Locator.GetService<ISearchRequestsRepository>().Load(id);
            if (request == null)
            {
                //UINotificationManager.Error("Такой поисковый запрос не найден");
                return RedirectToAction("Index");
            }

            // Событие аудита
            PushAuditEvent(AuditEventTypes.Editing, string.Format("Удаление поискового запроса {0}", request.Title));

            // Исполняем запрос
            CurrentUser.SearchRequests.Remove(request);
            Locator.GetService<IUsersRepository>().SubmitChanges();

            UINotificationManager.Success("Поисковый запрос был успешно удален");

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Возвращает детальный вид с объектами, подходящими под запрос
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("search-requests/request-objects")][AuthorizationCheck]
        public ActionResult SearchRequestObjects(long id)
        {
            // Ищем запрос
            var request = Locator.GetService<ISearchRequestsRepository>().Load(id);
            if (request == null)
            {
                return Content("Not found");
            }

            // Проверяем
            foreach (
                var requestObject in
                    request.SearchRequestObjects.Where(
                        ro =>
                            (ro.EstateObject.Status != (short)EstateStatuses.Active) &&
                            ro.Status != (short) SearchRequestObjectStatus.Declined))
            {
                requestObject.Status = (short) SearchRequestObjectStatus.Declined;
                requestObject.DeclineReasonPrice = false;
                requestObject.DeclineReason = "Объект снят с продажи";
                requestObject.SearchRequestObjectComments.Add(new SearchRequestObjectComment()
                {
                    DateCreated = DateTimeZone.Now,
                    SearchRequestObject = requestObject,
                    Text = "Объект снят с продажи",
                    UserId = -1
                });
            }
            Locator.GetService<ISearchRequestsRepository>().SubmitChanges();

            return PartialView(request);
        }

        /// <summary>
        /// Обрабатывает частичный вид грида
        /// </summary>
        /// <param name="id">Идентификатор запроса</param>
        /// <param name="status">Статус объектов в запросе</param>
        /// <returns></returns>
        [AuthorizationCheck]
        public ActionResult SearchRequestObjectsPartial(long id, SearchRequestObjectStatus status)
        {
            // Ищем запрос
            var request = Locator.GetService<ISearchRequestsRepository>().Load(id);
            if (request == null)
            {
                return Content("Not found");
            }

            IEnumerable objs;
            switch (status)
            {
                case SearchRequestObjectStatus.New:
                    objs =
                        request.SearchRequestObjects.Where(a => a.Status == (short) status)
                            .OrderByDescending(a => a.DateCreated)
                            .Select(a => new RequestObjectModel(a))
                            .ToList();
                    break;
                case SearchRequestObjectStatus.Accepted:
                case SearchRequestObjectStatus.Declined:
                    objs =
                        request.SearchRequestObjects.Where(a => a.Status == (short)status)
                            .OrderByDescending(a => a.DateMoved)
                            .Select(a => new RequestObjectModel(a))
                            .ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("status");
            }

            return PartialView(new RequestObjectsListModel()
            {
                Data = objs,
                Request = request,
                Status = status
            });
        }

        /// <summary>
        /// Возвращает на клиент комментарии к указанному поисковому запросу
        /// </summary>
        /// <param name="id">Идентификатор запросов</param>
        /// <param name="roid">Идентификатор объекта в запросе</param>
        /// <returns></returns>
        [Route("search-requests/request-object-comments")]
        [AuthorizationCheck]
        public ActionResult SearchRequestObjectComments(long id, long roid)
        {
            // Ищем запрос
            var request = Locator.GetService<ISearchRequestsRepository>().Load(id);
            if (request == null)
            {
                return Content("Not found");
            }

            // Ищем объект
            var requestObject = request.SearchRequestObjects.FirstOrDefault(a => a.Id == roid);
            if (requestObject == null)
            {
                return Content("Not found");
            }

            return PartialView(requestObject.SearchRequestObjectComments.OrderBy(c => c.DateCreated).ToList());
        }

        /// <summary>
        /// Обрабатывает принятие объекта в работу
        /// </summary>
        /// <param name="rid">Идентификатор запроса</param>
        /// <param name="roid">Идентификатор объекта в запросе</param>
        /// <param name="comments">Комментарий к изменению</param>
        /// <returns></returns>
        [Route("search-requests/accept-object")][HttpPost][AuthorizationCheck]
        public ActionResult AcceptObject(long rid, long roid, string comments)
        {
            // Ищем запрос
            var request = Locator.GetService<ISearchRequestsRepository>().Load(rid);
            if (request == null)
            {
                return Json(new {success = false, msg = "Не найдено"});
            }

            // Ищем объект
            var requestObject = request.SearchRequestObjects.FirstOrDefault(a => a.Id == roid);
            if (requestObject == null)
            {
                return Json(new { success = false, msg = "Не найдено" });
            }

            requestObject.Status = (short) SearchRequestObjectStatus.Accepted;
            requestObject.DateMoved = DateTimeZone.Now;
            requestObject.DeclineReasonPrice = false;
            requestObject.DeclineReason = null;
            requestObject.SearchRequestObjectComments.Add(new SearchRequestObjectComment()
            {
                DateCreated = DateTimeZone.Now,
                SearchRequestObject = requestObject,
                Text = "Объект принят в работу",
                User = CurrentUser
            });
            if (!String.IsNullOrEmpty(comments))
            {
                requestObject.SearchRequestObjectComments.Add(new SearchRequestObjectComment()
                {
                    DateCreated = DateTimeZone.Now,
                    SearchRequestObject = requestObject,
                    Text = comments,
                    User = CurrentUser
                });
            }
            Locator.GetService<ISearchRequestsRepository>().SubmitChanges();

            return Json(new {success = true}); 
        }

        /// <summary>
        /// Обрабатывает принятие объекта в работу
        /// </summary>
        /// <param name="rid">Идентификатор запроса</param>
        /// <param name="roid">Идентификатор объекта в запросе</param>
        /// <param name="comments">Комментарий к изменению</param>
        /// <param name="declineReasonPrice">Причина отклонения цена</param>
        /// <returns></returns>
        [Route("search-requests/decline-object")][HttpPost][AuthorizationCheck]
        public ActionResult DeclientObject(long rid, long roid, string comments, bool declineReasonPrice)
        {
            // Ищем запрос
            var request = Locator.GetService<ISearchRequestsRepository>().Load(rid);
            if (request == null)
            {
                return Json(new {success = false, msg = "Не найдено"});
            }

            // Ищем объект
            var requestObject = request.SearchRequestObjects.FirstOrDefault(a => a.Id == roid);
            if (requestObject == null)
            {
                return Json(new { success = false, msg = "Не найдено" });
            }

            requestObject.Status = (short) SearchRequestObjectStatus.Declined;
            requestObject.DateMoved = DateTimeZone.Now;
            requestObject.DeclineReasonPrice = declineReasonPrice;
	        requestObject.OldPrice = requestObject.EstateObject.ObjectMainProperties.Price;
            requestObject.DeclineReason = comments;
            requestObject.SearchRequestObjectComments.Add(new SearchRequestObjectComment()
            {
                DateCreated = DateTimeZone.Now,
                SearchRequestObject = requestObject,
                Text = "Объкт отклонен",
                User = CurrentUser
            });
            if (!String.IsNullOrEmpty(comments))
            {
                requestObject.SearchRequestObjectComments.Add(new SearchRequestObjectComment()
                {
                    DateCreated = DateTimeZone.Now,
                    SearchRequestObject = requestObject,
                    Text = comments,
                    User = CurrentUser
                });
            }
            Locator.GetService<ISearchRequestsRepository>().SubmitChanges();

            return Json(new {success = true}); 
        }

        /// <summary>
        /// Обрабатывает принятие объекта в работу
        /// </summary>
        /// <param name="rid">Идентификатор запроса</param>
        /// <param name="roid">Идентификатор объекта в запросе</param>
        /// <param name="comments">Комментарий к изменению</param>
        /// <returns></returns>
        [Route("search-requests/comment-object")][HttpPost][AuthorizationCheck]
        public ActionResult CommentObject(long rid, long roid, string comments)
        {
            // Ищем запрос
            var request = Locator.GetService<ISearchRequestsRepository>().Load(rid);
            if (request == null)
            {
                return Json(new {success = false, msg = "Не найдено"});
            }

            // Ищем объект
            var requestObject = request.SearchRequestObjects.FirstOrDefault(a => a.Id == roid);
            if (requestObject == null)
            {
                return Json(new { success = false, msg = "Не найдено" });
            }

            if (!String.IsNullOrEmpty(comments))
            {
                requestObject.SearchRequestObjectComments.Add(new SearchRequestObjectComment()
                {
                    DateCreated = DateTimeZone.Now,
                    SearchRequestObject = requestObject,
                    Text = comments,
                    User = CurrentUser
                });
            }
            Locator.GetService<ISearchRequestsRepository>().SubmitChanges();

            return Json(new {success = true}); 
        }

		/// <summary>
		/// Возвращает на клиент информацию об обновленных значениях количества
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[Route("search-requests/update-headers")][HttpPost][AuthorizationCheck]
	    public ActionResult UpdateHeaders(long id)
	    {
			// Ищем запрос
			var request = Locator.GetService<ISearchRequestsRepository>().Load(id);
			if (request == null)
			{
				return Json(new { success = false, msg = "Не найдено" });
			}

			return Json(new
			{
				success = true,
				newObjects = request.SearchRequestObjects.Count(ro => ro.Status == (short) SearchRequestObjectStatus.New),
				acceptedObjects = request.SearchRequestObjects.Count(ro => ro.Status == (short) SearchRequestObjectStatus.Accepted),
				declinedObjects = request.SearchRequestObjects.Count(ro => ro.Status == (short) SearchRequestObjectStatus.Declined),
			});
	    }

        /// <summary>
        /// Обрабатывает принятие объекта в работу
        /// </summary>
        /// <param name="rid">Идентификатор запроса</param>
        /// <param name="roid">Идентификатор объекта в запросе</param>
        /// <param name="comments">Комментарий к изменению</param>
        /// <returns></returns>
        [Route("search-requests/accept-object-all")]
        [HttpPost]
        [AuthorizationCheck]
        public ActionResult AcceptObjectAll(List<RequestObjectCommentsModel> data)
        {
            foreach (var item in data)
            {
                var rid = item.id;
                var roid = item.roid;
                var comments = item.comment;

                // Ищем запрос
                var request = Locator.GetService<ISearchRequestsRepository>().Load(rid);
                if (request == null)
                {
                    return Json(new { success = false, msg = "Не найдено" });
                }

                // Ищем объект
                var requestObject = request.SearchRequestObjects.FirstOrDefault(a => a.Id == roid);
                if (requestObject == null)
                {
                    return Json(new { success = false, msg = "Не найдено" });
                }

                requestObject.Status = (short)SearchRequestObjectStatus.Accepted;
                requestObject.DateMoved = DateTimeZone.Now;
                requestObject.DeclineReasonPrice = false;
                requestObject.DeclineReason = null;
                requestObject.SearchRequestObjectComments.Add(new SearchRequestObjectComment()
                {
                    DateCreated = DateTimeZone.Now,
                    SearchRequestObject = requestObject,
                    Text = "Объект принят в работу",
                    User = CurrentUser
                });
                if (!String.IsNullOrEmpty(comments))
                {
                    requestObject.SearchRequestObjectComments.Add(new SearchRequestObjectComment()
                    {
                        DateCreated = DateTimeZone.Now,
                        SearchRequestObject = requestObject,
                        Text = comments,
                        User = CurrentUser
                    });
                }
                Locator.GetService<ISearchRequestsRepository>().SubmitChanges();
            }

            return Json(new { success = true });
        }

        /// <summary>
        /// Обрабатывает принятие объекта в работу
        /// </summary>
        /// <param name="rid">Идентификатор запроса</param>
        /// <param name="roid">Идентификатор объекта в запросе</param>
        /// <param name="comments">Комментарий к изменению</param>
        /// <param name="declineReasonPrice">Причина отклонения цена</param>
        /// <returns></returns>
        [Route("search-requests/decline-object-all")]
        [HttpPost]
        [AuthorizationCheck]
        public ActionResult DeclientObjectAll(List<RequestObjectCommentsModel> data)
        {
            foreach (var item in data)
            {
                var rid = item.id;
                var roid = item.roid;
                var comments = item.comment;
                var declineReasonPrice = item.declineReasonPrice;

                // Ищем запрос
                var request = Locator.GetService<ISearchRequestsRepository>().Load(rid);
                if (request == null)
                {
                    return Json(new { success = false, msg = "Не найдено" });
                }

                // Ищем объект
                var requestObject = request.SearchRequestObjects.FirstOrDefault(a => a.Id == roid);
                if (requestObject == null)
                {
                    return Json(new { success = false, msg = "Не найдено" });
                }

                requestObject.Status = (short)SearchRequestObjectStatus.Declined;
                requestObject.DateMoved = DateTimeZone.Now;
                requestObject.DeclineReasonPrice = declineReasonPrice;
                requestObject.OldPrice = requestObject.EstateObject.ObjectMainProperties.Price;
                requestObject.DeclineReason = comments;
                requestObject.SearchRequestObjectComments.Add(new SearchRequestObjectComment()
                {
                    DateCreated = DateTimeZone.Now,
                    SearchRequestObject = requestObject,
                    Text = "Объкт отклонен",
                    User = CurrentUser
                });

                if (!String.IsNullOrEmpty(comments))
                {
                    requestObject.SearchRequestObjectComments.Add(new SearchRequestObjectComment()
                    {
                        DateCreated = DateTimeZone.Now,
                        SearchRequestObject = requestObject,
                        Text = comments,
                        User = CurrentUser
                    });
                }
                Locator.GetService<ISearchRequestsRepository>().SubmitChanges();
            }

            return Json(new { success = true });
        }

        #endregion

        #region Подходящие запросы

        /// <summary>
        /// Отображает поисковые запросы, подходящие к указанному объекту
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        [Route("search-requests/by-object/{id}")]
        [AuthorizationCheck()]
        public ActionResult ObjectRequests(long id)
        {
            // Ищем объект и проверяем привилегии
            var estateObject = Locator.GetService<IEstateObjectsRepository>().Load(id);
            if (estateObject == null)
            {
                Locator.GetService<IUINotificationManager>().Error(string.Format("Объект №{0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            if (estateObject.Status != (short)EstateStatuses.Active)
            {
                Locator.GetService<IUINotificationManager>().Error(string.Format("Объект №{0} не находится в активном статусе", id));
                return RedirectToAction("Index", "Account");
            }

            PushNavigationItem("Поисковые запросы", "Поисковые запросы в системе", "/search-requests", true);
            PushNavigationItem(string.Format("Запросы для объекта №{0}", id), "Поисковые запросы в системе, подходящие для указанного объекта", "/search-requests/by-object/" + id, true);

            // Проверяем, что у нас существующими запросами, которые не отклонены и были удалены
            foreach (
                var matchedRequest in
                    estateObject.EstateObjectMatchedSearchRequests.Where(
                        mr => mr.SearchRequest == null && mr.Status != (short)SearchRequestObjectStatus.Declined))
            {
                matchedRequest.Status = (short)SearchRequestObjectStatus.Declined;
                matchedRequest.RequestDateDeleted = DateTimeZone.Now;
                // Добавляем коммент 
                matchedRequest.EstateObjectMatchedSearchRequestComments.Add(new EstateObjectMatchedSearchRequestComment()
                {
                    UserId = -1,
                    EstateObjectMatchedSearchRequest = matchedRequest,
                    DateCreated = DateTimeZone.Now,
                    Text = "Запрос удален пользователем-создателем"
                });
            }

            // Проводим сканирование системыи ищем запросы, подходящие под этот объект
            var processedRequestIds = estateObject.EstateObjectMatchedSearchRequests.Select(mr => mr.RequestId);
            var objectsTriggerManager = Locator.GetService<IObjectsTriggerManager>();

            var matchedRequests = objectsTriggerManager.GetMatchedRequests(estateObject, processedRequestIds);
            foreach (var matched in matchedRequests)
            {
                estateObject.EstateObjectMatchedSearchRequests.Add(new EstateObjectMatchedSearchRequest()
                {
                    DateCreated = DateTimeZone.Now,
                    EstateObject = estateObject,
                    RequestDateCreated = matched.DateCreated,
                    RequestTitle = matched.Title,
                    SearchRequest = matched,
                    User = matched.User,
                    Status = (short)SearchRequestObjectStatus.New
                });
            }

            // Сохраняем изменения
            Locator.GetService<ISearchRequestsRepository>().SubmitChanges();

            PushAuditEvent(AuditEventTypes.ViewPage, string.Format("Просмотр списка подходящих запросов для объекта №{0}", id));

            return View(estateObject);
        }

        /// <summary>
        /// Обрабатывает каллбак из грида и отдает обработанные данные
        /// </summary>
        /// <param name="model">Модель данных</param>
        /// <returns></returns>
        public ActionResult MatchedRequestsPartial(MatchedRequestsListModel model)
        {
            // Ищем объект
            var rep = Locator.GetService<IEstateObjectsRepository>();
            var estateObject = rep.Load(model.ObjectId);

            // Устанавливаем данные
            model.EstateObject = estateObject;
            model.Data = estateObject.EstateObjectMatchedSearchRequests.Where(mr => mr.Status == (short)model.Status).ToList();

            return PartialView(model);
        }

        /// <summary>
        /// Загружает комментарии на указанный подходящий поисковый запрос для указанного объекта
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <param name="rid">Идентификатор контейнера подходящего поискового запроса</param>
        /// <returns></returns>
        [Route("search-requests/matched-comments")]
        [AuthorizationCheck]
        public ActionResult MatchedRequestComments(long id, long rid)
        {
            // Ищем объект
            var rep = Locator.GetService<IEstateObjectsRepository>();
            var estateObject = rep.Load(id);

            if (estateObject == null)
            {
                return Content("Объект не найден");
            }

            var matchedRequest = estateObject.EstateObjectMatchedSearchRequests.FirstOrDefault(a => a.Id == rid);
            if (matchedRequest == null)
            {
                return Content("Подходящий запрос не найден");
            }

            return
                PartialView(matchedRequest.EstateObjectMatchedSearchRequestComments.OrderBy(a => a.DateCreated).ToList());
        }

        /// <summary>
        /// Обрабатывает изменения статуса поискового запроса
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <param name="rid">Идентификатор запроса</param>
        /// <param name="newStatus">Новый статус</param>
        /// <param name="comments">Комментарии к изменению</param>
        /// <param name="statusSection">Секция куда перейти</param>
        /// <returns></returns>
        [Route("search-requests/change-matched-request-status")]
        [HttpPost]
        [AuthorizationCheck]
        public ActionResult MatchedRequestChangeStatus(long id, long rid, short newStatus, string comments, string statusSection)
        {
            // Ищем объект
            var rep = Locator.GetService<IEstateObjectsRepository>();
            var estateObject = rep.Load(id);
            if (estateObject == null)
            {
                Locator.GetService<IUINotificationManager>().Error("Такой объект не найден");
                return RedirectToAction("Index");
            }

            // Ищем запрос
            var matchedRequest = estateObject.EstateObjectMatchedSearchRequests.FirstOrDefault(a => a.Id == rid);
            if (matchedRequest == null)
            {
                Locator.GetService<IUINotificationManager>().Error("Такой запрос не найден");
                return RedirectToAction("ObjectRequests",new {id});
            }

            // Изменяем статус
            if (matchedRequest.Status != newStatus && newStatus != -1)
            {
                matchedRequest.Status = newStatus;
                matchedRequest.DateMoved = DateTimeZone.Now;
                matchedRequest.EstateObjectMatchedSearchRequestComments.Add(new EstateObjectMatchedSearchRequestComment()
                {
                    UserId = CurrentUser.Id,
                    DateCreated = DateTimeZone.Now,
                    EstateObjectMatchedSearchRequest = matchedRequest,
                    Text = String.Format("Запрос перенесен в \"{0}\"", ((SearchRequestObjectStatus)newStatus).GetEnumMemberName())
                });
            }
            
            // Пишем комментарии
            if (!String.IsNullOrEmpty(comments))
            {
                matchedRequest.EstateObjectMatchedSearchRequestComments.Add(new EstateObjectMatchedSearchRequestComment()
                {
                    UserId = CurrentUser.Id,
                    DateCreated = DateTimeZone.Now,
                    EstateObjectMatchedSearchRequest = matchedRequest,
                    Text = comments
                });
            }

            // Сохраняем
            rep.SubmitChanges();

            Locator.GetService<IUINotificationManager>().Success("Подходящий запрос был успешно изменен");

            return Redirect(string.Format("/search-requests/by-object/{0}#{1}", id, statusSection));
        }

        #endregion

        public class RequestObjectCommentsModel
        {
            public Int64 id { get; set; }

            public Int64 roid { get; set; }

            public String comment { get; set; }

            public Boolean declineReasonPrice { get; set; }
        }
    }
}
