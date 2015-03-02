using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.DataVisualization.Charting;
using NLog;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Routing;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Classes.Extensions;
using RDV.Web.Classes.Forms;
using RDV.Web.Classes.Forms.Fields;
using RDV.Web.Classes.Forms.Validators;
using RDV.Web.Classes.Notification.Mail;
using RDV.Web.Classes.Security;
using RDV.Web.Models.Objects;
using RDV.Domain.Core;
using RDV.Domain.Infrastructure.Misc;

namespace RDV.Web.Controllers
{
    /// <summary>
    /// Контроллер управления объектами
    /// </summary>
    public class ObjectsController : BaseController
    {
        /// <summary>
        /// Абстрактный репозиторий объектов
        /// </summary>
        public IEstateObjectsRepository ObjectsRepository { get; private set; }

        /// <summary>
        /// Логгер
        /// </summary>
        public Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public ObjectsController()
            : base()
        {
            // Инъекция
            ObjectsRepository = Locator.GetService<IEstateObjectsRepository>();
        }

        /// <summary>
        /// Отображает карточку указанного объекта
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        [Route("objects/{id}/card")]
        public ActionResult Card(long id)
        {
            // Ищем объект
            var obj = ObjectsRepository.Load(id);
            if (obj == null)
            {
                return View("ObjectNotFound");
            }

            // Nav chain
            PushObjectNavigationChain(obj);

            // Отдаем вид с объектом
            return View(obj);
        }

        /// <summary>
        /// Отображает карточку указанного объекта
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        [Route("objects/{id}/card-details")]
        public ActionResult CardDetails(long id)
        {
            // Ищем объект
            var obj = ObjectsRepository.Load(id);
            if (obj == null)
            {
                return View("ObjectNotFound");
            }

            // Отдаем вид с объектом
            return PartialView(obj);
        }

        /// <summary>
        /// Выполняет непосредственное добавление объекта текущему пользователю
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.AddObjects)]
        [Route("objects/add")]
        [HttpPost]
        public ActionResult Add(short type, short operation)
        {
            // Создаем объект и заполняем его самые основные свойства
            var estateObject = new EstateObject()
                                   {
                                       Status = (short)EstateStatuses.Draft,
                                       Operation = operation,
                                       ObjectType = type,
                                       CreatedBy = CurrentUser.Id,
                                       DateCreated = DateTimeZone.Now,
                                       ModifiedBy = -1,
                                       ObjectMainProperties = new ObjectMainProperty()
                                                                  {
                                                                      MultilistingBonusType = 355,
                                                                      
                                                                  },
                                       ObjectChangementProperties = new ObjectChangementProperty()
                                                                        {
                                                                            ChangedBy = -1,
                                                                            CreatedBy = CurrentUser.Id,
                                                                            DateCreated = DateTimeZone.Now,
                                                                            StatusChangedBy = -1
                                                                        },
                                       UserId = CurrentUser.Id,
                                       ObjectAdditionalProperties = new ObjectAdditionalProperty()
                                           {
                                               AgreementType = 265
                                           },
                                       ObjectCommunications = new ObjectCommunication(),
                                       ObjectRatingProperties = new ObjectRatingProperty(),
                                       Address = new Address()
                                           {
                                               CountryId = -1,
                                               RegionId = -1,
                                               RegionDistrictId = -1,
                                               CityId = -1,
                                               CityDistrictId = -1,
                                               DistrictResidentialAreaId = -1,
                                               StreetId = -1
                                           }
                                   };
            estateObject.ObjectMainProperties.EstateObject = estateObject;
            estateObject.ObjectChangementProperties.EstateObject = estateObject;
            estateObject.ObjectAdditionalProperties.EstateObject = estateObject;
            estateObject.ObjectCommunications.EstateObject = estateObject;
            estateObject.ObjectRatingProperties.EstateObject = estateObject;
            estateObject.Address.EstateObject = estateObject;

            // Добавляем начальный элемент истории к объекту
            var historyItem = new ObjectHistoryItem()
                                  {
                                      ClientId = -1,
                                      CompanyId = -1,
                                      CreatedBy = CurrentUser.Id,
                                      EstateObject = estateObject,
                                      HistoryStatus = -1,
                                      DateCreated = estateObject.DateCreated
                                  };
            estateObject.ObjectHistoryItems.Add(historyItem);

            // Добавляем объект
            ObjectsRepository.Add(estateObject);
            ObjectsRepository.SubmitChanges();

            // Нотификация
            UINotificationManager.Success("Объект успешно создан");

            // Переходим на основную форму редактирования объекта
            return Redirect(string.Format("/objects/{0}/service", estateObject.Id));
        }

        [Route("objects/changeContactPhone")]
        [HttpGet]
        public ActionResult ChangeContactPhone()
        {
            //var userService = Locator.GetService<IUsersRepository>();

            //var props = ObjectsRepository.FindAll()
            //    .Where(x => x.ObjectMainProperties.ContactPersonId.HasValue)
            //    .Select(x => x.ObjectMainProperties)
            //    .ToList();

            //foreach (var item in props)
            //{
            //    var person = userService.Load(item.ContactPersonId.Value);

            //    item.ContactPhone = person.Phone;
            //}

            //ObjectsRepository.SubmitChanges();

            //return Json("ok", JsonRequestBehavior.AllowGet);

            var userService = Locator.GetService<IUsersRepository>();

            var props = ObjectsRepository.FindAll()
                .Where(x => x.ObjectMainProperties.ContactPersonId.HasValue)
                .Select(x => x.ObjectMainProperties)
                .ToList();

            foreach (var item in props)
            {
                var person = userService.Load(item.ContactPersonId.Value);

                item.ContactPhone = (Int16)TypeContactPhone.AgentPhone1;
                item.ContactCompanyId = null;
            }

            ObjectsRepository.SubmitChanges();

            return Json("ok", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Отображает форму редактирования сервисной информации об объекте
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("objects/{id}/service")]
        public ActionResult ServiceInfo(long id)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Формируем модель
            var model = new ObjectFormModel(estateObject, FormPageFieldsFactory.ServicePageList(estateObject, CurrentUser));
            model.Fields.ReadValuesFromObject(model.EstateObject);

            // Nav chain
            PushObjectNavigationChain(estateObject);

            return View(model);
        }

        /// <summary>
        /// Сохраняет сервисные данные по объекту
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <param name="action">Действие проводимое после сохранения объекта</param>
        /// <param name="collection">Коллекция значений формы</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [HttpPost]
        public ActionResult SaveServiceInfo(long id, string action, FormCollection collection)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Не даем изменять статусы или поля у объектов статусе сделка
            if (estateObject.Status == (short) EstateStatuses.Deal)
            {
                UINotificationManager.Error(string.Format("Объект №{0} имеет статуса Сделка, поэтому не может быть отредактирован", estateObject.Id));
                // Определяем куда перенаправляться
                switch (action)
                {
                    case "next":
                        return Redirect(string.Format("/objects/{0}/location", estateObject.Id));
                    default:
                        return DesideSectionRedirect(action, estateObject.Id);
                }
            }

            // Создаем объект полей и считываем их с коллекций формы
            var fields = FormPageFieldsFactory.ServicePageList(estateObject, CurrentUser);
            fields.ReadValuesFromFormCollection(collection);

            // Проверяем было ли проведено изменение в цене
            var priceChanged = false;
            double? oldPrice = estateObject.ObjectMainProperties.Price;
            long? oldCurrency = estateObject.ObjectMainProperties.Currency;

            // Присваиваем их объекту и сохраняем изменения
            fields.WriteValuesToObject(estateObject);
            estateObject.ModifiedBy = CurrentUser.Id;
            estateObject.DateModified = DateTimeZone.Now;
            estateObject.ObjectChangementProperties.ChangedBy = CurrentUser.Id;
            estateObject.ObjectChangementProperties.DateModified = DateTimeZone.Now;

            if (estateObject.ObjectAdditionalProperties.AgreementType != 354)
            {
                estateObject.ObjectMainProperties.MultilistingBonus = null;
                estateObject.ObjectMainProperties.MultilistingBonusType = null;
            }

            ObjectsRepository.SubmitChanges();

            // Проверяем, была ли у нас изменена цена или валюта
            priceChanged = oldPrice != estateObject.ObjectMainProperties.Price |
                           estateObject.ObjectMainProperties.Currency != oldCurrency;
            if (priceChanged)
            {
                var priceChangement = new ObjectPriceChangement()
                    {
                        EstateObject = estateObject,
                        Currency = estateObject.ObjectMainProperties.Currency,
                        Value = estateObject.ObjectMainProperties.Price,
                        DateChanged = DateTimeZone.Now,
                        ChangedBy = CurrentUser.Id
                    };
                estateObject.ObjectPriceChangements.Add(priceChangement);
                estateObject.ObjectChangementProperties.PriceChanged = DateTimeZone.Now;
                if (estateObject.ObjectMainProperties.Price.HasValue && oldPrice.HasValue)
                {
                    estateObject.ObjectChangementProperties.PriceChanging = estateObject.ObjectMainProperties.Price - oldPrice;    
                }
                
                ObjectsRepository.SubmitChanges();
                

                // Уведомляем систему тригеров об изменении цены на объект
                if (estateObject.Status >= (short) EstateStatuses.Active)
                {
                    var triggerManager = Locator.GetService<IObjectsTriggerManager>();
                    triggerManager.ObjectPriceChanged(estateObject,estateObject.ObjectMainProperties.Price);    
                }
            }

            // Определяем куда перенаправляться
            switch (action)
            {
                case "next":
                    return Redirect(string.Format("/objects/{0}/location", estateObject.Id));
                default:
                    return DesideSectionRedirect(action, estateObject.Id);
            }
        }

        /// <summary>
        /// Отображает форму редактирования локационной информации об объекте
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("objects/{id}/location")]
        public ActionResult LocationInfo(long id)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Формируем модель
            var model = new ObjectFormModel(estateObject, FormPageFieldsFactory.LocationPageList(estateObject));
            model.Fields.ReadValuesFromObject(model.EstateObject);

            // Nav chain
            PushObjectNavigationChain(estateObject);

            return View(model);
        }

        /// <summary>
        /// Сохраняет локационные данные по объекту
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <param name="action">Действие проводимое после сохранения объекта</param>
        /// <param name="collection">Коллекция значений формы</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [HttpPost]
        public ActionResult SaveLocationInfo(long id, string action, FormCollection collection)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Не даем изменять статусы или поля у объектов статусе сделка
            if (estateObject.Status == (short)EstateStatuses.Deal)
            {
                UINotificationManager.Error(string.Format("Объект №{0} имеет статуса Сделка, поэтому не может быть отредактирован", estateObject.Id));
                // Определяем куда перенаправляться
                switch (action)
                {
                    case "next":
                        return Redirect(string.Format("/objects/{0}/tech", estateObject.Id));
                    case "prev":
                        return Redirect(string.Format("/objects/{0}/service", estateObject.Id));
                    default:
                        return DesideSectionRedirect(action, estateObject.Id);
                }
            }

            // Создаем объект полей и считываем их с коллекций формы
            var fields = FormPageFieldsFactory.LocationPageList(estateObject, true);
            fields.ReadValuesFromFormCollection(collection);

            // Присваиваем их объекту и сохраняем изменения
            fields.WriteValuesToObject(estateObject);
            estateObject.ModifiedBy = CurrentUser.Id;
            estateObject.DateModified = DateTimeZone.Now;
            estateObject.ObjectChangementProperties.ChangedBy = CurrentUser.Id;
            estateObject.ObjectChangementProperties.DateModified = DateTimeZone.Now;
            ObjectsRepository.SubmitChanges();

            // Определяем куда перенаправляться
            switch (action)
            {
                case "next":
                    return Redirect(string.Format("/objects/{0}/tech", estateObject.Id));
                case "prev":
                    return Redirect(string.Format("/objects/{0}/service", estateObject.Id));
                default:
                    return DesideSectionRedirect(action, estateObject.Id);
            }
        }

        /// <summary>
        /// Отображает форму редактирования технической информации об объекте
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("objects/{id}/tech")]
        public ActionResult TechInfo(long id)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Формируем модель
            var model = new ObjectFormModel(estateObject, FormPageFieldsFactory.TechPageList(estateObject));
            model.Fields.ReadValuesFromObject(model.EstateObject);

            // Nav chain
            PushObjectNavigationChain(estateObject);

            return View(model);
        }

        /// <summary>
        /// Сохраняет технические данные по объекту
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <param name="action">Действие проводимое после сохранения объекта</param>
        /// <param name="collection">Коллекция значений формы</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [HttpPost]
        public ActionResult SaveTechInfo(long id, string action, FormCollection collection)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Не даем изменять статусы или поля у объектов статусе сделка
            if (estateObject.Status == (short)EstateStatuses.Deal)
            {
                UINotificationManager.Error(string.Format("Объект №{0} имеет статуса Сделка, поэтому не может быть отредактирован", estateObject.Id));
                // Определяем куда перенаправляться
                switch (action)
                {
                    case "next":
                        return Redirect(string.Format("/objects/{0}/infrastructure", estateObject.Id));
                    case "prev":
                        return Redirect(string.Format("/objects/{0}/location", estateObject.Id));
                    default:
                        return DesideSectionRedirect(action, estateObject.Id);
                }
            }

            // Создаем объект полей и считываем их с коллекций формы
            var fields = FormPageFieldsFactory.TechPageList(estateObject);
            fields.ReadValuesFromFormCollection(collection);

            // Присваиваем их объекту и сохраняем изменения
            fields.WriteValuesToObject(estateObject);
            estateObject.ModifiedBy = CurrentUser.Id;
            estateObject.DateModified = DateTimeZone.Now;
            estateObject.ObjectChangementProperties.ChangedBy = CurrentUser.Id;
            estateObject.ObjectChangementProperties.DateModified = DateTimeZone.Now;
            ObjectsRepository.SubmitChanges();

            // Определяем куда перенаправляться
            switch (action)
            {
                case "next":
                    return Redirect(string.Format("/objects/{0}/infrastructure", estateObject.Id));
                case "prev":
                    return Redirect(string.Format("/objects/{0}/location", estateObject.Id));
                default:
                    return DesideSectionRedirect(action, estateObject.Id);
            }
        }

        /// <summary>
        /// Отображает форму редактирования инфраструктурной информации об объекте
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("objects/{id}/infrastructure")]
        public ActionResult InfrastructureInfo(long id)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Формируем модель
            var model = new ObjectFormModel(estateObject, FormPageFieldsFactory.InfrastructurePageList(estateObject));
            model.Fields.ReadValuesFromObject(model.EstateObject);

            // Nav chain
            PushObjectNavigationChain(estateObject);

            return View(model);
        }

        /// <summary>
        /// Сохраняет инфраструктурные данные по объекту
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <param name="action">Действие проводимое после сохранения объекта</param>
        /// <param name="collection">Коллекция значений формы</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [HttpPost]
        public ActionResult SaveInfrastructureInfo(long id, string action, FormCollection collection)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Не даем изменять статусы или поля у объектов статусе сделка
            if (estateObject.Status == (short)EstateStatuses.Deal)
            {
                UINotificationManager.Error(string.Format("Объект №{0} имеет статуса Сделка, поэтому не может быть отредактирован", estateObject.Id));
                // Определяем куда перенаправляться
                switch (action)
                {
                    case "next":
                        return Redirect(string.Format("/objects/{0}/expluatation", estateObject.Id));
                    case "prev":
                        return Redirect(string.Format("/objects/{0}/tech", estateObject.Id));
                    default:
                        return DesideSectionRedirect(action, estateObject.Id);
                }
            }

            // Создаем объект полей и считываем их с коллекций формы
            var fields = FormPageFieldsFactory.InfrastructurePageList(estateObject);
            fields.ReadValuesFromFormCollection(collection);

            // Присваиваем их объекту и сохраняем изменения
            fields.WriteValuesToObject(estateObject);
            estateObject.ModifiedBy = CurrentUser.Id;
            estateObject.DateModified = DateTimeZone.Now;
            estateObject.ObjectChangementProperties.ChangedBy = CurrentUser.Id;
            estateObject.ObjectChangementProperties.DateModified = DateTimeZone.Now;
            ObjectsRepository.SubmitChanges();

            // Определяем куда перенаправляться
            switch (action)
            {
                case "next":
                    return Redirect(string.Format("/objects/{0}/expluatation", estateObject.Id));
                case "prev":
                    return Redirect(string.Format("/objects/{0}/tech", estateObject.Id));
                default:
                    return DesideSectionRedirect(action, estateObject.Id);
            }
        }

        /// <summary>
        /// Отображает форму редактирования эксплуатационной информации об объекте
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("objects/{id}/expluatation")]
        public ActionResult ExpluatationInfo(long id)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Формируем модель
            var model = new ObjectFormModel(estateObject, FormPageFieldsFactory.ExpluatationPageList(estateObject));
            model.Fields.ReadValuesFromObject(model.EstateObject);

            // Nav chain
            PushObjectNavigationChain(estateObject);

            return View(model);
        }

        /// <summary>
        /// Сохраняет эксплутационные данные по объекту
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <param name="action">Действие проводимое после сохранения объекта</param>
        /// <param name="collection">Коллекция значений формы</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [HttpPost]
        public ActionResult SaveExpluatationInfo(long id, string action, FormCollection collection)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Не даем изменять статусы или поля у объектов статусе сделка
            if (estateObject.Status == (short)EstateStatuses.Deal)
            {
                UINotificationManager.Error(string.Format("Объект №{0} имеет статуса Сделка, поэтому не может быть отредактирован", estateObject.Id));
                // Определяем куда перенаправляться
                switch (action)
                {
                    case "next":
                        return Redirect(string.Format("/objects/{0}/legal", estateObject.Id));
                    case "prev":
                        return Redirect(string.Format("/objects/{0}/infrastructure", estateObject.Id));
                    default:
                        return DesideSectionRedirect(action, estateObject.Id);
                }
            }

            // Создаем объект полей и считываем их с коллекций формы
            var fields = FormPageFieldsFactory.ExpluatationPageList(estateObject);
            fields.ReadValuesFromFormCollection(collection);

            // Присваиваем их объекту и сохраняем изменения
            fields.WriteValuesToObject(estateObject);
            estateObject.ModifiedBy = CurrentUser.Id;
            estateObject.DateModified = DateTimeZone.Now;
            estateObject.ObjectChangementProperties.ChangedBy = CurrentUser.Id;
            estateObject.ObjectChangementProperties.DateModified = DateTimeZone.Now;
            ObjectsRepository.SubmitChanges();

            // Определяем куда перенаправляться
            switch (action)
            {
                case "next":
                    return Redirect(string.Format("/objects/{0}/legal", estateObject.Id));
                case "prev":
                    return Redirect(string.Format("/objects/{0}/infrastructure", estateObject.Id));
                default:
                    return DesideSectionRedirect(action, estateObject.Id);
            }
        }

        /// <summary>
        /// Отображает форму редактирования юридической информации об объекте
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("objects/{id}/legal")]
        public ActionResult LegalInfo(long id)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Формируем модель
            var model = new ObjectFormModel(estateObject, FormPageFieldsFactory.LegalPageList(estateObject));
            model.Fields.ReadValuesFromObject(model.EstateObject);

            // Nav chain
            PushObjectNavigationChain(estateObject);

            return View(model);
        }

        /// <summary>
        /// Сохраняет юридические данные по объекту
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <param name="action">Действие проводимое после сохранения объекта</param>
        /// <param name="collection">Коллекция значений формы</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [HttpPost]
        public ActionResult SaveLegalInfo(long id, string action, FormCollection collection)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Не даем изменять статусы или поля у объектов статусе сделка
            if (estateObject.Status == (short)EstateStatuses.Deal)
            {
                UINotificationManager.Error(string.Format("Объект №{0} имеет статуса Сделка, поэтому не может быть отредактирован", estateObject.Id));
                // Определяем куда перенаправляться
                switch (action)
                {
                    case "next":
                        return Redirect(string.Format("/objects/{0}/main", estateObject.Id));
                    case "prev":
                        return Redirect(string.Format("/objects/{0}/infrastructure", estateObject.Id));
                    default:
                        return DesideSectionRedirect(action, estateObject.Id);
                }
            }

            // Создаем объект полей и считываем их с коллекций формы
            var fields = FormPageFieldsFactory.LegalPageList(estateObject);
            fields.ReadValuesFromFormCollection(collection);

            // Присваиваем их объекту и сохраняем изменения
            fields.WriteValuesToObject(estateObject);
            estateObject.ModifiedBy = CurrentUser.Id;
            estateObject.DateModified = DateTimeZone.Now;
            estateObject.ObjectChangementProperties.ChangedBy = CurrentUser.Id;
            estateObject.ObjectChangementProperties.DateModified = DateTimeZone.Now;
            ObjectsRepository.SubmitChanges();

            // Определяем куда перенаправляться
            switch (action)
            {
                case "next":
                    return Redirect(string.Format("/objects/{0}/main", estateObject.Id));
                case "prev":
                    return Redirect(string.Format("/objects/{0}/infrastructure", estateObject.Id));
                default:
                    return DesideSectionRedirect(action, estateObject.Id);
            }
        }

        /// <summary>
        /// Отображает форму редактирования самой наиглавнейшей информации об объекте
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("objects/{id}/main")]
        public ActionResult MainInfo(long id)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Формируем модель
            var model = new ObjectFormModel(estateObject, FormPageFieldsFactory.MainPageList(estateObject));
            model.Fields.ReadValuesFromObject(model.EstateObject);

            // Nav chain
            PushObjectNavigationChain(estateObject);

            return View(model);
        }

        /// <summary>
        /// Сохраняет основные данные по объекту
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <param name="action">Действие проводимое после сохранения объекта</param>
        /// <param name="collection">Коллекция значений формы</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [HttpPost]
        public ActionResult SaveMainInfo(long id, string action, FormCollection collection)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Не даем изменять статусы или поля у объектов статусе сделка
            if (estateObject.Status == (short)EstateStatuses.Deal)
            {
                UINotificationManager.Error(string.Format("Объект №{0} имеет статуса Сделка, поэтому не может быть отредактирован", estateObject.Id));
                // Определяем куда перенаправляться
                switch (action)
                {
                    case "next":
                        return Redirect(string.Format("/objects/{0}/media", estateObject.Id));
                    case "prev":
                        return Redirect(string.Format("/objects/{0}/legal", estateObject.Id));
                    default:
                        return DesideSectionRedirect(action, estateObject.Id);
                }
            }

            // Создаем объект полей и считываем их с коллекций формы
            var fields = FormPageFieldsFactory.MainPageList(estateObject);
            fields.ReadValuesFromFormCollection(collection);

            // Присваиваем их объекту и сохраняем изменения
            fields.WriteValuesToObject(estateObject);
            estateObject.ModifiedBy = CurrentUser.Id;
            estateObject.DateModified = DateTimeZone.Now;
            estateObject.ObjectChangementProperties.ChangedBy = CurrentUser.Id;
            estateObject.ObjectChangementProperties.DateModified = DateTimeZone.Now;
            ObjectsRepository.SubmitChanges();

            // Определяем куда перенаправляться
            switch (action)
            {
                case "next":
                    return Redirect(string.Format("/objects/{0}/media", estateObject.Id));
                case "prev":
                    return Redirect(string.Format("/objects/{0}/legal", estateObject.Id));
                default:
                    return DesideSectionRedirect(action, estateObject.Id);
            }
        }

        /// <summary>
        /// Отправляет на клиент HTML разметку полей для выбора из гео справочника
        /// </summary>
        /// <param name="estateObjectId">Идентификатор объекта недвижимости</param>
        /// <param name="fieldType">Тип поля, которое требуется получить</param>
        /// <param name="parentId">Выбранный родительский объект</param>
        /// <returns></returns>
        public ActionResult LocationFields(long estateObjectId, string fieldType, long parentId)
        {
            // Ищем объект
            var currentEstateObject = ObjectsRepository.Load(estateObjectId);
            if (currentEstateObject == null)
            {
                return Content(string.Format("Объект с идентификатором {0} не найден", estateObjectId));
            }

            // Гео менеджер
            var geoManager = Locator.GetService<IGeoManager>();

            var renderList = new List<ObjectFormField>();

            // В зависимости от затребованного поля - отдаем им поля
            switch (fieldType)
            {
                case "geo-region":
                    renderList.Add(new ObjectFormDropDownField()
                    {
                        Caption = "Регион",
                        Name = "geo-region",
                        Tooltip = "Регион расположения объекта",
                        Items = geoManager.RegionsRepository.Search(r => r.CountryId == parentId).OrderBy(c => c.Name).ToDictionary(c => c.Id.ToString(), c => c.Name),
                        InsertBlankItem = true
                    });
                    break;
                case "geo-region-district":
                    renderList.Add(new ObjectFormDropDownField()
                    {
                        Caption = "Район региона",
                        Name = "geo-region-district",
                        Tooltip = "Район региона, в котором расположен объект",
                        Items = geoManager.RegionsDistrictsRepository.Search(r => r.RegionId == parentId).OrderBy(c => c.Name).ToDictionary(c => c.Id.ToString(), c => c.Name),
                        InsertBlankItem = true
                    });
                    break;
                case "geo-city":
                    renderList.Add(new ObjectFormDropDownField()
                    {
                        Caption = "Город",
                        Name = "geo-city",
                        Tooltip = "Город, в котором расположен объект",
                        Items = geoManager.CitiesRepository.Search(r => r.RegionDistrictId == parentId).OrderBy(c => c.Name).ToDictionary(c => c.Id.ToString(), c => c.Name),
                        InsertBlankItem = true
                    });
                    break;
                case "geo-city-district":
                    renderList.Add(new ObjectFormDropDownField()
                    {
                        Caption = "Район города",
                        Name = "geo-city-district",
                        Tooltip = "Район города, в котором расположен объект",
                        Items = geoManager.DistrictsRepository.Search(r => r.CityId == parentId).OrderBy(c => c.Name).ToDictionary(c => c.Id.ToString(), c => c.Name),
                        InsertBlankItem = true
                    });
                    break;
                case "geo-residential-area":
                    renderList.Add(new ObjectFormStreetField()
                    {
                        Caption = "Улица",
                        Name = "geo-street",
                        Tooltip = "Улица, на которой расположен объект",
                    });
                    renderList.Add(new ObjectFormDropDownField()
                    {
                        Caption = "Жилой массив города",
                        Name = "geo-residential-area",
                        Tooltip = "Жилой массив, в котором расположен объект",
                        Items = geoManager.ResidentialAreasRepository.Search(r => r.DistrictId == parentId).OrderBy(c => c.Name).ToDictionary(c => c.Id.ToString(), c => c.Name),
                        InsertBlankItem = true
                    });
                    renderList.Add(new ObjectFormTextField()
                    {
                        Caption = "Номер дома",
                        Name = "house-number",
                        Tooltip = "Номер дома объекта недвижимости",
                        Required = "selling/lising",
                        Validator = new FieldValidator()
                            {
                                new MaxLengthValidationRule(50),
                                new HouseNumberValidationRule()
                            },
                        RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent)
                                }
                    });
                    renderList.Add(new ObjectFormTextField()
                    {
                        Caption = "Номер корпуса",
                        Name = "block-number",
                        Tooltip = "Номер корпуса объекта недвижимости",
                        Validator = new FieldValidator()
                            {
                                new MaxLengthValidationRule(50)
                            },
                        RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                },
                    });
                    renderList.Add(new ObjectFormTextField()
                    {
                        Caption = "Номер квартиры",
                        Name = "flat-number",
                        Tooltip = "Номер квартиры объекта недвижимости",
                        Required = "selling/lising",
                        Validator = new FieldValidator()
                            {
                                new MaxLengthValidationRule(50)
                            },
                        RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                },
                    });
                    renderList.Add(new ObjectFormTextField()
                    {
                        Caption = "Номер участка",
                        Name = "land-number",
                        Tooltip = "Номер участка объекта недвижимости",
                        Validator = new FieldValidator()
                            {
                                new MaxLengthValidationRule(50)
                            },
                        RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                },
                    });
                    break;
            }

            // Фильтруем поля
            var context = new FieldRenderingContext(currentEstateObject, CurrentUser);
            renderList = renderList.Where(f => f.CheckVisibility(context)).ToList();

            return
                Content(String.Join("\n",
                                    renderList.Select(
                                        f => f.RenderField(new FieldRenderingContext(currentEstateObject, CurrentUser)))));
        }

        /// <summary>
        /// Отображает форму редактирования медиа информации об объекте
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("objects/{id}/media")]
        public ActionResult MediaInfo(long id)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Формируем модель
            var model = new MediaPageModel(estateObject);

            // Nav chain
            PushObjectNavigationChain(estateObject);

            return View(model);
        }

        /// <summary>
        /// Сохраняет локационные данные по объекту
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <param name="action">Действие проводимое после сохранения объекта</param>
        /// <param name="collection">Коллекция значений формы</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [HttpPost]
        public ActionResult SaveMediaInfo(long id, string action, FormCollection collection)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Сохраняем загруженные файлы
            var filesRepository = Locator.GetService<IStoredFilesRepository>();
            var index = 0;
            foreach (string fileName in Request.Files)
            {
                var postedFile = Request.Files[index];
                index++;
                // Проверяем что файл собственно прислан
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    // Устанавливаем тип файла. Сохраняем только те файлы которые знает
                    bool needSave = false;
                    string folder = String.Empty;
                    ObjectMediaTypes mediaType = ObjectMediaTypes.Photo;
                    if (postedFile.ContentType.ToLower().Contains("image"))
                    {
                        needSave = true;
                        folder = "ObjectPhotos";
                        mediaType = ObjectMediaTypes.Photo;
                    }
                    else if (postedFile.ContentType.ToLower().Contains("video"))
                    {
                        needSave = true;
                        folder = "ObjectVideos";
                        mediaType = ObjectMediaTypes.Video;
                    }
                    if (needSave)
                    {
                        // Сохраняем файл в системе хранения файлов
                        var storedFile = filesRepository.SavePostedFile(postedFile, folder,true);
                        // Создаем медиа объект для файла
                        var mediaObject = new ObjectMedia()
                                              {
                                                  CreatedBy = CurrentUser.Id,
                                                  DateCreated = DateTimeZone.Now,
                                                  MediaType = (short)mediaType,
                                                  MediaUrl = storedFile.GetURI(),
                                                  EstateObject = estateObject
                                              };
                        ObjectsRepository.SubmitChanges();
                    }
                }
            }

            // Проводим сохранение описаний, заданных в таблице
            foreach (var key in collection.AllKeys.Where(k => k.StartsWith("title-")))
            {
                // Извлекаем идентификатор медиа, описание которого задали и ищем в списке медиа элемент
                var sId = Convert.ToInt64(key.Split('-')[1]);
                var media = estateObject.ObjectMedias.FirstOrDefault(m => m.Id == sId);
                if (media != null)
                {
                    media.Title = collection[key];
                }
            }

            // Проводим сохранение позиций фотографий, заданных в таблице
            foreach(var key in collection.AllKeys.Where(k => k.StartsWith("position-")))
            {
                // Извлекаем идентификатор медиа, позицию которого задали и ищем в списке медиа элемент
                var sId = Convert.ToInt64(key.Split('-')[1]);
                var media = estateObject.ObjectMedias.FirstOrDefault(m => m.Id == sId);
                if (media != null)
                {
                    media.Position = Convert.ToInt32(collection[key]);
                }   
            }

            // Устанавливаем главную фотографию или медиа элемент
            var mainId = Convert.ToInt64(collection["mainid"]);
            foreach (var objectMedia in estateObject.ObjectMedias)
            {
                objectMedia.IsMain = false;
                if (objectMedia.Id == mainId)
                {
                    objectMedia.IsMain = true;
                }
            }

            // Проводим удаление объектов
            foreach (var key in collection.AllKeys.Where(k => k.StartsWith("delete-")))
            {
                // Извлекаем идентификатор медиа, описание которого задали и ищем в списке медиа элемент
                var sId = Convert.ToInt64(key.Split('-')[1]);
                var media = estateObject.ObjectMedias.FirstOrDefault(m => m.Id == sId);
                if (media != null)
                {
                    media.EstateObject = null;
                    estateObject.ObjectMedias.Remove(media);
                    filesRepository.DeleteFile(media.PreviewUrl);
                    filesRepository.DeleteFile(media.MediaUrl);
                }
            }

            // Присваиваем общие изменения в объекте
            estateObject.ModifiedBy = CurrentUser.Id;
            estateObject.DateModified = DateTimeZone.Now;
            estateObject.ObjectChangementProperties.ChangedBy = CurrentUser.Id;
            estateObject.ObjectChangementProperties.DateModified = DateTimeZone.Now;
            ObjectsRepository.SubmitChanges();

            // Определяем куда перенаправляться
            switch (action)
            {
                case "prev":
                    return Redirect(string.Format("/objects/{0}/expluatation", estateObject.Id));
                default:
                    return DesideSectionRedirect(action, estateObject.Id);
            }
        }

        /// <summary>
        /// Сохраняет фотографии, загруженные с помощью Flesh загрузчика
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        //[AuthorizationCheck()]
        [HttpPost]
        public ActionResult SaveMediaInfoEx(long id)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            /*long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }*/

            // Сохраняем загруженные файлы
            var filesRepository = Locator.GetService<IStoredFilesRepository>();
            var index = 0;

            Logger.Log(LogLevel.Info,string.Format("Скрипт Макса прислал ид = {0} и {1} файлов", id,Request.Files.Count));
            
            foreach (string fileName in Request.Files)
            {
                var postedFile = Request.Files[index];
                index++;
                // Проверяем что файл собственно прислан
                if (postedFile != null && postedFile.ContentLength > 0)
                {
                    // Проверяем количество сохраненных файлов
                    if (estateObject.GetObjectsMedia(true).Count >= 15)
                    {
                        UINotificationManager.Error("Вы достигли максимального количества фотографий (не более 15 штук) для данного объекта.");
                        break;
                    }
                    // Устанавливаем тип файла. Сохраняем только те файлы которые знает
                    bool needSave = false;
                    string folder = String.Empty;
                    ObjectMediaTypes mediaType = ObjectMediaTypes.Photo;
                    if (postedFile.ContentType.ToLower().Contains("image"))
                    {
                        needSave = true;
                        folder = "ObjectPhotos";
                        mediaType = ObjectMediaTypes.Photo;
                    }
                    else if (postedFile.ContentType.ToLower().Contains("video"))
                    {
                        needSave = true;
                        folder = "ObjectVideos";
                        mediaType = ObjectMediaTypes.Video;
                    }
                    if (needSave)
                    {
                        // Сохраняем файл в системе хранения файлов
                        var storedFile = filesRepository.SavePostedFile(postedFile, folder,true);
                        var title = Server.UrlDecode(Request[String.Format("title{0}", index - 1)]);
                        // Создаем медиа объект для файла
                        var mediaObject = new ObjectMedia()
                        {
                            //CreatedBy = CurrentUser.Id,
                            DateCreated = DateTimeZone.Now,
                            MediaType = (short)mediaType,
                            MediaUrl = storedFile.GetURI(),
                            EstateObject = estateObject,
                            Title = title
                        };
                        ObjectsRepository.SubmitChanges();
                    }
                }
            }

            // Присваиваем общие изменения в объекте
            //estateObject.ModifiedBy = CurrentUser.Id;
            estateObject.DateModified = DateTimeZone.Now;
            //estateObject.ObjectChangementProperties.ChangedBy = CurrentUser.Id;
            estateObject.ObjectChangementProperties.DateModified = DateTimeZone.Now;
            ObjectsRepository.SubmitChanges();

            // Переходим обратно
            return RedirectToAction("MediaInfo", new {id = id});
        }

        /// <summary>
        /// Отображает страницу со списком изменений цены по объекту
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ViewObjects)]
        [Route("objects/{id}/price-changements")]
        public ActionResult PriceChangements(long id)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Формируем модель
            var model = estateObject.ObjectPriceChangements.OrderByDescending(d => d.DateChanged).ToList();
            ViewBag.ObjectId = id;

            // Nav chain
            PushObjectNavigationChain(estateObject);
            PushNavigationItem("Изменение цены", "История изменений цены", String.Empty, false);

            return View(model);
        }

        /// <summary>
        /// Отображает изображение с графиком изменения цены и валюты по объекту
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ViewObjects)]
        [Route("objects/{id}/price-changements-graph")]
        public ActionResult PriceChangementGraph(long id)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Конструируем диаграмму
            var chart = new Chart()
            {
                Height = 400,
                Width = 600,
                ImageType = ChartImageType.Png
            };
            chart.Titles.Add("Динамика измения цены");
            var mainArea = chart.ChartAreas.Add("MainArea");
            mainArea.AxisX.IsLabelAutoFit = true;
            mainArea.AxisY.IsLabelAutoFit = true;
            mainArea.AxisY.LineColor = Color.LightGray;
            mainArea.AxisX.LineColor = Color.LightGray;
            mainArea.AxisX.Title = "Дата";
            mainArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            mainArea.AxisY.MajorGrid.LineColor = Color.LightGray;

            var mainLegend = chart.Legends.Add("MainLegend");
            mainLegend.Docking = Docking.Bottom;
            mainLegend.TableStyle = LegendTableStyle.Wide;

            var objectsSeries = chart.Series.Add("Изменение цены на объект");
            objectsSeries.ChartArea = "MainArea";
            objectsSeries.ChartType = SeriesChartType.Line;
            objectsSeries.Legend = "MainLegend";
            objectsSeries.MarkerStyle = MarkerStyle.Circle;

            // Рендерим данные на диаграмму
            var position = 0;
            foreach (var data in estateObject.ObjectPriceChangements.OrderBy(c => c.DateChanged))
            {
                position++;
                objectsSeries.Points.Add(new DataPoint()
                {
                    XValue = position,
                    YValues = new double[] { data.Value.HasValue ? data.Value.Value : 0 },
                    AxisLabel = data.DateChanged.FormatDate()
                });

            }

            // Сохраняем диаграмму как изображение
            var stream = new MemoryStream();
            chart.SaveImage(stream, ChartImageFormat.Png);
            stream.Position = 0;

            // Отдаем изображение
            return File(stream, "image/png");
        }

        /// <summary>
        /// Отображает информацию об клиенте, связанном с текущим объектом
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns>Перенаправляет на форму редактирования клиента</returns>
        [AuthorizationCheck(Permission.ViewCompanyClients)]
        [Route("objects/{id}/client")]
        public ActionResult ObjectClient(long id)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("ServiceInfo");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Проверяем, есть ли клиент
            if (estateObject.ClientId != -1)
            {
                return RedirectToAction("EditClient", "Account", new { id = estateObject.ClientId });
            }
            else
            {
                UINotificationManager.Error(string.Format("Клиент для данного объекта не задан. Задайте его на вкладке сервисной информации и сохраните изменения"));
                return RedirectToAction("ServiceInfo");
            }
        }

        /// <summary>
        /// Решает, в какую секцию перенаправить пользователя на основании отправленных данных
        /// </summary>
        /// <param name="action">Действие</param>
        /// <param name="objectId">Идентификатор объекта</param>
        /// <returns></returns>
        private ActionResult DesideSectionRedirect(string action, long objectId)
        {
            switch (action)
            {
                case "main":
                    return Redirect(String.Format("/objects/{0}/main", objectId));
                case "tech":
                    return Redirect(String.Format("/objects/{0}/tech", objectId));
                case "legal":
                    return Redirect(String.Format("/objects/{0}/legal", objectId));
                case "infrastructure":
                    return Redirect(String.Format("/objects/{0}/infrastructure", objectId));
                case "expluatation":
                    return Redirect(String.Format("/objects/{0}/expluatation", objectId));
                case "location":
                    return Redirect(String.Format("/objects/{0}/location", objectId));
                case "service":
                    return Redirect(String.Format("/objects/{0}/service", objectId));
                case "media":
                    return Redirect(String.Format("/objects/{0}/media", objectId));
                default:
                    var obj = ObjectsRepository.Load(objectId);
                    var sb = new StringBuilder();
                    sb.Append("/account/objects/");
                    if (obj.UserId == CurrentUser.Id)
                    {
                        sb.Append("my");
                    }
                    else if (obj.User.Company != null && obj.User.Company.Id == CurrentUser.CompanyId)
                    {
                        sb.Append("company");
                    }
                    else
                    {
                        sb.Append("all");
                    }
                    switch ((EstateStatuses)obj.Status)
                    {
                        case EstateStatuses.Draft:
                            sb.Append("#drafts");
                            break;
                        case EstateStatuses.Active:
                            sb.Append("#active");
                            break;
                        case EstateStatuses.Advance:
                            sb.Append("#advance");
                            break;
                        case EstateStatuses.TemporarilyWithdrawn:
                            sb.Append("#temp-withdrawn");
                            break;
                        case EstateStatuses.Withdrawn:
                            sb.Append("#withdrawn");
                            break;
                        case EstateStatuses.Deal:
                            sb.Append("#deal");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    return Redirect(sb.ToString());
            }
        }

        /// <summary>
        /// Возвращает на клиент данные об фотографиях объекта в формате пригодном для использовании в fancybox
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ViewObjects)]
        [Route("objects/get-photos/{id}")]
        public JsonResult GetObjectPhotos(long id)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                throw new Exception(string.Format("Объект с идентификатором {0} не найден", id));
            }

            // Выбираем модели
            var models =
                estateObject.ObjectMedias.Where(m => m.MediaType == (short)ObjectMediaTypes.Photo).OrderByDescending(
                    d => d.DateCreated).Select(i => new ObjectMediaModel(i)).Select(
                        i => new { title = i.Title, href = i.MediaFullUrl }).ToList();

            // Отдаем данные на клиент
            return Json(new
                            {
                                count = models.Count,
                                images = models
                            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Отображает страницу с историей статусов объектов
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ViewObjects)]
        [Route("objects/{id}/history")]
        public ActionResult History(long id)
        {
            // Ищем объект
            var estateObject = ObjectsRepository.Load(id);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификатором {0} не найден", id));
                return RedirectToAction("Index", "Account");
            }

            // Определяем тип требуемого пермишенна
            long targetPermission;
            if (estateObject.UserId == CurrentUser.Id)
            {
                targetPermission = Permission.EditOwnObjects;
            }
            else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
            {
                targetPermission = Permission.EditCompanyObjects;
            }
            else
            {
                targetPermission = Permission.EditAllObjects;
            }

            // Проверяем, есть ли у нас пермишены
            if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
            {
                UINotificationManager.Error(String.Format("Недостаточно правилегий на редактирование объекта №{0}", estateObject.Id));
                return RedirectToAction("Card", new { id = id });
            }

            // Формируем модель
            var model = estateObject.ObjectHistoryItems.OrderBy(d => d.DateCreated).ToList();
            ViewBag.ObjectId = id;

            // Nav chain
            PushObjectNavigationChain(estateObject);
            PushNavigationItem("История операция", "История операций, совершенных над объектом", String.Empty, false);

            return View(model);
        }

        /// <summary>
        /// Обрабатывает запрос на заказ звонка от агента
        /// </summary>
        /// <param name="objectId">Идентификатор объекта</param>
        /// <param name="phone">Телефон по которому связаться</param>
        /// <returns>JSON success</returns>
        [HttpPost]
        [Route("objects/callback")]
        public JsonResult Callback(long objectId, string phone)
        {
            try
            {
                // Ищем объект
                var obj = ObjectsRepository.Load(objectId);
                if (obj == null)
                {
                    throw new ObjectNotFoundException(String.Format("Объект с идентификатором {0} не найден", objectId));
                }

                // Формируем модель для нотификации
                var model = new
                                {
                                    FirstName = obj.User.FirstName,
                                    LastName = obj.User.LastName,
                                    SurName = obj.User.SurName,
                                    Id = objectId,
                                    Address = obj.Address.ToShortAddressString(),
                                    Phone = phone
                                };

                // Нотифицируем по Email и SMS
                Locator.GetService<IMailNotificationManager>().Notify(obj.User, String.Format("Заказ звонка по объекту №{0}", objectId), MailTemplatesFactory.GetCallbackTemplate(model).ToString());
                Locator.GetService<ISMSNotificationManager>().Notify(obj.User, String.Format("Заказ звонка на номер {0} по объекту №{1} ({2})", model.Phone, model.Id, model.Address));

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                return Json(new
                                {
                                    success = false,
                                    msg = e.Message
                                });
            }
        }
        [HttpPost]
        [Route("objects/view-request")]
        public JsonResult ViewRequest(long objectId, string phone, string name, string viewDate)
        {
            try
            {
                // Ищем объект
                var obj = ObjectsRepository.Load(objectId);
                if (obj == null)
                {
                    throw new ObjectNotFoundException(String.Format("Объект с идентификатором {0} не найден", objectId));
                }

                // Формируем модель для нотификации
                var model = new
                                {
                                    FirstName = obj.User.FirstName,
                                    LastName = obj.User.LastName,
                                    SurName = obj.User.SurName,
                                    Id = objectId,
                                    Address = obj.Address.ToShortAddressString(),
                                    Phone = phone,
                                    Name = name,
                                    ViewDate = viewDate
                                };

                // Нотифицируем по Email и SMS
                Locator.GetService<IMailNotificationManager>().Notify(obj.User, String.Format("Заявка на осмотр объекта №{0}", objectId), MailTemplatesFactory.GetViewRequestTemplate(model).ToString());
                Locator.GetService<ISMSNotificationManager>().Notify(obj.User, String.Format("Поступила заявка на осмотр объекта №{0} на {1} от {2} ({3})", model.Id, model.ViewDate, model.Name, model.Phone));

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                return Json(new
                                {
                                    success = false,
                                    msg = e.Message
                                });
            }
        }

        /// <summary>
        /// Ищет похожие объекты и перенаправляет на страницу результатов поиска
        /// </summary>
        /// <param name="id">Идентификатор объекта, похожие которого требуется найти</param>
        /// <returns>Страница с результатами поиска</returns>
        [Route("objects/find-similar/{id}")]
        public ActionResult FindSimilar(long id)
        {
            // Ищем объект
            var obj = ObjectsRepository.Load(id);
            if (obj == null)
            {
                return View("ObjectNotFound");
            }

            // В зависимости от типа объекта, устанавливаем критерии поиска
            var redirectUrl = new StringBuilder("/search/results");
            redirectUrl.AppendFormat("?operation={0}&objectType={1}", obj.Operation, obj.ObjectType);
            switch ((EstateTypes)obj.ObjectType)
            {
                case EstateTypes.Room:
                case EstateTypes.Flat:
                    if (obj.Address.GeoCity != null)
                    {
                        redirectUrl.AppendFormat("&cityId={0}", obj.Address.CityId);
                    }
                    if (obj.Address.GeoDistrict != null)
                    {
                        redirectUrl.AppendFormat("&districtIds={0}", obj.Address.CityDistrictId);
                    }
                    if (obj.Address.GeoResidentialArea != null)
                    {
                        redirectUrl.AppendFormat("&areaIds={0}", obj.Address.DistrictResidentialAreaId);
                    }
                    if (obj.ObjectAdditionalProperties.RoomsCount.HasValue)
                    {
                        redirectUrl.AppendFormat("&af_rooms-count={0}", obj.ObjectAdditionalProperties.RoomsCount);
                    }
                    break;
                case EstateTypes.Office:
                case EstateTypes.House:
                    if (obj.Address.GeoCity != null)
                    {
                        redirectUrl.AppendFormat("&cityId={0}", obj.Address.CityId);
                    }
                    if (obj.Address.GeoDistrict != null)
                    {
                        redirectUrl.AppendFormat("&districtIds={0}", obj.Address.CityDistrictId);
                    }
                    if (obj.Address.GeoResidentialArea != null)
                    {
                        redirectUrl.AppendFormat("&areaIds={0}", obj.Address.DistrictResidentialAreaId);
                    }
                    if (obj.ObjectAdditionalProperties.RoomsCount.HasValue)
                    {
                        redirectUrl.AppendFormat("&af_rooms-count={0}", obj.ObjectAdditionalProperties.RoomsCount);
                    }
                    break;
                case EstateTypes.Land:
                case EstateTypes.Garage:
                    if (obj.Address.GeoCity != null)
                    {
                        redirectUrl.AppendFormat("&cityId={0}", obj.Address.CityId);
                    }
                    if (obj.Address.GeoDistrict != null)
                    {
                        redirectUrl.AppendFormat("&districtIds={0}", obj.Address.CityDistrictId);
                    }
                    if (obj.Address.GeoResidentialArea != null)
                    {
                        redirectUrl.AppendFormat("&areaIds={0}", obj.Address.DistrictResidentialAreaId);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Редиректим на указанный урл
            return Redirect(redirectUrl.ToString());
        }

        /// <summary>
        /// Создает навигационную цепочку для указанного объекта недвижимости
        /// </summary>
        /// <param name="obj">Объект</param>
        private void PushObjectNavigationChain(EstateObject obj)
        {
            if (CurrentUser == null)
            {
                return;
            }

            // Добавляем во вью баг
            ViewBag.ObjectId = obj.Id;
            ViewBag.ClientId = obj.ClientId;
            
            // Корневой элемент
            PushNavigationItem("Личный кабинет", "Личный кабинет пользователя", "/account/");
            var latestUrl = "";
            // Элемент раздела личного кабинета
            if (obj.UserId == CurrentUser.Id)
            {
                latestUrl = "/account/objects/my";
                PushNavigationItem("Мои объекты", "Объекты, созданные мной", latestUrl);
            }
            else if (obj.User.Company != null && obj.User.Company.Id == CurrentUser.CompanyId)
            {
                latestUrl = "/account/objects/company";
                PushNavigationItem("Объекты компании", "Объекты, созданные сотрудниками моей компании", latestUrl);
            }
            else
            {
                latestUrl = "/account/objects/all";
                PushNavigationItem("Все объекты", "Все объекты системы", latestUrl);
            }
            // Элемент статуса в разделе
            string title = "", postfix = "";
            switch ((EstateStatuses)obj.Status)
            {
                case EstateStatuses.Draft:
                    title = "Черновики";
                    postfix = "drafts";
                    break;
                case EstateStatuses.Active:
                    title = "Активные";
                    postfix = "active";
                    break;
                case EstateStatuses.Advance:
                    title = "Внесен аванс";
                    postfix = "advance";
                    break;
                case EstateStatuses.TemporarilyWithdrawn:
                    title = "Временно снятые с продажи";
                    postfix = "temp-withdrawn";
                    break;
                case EstateStatuses.Withdrawn:
                    title = "Снятые с продажи";
                    postfix = "withdrawn";
                    break;
                case EstateStatuses.Deal:
                    title = "Сделка";
                    postfix = "deal";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            PushNavigationItem(title, "Статус объекта", string.Format("{0}#{1}", latestUrl, postfix));
            // Элемент самого объекта
            PushNavigationItem(string.Format("Объект №{0}", obj.Id), "Объект", string.Empty, false);

            String urlNewObjectPhoto = "/Content/images/mainpage/new-object-photo.png";
            var filesRep = Locator.GetService<IStoredFilesRepository>();
            if (obj.ObjectMainProperties.IsSetNumberAgency.HasValue && obj.ObjectMainProperties.IsSetNumberAgency.Value && obj.ObjectMainProperties.ContactPhone.HasValue)
            {
                if (obj.ObjectMainProperties.ContactPersonId.HasValue)
                {
                    var contactPerson = Locator.GetService<IUsersRepository>().Load(obj.ObjectMainProperties.ContactPersonId.Value);
                    ViewBag.PhotoUrl = !String.IsNullOrEmpty(contactPerson.PhotoUrl) ? filesRep.ResolveFileUrl(contactPerson.PhotoUrl) : urlNewObjectPhoto;
                    ViewBag.Name = contactPerson.ToString();

                    if (obj.ObjectMainProperties.ContactPhone.Value == (Int16)TypeContactPhone.AgentPhone1)
                    {
                        ViewBag.ContactPhone = contactPerson.Phone.FormatPhoneNumber();
                    }
                    else if (obj.ObjectMainProperties.ContactPhone.Value == (Int16)TypeContactPhone.AgentPhone2)
                    {
                        ViewBag.ContactPhone = contactPerson.Phone2.FormatPhoneNumber();
                    }
                }
                else if (obj.ObjectMainProperties.ContactCompanyId.HasValue)
                {
                    var contactCompany = Locator.GetService<ICompaniesRepository>().Load(obj.ObjectMainProperties.ContactCompanyId.Value);
                    ViewBag.PhotoUrl = !String.IsNullOrEmpty(obj.User.PhotoUrl) ? filesRep.ResolveFileUrl(obj.User.PhotoUrl) : urlNewObjectPhoto;
                    ViewBag.Name = obj.User.ToString();

                    if (obj.ObjectMainProperties.ContactPhone.Value == (Int16)TypeContactPhone.CompanyPhone1)
                    {
                        ViewBag.ContactPhone = contactCompany.Phone1.FormatPhoneNumber();
                    }
                    else if (obj.ObjectMainProperties.ContactPhone.Value == (Int16)TypeContactPhone.CompanyPhone2)
                    {
                        ViewBag.ContactPhone = contactCompany.Phone2.FormatPhoneNumber();
                    }
                    else if (obj.ObjectMainProperties.ContactPhone.Value == (Int16)TypeContactPhone.CompanyPhone3)
                    {
                        ViewBag.ContactPhone = contactCompany.Phone3.FormatPhoneNumber();
                    }
                }
            }
            else
            {
                ViewBag.ContactPhone = obj.User.Phone;
                ViewBag.PhotoUrl = !String.IsNullOrEmpty(obj.User.PhotoUrl) ? filesRep.ResolveFileUrl(obj.User.PhotoUrl) : urlNewObjectPhoto;
                ViewBag.Name = obj.User.ToString();
            }
        }

        /// <summary>
        /// Возвращает автокомплит со списком всех улиц либо улиц в указанном районе
        /// </summary>
        /// <param name="cityId">Идентификатор города для выборки улиц</param>
        /// <param name="districtId">Идентификатор района, если есть</param>
        /// <param name="term">Строка для поиска по улицам</param>
        /// <returns></returns>
        [Route("objects/streets-autocomplete")]
        [AuthorizationCheck(Permission.ViewObjects)]
        public ActionResult StreetsAutocomplete(long cityId, long? districtId,string term)
        {
            try
            {
                // Репозиторий улиц
                var manager = Locator.GetService<IGeoManager>();

                // Выбираем все улицы в указанном городе
                var streets =
                    manager.StreetsRepository.Search(s => s.GeoResidentialArea.GeoDistrict.GeoCity.Id == cityId);

                // Дополнительно фильтруем по районам если прислано
                if (districtId.HasValue)
                {
                    streets = streets.Where(s => s.GeoResidentialArea.GeoDistrict.Id == districtId.Value);
                }

                // Фильтруем по названию
                streets = streets.Where(s => s.Name.ToLower().Contains(term.ToLower()));

                // Отдаем результат
                return Json(streets.Select(s => new
                    {
                        id = s.Id,
                        name = s.Name,
                        displayName = String.Format("{0} ({1})", s.Name, s.GeoResidentialArea.Name),
                        areaId = s.AreaId,
                        districtId = s.GeoResidentialArea.DistrictId
                    }),JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                return Json(new { success = false, msg = e.Message });
            }
        }

        /// <summary>
        /// Возвращает частичный вид, содерджащий информацию о последних изменениях в статусе объекта
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns>Частичный вид</returns>
        [HttpPost][Route("objects/get-status-info")]
        public ActionResult GetObjectStatusInfo(long id)
        {
            var obj = ObjectsRepository.Load(id);
            if (obj == null)
            {
                return Content("Объект не найден");
            }
            return PartialView(obj);
        }

		/// <summary>
		/// Раннер объектов, выполняющих проверку
		/// </summary>
		/// <returns></returns>
	    public ActionResult CheckObjects()
	    {
		    // Получаем список объектов, который требуется обработать
            var objectsRep = Locator.GetService<IEstateObjectsRepository>();
            var notificationsRep = Locator.GetService<IObjectNotificationsRepository>();
            var mailNotificationManager = Locator.GetService<IMailNotificationManager>();
            var smsNotificationManager = Locator.GetService<ISMSNotificationManager>();
            var objects =
                objectsRep.Search(
                    o =>
                    o.Status == (short) EstateStatuses.Active ||
                    o.Status == (short) EstateStatuses.TemporarilyWithdrawn);

            var baseDays = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ForgotPeriod"]);

            // Обрабатываем
		    foreach (var estateObject in objects)
		    {
			    // Выполняем обработку в зависимости от статуса
			    switch ((EstateStatuses) estateObject.Status)
			    {
				    case EstateStatuses.Active: // Проверка забытых объектов
					    var daysUnedited =
						    (DateTimeZone.Now -
						     (estateObject.DateModified.HasValue
							     ? estateObject.DateModified.Value
							     : estateObject.DateCreated.Value)).TotalDays;

					    if (daysUnedited > baseDays && daysUnedited < baseDays + 2)
					    {
						    var message1 =
							    String.Format(
                                    "По объекту Вашей компании <a href=\"http://nprdv.ru/objects/{0}/card\">{1}</a> ({2}) не было изменений в течении 2-х месяцев. <a href=\"http://nprdv.ru/account/objects?objIdRefreshFromEmail={0}\">Обновите</a> информацию об объекте или через 2 дня он будет переведен в статус \"Черновик\"",
								    estateObject.Id, estateObject.Address.ToShortAddressString(),
								    ((EstateTypes) estateObject.ObjectType).GetEnumMemberName(), baseDays);
						    var message2 =
							    String.Format(
                                    "По вашему объекту <a href=\"http://www.nprdv.ru/objects/{4}/card\">{0},{1} {2}</a> не было изменений в течении 2-х месяцев. <a href=\"http://nprdv.ru/account/objects?objIdRefreshFromEmail={4}\">Обновите</a> информацию об объекте или через 2 дня он будет перемещен в статус \"Черновик\"",
								    ((EstateTypes) estateObject.ObjectType).GetEnumMemberName(),
								    estateObject.Address.GeoStreet != null ? estateObject.Address.GeoStreet.Name : "",
								    estateObject.Address.House, baseDays,estateObject.Id);

						    // Ищем директора и офис менеджкера
						    var director = estateObject.User.Company.Director;
						    var managers = estateObject.User.Company.Users.Where(u => u.RoleId == 7).ToList();
						    if (director != null)
						    {
							    mailNotificationManager.Notify(director, "Забытый объект", message1);
						    }
						    if (managers.Count > 0)
						    {
							    managers.ForEach(manager => mailNotificationManager.Notify(manager, "Забытый объект", message1));
						    }
						    mailNotificationManager.Notify(estateObject.User, "Забытый объект", message2);
					    }
					    else if (daysUnedited > baseDays + 2)
					    {
						    var message3 =
							    String.Format(
								    "Объект Вашей компании <a href=\"http://nprdv.ru/objects/{0}/card\">{1}</a> ({2}) перенесен в статус \"Черновик\", потому что информация о нем не обновлялась в течении {3} дней",
								    estateObject.Id, estateObject.Address.ToShortAddressString(),
								    ((EstateTypes) estateObject.ObjectType).GetEnumMemberName(), baseDays);
						    var message4 =
							    String.Format(
									"Ваш объект <a href=\"http://www.nprdv.ru/objects/{4}/card\">{0},{1} {2}</a> перенесен в статус \"Черновик\", потому что информация о нем не обновлялась в течении {3} дней",
								    ((EstateOperations) estateObject.Operation).GetEnumMemberName(),
								    estateObject.Address.GeoStreet != null ? estateObject.Address.GeoStreet.Name : "",
								    estateObject.Address.House, baseDays,estateObject.Id);

						    // Изменяем статус объекта
						    estateObject.Status = (short) EstateStatuses.Draft;
						    estateObject.ObjectChangementProperties.DateMoved = DateTimeZone.Now;

						    // Создаем элемент истории
						    var objectHistoryItem = new ObjectHistoryItem()
						    {
							    ClientId = -1,
							    CompanyId = -1,
							    DateCreated = DateTimeZone.Now,
							    CreatedBy = -1,
							    HistoryStatus = (short) EstateStatuses.Draft,
							    EstateObject = estateObject
						    };
						    estateObject.ObjectHistoryItems.Add(objectHistoryItem);

						    // Ищем директора и офис менеджкера
						    var director = estateObject.User.Company.Director;
						    var managers = estateObject.User.Company.Users.Where(u => u.RoleId == 7).ToList();
						    if (director != null)
						    {
							    mailNotificationManager.Notify(director, "Забытый объект", message3);
						    }
						    if (managers.Count > 0)
						    {
							    managers.ForEach(manager => mailNotificationManager.Notify(manager, "Забытый объект", message3));
						    }
						    mailNotificationManager.Notify(estateObject.User, "Забытый объект", message4);
					    }
					    break;
				    case EstateStatuses.TemporarilyWithdrawn: // Проверка временно снятых объектов
					    if (estateObject.ObjectChangementProperties.DelayToDate.HasValue)
					    {
						    // Узнаем количество просроченных дней
						    var daysDelayed =
							    (DateTimeZone.Now - (estateObject.ObjectChangementProperties.DelayToDate.Value)).TotalDays;


						    if (DateTimeZone.Now.Date == estateObject.ObjectChangementProperties.DelayToDate.Value.Date)
						    {
							    var message1 =
								    String.Format(
									    "Объект вашей компании <a href=\"http://nprdv.ru/objects/{0}/card\">{1}</a> измените статус объекта или через 2 дня он будет переведен в статус \"Черновик\"",
									    estateObject.Id, estateObject.Address.ToShortAddressString());
							    var message2 =
								    String.Format(
										"Истек срок статус \"Временно снято с продажи\" <a href=\"http://www.nprdv.ru/objects/{3}/card\">{0},{1} {2}</a>. Измените статус объекта или через 2 дня он будет перемещен в статус \"Черновик\"",
									    ((EstateTypes) estateObject.ObjectType).GetEnumMemberName(),
									    estateObject.Address.GeoStreet != null ? estateObject.Address.GeoStreet.Name : "",
									    estateObject.Address.House, estateObject.Id);

							    // Ищем директора и офис менеджкера
							    var director = estateObject.User.Company.Director;
							    var managers = estateObject.User.Company.Users.Where(u => u.RoleId == 7).ToList();
							    if (director != null)
							    {
								    mailNotificationManager.Notify(director, "Временно снятый объект", message1);
							    }
							    if (managers.Count > 0)
							    {
								    managers.ForEach(manager => mailNotificationManager.Notify(manager, "Временно снятый объект", message1));
							    }
							    mailNotificationManager.Notify(estateObject.User,"Истек статус \"Временно Снято с продажи\"", message2);
						    }
						    else if (daysDelayed > 2)
						    {
							    var message3 =
								    String.Format(
									    "Объект Вашей компании <a href=\"http://nprdv.ru/objects/{0}/card\">{1}</a> перенесен в статус \"Черновик\"",
									    estateObject.Id, estateObject.Address.ToShortAddressString());
							    var message4 =
								    String.Format(
									    "Ваш объект <a href=\"http://www.nprdv.ru/objects/{3}/card\">{0},{1} {2}</a> перенесен в статус \"Черновик\"",
									    ((EstateTypes) estateObject.ObjectType).GetEnumMemberName(),
									    estateObject.Address.GeoStreet != null ? estateObject.Address.GeoStreet.Name : "",
									    estateObject.Address.House,estateObject.Id);

							    // Изменяем статус объекта
							    estateObject.Status = (short) EstateStatuses.Draft;

							    // Создаем элемент истории
							    var objectHistoryItem = new ObjectHistoryItem()
							    {
								    ClientId = -1,
								    CompanyId = -1,
								    DateCreated = DateTimeZone.Now,
								    CreatedBy = -1,
								    HistoryStatus = (short) EstateStatuses.Draft,
								    EstateObject = estateObject
							    };
							    estateObject.ObjectHistoryItems.Add(objectHistoryItem);

							    // Ищем директора и офис менеджкера
							    var director = estateObject.User.Company.Director;
							    var managers = estateObject.User.Company.Users.Where(u => u.RoleId == 7).ToList();
							    if (director != null)
							    {
								    mailNotificationManager.Notify(director, "Временно снятый объект", message3);
							    }
							    if (managers.Count > 0)
							    {
								    managers.ForEach(manager => mailNotificationManager.Notify(manager, "Временно снятый объект", message3));
							    }
								mailNotificationManager.Notify(estateObject.User,"Истек статус \"Временно Снято с продажи\"", message4);

							    estateObject.ObjectManagerNotifications.Clear();
						    }
					    }
					    break;
			    }
			    objectsRep.SubmitChanges();
		    }

		    return Content("OK");
	    }
    }
}
