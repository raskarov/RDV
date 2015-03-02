using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Geo;
using RDV.Domain.Infrastructure.Mailing.Templates;
using RDV.Domain.Infrastructure.Routing;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Classes.Forms;
using RDV.Web.Classes.Forms.Fields;
using RDV.Web.Classes.Search.Interfaces;
using RDV.Web.Classes.Security;
using RDV.Web.Models.Search;
using RDV.Domain.Core;

namespace RDV.Web.Controllers
{
    /// <summary>
    /// Контроллер, управляющий всеми аспектами поиска объектов
    /// </summary>
    public class SearchController : BaseController
    {
        //
        // GET: /Search/
        /// <summary>
        /// Отображает форму расширенного поиска объектов
        /// </summary>
        /// <returns></returns>
        [Route("search")]
        public ActionResult Index()
        {
            PushNavigationItem("Поиск объектов","Страница поиска объектов","/search/",false);

            // Страница поиска объектов
            return View();
        }

        /// <summary>
        /// Отображает форму с результатами поиска
        /// </summary>
        /// <returns></returns>
        [Route("search/results")]
        public ActionResult Results()
        {
            // Формируем модель
            var searchData = new SearchFormModel();

            // Пытаемся считать
            try
            {
                searchData.ReadFromFormCollection(Request);
            }
            catch (Exception e)
            {
                UINotificationManager.Error("Не удалось считать модель поиска: " + e.Message);
                return RedirectToAction("Index");
            }

            // Поиск объектов
            var manager = Locator.GetService<IObjectSearchManager>();
            IList<EstateObject> results = manager.SearchObjects(searchData);

            // TODO: искать объекты по дополнительным критериям

            var model = new SearchResultsModel()
            {
                SearchData = searchData,
                SearchResults = results.OrderByDescending(o => o.AbsoluteBonusSize()).ToList() // Сортируем по размеру комиссии
            };

            PushNavigationItem("Поиск объектов", "Страница поиска объектов", "/search/");
            PushNavigationItem("Результаты поиска", "Страница c результатами поиска объектов", "/search/results",false);

            // Отображаем вид с результатами поиска
            return View(model);
        }

        /// <summary>
        /// Возвращает на клиент часть разметки, содержащую критерии для поиска
        /// </summary>
        /// <returns>Критерии поиска</returns>
        [Route("search/criterias")]
        public PartialViewResult SearchCriterias()
        {
            return PartialView(FieldsCache.AllSearchFields.Value);
        }

        /// <summary>
        /// Отправляет на клиент данные об указанном поле
        /// </summary>
        /// <param name="field">Имя поля</param>
        /// <returns>Данные по разметке поля</returns>
        [Route("search/criteria")]
        public JsonResult SearchCriteria(string field)
        {
            var fieldInfo = FieldsCache.AllSearchFields.Value.First(f => f.Name == field);
            return
                Json(
                    new
                        {
                            caption = fieldInfo.Caption,
                            description = fieldInfo.Tooltip,
                            fieldMarkup =
                        fieldInfo.RenderFieldEditor(
                            new FieldRenderingContext(Locator.GetService<IEstateObjectsRepository>().GetTempObject(),
                                                      Locator.GetService<IUsersRepository>().GetGuestUser())).Replace("name=\"","name=\"af_"),
                            filterMarkup = GetFilterMarkup(fieldInfo)
                        });
        }

        /// <summary>
        /// Возвращает разметку фильтра для указанного поля
        /// </summary>
        /// <param name="fieldInfo">Информация</param>
        private string GetFilterMarkup(ObjectFormField fieldInfo)
        {
            return fieldInfo.GetFilterMarkup();
        }

        /// <summary>
        /// Отправляет на клиент сведения о районах города в JSON формате
        /// </summary>
        /// <param name="cityId">Идентификатор города</param>
        /// <returns></returns>
        [Route("search/districts")]
        [HttpPost]
        public JsonResult Districts(long cityId)
        {
            // Географический менеджер
            var geoManager = Locator.GetService<IGeoManager>();
            if (cityId == -1)
            {
                return Json(new {success = true, districts = new object[]{}});
            }

            // Ищем город
            var city = geoManager.CitiesRepository.Load(cityId);
            if (city == null)
            {
                throw new ObjectNotFoundException(String.Format("Город с идентификатором {0} не найден",cityId));
            }

            // Отдаем результат
            return Json(new
                            {
                                success = true,
                                districts = city.GeoDistricts.Where(d => !String.IsNullOrEmpty(d.Bounds)).Select(d => new
                                                                              {
                                                                                  name = d.Name,
                                                                                  coordinates = GeoUtils.ParseBoundsCoordinates(d.Bounds),
                                                                                  id = d.Id
                                                                              })
                            });
        }

        /// <summary>
        /// Отправляет на клиент сведения о жилых массивов района города в JSON формате
        /// </summary>
        /// <param name="districtId">Идентификатор города</param>
        /// <returns></returns>
        [Route("search/areas")]
        [HttpPost]
        public JsonResult Areas(long districtId)
        {
            // Географический менеджер
            var geoManager = Locator.GetService<IGeoManager>();
            if (districtId == -1)
            {
                return Json(new {success = true, areas = new object[]{}});
            }

            // Ищем район
            var district = geoManager.DistrictsRepository.Load(districtId);
            if (district == null)
            {
                throw new ObjectNotFoundException(String.Format("Район города с идентификатором {0} не найден",districtId));
            }

            // Отдаем результат
            return Json(new
                            {
                                success = true,
                                parentCoords = GeoUtils.ParseBoundsCoordinates(district.Bounds),
                                areas = district.GeoResidentialAreas.Where(d => !String.IsNullOrEmpty(d.Bounds)).Select(d => new
                                                                              {
                                                                                  name = d.Name,
                                                                                  coordinates = GeoUtils.ParseBoundsCoordinates(d.Bounds),
                                                                                  id = d.Id
                                                                              })
                            });
        }

        /// <summary>
        /// Отправляет на клиент сведения о жилых массивов района города в JSON формате
        /// </summary>
        /// <param name="districtId">Идентификатор города</param>
        /// <returns></returns>
        [Route("search/multi-areas")]
        [HttpPost]
        public JsonResult MultiAreas(ComplexMultiAreaModel data)
        {
            var result = new List<ResultMultiAreaModel>();

            // Географический менеджер
            var geoManager = Locator.GetService<IGeoManager>();
            if (data.DistrictIds == null || (data.DistrictIds != null && data.DistrictIds.Count == 0))
            {
                result.Add(new ResultMultiAreaModel() { success = true, areas = new List<ResultAreaMultiAreaModel>() });
                return Json(result);
            }

            foreach (var districtId in data.DistrictIds)
            {
                // Ищем район
                var district = geoManager.DistrictsRepository.Load(districtId);
                if (district == null)
                {
                    throw new ObjectNotFoundException(String.Format("Район города с идентификатором {0} не найден", districtId));
                }

                // Отдаем результат
                result.Add(new ResultMultiAreaModel()
                {
                    success = true,
                    parentCoords = GeoUtils.ParseBoundsCoordinates(district.Bounds),
                    areas = district.GeoResidentialAreas.Where(d => !String.IsNullOrEmpty(d.Bounds)).Select(d => new ResultAreaMultiAreaModel()
                    {
                        name = d.Name,
                        coordinates = GeoUtils.ParseBoundsCoordinates(d.Bounds),
                        id = d.Id
                    })
                    .ToList()
                });
            }

            return Json(result);
        }

        /// <summary>
        /// Возвращает кусок разметки с компонентом выбора районов
        /// </summary>
        /// <param name="cityId">Идентификатор города</param>
        /// <returns></returns>
        [Route("search/districts-selector")]
        public ActionResult DistrictsSelector(long cityId)
        {
            // Получаем данные
            var geoManager = Locator.GetService<IGeoManager>();
            var districts = geoManager.DistrictsRepository.Search(d => d.CityId == cityId).ToList();

            return PartialView(districts);
        }

        /// <summary>
        /// Возвращает кусок разметки с компонентом выбора жилых массивов
        /// </summary>
        /// <param name="districtId"></param>
        /// <returns></returns>
        [Route("search/areas-selector")]
        public ActionResult AreasSelector(long districtId)
        {
            // Получаем данные
            var geoManager = Locator.GetService<IGeoManager>();
            var areas = geoManager.ResidentialAreasRepository.Search(d => d.DistrictId == districtId).ToList();

            return PartialView(areas);
        }

        /// <summary>
        /// Возвращает кусок разметки с компонентом выбора жилых массивов
        /// </summary>
        /// <param name="districtId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("search/agents-multi-selector")]
        public ActionResult AgentsMultiSelector(ComplexSearchFormModel data)
        {
            // Получаем данные
            var userRepo = Locator.GetService<IUsersRepository>();
            var agents = userRepo.Search(d => data.CompanyIds.Contains(d.CompanyId)/* && d.Activated*/).OrderBy(d => d.LastName).ToList();

            return PartialView("AgentsSelector", agents);
        }

        /// <summary>
        /// Возвращает кусок разметки с компонентом выбора агентов
        /// </summary>
        /// <param name="districtId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("search/areas-multi-selector")]
        public ActionResult AreasMultiSelector(ComplexSearchFormModel data)
        {
            // Получаем данные
            var geoManager = Locator.GetService<IGeoManager>();
            var areas = geoManager.ResidentialAreasRepository.Search(d => data.DistrictIds.Contains(d.DistrictId)).OrderBy(d => d.Name).ToList();

            return PartialView("AreasSelector", areas);
        }

        /// <summary>
        /// Возвращает кусок разметки с компонентом выбора улиц
        /// </summary>
        /// <param name="areaIds"></param>
        /// <returns></returns>
        [Route("search/streets-selector")]
        public ActionResult StreetsSelector(string areaIds)
        {
            // Получаем данные
            var geoManager = Locator.GetService<IGeoManager>();
            var areas =
                areaIds.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).Select(s => Convert.ToInt64(s)).Select(
                    i => geoManager.ResidentialAreasRepository.Load(i)).Where(a => a != null).Select(a => a.Id).ToList();
            var streets = geoManager.StreetsRepository.Search(s => areas.Contains(s.AreaId)).OrderBy(s => s.Name).ToList();

            return PartialView(streets);
        }

        /// <summary>
        /// Сохраняет поисковый запрос для текущего пользователя
        /// </summary>
        /// <param name="model">Модель данных поискового запроса</param>
        /// <returns></returns>
        [AuthorizationCheck()][Route("search/save-search-request")]
        public ActionResult SaveSearchRequest(SearchFormModel model)
        {
            // Пытаемся считать
            try
            {
                model.ReadFromFormCollection(Request);
            }
            catch (Exception e)
            {
                UINotificationManager.Error("Не удалось считать модель поиска: " + e.Message);
                return RedirectToAction("Index");
            }

            // Проверяем что поисковый запрос имеет имя
            if (String.IsNullOrEmpty(model.SearchRequestName))
            {
                return RedirectToAction("Index");
            }

            // Сохраняем запрос
            var request = new SearchRequest()
                {
                    DateCreated = DateTimeZone.Now,
                    TimesUsed = 0,
                    Title = model.SearchRequestName,
                    User = CurrentUser,
                    SearchUrl = Request.Url.Query
                };
            CurrentUser.SearchRequests.Add(request);
            Locator.GetService<IUsersRepository>().SubmitChanges();
            UINotificationManager.Success(string.Format("Поисковый запрос с именем {0} был успешно сохранен", request.Title));

            // Событие аудита
            PushAuditEvent(AuditEventTypes.Editing, string.Format("Создание поискового запроса {0}", request.Title));

            // Наполняем поисковый запрос объектами под него подходящими
            var manager = Locator.GetService<IObjectSearchManager>();
            var matchedObjects = manager.SearchObjects(model);
            if (matchedObjects.Count == 0)
            {
                UINotificationManager.Success("Под указанный поисковый запрос не попал ни один объект");
            }
            else
            {
                request.SearchRequestObjects.AddRange(matchedObjects.Select(mo => new SearchRequestObject()
                    {
                        DateCreated = DateTimeZone.Now,
                        Status = (short)SearchRequestObjectStatus.New,
                        New = true,
                        TriggerEvent = (short) SearchRequestTriggerEvent.RequestCreation,
                        EstateObject = mo,
                        SearchRequest = request
                    }));
                Locator.GetService<IUsersRepository>().SubmitChanges();
                UINotificationManager.Success(string.Format("Поисковый запрос успешно наполнен {0} объектами подходящими под его критерии", matchedObjects.Count));

                // Уведомляем пользователей, что под их объект зарегистрирован новый подходящий запрос
                foreach (var matchedObject in matchedObjects)
                {
                    
                    MailNotificationManager.Notify(matchedObject.User, "Зарегистрирован подходящий запрос", String.Format("Уважаемый {0} {1},<br/>по вашему объекту <a href='http://nprdv.ru/objects/{2}/card'>№{2}</a>, {3}, {4}, {5} зарегистрирован новый подходящий запрос. <a href='http://nprdv.ru/search-requests/by-object/{2}'>Посмотреть</a>",matchedObject.User.FirstName, matchedObject.User.SurName, matchedObject.Id, ((EstateOperations)matchedObject.Operation).GetEnumMemberName(), ((EstateTypes)matchedObject.ObjectType).GetEnumMemberName(), matchedObject.Address.ToShortAddressString()));
                }
            }

            return RedirectToAction("Index", "SearchRequests");
        }
    }
}
