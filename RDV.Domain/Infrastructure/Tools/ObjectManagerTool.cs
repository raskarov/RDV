using System;
using System.IO;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Mailing.Templates;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Domain.Core;

namespace RDV.Domain.Infrastructure.Tools
{
    /// <summary>
    /// Инструмент, управляющий состоянием объектов
    /// </summary>
    public class ObjectManagerTool:BaseTool
    {
        /// <summary>
        /// Стандантрый конструктор
        /// </summary>
        public ObjectManagerTool():base()
        {
            Id = "object-manager";
            Title = "Менеджер автостатусов объектов";
            Interval = ToolLaunchInterval.EveryDay;
        }

        /// <summary>
        /// Действие, выполняемое непосредственно в задаче
        /// </summary>
        public override void Execute()
        {
	        return;
            // Создаем скоуп
            using (var httpscope = Locator.BeginNestedHttpRequestScope())
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
                    switch ((EstateStatuses)estateObject.Status)
                    {
                        case EstateStatuses.Active: // Проверка забытых объектов
                            var daysUnedited =
                                (DateTimeZone.Now -
                                 (estateObject.DateModified.HasValue
                                      ? estateObject.DateModified.Value
                                      : estateObject.DateCreated.Value)).TotalDays;
                            
                            if (daysUnedited > baseDays && daysUnedited <= baseDays+2)
                            {
                                var message1 =
                                    String.Format(
                                        "По объекту Вашей компании <a href=\"http://nprdv.ru/objects/{0}/card\">{1}</a> ({2}) не было изменений {3} дней. Обновите информацию по объекту или через 2 дня он будет переведен в статус \"Черновик\"", estateObject.Id,estateObject.Address.ToShortAddressString(),((EstateTypes)estateObject.ObjectType).GetEnumMemberName(),baseDays);
                                var message2 =
                                    String.Format(
                                        "По вашему объекту {0},{1} {2} не было изменений в течении {3} дней. Обновите информацию об объекте или через 2 дня он будет перемещен в статус \"Черновик\"",
                                        ((EstateOperations) estateObject.Operation).GetEnumMemberName(),
                                        estateObject.Address.GeoStreet != null ? estateObject.Address.GeoStreet.Name : "",
                                        estateObject.Address.House,baseDays);

                                // Ищем директора и офис менеджкера
                                var director = estateObject.User.Company.Director;
                                var managers = estateObject.User.Company.Users.Where(u => u.RoleId == 7).ToList();
                                if (director != null)
                                {
                                    mailNotificationManager.Notify(director,"Забытый объект",message1);
                                }
                                if (managers.Count > 0)
                                {
                                    managers.ForEach(manager => mailNotificationManager.Notify(manager,"Забытый объект",message1));
                                }
                                mailNotificationManager.Notify(estateObject.User,"Забытый объект",message2);
                            }
                            else if (daysUnedited > baseDays + 2)
                            {
                                var message3 =
                                    String.Format(
                                        "Объект Вашей компании <a href=\"http://nprdv.ru/objects/{0}/card\">{1}</a> ({2}) перенесен в статус \"Черновик\", потому что информация о нем не обновлялась в течении {3} дней", estateObject.Id,estateObject.Address.ToShortAddressString(),((EstateTypes)estateObject.ObjectType).GetEnumMemberName(),baseDays);
                                var message4 =
                                    String.Format(
                                        "Ваш объект {0},{1} {2} перенесен в статус \"Черновик\", потому что информация о нем не обновлялась в течении {3} дней",
                                        ((EstateOperations) estateObject.Operation).GetEnumMemberName(),
                                        estateObject.Address.GeoStreet != null ? estateObject.Address.GeoStreet.Name : "",
                                        estateObject.Address.House,baseDays);

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
                                    mailNotificationManager.Notify(director,"Забытый объект",message3);
                                }
                                if (managers.Count > 0)
                                {
                                    managers.ForEach(manager => mailNotificationManager.Notify(manager,"Забытый объект",message3));
                                }
                                mailNotificationManager.Notify(estateObject.User,"Забытый объект",message4);
                            }
                            break;
                        case EstateStatuses.TemporarilyWithdrawn: // Проверка временно снятых объектов
                            if (estateObject.ObjectChangementProperties.DelayToDate.HasValue)
                            {
                                // Узнаем количество просроченных дней
                                var daysDelayed =
                                    (DateTimeZone.Now - (estateObject.ObjectChangementProperties.DelayToDate.Value)).TotalDays;
                                

                                if (DateTimeZone.Now == estateObject.ObjectChangementProperties.DelayToDate && !notificationsRep.HasObjectNotification(estateObject,ObjectNotificationTypes.TemporalyWithdraw1))
                                {
                                    var message1 =
                                    String.Format(
                                        "Объект вашей компании <a href=\"http://nprdv.ru/objects/{0}/card\">{1}</a> измените статус объекта или через 2 дня он будет переведен в статус \"Черновик\"", estateObject.Id, estateObject.Address.ToShortAddressString());
                                    var message2 =
                                        String.Format(
                                            "Истек срок статус \"Временно снято с продажи\" {0},{1} {2}. Измените статус объекта или через 2 дня он будет перемещен в статус \"Черновик\"",
                                            ((EstateOperations)estateObject.ObjectType).GetEnumMemberName(),
                                            estateObject.Address.GeoStreet != null ? estateObject.Address.GeoStreet.Name : "",
                                            estateObject.Address.House);

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
                                    smsNotificationManager.Notify(estateObject.User, message2);

                                    notificationsRep.AddObjectNotification(estateObject, ObjectNotificationTypes.TemporalyWithdraw1);
                                }
                                else if (daysDelayed > 2)
                                {
                                    var message3 =
                                    String.Format(
                                        "Объект Вашей компании <a href=\"http://nprdv.ru/objects/{0}/card\">{1}</a> перенесен в статус \"Черновик\"", estateObject.Id, estateObject.Address.ToShortAddressString());
                                    var message4 =
                                        String.Format(
                                            "Ваш объект {0},{1} {2} перенесен в статус \"Черновик\"",
                                            ((EstateOperations)estateObject.Operation).GetEnumMemberName(),
                                            estateObject.Address.GeoStreet != null ? estateObject.Address.GeoStreet.Name : "",
                                            estateObject.Address.House);

                                    // Изменяем статус объекта
                                    estateObject.Status = (short)EstateStatuses.Draft;

                                    // Создаем элемент истории
                                    var objectHistoryItem = new ObjectHistoryItem()
                                    {
                                        ClientId = -1,
                                        CompanyId = -1,
                                        DateCreated = DateTimeZone.Now,
                                        CreatedBy = -1,
                                        HistoryStatus = (short)EstateStatuses.Draft,
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
                                    smsNotificationManager.Notify(estateObject.User, message4);

                                    estateObject.ObjectManagerNotifications.Clear();
                                }
                            }
                            break;
                    }
                    objectsRep.SubmitChanges();
                }
            }
        }
    }
}