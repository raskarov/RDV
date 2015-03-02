// ============================================================
// 
// 	RDV
// 	RDV.Domain 
// 	SystemStatsTool.cs
// 
// 	Created by: ykorshev 
// 	 at 25.10.2013 10:53
// 
// ============================================================

using System;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Domain.Core;

namespace RDV.Domain.Infrastructure.Tools
{
    public class SystemStatsTool: BaseTool
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SystemStatsTool()
        {
            Id = "system-stats";
            Title = "Утилита записи системных показателей";
            Interval = ToolLaunchInterval.EveryDay;
        }

        /// <summary>
        /// Действие, выполняемое непосредственно в задаче
        /// </summary>
        public override void Execute()
        {
            using (var scope = Locator.BeginNestedHttpRequestScope())
            {
                var objectsRep = Locator.GetService<IEstateObjectsRepository>();
                var usersRep = Locator.GetService<IUsersRepository>();
                var searchRequestsRep = Locator.GetService<ISearchRequestsRepository>();
                var auditEventsRep = Locator.GetService<IAuditEventsRepository>();
                var statsRep = Locator.GetService<ISystemStatsRepository>();
                var servicesRep = Locator.GetService<IServiceTypesRepository>();

                // Начинаем выборку
                // Количество запросов в системе
                statsRep.Add(new SystemStat()
                {
                    StatDateTime = DateTimeZone.Now,
                    StatType = (short)SystemStatsType.RequestsCount,
                    Value = searchRequestsRep.FindAll().Count()
                });
                // Количество объектов мультилистинга в системе
                statsRep.Add(new SystemStat()
                {
                    StatDateTime = DateTimeZone.Now,
                    StatType = (short)SystemStatsType.MultilistingsObjectCount,
                    Value = objectsRep.Search(
                    o =>
                    o.Status == (short)EstateStatuses.Active && o.ObjectAdditionalProperties.AgreementType.HasValue && o.ObjectAdditionalProperties.AgreementType == 354).Count()
                });
                // Количество совместных сделок
                statsRep.Add(new SystemStat()
                {
                    StatDateTime = DateTimeZone.Now,
                    StatType = (short)SystemStatsType.IntersystemDealsCount,
                    Value = objectsRep.Search(
                        o =>
                        o.ObjectHistoryItems.Any(
                            h =>
                            h.HistoryStatus == (short)EstateStatuses.Deal && h.CompanyId != -1)).Count()
                });
                // Количество авторизовавшихся пользователей
                statsRep.Add(new SystemStat()
                {
                    StatDateTime = DateTimeZone.Now,
                    StatType = (short)SystemStatsType.AuthorizedUsersCount,
                    Value = auditEventsRep.Search(
                        a => a.EventDate.Date == DateTimeZone.Now.Date && a.Message.Contains("Начало сессии")).GroupBy(g => g.UserId)
                        .Count()
                });
                // Прирост зарегистрированных пользователей
                statsRep.Add(new SystemStat()
                {
                    StatDateTime = DateTimeZone.Now,
                    StatType = (short)SystemStatsType.RegistredUsersCount,
                    Value = usersRep.FindAll().Count(u => u.DateCreated.Value.Date == DateTimeZone.Now.Date)
                });
                // Среднее время продажи объекта
                statsRep.Add(new SystemStat()
                {
                    StatDateTime = DateTimeZone.Now,
                    StatType = (short)SystemStatsType.ObjectSellDaysCount,
                    Value = Convert.ToDecimal(objectsRep.Search(o => o.ObjectChangementProperties.DealDate.HasValue).Average(eo => (eo.ObjectChangementProperties.DealDate.Value - (eo.ObjectChangementProperties.DateRegisted ?? eo.DateCreated).Value).Days))
                });
                // Количество платных услух
                statsRep.Add(new SystemStat()
                {
                    StatDateTime = DateTimeZone.Now,
                    StatType = (short)SystemStatsType.ServicesCount,
                    Value = servicesRep.FindAll().Count()
                });   
            }
        }
    }
}