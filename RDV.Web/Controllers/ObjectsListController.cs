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
using RDV.Web.Classes.Enums;
using RDV.Web.Classes.Security;
using RDV.Web.Models.Objects;

namespace RDV.Web.Controllers
{
    /// <summary>
    /// Контроллер управления списком объектов
    /// </summary>
    public class ObjectsListController : BaseController
    {
        /// <summary>
        /// Отображает список объектов
        /// </summary>
        /// <returns></returns>
        [Route("account/objects")][AuthorizationCheck()]
        public ActionResult Index(EstateTypes estateType = EstateTypes.Flat, ObjectsListSection section = ObjectsListSection.MyObjects, Int32 objIdRefreshFromEmail = 0, Boolean isActive = false)
        {
            PushNavigationItem("Личный кабинет", "Корневая страница личного кабинета", "/account/");
            PushNavigationItem("Объекты","Объекты","/account/objects/",true);

            if (Session["estateType"] != null)
            {
                estateType = (EstateTypes) Session["estateType"];
                Session["estateType"] = null;
            }
            ViewBag.estateType = estateType;
            ViewBag.section = section;
            ViewBag.ObjIdRefreshFromEmail = objIdRefreshFromEmail;
            ViewBag.IsActive = isActive;

            if (objIdRefreshFromEmail != 0)
            {
                UINotificationManager.Success("Идет обновление объекта. Пожалуйста подождите...");
            }

            var rep = Locator.GetService<IEstateObjectsRepository>();
            IEnumerable<EstateObject> objects;

            // Проверяем безопасность
            switch (section)
            {
                case ObjectsListSection.MyObjects:
                    if (!CurrentUser.HasPermission(Permission.EditOwnObjects))
                    {
                        Locator.GetService<IUINotificationManager>().Error("Доступ к этому разделу запрещен");
                        return RedirectToAction("Index", "Main");
                    }
                    break;
                case ObjectsListSection.CompanyObjects:
                    if (!CurrentUser.HasPermission(Permission.EditCompanyObjects))
                    {
                        Locator.GetService<IUINotificationManager>().Error("Доступ к этому разделу запрещен");
                        return RedirectToAction("Index", "Main");
                    }
                    break;
                case ObjectsListSection.AllObjects:
                    if (!CurrentUser.HasPermission(Permission.EditAllObjects))
                    {
                        Locator.GetService<IUINotificationManager>().Error("Доступ к этому разделу запрещен");
                        return RedirectToAction("Index", "Main");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("section");
            }

            switch (section)
            {
                case ObjectsListSection.MyObjects:
                    objects = rep.Search(o => o.UserId == CurrentUser.Id && o.ObjectType == (short) estateType);
                    break;
                case ObjectsListSection.CompanyObjects:
                    objects = rep.Search(o => o.User.CompanyId == CurrentUser.CompanyId && o.ObjectType == (short)estateType);
                    break;
                case ObjectsListSection.AllObjects:
                    objects = rep.Search(o => o.ObjectType == (short) estateType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("section");
            }

            IEnumerable list = objects.Select(o => new ObjectItemModel(o));

            return View(list);
        }

        /// <summary>
        /// Callback обработчик для гридов
        /// </summary>
        /// <param name="EstateType"></param>
        /// <param name="Section"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        [AuthorizationCheck]
        public ActionResult ObjectsListPartial(EstateTypes EstateType, ObjectsListSection Section, EstateStatuses Status)
        {
            var rep = Locator.GetService<IEstateObjectsRepository>();
            IEnumerable<EstateObject> objects;

            // Проверяем безопасность
            switch (Section)
            {
                case ObjectsListSection.MyObjects:
                    if (!CurrentUser.HasPermission(Permission.EditOwnObjects))
                    {
                        Locator.GetService<IUINotificationManager>().Error("Доступ к этому разделу запрещен");
                        return Content("fuck you");
                    }
                    break;
                case ObjectsListSection.CompanyObjects:
                    if (!CurrentUser.HasPermission(Permission.EditCompanyObjects))
                    {
                        Locator.GetService<IUINotificationManager>().Error("Доступ к этому разделу запрещен");
                        return Content("fuck you");
                    }
                    break;
                case ObjectsListSection.AllObjects:
                    if (!CurrentUser.HasPermission(Permission.EditAllObjects))
                    {
                        Locator.GetService<IUINotificationManager>().Error("Доступ к этому разделу запрещен");
                        return Content("fuck you");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("section");
            }

            switch (Section)
            {
                case ObjectsListSection.MyObjects:
                    objects = rep.Search(o => o.UserId == CurrentUser.Id && o.ObjectType == (short)EstateType);
                    break;
                case ObjectsListSection.CompanyObjects:
                    objects = rep.Search(o => o.User.CompanyId == CurrentUser.CompanyId && o.ObjectType == (short)EstateType);
                    break;
                case ObjectsListSection.AllObjects:
                    objects = rep.Search(o => o.ObjectType == (short)EstateType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("section");
            }

            IEnumerable list = objects.Where(e => e.Status == (short) Status).Select(o => new ObjectItemModel(o)).AsEnumerable();

            // Отдаем данные
            return PartialView(new ObjectListModel()
            {
                Data = list,
                EstateType = EstateType,
                Section = Section,
                Status = Status
            });
        }

    }
}
