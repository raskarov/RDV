using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.DataVisualization.Charting;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Misc;
using RDV.Domain.Infrastructure.Routing;
using RDV.Domain.Interfaces.ImportExport;
using RDV.Domain.Interfaces.ImportExport.Geo;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.Interfaces.Repositories.Content;
using RDV.Domain.IoC;
using RDV.Web.Classes.ElFinderConnector.Facade;
using RDV.Web.Classes.Extensions;
using RDV.Web.Classes.Notification.Mail;
using RDV.Web.Classes.Security;
using RDV.Web.Models.Account.Profile;
using RDV.Web.Models.Administration.Audit;
using RDV.Web.Models.Administration.Companies;
using RDV.Web.Models.Administration.Content;
using RDV.Web.Models.Administration.Dictionaries;
using RDV.Web.Models.Administration.Payments;
using RDV.Web.Models.Administration.Roles;
using RDV.Web.Models.Administration.Users;
using RDV.Domain.Core;

namespace RDV.Web.Controllers
{
    /// <summary>
    /// Контроллер панели администрирования
    /// </summary>
    public class AdministrationController : BaseController
    {
        /// <summary>
        /// Текущий логгер
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Common

        /// <summary>
        /// Добавляет в стек навигацонной цепочки навигационный элемент корневой ссылки на раздел администрирования
        /// </summary>
        private void PushAdministrationNavigationItem()
        {
            PushNavigationItem("Личный кабинет", "Личный кабинет пользователя", "/account");
            PushNavigationItem("Администрирование", "Раздел администрирования системы", "/administration");
        }

        #endregion

        #region Дашбоард

        /// <summary>
        /// Отображает дашбоард панели администрирования
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ViewDashboard)]
        [Route("administration/dashboard")]
        public ActionResult Index()
        {
            PushAdministrationNavigationItem();
            PushNavigationItem("Дашбоард", "Сводная информация для администраторов системы", "administration/dashboard", false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр дашбоарда");

            return View();
        }

        /// <summary>
        /// Отображает график дашбоарда с указанными параметрами
        /// </summary>
        /// <returns>Изображение графика</returns>
        [AuthorizationCheck(Permission.ViewDashboard)]
        [Route("administration/dashboard/image")]
        public ActionResult DashboardImage(FormCollection collection)
        {
            DateTime startDate, endDate;
            if (!DateTime.TryParseExact(Request["startDate"], "dd.MM.yyyy", null, DateTimeStyles.AllowTrailingWhite, out startDate))
            {
                startDate = DateTimeZone.Now.AddMonths(-6);
            }
            if (!DateTime.TryParseExact(Request["endDate"], "dd.MM.yyyy", null, DateTimeStyles.AllowTrailingWhite, out endDate))
            {
                endDate = DateTimeZone.Now;
            }

            // Формируем набор данных
            var objectsRepository = Locator.GetService<IEstateObjectsRepository>();
            var usersRepository = Locator.GetService<IUsersRepository>();
            var companiesRepository = Locator.GetService<ICompaniesRepository>();
            var requestsRep = Locator.GetService<ISearchRequestsRepository>();
            var auditEventsRepository = Locator.GetService<IAuditEventsRepository>();
            Dictionary<DateTime, int> objectsData = new Dictionary<DateTime, int>();
            Dictionary<DateTime, int> usersData = new Dictionary<DateTime, int>();
            Dictionary<DateTime, int> companiesData = new Dictionary<DateTime, int>();
            Dictionary<DateTime, int> requestsData = new Dictionary<DateTime, int>();
            Dictionary<DateTime, int> multilistingObjectsData = new Dictionary<DateTime, int>();
            Dictionary<DateTime, int> interdealObjectsData = new Dictionary<DateTime, int>();
            Dictionary<DateTime, int> intensityUsersData = new Dictionary<DateTime, int>();
            for (var counter = startDate; counter <= endDate.AddMonths(0); counter = counter.AddMonths(1))
            {
                var monthStart = new DateTime(counter.Year, counter.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var objectThisMonth = objectsRepository.Search(
                    o => o.Status == (short)EstateStatuses.Active && o.DateCreated.HasValue && o.DateCreated.Value >= monthStart && o.DateCreated.Value <= monthEnd)
                    .Count();
                var usersThisMonth = usersRepository.Search(
                    o => o.DateCreated.HasValue && o.DateCreated.Value >= monthStart && o.DateCreated.Value <= monthEnd && o.Activated)
                    .Count();
                var companiesThisMonth = companiesRepository.Search(
                    o => o.DateCreated.HasValue && o.DateCreated.Value >= monthStart && o.DateCreated.Value <= monthEnd)
                    .Count();
                var requestsThisMonth = requestsRep.Search(
                    o => o.DateCreated.HasValue && o.DateCreated.Value >= monthStart && o.DateCreated.Value <= monthEnd)
                    .Count();
                objectsData[monthStart] = objectThisMonth;
                usersData[monthStart] = usersThisMonth;
                companiesData[monthStart] = companiesThisMonth;
                requestsData[monthStart] = requestsThisMonth;
                multilistingObjectsData[monthStart] = objectsRepository.Search(
                    o =>
                    o.Status == (short)EstateStatuses.Active && o.DateCreated.HasValue &&
                    o.DateCreated.Value >= monthStart && o.DateCreated.Value <= monthEnd && o.ObjectAdditionalProperties.AgreementType.HasValue && o.ObjectAdditionalProperties.AgreementType == 354).Count();
                interdealObjectsData[monthStart] =
                    objectsRepository.Search(
                        o =>
                        o.ObjectHistoryItems.Any(
                            h =>
                            h.HistoryStatus == (short)EstateStatuses.Deal && h.CompanyId != -1 &&
                            h.DateCreated.Value >= monthStart && h.DateCreated.Value <= monthEnd)).Count();
                intensityUsersData[monthStart] =
                    auditEventsRepository.Search(
                        a => a.EventDate >= monthStart && a.EventDate <= monthEnd && a.Message.Contains("Начало сессии")).GroupBy(g => g.UserId)
                        .Count();
            }

            // Конструируем диаграмму
            var chart = new Chart()
            {
                Height = 500,
                Width = 900,
                ImageType = ChartImageType.Png
            };
            chart.Titles.Add("Динамика развития системы");
            var mainArea = chart.ChartAreas.Add("MainArea");
            mainArea.AxisX.IsLabelAutoFit = true;
            mainArea.AxisY.IsLabelAutoFit = true;
            mainArea.AxisY.LineColor = Color.LightGray;
            mainArea.AxisX.LineColor = Color.LightGray;
            mainArea.AxisX.Title = "Месяцы";
            mainArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            mainArea.AxisY.MajorGrid.LineColor = Color.LightGray;

            var mainLegend = chart.Legends.Add("MainLegend");
            mainLegend.Docking = Docking.Bottom;
            mainLegend.TableStyle = LegendTableStyle.Wide;

            var objectsSeries = chart.Series.Add("Количество зарегистрированных объектов");
            objectsSeries.ChartArea = "MainArea";
            objectsSeries.ChartType = SeriesChartType.Column;
            objectsSeries.Legend = "MainLegend";
            objectsSeries.IsValueShownAsLabel = true;

            var usersSeries = chart.Series.Add("Количество активированных пользователей");
            usersSeries.ChartArea = "MainArea";
            usersSeries.ChartType = SeriesChartType.Column;
            usersSeries.Legend = "MainLegend";
            usersSeries.IsValueShownAsLabel = true;

            var activeUsersSeries = chart.Series.Add("Количество активных пользователей");
            activeUsersSeries.ChartArea = "MainArea";
            activeUsersSeries.ChartType = SeriesChartType.Column;
            activeUsersSeries.Legend = "MainLegend";
            activeUsersSeries.IsValueShownAsLabel = true;

            var companiesSeries = chart.Series.Add("Количество зарегистрированных компаний");
            companiesSeries.ChartArea = "MainArea";
            companiesSeries.ChartType = SeriesChartType.Column;
            companiesSeries.Legend = "MainLegend";
            companiesSeries.IsValueShownAsLabel = true;

            var requestsSeries = chart.Series.Add("Количество запросов");
            requestsSeries.ChartArea = "MainArea";
            requestsSeries.ChartType = SeriesChartType.Column;
            requestsSeries.Legend = "MainLegend";
            requestsSeries.IsValueShownAsLabel = true;

            var multilistingSeries = chart.Series.Add("Количество объектов Мультилистинга");
            multilistingSeries.ChartArea = "MainArea";
            multilistingSeries.ChartType = SeriesChartType.Column;
            multilistingSeries.Legend = "MainLegend";
            multilistingSeries.IsValueShownAsLabel = true;

            var interdealSeries = chart.Series.Add("Количество совместных сделок");
            interdealSeries.ChartArea = "MainArea";
            interdealSeries.ChartType = SeriesChartType.Column;
            interdealSeries.Legend = "MainLegend";
            interdealSeries.IsValueShownAsLabel = true;

            // Рендерим данные на диаграмму
            var position = 0;
            foreach (var dataPair in objectsData)
            {
                position++;
                objectsSeries.Points.Add(new DataPoint()
                {
                    XValue = position,
                    YValues = new double[] { dataPair.Value },
                    AxisLabel = dataPair.Key.ToString("MMM yyyy"),
                    ToolTip = dataPair.Value.ToString("0")
                });
                usersSeries.Points.Add(new DataPoint()
                {
                    XValue = position,
                    YValues = new double[] { usersData[dataPair.Key] },
                    AxisLabel = dataPair.Key.ToString("MMM yyyy"),
                    ToolTip = usersData[dataPair.Key].ToString("0")
                });
                activeUsersSeries.Points.Add(new DataPoint()
                {
                    XValue = position,
                    YValues = new double[] { intensityUsersData[dataPair.Key] },
                    AxisLabel = dataPair.Key.ToString("MMM yyyy"),
                    ToolTip = intensityUsersData[dataPair.Key].ToString("0")
                });
                companiesSeries.Points.Add(new DataPoint()
                {
                    XValue = position,
                    YValues = new double[] { companiesData[dataPair.Key] },
                    AxisLabel = dataPair.Key.ToString("MMM yyyy"),
                    ToolTip = companiesData[dataPair.Key].ToString("0")
                });
                requestsSeries.Points.Add(new DataPoint()
                {
                    XValue = position,
                    YValues = new double[] { requestsData[dataPair.Key] },
                    AxisLabel = dataPair.Key.ToString("MMM yyyy"),
                    ToolTip = requestsData[dataPair.Key].ToString("0")
                });
                multilistingSeries.Points.Add(new DataPoint()
                {
                    XValue = position,
                    YValues = new double[] { multilistingObjectsData[dataPair.Key] },
                    AxisLabel = dataPair.Key.ToString("MMM yyyy"),
                    ToolTip = multilistingObjectsData[dataPair.Key].ToString("0")
                });
                interdealSeries.Points.Add(new DataPoint()
                {
                    XValue = position,
                    YValues = new double[] { interdealObjectsData[dataPair.Key] },
                    AxisLabel = dataPair.Key.ToString("MMM yyyy"),
                    ToolTip = interdealObjectsData[dataPair.Key].ToString("0")
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
        /// Отображает страницу со значением показателей системы за указанную дату
        /// </summary>
        /// <param name="parsedDate">Дата среза показателей</param>
        /// <returns></returns>
        [Route("administration/system-stats")]
        [AuthorizationCheck(Permission.ManageStatistics)]
        public ActionResult DashboardSystemStats(string date = null)
        {
            PushAdministrationNavigationItem();
            PushNavigationItem("Дашбоард", "Сводная информация для администраторов системы", "administration/dashboard", false);
            PushNavigationItem("Показатели системы", "Сводная информация для администраторов системы", "administration/system-stats", false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр дашбоарда");

            var mgr = Locator.GetService<ISystemStatsRepository>();
            DateTime parsedDate;
            if (String.IsNullOrEmpty(date))
            {
                parsedDate = DateTimeZone.Now.Date;
            }
            else
            {
                parsedDate = DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.CurrentUICulture);
            }

            // Выбираем значения показателей за указанный день
            var stats = mgr.Search(s => s.StatDateTime.Date == parsedDate.Date).GroupBy(s => s.StatType).Select(g => g.First());

            // Отдаем все выбранные значения
            ViewBag.date = parsedDate;
            return View(stats);
        }

        /// <summary>
        /// Рендерит картинку с общих графиком поступления платежей в систему и их расхода по месяцам
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ViewDashboard)]
        [Route("administration/dashboard/payments-summary")]
        public ActionResult PaymentsSummaryDashboardImage(FormCollection collection)
        {
            DateTime startDate, endDate;
            if (!DateTime.TryParseExact(Request["startDate"], "dd.MM.yyyy", null, DateTimeStyles.AllowTrailingWhite, out startDate))
            {
                startDate = DateTimeZone.Now.AddMonths(-6);
            }
            if (!DateTime.TryParseExact(Request["endDate"], "dd.MM.yyyy", null, DateTimeStyles.AllowTrailingWhite, out endDate))
            {
                endDate = DateTimeZone.Now;
            }

            // Формируем набор данных
            var paymentsRepository = Locator.GetService<IPaymentsRepository>();
            Dictionary<DateTime, decimal> paymentsIncomeData = new Dictionary<DateTime, decimal>();
            Dictionary<DateTime, decimal> paymentsOutcomeData = new Dictionary<DateTime, decimal>();
            for (var counter = startDate; counter <= endDate.AddMonths(0); counter = counter.AddMonths(1))
            {
                var monthStart = new DateTime(counter.Year, counter.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var incomePayments = paymentsRepository.Search(p => p.Direction == (short)PaymentDirection.Income && p.Payed && p.DatePayed.HasValue && p.DatePayed.Value >= monthStart && p.DatePayed.Value <= monthEnd).ToList();
                var outcomePayments = paymentsRepository.Search(p => p.Direction == (short)PaymentDirection.Outcome && p.DateCreated.HasValue && p.DateCreated.Value >= monthStart && p.DateCreated.Value <= monthEnd).ToList();
                paymentsIncomeData[monthStart] = incomePayments.Count > 0 ? incomePayments.Sum(p => p.Amount) : 0;
                paymentsOutcomeData[monthStart] = outcomePayments.Count > 0 ? outcomePayments.Sum(p => p.Amount) : 0;
            }

            // Конструируем диаграмму
            var chart = new Chart()
            {
                Height = 500,
                Width = 900,
                ImageType = ChartImageType.Png
            };
            chart.Titles.Add("Динамика движения средств");
            var mainArea = chart.ChartAreas.Add("MainArea");
            mainArea.AxisX.IsLabelAutoFit = true;
            mainArea.AxisY.IsLabelAutoFit = true;
            mainArea.AxisY.LineColor = Color.LightGray;
            mainArea.AxisX.LineColor = Color.LightGray;
            mainArea.AxisX.Title = "Месяцы";
            mainArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            mainArea.AxisY.MajorGrid.LineColor = Color.LightGray;

            var mainLegend = chart.Legends.Add("MainLegend");
            mainLegend.Docking = Docking.Bottom;
            mainLegend.TableStyle = LegendTableStyle.Wide;

            var incomeSerier = chart.Series.Add("Сумма поступлений средств на счета пользователей");
            incomeSerier.ChartArea = "MainArea";
            incomeSerier.ChartType = SeriesChartType.Line;
            incomeSerier.Legend = "MainLegend";
            incomeSerier.MarkerStyle = MarkerStyle.Diamond;
            incomeSerier.IsValueShownAsLabel = true;

            var outcomeSerier = chart.Series.Add("Сумма списаний средств с счетов пользователей");
            outcomeSerier.ChartArea = "MainArea";
            outcomeSerier.ChartType = SeriesChartType.Line;
            outcomeSerier.Legend = "MainLegend";
            outcomeSerier.MarkerStyle = MarkerStyle.Diamond;
            outcomeSerier.IsValueShownAsLabel = true;

            // Рендерим данные на диаграмму
            var position = 0;
            foreach (var dataPair in paymentsIncomeData)
            {
                position++;
                incomeSerier.Points.Add(new DataPoint()
                {
                    XValue = position,
                    YValues = new double[] { Convert.ToDouble(dataPair.Value) },
                    AxisLabel = dataPair.Key.ToString("MMM yyyy"),
                    ToolTip = dataPair.Value.ToString("0") + "рублей"
                });
                outcomeSerier.Points.Add(new DataPoint()
                {
                    XValue = position,
                    YValues = new double[] { Convert.ToDouble(paymentsOutcomeData[dataPair.Key]) },
                    AxisLabel = dataPair.Key.ToString("MMM yyyy"),
                    ToolTip = paymentsOutcomeData[dataPair.Key].ToString("0") + "рублей"
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
        /// Отображает график статистики поступления денег от агентов различных компаний или самих компаний за выбранный период
        /// </summary>
        /// <param name="collection">Коллекция параметров</param>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ViewDashboard)]
        [Route("administration/dashboard/payments-companies-summary")]
        public ActionResult DashboardPaymentsCompanySummary(FormCollection collection)
        {
            DateTime startDate, endDate;
            if (!DateTime.TryParseExact(Request["startDate"], "dd.MM.yyyy", null, DateTimeStyles.AllowTrailingWhite, out startDate))
            {
                startDate = DateTimeZone.Now.AddMonths(-6);
            }
            if (!DateTime.TryParseExact(Request["endDate"], "dd.MM.yyyy", null, DateTimeStyles.AllowTrailingWhite, out endDate))
            {
                endDate = DateTimeZone.Now;
            }

            // Формируем набор данных
            var paymentsRepository = Locator.GetService<IPaymentsRepository>();
            Dictionary<Company, decimal> paymentsIncomeData = new Dictionary<Company, decimal>();
            foreach (
                var payment in
                    paymentsRepository.Search(
                        p =>
                        p.Direction == (short)PaymentDirection.Income && p.Payed && p.DatePayed.Value >= startDate &&
                        p.DatePayed.Value <= endDate))
            {
                // Проверяем добавляли мы уже эту компанию или нет? если нет то добавляем
                var company = payment.Company ?? payment.User.Company;
                if (company == null)
                {
                    continue; // Не считаем поступления от бомжей
                }
                if (!paymentsIncomeData.ContainsKey(company))
                {
                    paymentsIncomeData[company] = 0;
                }
                var currentAmount = paymentsIncomeData[company];
                currentAmount += payment.Amount;
                paymentsIncomeData[company] = currentAmount;
            }

            // Конструируем диаграмму
            var chart = new Chart()
            {
                Height = 500,
                Width = 900,
                ImageType = ChartImageType.Png
            };
            chart.Titles.Add("Статистка поступлений от компаний");
            var mainArea = chart.ChartAreas.Add("MainArea");
            mainArea.AxisX.IsLabelAutoFit = true;
            mainArea.AxisY.IsLabelAutoFit = true;
            mainArea.AxisY.LineColor = Color.LightGray;
            mainArea.AxisX.LineColor = Color.LightGray;
            mainArea.AxisX.Title = "Компании";
            mainArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            mainArea.AxisY.MajorGrid.LineColor = Color.LightGray;

            var mainLegend = chart.Legends.Add("MainLegend");
            mainLegend.Docking = Docking.Bottom;
            mainLegend.TableStyle = LegendTableStyle.Wide;

            var incomeSerier = chart.Series.Add("Сумма вложений от компании");
            incomeSerier.ChartArea = "MainArea";
            incomeSerier.ChartType = SeriesChartType.Pie;
            incomeSerier.Legend = "MainLegend";
            incomeSerier.MarkerStyle = MarkerStyle.Diamond;
            incomeSerier.IsValueShownAsLabel = true;

            // Рендерим данные на диаграмму
            var position = 0;
            foreach (var dataPair in paymentsIncomeData)
            {
                incomeSerier.Points.AddXY(dataPair.Key.Name, Convert.ToDouble(dataPair.Value));
            }

            // Сохраняем диаграмму как изображение
            var stream = new MemoryStream();
            chart.SaveImage(stream, ChartImageFormat.Png);
            stream.Position = 0;

            // Отдаем изображение
            return File(stream, "image/png");
        }

        #endregion

        #region Роли

        /// <summary>
        /// Отображает список ролей, созданных в системе
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("administration/roles")]
        public ActionResult Roles()
        {
            PushAdministrationNavigationItem();
            PushNavigationItem("Роли", "Роли, определенные в системе", "/administration/roles", false);

            // Выбираем роли
            var repository = Locator.GetService<IRolesRepository>();
            var roles = repository.GetActiveRoles().ToList();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка ролей в системе");

            // Отдаем их во вью
            return View(roles);
        }

        /// <summary>
        /// Создает в системе новую роль
        /// </summary>
        /// <param name="model">модель</param>
        /// <returns>Перенаправляет на страницу редактирования роли</returns>
        [AuthorizationCheck()]
        [Route("administration/roles/add-role")]
        [HttpPost]
        public ActionResult AddRole(NewRoleModel model)
        {
            try
            {
                // Репозиторий
                var rep = Locator.GetService<IRolesRepository>();

                // Проверяем безопасность
                if (CurrentUser.Id != model.UserId)
                {
                    throw new Exception("Неверный идентификатор пользователя");
                }

                // Проверяем что роль с таким именем не существует
                var role = rep.GetRoleByName(model.Name);
                if (role != null)
                {
                    throw new Exception(string.Format("Роль с таким именем {0} уже существует", model.Name));
                }

                // Создаем роль
                role = new Role()
                    {
                        CreatedBy = CurrentUser.Id,
                        DateCreated = DateTimeZone.Now,
                        ModifiedBy = -1,
                        Name = model.Name
                    };
                rep.Add(role);
                rep.SubmitChanges();

                Locator.GetService<IUINotificationManager>().Success("Новая роль была успешно создана");

                // Событие аудита
                PushAuditEvent(AuditEventTypes.Editing, "Создание новой роли " + model.Name);


                return Json(new { success = true, id = role.Id });
            }
            catch (Exception e)
            {
                Logger.Error("Ошибка при создании новой роли: " + e.Message);
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        /// <summary>
        /// Отображает форму редактирования роли
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("administration/roles/edit/{id}")]
        public ActionResult EditRole(long id)
        {
            PushAdministrationNavigationItem();
            PushNavigationItem("Роли", "Роли, определенные в системе", "/administration/roles", true);

            // Загружаем роль
            var repository = Locator.GetService<IRolesRepository>();
            var role = repository.Load(id);
            if (role == null)
            {
                Locator.GetService<IUINotificationManager>().Error("Запрашиваемая вами роль не найдена");
                return RedirectToAction("Roles");
            }

            PushNavigationItem("Редактирование роли " + role.Name, null, "/administration/roles/edit/" + role.Id, false);

            // Передаем во вью
            var model = new RoleModel()
                {
                    Id = role.Id,
                    Name = role.Name,
                    Permissions = role.RolePermissions.Select(p => new PermissionModel(p)).ToList()
                };
            ViewBag.allPermissions = repository.PermissionsRepository.GetAllPermissions().Select(p => new PermissionModel(p)).ToList();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр пермишеннов роли " + model.Name);

            return View(model);
        }

        /// <summary>
        /// Обрабатывает переименование роли
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("administration/roles/rename-role")]
        [HttpPost]
        public ActionResult RenameRole(RenameRoleModel model)
        {
            try
            {
                // Репозиторий
                var rep = Locator.GetService<IRolesRepository>();

                // Проверяем безопасность
                if (CurrentUser.Id != model.UserId)
                {
                    throw new Exception("Неверный идентификатор пользователя");
                }

                // Проверяем что роль с таким именем не существует
                var role = rep.Load(model.Id);
                if (role == null)
                {
                    throw new Exception(string.Format("Роль с идентификатором {0} не найдена", model.Id));
                }

                // Проверяем что у нас нет роли с аналогичным именем
                if (rep.GetRoleByName(model.Name) != null)
                {
                    throw new Exception(string.Format("Роль с таким именем {0} уже существует", model.Name));
                }

                // Проверяем что роль является системной
                if (role.IsSystemRole())
                {
                    throw new Exception("Нельзя переименовать системную роль");
                }

                // Запоминаем старое имя
                var oldName = role.Name;

                // Переименовываем роль
                role.Name = model.Name;
                role.DateModified = DateTimeZone.Now;
                role.ModifiedBy = CurrentUser.Id;
                rep.SubmitChanges();

                Locator.GetService<IUINotificationManager>().Success("Роль была успешно переименована");

                // Событие аудита
                PushAuditEvent(AuditEventTypes.Editing, string.Format("Переименование роли {0} на {1}", oldName, model.Name));

                return Json(new { success = true, id = role.Id });
            }
            catch (Exception e)
            {
                Logger.Error("Ошибка при переименовании роли: " + e.Message);
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        /// <summary>
        /// Обрабатывает создание или сохранение записи об определенной роли у пользователя
        /// </summary>
        /// <param name="model">Публичная модель сохранения</param>
        /// <param name="collection">Приватная модель в которой хранятся все значения формы плюс сведения об контекстных операциях </param>
        /// <returns>JSON ответ</returns>
        [AuthorizationCheck()]
        [Route("administration/roles/save-role-permission")]
        [HttpPost]
        public ActionResult SaveRolePermission(SaveRolePermissionModel model, FormCollection collection)
        {
            try
            {
                // Репозитории
                var repository = Locator.GetService<IRolesRepository>();

                // Проверяем безопасность
                if (CurrentUser.Id != model.UserId)
                {
                    throw new Exception("Неверный идентификатор пользователя");
                }

                // Проверяем роль
                var role = repository.Load(model.RoleId);
                if (role == null)
                {
                    throw new Exception(string.Format("Роль с идентификатором {0} не найдена", model.RoleId));
                }

                // проверяем пермишен
                var permission = repository.PermissionsRepository.Load(model.PermissionId);
                if (permission == null)
                {
                    throw new Exception(string.Format("Пермишен с идентификатором {0} не найден", model.PermissionId));
                }

                // В зависимости от создания или сохранения выполняем разные действия
                RolePermission rolePermission = null;
                if (model.RolePermissionId <= 0)
                {
                    // Идет добавление пермишена

                    // проверяем есть ли аналогичный пермишен у текущей роли
                    if (role.RolePermissions.Any(rp => rp.PermissionId == model.PermissionId))
                    {
                        throw new Exception(string.Format("Пермишен {0} уже существует у роли {1}", permission.DisplayName, role.Name));
                    }

                    // добавляем
                    rolePermission = new RolePermission()
                        {
                            CreatedBy = CurrentUser.Id,
                            DateCreated = DateTimeZone.Now,
                            PermissionId = permission.Id,
                            RoleId = role.Id,
                            ModifiedBy = -1
                        };
                    repository.AddRolePermission(role, rolePermission);
                }
                else
                {
                    // Идет сохранение пермишенна

                    // проверяем есть ли он у нас
                    rolePermission = role.RolePermissions.FirstOrDefault(rp => rp.Id == model.RolePermissionId);
                    if (rolePermission == null)
                    {
                        throw new Exception(string.Format("Пермишен с идентификатором {0} не найден у роли {1}", model.RolePermissionId, role.Name));
                    }

                    // Редактируем

                    // Проверяем не будет ли нас дубликатов пермишеннов у одной роли
                    if (model.PermissionId != rolePermission.PermissionId)
                    {
                        if (role.RolePermissions.Any(rp => rp.PermissionId == model.PermissionId))
                        {
                            throw new Exception(string.Format("Пермишен {0} уже существует у роли {1}", permission.DisplayName, role.Name));
                        }
                    }
                    rolePermission.PermissionId = permission.Id;
                    rolePermission.DateModified = DateTimeZone.Now;
                    rolePermission.ModifiedBy = CurrentUser.Id;
                }

                // Сохраняем изменения
                repository.SubmitChanges();

                // Теперь редактируем контекстные опции
                if (permission.OperationContext)
                {
                    // Сперва очищаем контекстные опции
                    repository.ClearRolePermissionContextOptions(rolePermission);

                    // Перебираем все присланные ключи
                    foreach (var formKey in collection.AllKeys.Where(k => k.StartsWith("Context_")))
                    {
                        var parts = formKey.Split('_');
                        if (parts.Length != 3)
                        {
                            continue; // Пропускаем неправильные ключи
                        }

                        // Извлекаем выбранную операцию и тип объекта
                        var objectType = Convert.ToInt16(parts[1]);
                        var estateOperation = Convert.ToInt16(parts[2]);

                        // Создаем под нее маппинг
                        var rolePermissionOption = new RolePermissionOption()
                            {
                                DateCreated = DateTimeZone.Now,
                                ObjectOperation = estateOperation,
                                ObjectType = objectType,
                                RolePermissionId = rolePermission.Id
                            };
                        repository.AddRolePermissionOption(rolePermission, rolePermissionOption);
                    }

                    // Сохраняем изменения
                    repository.SubmitChanges();
                }

                Locator.GetService<IUINotificationManager>().Success("Изменения в роли успешно сохранены");

                // Событие аудита
                PushAuditEvent(AuditEventTypes.Editing, model.PermissionId <= 0 ? String.Format("Добавление разрешения {0} к роли {1}", permission.DisplayName, role.Name) : String.Format("Редактирование разрешения {0} у роли {1}", permission.DisplayName, role.Name));

                // Отдаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error("Ошибка при сохранении пермишенна у роли: " + e.Message);
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        /// <summary>
        /// Обрабатывает удаление пермишеннов у определенной роли
        /// </summary>
        /// <param name="permissionIds">Идентификаторы пермишеннов</param>
        /// <param name="roleId">Идентификатор роли</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>JSON ответ</returns>
        [AuthorizationCheck()]
        [HttpPost]
        [Route("administration/roles/delete-role-permissions")]
        public JsonResult DeleteRolePermissions(string permissionIds, long roleId, long userId)
        {
            try
            {
                // Репозитории
                var rolesRepository = Locator.GetService<IRolesRepository>();

                // Проверяем безопасность
                if (CurrentUser.Id != userId)
                {
                    throw new Exception("Неверный идентификатор пользователя");
                }

                // Подгатавливаемся
                var role = rolesRepository.Load(roleId);
                if (role == null)
                {
                    throw new Exception(string.Format("Роль с идентификатором {0} не найдена", roleId));
                }
                var permissions =
                    permissionIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                        i => role.RolePermissions.FirstOrDefault(p => p.Id == i)).Where(p => p != null).ToList();

                var roleName = role.Name;

                // Удаляем
                rolesRepository.DeleteRolePermissions(role, permissions);
                rolesRepository.SubmitChanges();

                Locator.GetService<IUINotificationManager>().Success("Изменения успешно сохранены");

                if (permissions.Count > 0)
                {
                    PushAuditEvent(AuditEventTypes.Editing, String.Format("Удаление {0} разрешений у роли {1}", permissions.Count, roleName));
                }

                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при удалении пермишенна для роли {0} - {1}",
                                        roleId, e.Message));
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        /// <summary>
        /// Обрабатывает удаление ролей
        /// </summary>
        /// <param name="roleIds">Идентификаторы пермишеннов</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>JSON ответ</returns>
        [AuthorizationCheck()]
        [HttpPost]
        [Route("administration/roles/delete-roles")]
        public JsonResult DeleteRoles(string roleIds, long userId)
        {
            try
            {
                // Репозитории
                var rolesRepository = Locator.GetService<IRolesRepository>();

                // Проверяем безопасность
                if (CurrentUser.Id != userId)
                {
                    throw new Exception("Неверный идентификатор пользователя");
                }
                var roles =
                    roleIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                        rolesRepository.Load).Where(r => r != null && !r.IsSystemRole()).ToList();

                // Удаляем
                foreach (var role in roles)
                {
                    rolesRepository.Delete(role);
                }
                rolesRepository.SubmitChanges();

                Locator.GetService<IUINotificationManager>().Success("Изменения успешно сохранены");

                if (roles.Count > 0)
                {
                    PushAuditEvent(AuditEventTypes.Editing, String.Format("Удаление {0} ролей", roles.Count));
                }

                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при удалении ролей для пользователя {0} - {1}",
                                        CurrentUser.Login, e.Message));
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        #endregion

        #region Пользователи

        /// <summary>
        /// Отображает раздел управления пользователями системы
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageUsers)]
        [Route("administration/users")]
        public ActionResult Users()
        {
            PushAdministrationNavigationItem();
            PushNavigationItem("Пользователи", "Все пользователи системы", "/administration/users", false);

            // Репозитории
            var usersRep = Locator.GetService<IUsersRepository>();
            var rolesRep = Locator.GetService<IRolesRepository>();
            var companiesRep = Locator.GetService<ICompaniesRepository>();

            // Получаем список пользователей и ролей
            var users = usersRep.FindAll().OrderBy(r => r.Login);
            var roles = rolesRep.GetActiveRoles().Select(r => new RoleModel()
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList();
            var companies = companiesRep.GetActiveCompanies().OrderBy(c => c.Name).ToList();

            // Передаем список ролей во вью
            ViewBag.roles = roles;
            ViewBag.companies = companies;

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка пользователей системы");

            // Передаем данные во вью
            return View(users);
        }

        /// <summary>
        /// Возвращает информацию об пользователе с указанным идентификатором в JSON формате
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageUsers)]
        [Route("administration/users/get-info/{id}")]
        public JsonResult GetUserInfo(long id)
        {
            try
            {
                // Репозиторий
                var rep = Locator.GetService<IUsersRepository>();

                // Ищем пользователя
                var user = rep.Load(id);
                if (user == null)
                {
                    throw new Exception("Пользователь не найден");
                }

                // Получаем фотографию
                string photoUrl = null;
                if (!String.IsNullOrEmpty(user.PhotoUrl))
                {
                    photoUrl = Locator.GetService<IStoredFilesRepository>().ResolveFileUrl(user.PhotoUrl);
                }

                // Событие аудита
                PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр информации по пользователю " + user.ToString());



                // Отдаем информацию
                return Json(new
                    {
                        success = true,
                        user.Id,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.SurName,
                        user.Phone,
                        user.Phone2,
                        user.ICQ,
                        Birthdate = user.Birthdate.FormatDate(),
                        user.RoleId,
                        user.CompanyId,
                        user.Appointment,
                        SeniorityStartDate = user.SeniorityStartDate.FormatDate(),
                        image = photoUrl,
                    }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                var msg = String.Format("Ошибка при получении данных для пользователя с идентификатором {0}: {1}", id, e.Message);
                Logger.Error(msg);
                Response.StatusCode = 500;
                return Json(new { success = false, message = msg });
            }
        }

        /// <summary>
        /// Отображает форму создания нового пользователя
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageUsers)]
        [Route("administration/users/new")]
        public ActionResult NewUser()
        {
            // Репозитории
            var usersRep = Locator.GetService<IUsersRepository>();
            var rolesRep = Locator.GetService<IRolesRepository>();
            var companiesRep = Locator.GetService<ICompaniesRepository>();

            // Получаем список пользователей и ролей
            var roles = rolesRep.GetActiveRoles().Select(r => new RoleModel()
            {
                Id = r.Id,
                Name = r.Name
            }).ToList();
            var companies = companiesRep.GetActiveCompanies().OrderBy(c => c.Name).ToList();

            // Передаем список ролей во вью
            ViewBag.roles = roles;
            ViewBag.companies = companies;

            // Навигация
            PushAdministrationNavigationItem();
            PushNavigationItem("Пользователи", "Все пользователи системы", "/administration/users");
            PushNavigationItem("Создание пользователя", "Создание нового пользователя в системе", "/administration/users/new", false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Начало создание нового пользователя");

            // Отображаем вид
            return View("EditUser", new User() { Id = -1 });
        }

        /// <summary>
        /// Отображает форму редактирования пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageUsers)]
        [Route("administration/users/edit/{id}")]
        public ActionResult EditUser(long id)
        {
            // Репозитории
            var usersRep = Locator.GetService<IUsersRepository>();
            var rolesRep = Locator.GetService<IRolesRepository>();
            var companiesRep = Locator.GetService<ICompaniesRepository>();

            // Ищем пользователя
            var user = usersRep.Load(id);
            if (user == null)
            {
                UINotificationManager.Error(string.Format("Пользователь с идентификатором {0} не найден", id));
                return RedirectToAction("Index");
            }

            // Получаем список пользователей и ролей
            var roles = rolesRep.GetActiveRoles().Select(r => new RoleModel()
            {
                Id = r.Id,
                Name = r.Name
            }).ToList();
            var companies = companiesRep.GetActiveCompanies().OrderBy(c => c.Name).ToList();

            // Передаем список ролей во вью
            ViewBag.roles = roles;
            ViewBag.companies = companies;

            // Навигация
            PushAdministrationNavigationItem();
            PushNavigationItem("Пользователи", "Все пользователи системы", "/administration/users");
            PushNavigationItem("Редактирование пользователя " + user.ToString(), "Просмотр и редактирование профиля, указанного пользователя", "/administration/users/edit/" + id, false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр информации по пользователю " + user.ToString());

            // Отображаем вид
            return View(user);
        }

        /// <summary>
        /// Обрабатывает создание или изменение параметров пользователя через панель управления
        /// </summary>
        /// <param name="model">Модель данных</param>
        /// <returns>JSON ответ</returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageUsers)]
        [Route("administration/users/save-user")]
        public ActionResult SaveUser(SaveUserModel model)
        {
            try
            {
                // Репозиторий
                var usersRep = Locator.GetService<IUsersRepository>();
                var rolesRepository = Locator.GetService<IRolesRepository>();
                var companiesRep = Locator.GetService<ICompaniesRepository>();

                // Проверяем что роли и компании существуют
                var role = rolesRepository.Load(model.RoleId);
                if (role == null)
                {
                    throw new Exception(string.Format("Роль с идентификатором {0} не найдена", model.RoleId));
                }
                var company = companiesRep.Load(model.CompanyId);
                if (model.CompanyId != -1 && company == null)
                {
                    throw new Exception(string.Format("Компания с идентификатором {0} не найдена", model.CompanyId));
                }

                // В зависимости от операции, создаем или редактируем пользователя
                User user = null;
                bool userCreated = false, passwordChanged = false;
                if (model.Id <= 0)
                {

					// Проверяем есть ли у нас уже пользователь с таким Email
	                if (usersRep.ExistsUserWithLogin(model.Email))
	                {
		                Locator.GetService<IUINotificationManager>().Error("Такой пользователь уже существует");
		                return RedirectToAction("Users");
	                }

                    // Создаем пользователя
                    user = new User()
                        {
                            Login = model.Email,
                            PasswordHash = PasswordUtils.GenerateMD5PasswordHash(model.Password),
                            CreatedBy = CurrentUser.Id,
                            DateCreated = DateTimeZone.Now,
                            ModifiedBy = -1,
                            Status = 1, // Ставим статус активности,
                            Notifications = SubscribeUtils.All()
                        };
                    usersRep.Add(user);
                    userCreated = true;
                }
                else
                {
                    user = usersRep.Load(model.Id);
                    if (user == null)
                    {
                        throw new Exception("Пользователь не найден");
                    }

                    // Проверяем чтобы простой пользователь не мог отредактировать администратора
                    if (user.RoleId == 4 && CurrentUser.RoleId != 4)
                    {
                        throw new SecurityException("Только администратор может редактировать другого администратора");
                    }

                    user.DateModified = DateTimeZone.Now;
                    user.ModifiedBy = CurrentUser.Id;

                    // Проверяем изменен ли пароль
                    if (!String.IsNullOrEmpty(model.Password))
                    {
                        user.PasswordHash = PasswordUtils.GenerateMD5PasswordHash(model.Password);
                        passwordChanged = true;
                    }
                }

                // Обновляем данные
                user.Login = model.Email;
                user.Email = model.Email;
                user.FirstName = model.FirstName;
                user.SurName = model.SurName;
                user.LastName = model.LastName;
                user.Phone = model.Phone;
                user.Phone2 = model.Phone2;
                user.ICQ = model.ICQ;
                user.Birthdate = model.Birthdate;
                user.CompanyId = model.CompanyId;
                if (model.RoleId != user.RoleId)
                {
                    rolesRepository.AssignRole(user, role);
                }
                if (model.Appointment != user.Appointment && CurrentUser.HasPermission(Permission.EditUserAppointment))
                {
                    user.Appointment = model.Appointment;
                }
                else
                {
                    UINotificationManager.Error(string.Format("Должность сотрудника {0} не была изменена, поскольку у Вас отсутствуют необходимые для этого привилегии", user.ToString()));
                }
                user.SeniorityStartDate = model.SeniorityStartDate;
                user.CertificateNumber = model.CertificateNumber;
                user.CertificationDate = model.CertificationDate;
                user.CertificateEndDate = model.CertificateEndDate;
                user.PublicLoading = model.PublicLoading;
                user.AdditionalInformation = model.AdditionalInformation;

                // Проверяем что у пользователя изменился статус
                if (user.Status != (short)UserStatuses.Active &&
                    user.EstateObjects.Any(
                        o =>
                            o.Status == (short)EstateStatuses.Active ||
                            o.Status == (short)EstateStatuses.TemporarilyWithdrawn ||
                            o.Status == (short)EstateStatuses.Withdrawn || o.Status == (short)EstateStatuses.Advance))
                {
                    var objectsToMove = user.EstateObjects.Where(o =>
                        o.Status == (short)EstateStatuses.Active ||
                        o.Status == (short)EstateStatuses.TemporarilyWithdrawn ||
                        o.Status == (short)EstateStatuses.Withdrawn || o.Status == (short)EstateStatuses.Advance)
                        .ToList();
                    foreach (var obj in objectsToMove)
                    {
                        obj.Status = (short)EstateStatuses.Draft;
                        obj.DateModified = DateTimeZone.Now;
                        obj.ObjectChangementProperties.DateModified = DateTimeZone.Now;
                        obj.ObjectChangementProperties.DateMoved = DateTimeZone.Now;
                        obj.ObjectHistoryItems.Add(new ObjectHistoryItem()
                        {
                            ClientId = -1,
                            CompanyId = -1,
                            DateCreated = DateTimeZone.Now,
                            CreatedBy = CurrentUser.Id,
                            EstateObject = obj,
                            HistoryStatus = (int)EstateStatuses.Draft
                        });
                    }
                }

                // Сохраняем
                usersRep.SubmitChanges();
                Locator.GetService<IUINotificationManager>().Success("Пользователь был успешно сохранен");

                // Отправляем сообщения
                if (userCreated)
                {
                    Locator.GetService<IMailNotificationManager>().Notify(user, "Создание аккаунта", MailTemplatesFactory.GetUserCreationTemplate(model).ToString());
                }
                if (passwordChanged)
                {
                    Locator.GetService<IMailNotificationManager>().Notify(user, "Изменение пароля", MailTemplatesFactory.GetForcePasswordChangeTemplate(model).ToString());
                }

                // Событие аудита
                PushAuditEvent(AuditEventTypes.Editing, userCreated ? string.Format("Создание пользователя с логином {0}", user.Login) : string.Format("Редактирование данных пользователя {0}", user.Login));

                // Отдаем успешный результат
                return RedirectToAction("Users");
            }
            catch (Exception e)
            {
                var msg = String.Format("Ошибка при сохранении данных для пользователя с идентификатором {0}: {1}", model.Id, e.Message);
                Logger.Error(msg);
                UINotificationManager.Error(msg);
                return RedirectToAction("Users");
            }
        }

        /// <summary>
        /// Обрабатывает запрос на изменение статуса пользователям
        /// </summary>
        /// <param name="userIds">строка со с идентификаторами пользователей</param>
        /// <param name="newUsersStatus">Новый статус пользователей</param>
        /// <returns>Переходит на страницу списка пользователей</returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageUsers)]
        [Route("administration/users/change-status")]
        public ActionResult ChangeUsersStatus(string userIds, int newUsersStatus)
        {
            try
            {
                if (newUsersStatus != -1)
                {
                    // Пытаем изменить статус пользователей
                    var rep = Locator.GetService<IUsersRepository>();

                    // Находим пользователей
                    var users =
                        userIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                            rep.Load).Where(r => r != null).ToList();

                    // Изменяем и сохраняем
                    foreach (var user in users)
                    {
                        user.Status = newUsersStatus;

                        // Проверяем что у пользователя изменился статус
                        if (user.Status != (short)UserStatuses.Active &&
                            user.EstateObjects.Any(
                                o =>
                                    o.Status == (short)EstateStatuses.Active ||
                                    o.Status == (short)EstateStatuses.TemporarilyWithdrawn ||
                                    o.Status == (short)EstateStatuses.Withdrawn || o.Status == (short)EstateStatuses.Advance))
                        {
                            var objectsToMove = user.EstateObjects.Where(o =>
                                o.Status == (short)EstateStatuses.Active ||
                                o.Status == (short)EstateStatuses.TemporarilyWithdrawn ||
                                o.Status == (short)EstateStatuses.Withdrawn || o.Status == (short)EstateStatuses.Advance)
                                .ToList();
                            foreach (var obj in objectsToMove)
                            {
                                obj.Status = (short)EstateStatuses.Draft;
                                obj.DateModified = DateTimeZone.Now;
                                obj.ObjectChangementProperties.DateModified = DateTimeZone.Now;
                                obj.ObjectChangementProperties.DateMoved = DateTimeZone.Now;
                                obj.ObjectHistoryItems.Add(new ObjectHistoryItem()
                                {
                                    ClientId = -1,
                                    CompanyId = -1,
                                    DateCreated = DateTimeZone.Now,
                                    CreatedBy = CurrentUser.Id,
                                    EstateObject = obj,
                                    HistoryStatus = (int)EstateStatuses.Draft
                                });
                            }
                        }
                    }
                    rep.SubmitChanges();

                    Locator.GetService<IUINotificationManager>().Success("Изменения в статусах пользователей успешно сохранены");

                    if (users.Count > 0)
                    {
                        PushAuditEvent(AuditEventTypes.Editing, string.Format("Изменение статусов {0} пользователей", users.Count));
                    }
                }
            }
            catch (Exception e)
            {
                var msg = String.Format("Ошибка при измении статуста пользователей {0}: {1}", userIds, e.Message);
                Logger.Error(msg);
                Locator.GetService<IUINotificationManager>().Error(msg);
            }
            return RedirectToAction("Users");
        }

        #endregion

        #region Компании

        /// <summary>
        /// Отображает страницу управления компаниями
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageCompanies)]
        [Route("administration/companies")]
        public ActionResult Companies()
        {
            PushAdministrationNavigationItem();
            PushNavigationItem("Компании", "Все компании, зарегисрированные в системе", "/administration/companies", false);

            // Репозитории
            var companiesRep = Locator.GetService<ICompaniesRepository>();
            var usersRep = Locator.GetService<IUsersRepository>();

            // Подгтавливаем пользователей и компании
            var companies = companiesRep.FindAll().ToList();
            ViewBag.users = usersRep.GetActiveUsers().ToList();
            ViewBag.cities = Locator.GetService<IGeoManager>().CitiesRepository.FindAll().OrderBy(c => c.Name).ToList();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка всех компаний, зарегистрированных в системе");

            // Отдаем вид
            return View(companies);
        }

        /// <summary>
        /// Возвращает информацию о компании в JSON формате
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageCompanies)]
        [Route("administration/companies/get-info/{id}")]
        public ActionResult GetCompanyInfo(long id)
        {
            try
            {
                // Репозиторий
                var rep = Locator.GetService<ICompaniesRepository>();

                // Ищем пользователя
                var comp = rep.Load(id);
                if (comp == null)
                {
                    throw new Exception("Компания не найдена");
                }

                // Событие аудита
                PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр сведений о компании " + comp.Name);

                // Отдаем информацию
                return Json(new
                {
                    success = true,
                    comp.Id,
                    comp.Email,
                    comp.Name,
                    comp.ShortName,
                    comp.Description,
                    comp.DirectorId,
                    comp.CityId,
                    comp.Phone1,
                    comp.Phone2,
                    comp.Phone3,
                    comp.Address,
                    comp.Branch,
                    comp.ContactPerson,
                    comp.CompanyType,
                    comp.Inactive,
                    comp.IsServiceProvider,
                    comp.NDSPayer,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                var msg = String.Format("Ошибка при получении данных для компании с идентификатором {0}: {1}", id, e.Message);
                Logger.Error(msg);
                Response.StatusCode = 500;
                return Json(new { success = false, message = msg });
            }
        }

        /// <summary>
        /// Обрабатывает сохранение компании, создание или редактирование
        /// </summary>
        /// <param name="model">Модель данных с клиента</param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageCompanies)]
        [Route("administration/companies/save-company")]
        public JsonResult SaveCompanyModel(SaveCompanyModel model)
        {
            try
            {
                // Репозитории
                var companiesRep = Locator.GetService<ICompaniesRepository>();
                var usersRep = Locator.GetService<IUsersRepository>();

                // Валидируем директора
                var director = usersRep.Load(model.DirectorId);
                if (model.DirectorId != -1 && director == null)
                {
                    throw new Exception(string.Format("Директор с идентификатором {0} не найден", model.DirectorId));
                }

                // В зависимости от идентификатор выполняем или сохраняем
                Company company = null;
                if (model.Id <= 0)
                {
                    // Создаем компанию
                    company = new Company()
                        {
                            CreatedBy = CurrentUser.Id,
                            DateCreated = DateTimeZone.Now,
                            ModifiedBy = -1
                        };
                    companiesRep.Add(company);
                }
                else
                {
                    // Ищем компанию
                    company = companiesRep.Load(model.Id);
                    if (company == null)
                    {
                        throw new Exception("Редактируемая компания не найдена");
                    }
                    company.DateModified = DateTimeZone.Now;
                    company.ModifiedBy = CurrentUser.Id;
                }

                if (company.Id == 2 && model.Inactive)
                {
                    UINotificationManager.Error("Вы не можете заблокировать эту компанию.");
                    return Json(new { success = true });
                }

                // Выполняем заполнение полей
                company.Name = model.Name;
                company.ShortName = model.ShortName;
                company.Description = model.Description;
                company.DirectorId = model.DirectorId;
                company.Email = model.Email;
                company.Phone1 = model.Phone1;
                company.Phone2 = model.Phone2;
                company.Phone3 = model.Phone3;
                company.Address = model.Address;
                company.Branch = model.Branch;
                company.CompanyType = model.CompanyType;
                company.ContactPerson = model.ContactPerson;
                company.CityId = model.CityId;
                company.Inactive = model.Inactive;
                company.IsServiceProvider = model.IsServiceProvider;
                company.NDSPayer = model.NDSPayer;

                // Проверяем что компания стала неактивной
                if (company.Inactive)
                {
                    foreach (var user in company.Users)
                    {
                        user.Status = (short)UserStatuses.InActive;
                        if (user.Status == (short)UserStatuses.InActive && user.EstateObjects.Any(o =>
                            o.Status == (short)EstateStatuses.Active ||
                            o.Status == (short)EstateStatuses.TemporarilyWithdrawn ||
                            o.Status == (short)EstateStatuses.Withdrawn || o.Status == (short)EstateStatuses.Advance))
                        {
                            var objectsToMove = user.EstateObjects.Where(o =>
                                o.Status == (short)EstateStatuses.Active ||
                                o.Status == (short)EstateStatuses.TemporarilyWithdrawn ||
                                o.Status == (short)EstateStatuses.Withdrawn || o.Status == (short)EstateStatuses.Advance)
                                .ToList();
                            foreach (var obj in objectsToMove)
                            {
                                obj.Status = (short)EstateStatuses.Draft;
                                obj.DateModified = DateTimeZone.Now;
                                obj.ObjectChangementProperties.DateModified = DateTimeZone.Now;
                                obj.ObjectChangementProperties.DateMoved = DateTimeZone.Now;
                                obj.ObjectHistoryItems.Add(new ObjectHistoryItem()
                                {
                                    ClientId = -1,
                                    CompanyId = -1,
                                    DateCreated = DateTimeZone.Now,
                                    CreatedBy = CurrentUser.Id,
                                    EstateObject = obj,
                                    HistoryStatus = (int)EstateStatuses.Draft
                                });
                            }
                        }
                    }
                }

                // Сохраняем
                companiesRep.SubmitChanges();

                // Уведомляем
                Locator.GetService<IUINotificationManager>().Success("Изменения успешно сохранены");

                // Событие аудита
                PushAuditEvent(AuditEventTypes.Editing, model.Id <= 0 ? string.Format("Создание компании {0}", model.Name) : string.Format("Редактирование данных компании {0}", model.Name));

                // Отдаем успешный результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                var msg = String.Format("Ошибка при сохранении данных для компании с идентификатором {0}: {1}", model.Id, e.Message);
                Logger.Error(msg);
                Response.StatusCode = 500;
                return Json(new { success = false, message = msg });
            }
        }

        /// <summary>
        /// Обрабатывает удаление компаний
        /// </summary>
        /// <param name="companyIds">Идентификаторы компаний</param>
        /// <returns>JSON ответ</returns>
        [AuthorizationCheck(Permission.ManageCompanies)]
        [HttpPost]
        [Route("administration/companies/delete-companies")]
        public JsonResult DeleteCompanies(string companyIds)
        {
            try
            {
                // Репозитории
                var companiesRepository = Locator.GetService<ICompaniesRepository>();
                var companies =
                    companyIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                        companiesRepository.Load).Where(r => r != null).ToList();

                // Удаляем
                foreach (var company in companies)
                {
                    // Сбрасываем всех пользователей компании
                    foreach (var user in company.Users)
                    {
                        user.CompanyId = -1;
                    }
                    companiesRepository.Delete(company);
                }
                companiesRepository.SubmitChanges();

                Locator.GetService<IUINotificationManager>().Success("Изменения успешно сохранены");

                if (companies.Count > 0)
                {
                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Удаление {0} компаний", companies.Count));
                }

                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при удалении компаний для пользователя {0} - {1}",
                                        CurrentUser.Login, e.Message));
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        #endregion

        #region Справочники

        /// <summary>
        /// Отображает страницу со списком справочников
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageDictionaries)]
        [Route("administration/dictionaries")]
        public ActionResult Dictionaries()
        {
            PushAdministrationNavigationItem();
            PushNavigationItem("Справочники", "Все справочники системы", "/administration/dictionaries", false);

            // Отображаем справочники
            var repository = Locator.GetService<IDictionariesRepository>();

            // Выбираем справочники
            var dictionaries = repository.GetAllDictionaries();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка всех справочников");

            // Отдаем во вью
            return View(dictionaries);
        }

        /// <summary>
        /// Обрабатывает добавление нового справочника в систему
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageDictionaries)]
        [Route("administration/dictionaries/add-dictionary")]
        public JsonResult AddDictionary(NewDictionaryModel model)
        {
            try
            {
                // Репозиторий
                var rep = Locator.GetService<IDictionariesRepository>();

                // Проверяем что справочника нет
                if (rep.GetDictionaryByName(model.SystemName) != null)
                {
                    throw new Exception(string.Format("Справочник с системным именем {0} уже существует", model.SystemName));
                }

                // Создаем справочник
                var dictionary = new Dictionary()
                    {
                        DisplayName = model.DisplayName,
                        SystemName = model.SystemName,
                        DateCreated = DateTimeZone.Now,
                        CreatedBy = CurrentUser.Id,
                        ModifiedBy = CurrentUser.Id,
                        DateModified = DateTimeZone.Now
                    };
                rep.Add(dictionary);

                // Сохраняем изменения в справочнике
                rep.SubmitChanges();

                Locator.GetService<IUINotificationManager>().Success(string.Format("Новый справочник {0} ({1}) успешно создан", model.DisplayName, model.SystemName));

                // Проверяем нужно ли заполнить справочник значенями по умолчанию
                if (!String.IsNullOrEmpty(model.DefaultValues))
                {
                    // Разделяем строку по линиям
                    var values = model.DefaultValues.Split(new[] { "\n", "\r", "\n\r" },
                                                           StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (values.Count > 0)
                    {
                        // Наполняем значениям
                        foreach (var value in values)
                        {
                            // Создаем справочное значение
                            var newValue = new DictionaryValue()
                                {
                                    DateCreated = DateTimeZone.Now,
                                    CreatedBy = CurrentUser.Id,
                                    Value = value,
                                    ModifiedBy = -1,
                                    DictionaryId = dictionary.Id
                                };
                            rep.DictionaryValuesRepository.Add(newValue);
                        }
                        rep.DictionaryValuesRepository.SubmitChanges();
                        Locator.GetService<IUINotificationManager>().Success(string.Format("В новый справочник успешно добавлено {0} новых значений", values.Count));
                    }
                }

                // Событие аудита
                PushAuditEvent(AuditEventTypes.Editing, string.Format("Создание справочника {0}", model.DisplayName));

                // Отдаем результат
                return Json(new { success = true, id = dictionary.Id });
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при создании справочника: {0}", e.Message));
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        /// <summary>
        /// Отображает страницу редактирования содержимого справочника с указанным идентификатором
        /// </summary>
        /// <param name="id">Идентификатор справочника</param>
        /// <returns>Страница редактирования справочника</returns>
        [AuthorizationCheck(Permission.ManageDictionaries)]
        [Route("administration/dictionaries/edit/{id}")]
        public ActionResult EditDictionary(long id)
        {
            PushAdministrationNavigationItem();
            PushNavigationItem("Справочники", "Все справочники системы", "/administration/dictionaries", true);

            // Репозиторий
            var rep = Locator.GetService<IDictionariesRepository>();

            // Получаем справочник и преобразуем его в модель
            var dictionary = rep.Load(id);
            if (dictionary == null)
            {
                Locator.GetService<IUINotificationManager>().Error(string.Format("Справочник с идентификатором {0} не найден в системе", id));
                return RedirectToAction("Dictionaries");
            }

            PushNavigationItem("Редактирование справочника " + dictionary.DisplayName, String.Format("Редактирование справочника {0} - {1}: {2}", dictionary.DisplayName, dictionary.SystemName, dictionary.Description), "administration/dictionaries/edit/" + dictionary.Id, false);

            var model = new EditDictionaryModel(dictionary);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, string.Format("Просмотр справочника {0}", model.DisplayName));

            return View(model);
        }

        /// <summary>
        /// Обрабатывает переименование справочника
        /// </summary>
        /// <param name="model">Модель переименования</param>
        /// <returns>JSON ответ</returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageDictionaries)]
        [Route("administration/dictionaries/rename-dictionary")]
        public ActionResult RenameDictionary(RenameDictionaryModel model)
        {
            try
            {
                // Репозиторий
                var rep = Locator.GetService<IDictionariesRepository>();

                // Ищем справочник для переименования
                var dict = rep.Load(model.Id);
                if (dict == null)
                {
                    throw new Exception("Справочник не найден");
                }

                // Смотрим в какой справочник мы пытаемся переименоваться
                if (dict.SystemName.ToLower() != model.SystemName.ToLower() && rep.GetDictionaryByName(model.SystemName) != null)
                {
                    throw new Exception(string.Format("Справочник с таким системным именем {0} уже существует", model.SystemName));
                }

                var oldName = dict.DisplayName;

                // Переименовываем
                dict.SystemName = model.SystemName;
                dict.DisplayName = model.DisplayName;
                dict.DateModified = DateTimeZone.Now;
                dict.ModifiedBy = CurrentUser.Id;

                // Сохраняем
                rep.SubmitChanges();

                Locator.GetService<IUINotificationManager>().Success("Справочник успешно переименован");

                // Событие аудита
                PushAuditEvent(AuditEventTypes.Editing, string.Format("Переименование справочника {0}", oldName));

                // Отдаем успешный результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при переименовании справочника: {0}", e.Message));
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        /// <summary>
        /// Обрабатывает сохранение справочного значения в справочнике
        /// </summary>
        /// <param name="model">Модель с данными по редактированию</param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageDictionaries)]
        [Route("administration/dictionaries/save-dictionary-value")]
        public ActionResult SaveDictionaryValue(DictionaryValueModel model)
        {
            try
            {
                // Репозиторий
                var rep = Locator.GetService<IDictionariesRepository>();

                //Загружаем справочник
                var dict = rep.Load(model.DictionaryId);
                if (dict == null)
                {
                    throw new Exception("Справочник не найден");
                }

                // Проверяем создаем мы или редактируем
                if (model.Id <= 0)
                {
                    // Создаем
                    var newValue = new DictionaryValue()
                        {
                            CreatedBy = CurrentUser.Id,
                            ModifiedBy = -1,
                            DateCreated = DateTimeZone.Now,
                            DictionaryId = dict.Id,
                            Value = model.Value,
							ShortValue = model.ShortValue
                        };
                    rep.DictionaryValuesRepository.Add(newValue);
                }
                else
                {
                    // Редактируем
                    var value = rep.DictionaryValuesRepository.Load(model.Id);
                    if (value == null)
                    {
                        throw new Exception("Значение справочника не найдено");
                    }
                    value.Value = model.Value;
	                value.ShortValue = model.ShortValue;
                    value.ModifiedBy = CurrentUser.Id;
                    value.DateModified = DateTimeZone.Now;
                }

                // Сохраняем изменения
                rep.SubmitChanges();
                rep.DictionaryValuesRepository.SubmitChanges(); // На случай если дата контексты рассинхронизовались

                Locator.GetService<IUINotificationManager>().Success("Изменения в справочнике успешно сохранены");

                // Событие аудита
                PushAuditEvent(AuditEventTypes.Editing, model.Id <= 0 ? string.Format("Добавление значения {0} в справочник {1}", model.Value, dict.DisplayName) : string.Format("Редактирование значения {0} в справочнике {1}", model.Value, dict.DisplayName));

                // Отдаем изменения
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error(String.Format(
                    "Ошибка при сохранении значения справочника {0} с идентификатором {1} : {2}", model.DictionaryId,
                    model.Id, e.Message));
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        /// <summary>
        /// Обрабатывает удаление значений из справочников
        /// </summary>
        /// <param name="valuesIds">Идентификаторы значений</param>
        /// <param name="dictionaryId">Идентификатор справочника</param>
        /// <returns>JSON ответ</returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageDictionaries)]
        [Route("administration/dictionaries/delete-dictionary-values")]
        public ActionResult DeleteDictionaryValues(string valuesIds, long dictionaryId)
        {
            try
            {
                // Репозитории
                var repository = Locator.GetService<IDictionariesRepository>();

                var dValues =
                    valuesIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                        repository.DictionaryValuesRepository.Load).Where(r => r != null).ToList();

                // Удаляем
                foreach (var value in dValues)
                {
                    repository.DictionaryValuesRepository.Delete(value);
                }
                repository.SubmitChanges();

                Locator.GetService<IUINotificationManager>().Success("Изменения успешно сохранены");

                if (dValues.Count > 0)
                {
                    // Событие аудита
                    PushAuditEvent(AuditEventTypes.Editing, String.Format("Удаление {0} значений из справочника", dValues.Count));
                }

                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при удалении значений справочников: {0}", e.Message));
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        /// <summary>
        /// Обрабатывает удаление справочников из системы
        /// </summary>
        /// <param name="dictionaryIds">Идентификаторы справочников</param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageDictionaries)]
        [Route("administration/roles/delete-dictionaries")]
        public ActionResult DeleteDictionaries(string dictionaryIds)
        {
            try
            {
                // Репозитории
                var repository = Locator.GetService<IDictionariesRepository>();

                var dicts =
                    dictionaryIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                        repository.Load).Where(r => r != null).ToList();

                // Удаляем
                foreach (var dictionary in dicts)
                {
                    repository.Delete(dictionary);
                }
                repository.SubmitChanges();

                Locator.GetService<IUINotificationManager>().Success("Изменения успешно сохранены");

                if (dicts.Count > 0)
                {
                    // Событие аудита
                    PushAuditEvent(AuditEventTypes.Editing, String.Format("Удаление {0} справочников", dicts.Count));
                }

                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при удалении справочников: {0}", e.Message));
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        #endregion

        #region Гео справочник

        /// <summary>
        /// Отображает страницу управления географическим справочником
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageDictionaries)]
        [Route("administration/dictionaries/geo")]
        public ActionResult GeoDictionary()
        {
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Справочники", "Все справочники системы", "/administration/dictionaries", true);
            PushNavigationItem("Гео справочник", "Страница редактирования географического справочника системы, содержащего гео-объекты", "/administration/dictionaries/geo", true);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр географического справочника");

            // Отображаем вид
            return View();
        }

        /// <summary>
        /// Обрабатывает получение географических объектов
        /// </summary>
        /// <param name="id">Идентификатор родительского объекта, вызвавшего получение</param>
        /// <param name="level">Уровень родительского объекта, вызвавшего получение</param>
        /// <returns>JSON данные</returns>
        [Route("administration/dictionaries/geo/get-geo-data")]
        [AuthorizationCheck(Permission.ManageDictionaries)]
        public JsonResult GetGeoData(string id, long level)
        {
            try
            {
                var manager = Locator.GetService<IGeoManager>();

                // Хранилище результата
                IEnumerable<Dictionary<string, object>> result = null;

                // Проверяем, пытаемся ли мы инициализировать древо впервые
                if (level == -1)
                {
                    // пытаемся
                    result = manager.CountriesRepository.FindAll().OrderBy(c => c.Name).Select(c => c.ConvertToJSTreeJson()).ToList();
                }
                else
                {
                    // Подгатавливаемся
                    var idNumeric = Convert.ToInt64(id.Split('-')[1]);
                    switch ((GeoLevels)level)
                    {
                        case GeoLevels.Country:
                            var country = manager.CountriesRepository.Load(idNumeric);
                            result = country.GeoRegions.OrderBy(o => o.Name).Select(o => o.ConvertToJSTreeJson()).ToList();
                            break;
                        case GeoLevels.Region:
                            var region = manager.RegionsRepository.Load(idNumeric);
                            result = region.GeoRegionDistricts.OrderBy(o => o.Name).Select(o => o.ConvertToJSTreeJson()).ToList();
                            break;
                        case GeoLevels.RegionDistrict:
                            var regionDistrict = manager.RegionsDistrictsRepository.Load(idNumeric);
                            result = regionDistrict.GeoCities.OrderBy(o => o.Name).Select(o => o.ConvertToJSTreeJson()).ToList();
                            break;
                        case GeoLevels.City:
                            var city = manager.CitiesRepository.Load(idNumeric);
                            result = city.GeoDistricts.OrderBy(o => o.Name).Select(o => o.ConvertToJSTreeJson(true)).ToList();
                            break;
                        case GeoLevels.District:
                            var district = manager.DistrictsRepository.Load(idNumeric);
                            result = district.GeoResidentialAreas.OrderBy(o => o.Name).Select(o => o.ConvertToJSTreeJson()).ToList();
                            break;
                        case GeoLevels.ResidentialArea:
                            var area = manager.ResidentialAreasRepository.Load(idNumeric);
                            result = area.GeoStreets.OrderBy(o => o.Name).Select(o => o.ConvertToJSTreeJson(true)).ToList();
                            break;
                        case GeoLevels.Street:
                            var street = manager.StreetsRepository.Load(idNumeric);
                            result = street.GeoObjects.OrderBy(o => o.Name).Select(o => o.ConvertToJSTreeJson()).ToList();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("level");
                    }
                }

                // Отдаем результат на сериализацию
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                var message = String.Format("Ошибка в ходе формирования географического древа: {0}", e.Message);
                Logger.Error(message);
                Response.StatusCode = 500;
                return Json(new { success = false, message = message });
            }
        }

        /// <summary>
        /// Обрабатывает создание или сохранение географического объекта
        /// </summary>
        /// <param name="model">Модель данных по объекту</param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageDictionaries)]
        [Route("administration/dictionaries/geo/edit")]
        public JsonResult EditGeoObject(GeoObjectEditModel model)
        {
            try
            {
                // Менеджер
                var manager = Locator.GetService<IGeoManager>();

                // Данные для посылки на клиент
                object serverData = null;

                // В зависимости от типа операции создаем или редактируем объект
                if (model.Id <= 0)
                {
                    // Идет создание объекта
                    switch (model.ObjectType)
                    {
                        case GeoLevels.Region:
                            // Ищем и валидируем родительский объект
                            var parentCountry = manager.CountriesRepository.Load(model.ParentObjectId);
                            if (parentCountry == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Страна с идентификатором {0} не найдена", model.ParentObjectId));
                            }
                            // Создаем
                            var newRegion = new GeoRegion()
                                                {
                                                    CountryId = model.ParentObjectId,
                                                    DateCreated = DateTimeZone.Now,
                                                    Name = model.Name
                                                };
                            manager.RegionsRepository.Add(newRegion);
                            manager.RegionsRepository.SubmitChanges();
                            serverData = newRegion.ConvertToJSTreeJson();
                            break;
                        case GeoLevels.RegionDistrict:
                            // Ищем и валидируем родительский объект
                            var parentRegion = manager.CountriesRepository.Load(model.ParentObjectId);
                            if (parentRegion == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Регион с идентификатором {0} не найден", model.ParentObjectId));
                            }
                            // Создаем
                            var newRegionDistrict = new GeoRegionDistrict()
                                                {
                                                    RegionId = model.ParentObjectId,
                                                    DateCreated = DateTimeZone.Now,
                                                    Name = model.Name
                                                };
                            manager.RegionsDistrictsRepository.Add(newRegionDistrict);
                            manager.RegionsDistrictsRepository.SubmitChanges();
                            serverData = newRegionDistrict.ConvertToJSTreeJson();
                            break;
                        case GeoLevels.City:
                            // Ищем и валидируем родительский объект
                            var parentRegionDistrict = manager.RegionsDistrictsRepository.Load(model.ParentObjectId);
                            if (parentRegionDistrict == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Район региона с идентификатором {0} не найден", model.ParentObjectId));
                            }
                            // Создаем
                            var newCity = new GeoCity()
                            {
                                RegionDistrictId = model.ParentObjectId,
                                DateCreated = DateTimeZone.Now,
                                Name = model.Name
                            };
                            manager.CitiesRepository.Add(newCity);
                            manager.CitiesRepository.SubmitChanges();
                            serverData = newCity.ConvertToJSTreeJson();
                            break;
                        case GeoLevels.District:
                            // Ищем и валидируем родительский объект
                            var parentCity = manager.CitiesRepository.Load(model.ParentObjectId);
                            if (parentCity == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Город с идентификатором {0} не найден", model.ParentObjectId));
                            }
                            // Создаем
                            var newDistrict = new GeoDistrict()
                            {
                                CityId = model.ParentObjectId,
                                DateCreated = DateTimeZone.Now,
                                Name = model.Name,
                                Bounds = model.Bounds
                            };
                            manager.DistrictsRepository.Add(newDistrict);
                            manager.DistrictsRepository.SubmitChanges();
                            serverData = newDistrict.ConvertToJSTreeJson();
                            break;
                        case GeoLevels.ResidentialArea:
                            // Ищем и валидируем родительский объект
                            var parentDistrict = manager.DistrictsRepository.Load(model.ParentObjectId);
                            if (parentDistrict == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Район с идентификатором {0} не найден", model.ParentObjectId));
                            }
                            // Создаем
                            var newArea = new GeoResidentialArea()
                            {
                                DistrictId = model.ParentObjectId,
                                DateCreated = DateTimeZone.Now,
                                Name = model.Name,
                                Bounds = model.Bounds
                            };
                            manager.ResidentialAreasRepository.Add(newArea);
                            manager.ResidentialAreasRepository.SubmitChanges();
                            serverData = newArea.ConvertToJSTreeJson();
                            break;
                        case GeoLevels.Street:
                            // Ищем и валидируем родительский объект
                            var parentArea = manager.ResidentialAreasRepository.Load(model.ParentObjectId);
                            if (parentArea == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Жилая зона с идентификатором {0} не найдена", model.ParentObjectId));
                            }
                            // Создаем
                            var newStreet = new GeoStreet()
                            {
                                AreaId = model.ParentObjectId,
                                DateCreated = DateTimeZone.Now,
                                Name = model.Name
                            };
                            manager.StreetsRepository.Add(newStreet);
                            manager.StreetsRepository.SubmitChanges();
                            serverData = newStreet.ConvertToJSTreeJson();
                            break;
                        case GeoLevels.Object:
                            // Ищем и валидируем родительский объект
                            var parentStreet = manager.StreetsRepository.Load(model.ParentObjectId);
                            if (parentStreet == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Улица с идентификатором {0} не найдена", model.ParentObjectId));
                            }
                            // Создаем
                            var newObject = new GeoObject()
                            {
                                StreetId = model.ParentObjectId,
                                DateCreated = DateTimeZone.Now,
                                Name = model.Name
                            };
                            manager.ObjectsRepository.Add(newObject);
                            manager.ObjectsRepository.SubmitChanges();
                            serverData = newObject.ConvertToJSTreeJson();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    // Идет редактирование узла
                    switch (model.ObjectType)
                    {
                        case GeoLevels.Country:
                            // Ищем объект
                            var country = manager.CountriesRepository.Load(model.Id);
                            if (country == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Страна с идентификатором {0} не найдена", model.Id));
                            }
                            // Редактируем
                            country.Name = model.Name;
                            country.DateModified = DateTimeZone.Now;
                            manager.CountriesRepository.SubmitChanges();
                            serverData = country.Name;
                            break;
                        case GeoLevels.Region:
                            // Ищем объект
                            var region = manager.RegionsRepository.Load(model.Id);
                            if (region == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Регион с идентификатором {0} не найден", model.Id));
                            }
                            // Редактируем
                            region.Name = model.Name;
                            region.DateModified = DateTimeZone.Now;
                            manager.RegionsRepository.SubmitChanges();
                            serverData = region.Name;
                            break;
                        case GeoLevels.RegionDistrict:
                            // Ищем объект
                            var regionDistrict = manager.RegionsDistrictsRepository.Load(model.Id);
                            if (regionDistrict == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Район региона с идентификатором {0} не найден", model.Id));
                            }
                            // Редактируем
                            regionDistrict.Name = model.Name;
                            regionDistrict.DateModified = DateTimeZone.Now;
                            manager.RegionsRepository.SubmitChanges();
                            serverData = regionDistrict.Name;
                            break;
                        case GeoLevels.City:
                            // Ищем объект
                            var city = manager.CitiesRepository.Load(model.Id);
                            if (city == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Город с идентификатором {0} не найден", model.Id));
                            }
                            // Редактируем
                            city.Name = model.Name;
                            city.DateModified = DateTimeZone.Now;
                            manager.CitiesRepository.SubmitChanges();
                            serverData = city.Name;
                            break;
                        case GeoLevels.District:
                            // Ищем объект
                            var district = manager.DistrictsRepository.Load(model.Id);
                            if (district == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Район с идентификатором {0} не найден", model.Id));
                            }
                            // Редактируем
                            district.Name = model.Name;
                            district.DateModified = DateTimeZone.Now;
                            district.Bounds = model.Bounds;
                            manager.DistrictsRepository.SubmitChanges();
                            serverData = district.Name;
                            break;
                        case GeoLevels.ResidentialArea:
                            // Ищем объект
                            var area = manager.ResidentialAreasRepository.Load(model.Id);
                            if (area == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Жилмассив с идентификатором {0} не найден", model.Id));
                            }
                            // Редактируем
                            area.Name = model.Name;
                            area.DateModified = DateTimeZone.Now;
                            area.Bounds = model.Bounds;
                            manager.ResidentialAreasRepository.SubmitChanges();
                            serverData = area.Name;
                            break;
                        case GeoLevels.Street:
                            // Ищем объект
                            var street = manager.StreetsRepository.Load(model.Id);
                            if (street == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Улица с идентификатором {0} не найдена", model.Id));
                            }
                            // Редактируем
                            street.Name = model.Name;
                            street.DateModified = DateTimeZone.Now;
                            manager.StreetsRepository.SubmitChanges();
                            serverData = street.Name;
                            break;
                        case GeoLevels.Object:
                            // Ищем объект
                            var geoObject = manager.ObjectsRepository.Load(model.Id);
                            if (geoObject == null)
                            {
                                throw new ObjectNotFoundException(String.Format("Объект с идентификатором {0} не найден", model.Id));
                            }
                            // Редактируем
                            geoObject.Name = model.Name;
                            geoObject.DateModified = DateTimeZone.Now;
                            manager.ObjectsRepository.SubmitChanges();
                            serverData = geoObject.Name;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                // Событие аудита
                PushAuditEvent(AuditEventTypes.Editing, String.Format(model.Id <= 0 ? "Добавление объекта {0} в географический справочник" : "Редактирование объекта {0} в географическом справочнике", model.Name));

                // Отдаем данные на клиент
                return Json(new { success = true, serverData = serverData });
            }
            catch (Exception e)
            {
                var message = String.Format("Ошибка в ходе сохранения географического объекта: {0}", e.Message);
                Logger.Error(message);
                Response.StatusCode = 500;
                return Json(new { success = false, message = message });
            }
        }

        /// <summary>
        /// Обрабатывает удаление геообъекта а так же всех его дочерних объектов. Операцияя не завершиться, если на указанный геообъект ссылается хотя бы один из адресов объектов недвижимости
        /// </summary>
        /// <param name="model">Модель удаления геобъекта</param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageDictionaries)]
        [Route("administration/dictionaries/geo/delete")]
        public ActionResult DeleteGeoObject(GeoObjectDeleteModel model)
        {
            try
            {
                // менеджер
                var manager = Locator.GetService<IGeoManager>();

                switch (model.ObjectType)
                {
                    case GeoLevels.Country:
                        // Ищем объект
                        var country = manager.CountriesRepository.Load(model.Id);
                        if (country == null)
                        {
                            throw new ObjectNotFoundException(String.Format("Страна с идентификатором {0} не найдена", model.Id));
                        }
                        // Удаляем
                        manager.CountriesRepository.Delete(country);
                        manager.CountriesRepository.SubmitChanges();
                        break;
                    case GeoLevels.Region:
                        // Ищем объект
                        var region = manager.RegionsRepository.Load(model.Id);
                        if (region == null)
                        {
                            throw new ObjectNotFoundException(String.Format("Регион с идентификатором {0} не найден", model.Id));
                        }
                        manager.RegionsRepository.Delete(region);
                        manager.RegionsRepository.SubmitChanges();
                        break;
                    case GeoLevels.RegionDistrict:
                        // Ищем объект
                        var regionDistrict = manager.RegionsDistrictsRepository.Load(model.Id);
                        if (regionDistrict == null)
                        {
                            throw new ObjectNotFoundException(String.Format("Район региона с идентификатором {0} не найден", model.Id));
                        }
                        manager.RegionsDistrictsRepository.Delete(regionDistrict);
                        manager.RegionsDistrictsRepository.SubmitChanges();
                        break;
                    case GeoLevels.City:
                        // Ищем объект
                        var city = manager.CitiesRepository.Load(model.Id);
                        if (city == null)
                        {
                            throw new ObjectNotFoundException(String.Format("Город с идентификатором {0} не найден", model.Id));
                        }
                        manager.CitiesRepository.Delete(city);
                        manager.CitiesRepository.SubmitChanges();
                        break;
                    case GeoLevels.District:
                        // Ищем объект
                        var district = manager.DistrictsRepository.Load(model.Id);
                        if (district == null)
                        {
                            throw new ObjectNotFoundException(String.Format("Район с идентификатором {0} не найден", model.Id));
                        }
                        manager.DistrictsRepository.Delete(district);
                        manager.DistrictsRepository.SubmitChanges();
                        break;
                    case GeoLevels.ResidentialArea:
                        // Ищем объект
                        var area = manager.ResidentialAreasRepository.Load(model.Id);
                        if (area == null)
                        {
                            throw new ObjectNotFoundException(String.Format("Жилмассив с идентификатором {0} не найден", model.Id));
                        }
                        manager.ResidentialAreasRepository.Delete(area);
                        manager.ResidentialAreasRepository.SubmitChanges();
                        break;
                    case GeoLevels.Street:
                        // Ищем объект
                        var street = manager.StreetsRepository.Load(model.Id);
                        if (street == null)
                        {
                            throw new ObjectNotFoundException(String.Format("Улица с идентификатором {0} не найдена", model.Id));
                        }
                        manager.StreetsRepository.Delete(street);
                        manager.StreetsRepository.SubmitChanges();
                        break;
                    case GeoLevels.Object:
                        // Ищем объект
                        var geoObject = manager.ObjectsRepository.Load(model.Id);
                        if (geoObject == null)
                        {
                            throw new ObjectNotFoundException(String.Format("Объект с идентификатором {0} не найден", model.Id));
                        }
                        manager.ObjectsRepository.Delete(geoObject);
                        manager.ObjectsRepository.SubmitChanges();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // Событие аудита
                PushAuditEvent(AuditEventTypes.Editing, String.Format("Удаление географического объекта типа {0} с идентификатором {1}", model.ObjectType.GetEnumMemberName(), model.Id));

                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                var message = String.Format("Ошибка в ходе удаления географического объекта: {0}", e.Message);
                Logger.Error(message);
                Response.StatusCode = 500;
                return Json(new { success = false, message = message });
            }
        }

        /// <summary>
        /// Обрабатывает импортирование данных с клиента
        /// </summary>
        /// <param name="importFormat">Тип импортируемого формата</param>
        /// <returns>Обновляет страницу геосправочника</returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageDictionaries)]
        [Route("administration/dictionaries/geo/import")]
        public ActionResult ImportGeoData(DataFormats importFormat)
        {
            try
            {
                // Проверяем файл
                var dataFile = Request.Files["DataFile"];
                if (dataFile == null || dataFile.ContentLength == 0)
                {
                    throw new Exception("Файл должен быть загружен");
                }

                // На основании указанного формата инициализируем импортер
                IImporter importer = null;
                switch (importFormat)
                {
                    case DataFormats.XLS:
                        importer = Locator.GetService<IGeoXLSImporter>();
                        ((IGeoXLSImporter)importer).SkipRow = 1;
                        break;
                    case DataFormats.XML:
                        importer = Locator.GetService<IGeoXMLImporter>();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Данный формат не поддерживается в этой процедуре импорта");
                }

                // Выполняем импорт
                var importResult = importer.ImportStream(dataFile.InputStream);

                // Если не словили ошибок, то выводим сообщение об успешном импорте
                UINotificationManager.Success(String.Format("Импорт успешно завершен. Импортировано {0} объектов", importResult.ImportedCount));

                // Событие аудита
                PushAuditEvent(AuditEventTypes.Editing, string.Format("Импортирование геоданных. Импортировано: {0} объектов", importResult.ImportedCount));
            }
            catch (Exception e)
            {
                UINotificationManager.Error("Ошибка в ходе импортирования гео данных: " + e.Message);
            }
            return RedirectToAction("GeoDictionary");
        }

        #endregion

        #region Контент системы

        #region Статические страницы

        /// <summary>
        /// Отображает панель управления контентом системы - раздел управления статическими страницами
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content")]
        public ActionResult Content()
        {
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Контент", "Управление контентом системы", "/administration/content", false);

            // Выбираем страницы
            var pages =
                Locator.GetService<IStaticPagesRepository>().FindAll().OrderByDescending(d => d.DateModified).
                    ThenByDescending(d => d.DateCreated).ToList();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр панели управления HTML страницами системы");

            return View(pages);
        }

        /// <summary>
        /// Отображает форму добавления новой страницы в систему
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/add-page")]
        public ActionResult AddStaticPage()
        {
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Контент", "Управление контентом системы", "/administration/content");
            PushNavigationItem("Новая страница", "Создание новой статической страницы в системе", "/administration/content/add-page", false);

            return View("EditStaticPage", new StaticPageModel() { Id = -1 });
        }

        /// <summary>
        /// Отображает форму редактирования указанной статической страницы
        /// </summary>
        /// <param name="id">Идентификатор статической страницы</param>
        /// <returns>Страница редактирования</returns>
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/edit-page/{id}")]
        public ActionResult EditStaticPage(long id)
        {
            // Репозиторий
            var rep = Locator.GetService<IStaticPagesRepository>();

            // Загружаем страницу
            var page = rep.Load(id);
            if (page == null)
            {
                UINotificationManager.Error(string.Format("Страница с идентификатором {0} не найдена", id));
                return RedirectToAction("Content");
            }

            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Контент", "Управление контентом системы", "/administration/content");
            PushNavigationItem("Редактирование страницы " + page.Title, "", "/administration/content/edit-page/" + page.Id, false);

            return View("EditStaticPage", new StaticPageModel(page));
        }

        /// <summary>
        /// Сохраняет изменения в существующей странице или создает новую
        /// </summary>
        /// <param name="model">Модель данных для страницы</param>
        /// <returns>Переходит на список всех страниц</returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageContent)]
        [ValidateInput(false)]
        [Route("administration/content/save-page")]
        public ActionResult SaveStaticPage(StaticPageModel model)
        {
            try
            {
                // Репозиторий
                var rep = Locator.GetService<IStaticPagesRepository>();

                // В зависимости от операции, создаем или редактируем страницу
                if (model.Id <= 0)
                {
                    // Создаем страницу
                    var newPage = new StaticPage()
                                      {
                                          Title = model.Title,
                                          Route = model.Route,
                                          Content = model.Content,
                                          DateCreated = DateTimeZone.Now,
                                          CreatedBy = CurrentUser.Id,
                                          ModifiedBy = CurrentUser.Id,
                                          DateModified = DateTimeZone.Now
                                      };

                    // Добавляем в базу
                    rep.Add(newPage);
                    rep.SubmitChanges();

                    // Добавляем роут в таблицу роутов
                    RoutesManager.RegisterRoute("Static-page-" + newPage.Id, newPage.Route, new { controller = "Pages", action = "ViewPage", id = newPage.Id }, true);

                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Создание новой страницы {0}", newPage.Title));
                }
                else
                {
                    // Ищем и редактируем страницу
                    var page = rep.Load(model.Id);
                    if (page == null)
                    {
                        throw new ObjectNotFoundException(string.Format("Страница с идентификатором {0} не найдена", model.Id));
                    }

                    var oldRoute = page.Route;

                    page.Title = model.Title;
                    page.Route = model.Route;
                    page.Content = model.Content;
                    page.DateModified = DateTimeZone.Now;
                    page.ModifiedBy = CurrentUser.Id;

                    // Сохраняем изменения
                    rep.SubmitChanges();

                    if (oldRoute != model.Route)
                    {
                        // Требуется отредактировать список роутов
                        RoutesManager.UpdateRoute(oldRoute, model.Route);
                    }

                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Редактирование страницы {0}", page.Title));
                }

                // Выводим успешное сообщение
                UINotificationManager.Success("Изменения успешно сохранены");

                // Отображаем список страниц
                return RedirectToAction("Content");
            }
            catch (Exception e)
            {
                UINotificationManager.Error(e.Message);
                return RedirectToAction("Content");
            }
        }

        /// <summary>
        /// Удаляет статические страницы с указанными идентификаторами
        /// </summary>
        /// <param name="pageIds"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/delete-pages")]
        public ActionResult DeleteStaticPages(string pageIds)
        {
            try
            {
                // Репозитории
                var pagesRep = Locator.GetService<IStaticPagesRepository>();
                var pages =
                    pageIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                        pagesRep.Load).Where(r => r != null).ToList();

                // Удаляем
                foreach (var page in pages)
                {
                    pagesRep.Delete(page);
                }
                pagesRep.SubmitChanges();

                UINotificationManager.Success("Изменения успешно сохранены");

                if (pages.Count > 0)
                {
                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Удаление {0} статических страниц", pages.Count));
                }

                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при удалении статических страниц для пользователя {0} - {1}",
                                        CurrentUser.Login, e.Message));
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        #endregion

        #region Публикации

        /// <summary>
        /// Отображает страницу с публикациями системы
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/news")]
        public ActionResult News()
        {
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Контент", "Управление контентом системы", "/administration/content", true);
            PushNavigationItem("Публикации", "Управление публикациями системы", "/administration/content/news", false);

            // Выбираем страницы
            var articles =
                Locator.GetService<IArticlesRepository>().FindAll().OrderByDescending(d => d.PublicationDate).
                    ThenByDescending(d => d.DateModified).ThenByDescending(d => d.DateCreated).ToList();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр панели управления публикациями");

            return View(articles);
        }

        /// <summary>
        /// Отображает форму добавления новой публикации в систему
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/add-news")]
        public ActionResult AddArticle()
        {
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Контент", "Управление контентом системы", "/administration/content");
            PushNavigationItem("Публикации", "Управление публикациями системы", "/administration/content/news", true);
            PushNavigationItem("Новая страница", "Создание новой статической страницы в системе", "/administration/content/add-page", false);

            return View("EditArticle", new ArticleModel() { Id = -1, ArticleType = ArticleTypes.News, PublicationDate = DateTimeZone.Now });
        }

        /// <summary>
        /// Отображает форму редактирования указанной публикации
        /// </summary>
        /// <param name="id">Идентификатор публикации</param>
        /// <returns>Страница редактирования</returns>
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/edit-news/{id}")]
        public ActionResult EditArticle(long id)
        {
            // Репозиторий
            var rep = Locator.GetService<IArticlesRepository>();

            // Загружаем страницу
            var article = rep.Load(id);
            if (article == null)
            {
                UINotificationManager.Error(string.Format("Публикация с идентификатором {0} не найдена", id));
                return RedirectToAction("News");
            }

            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Контент", "Управление контентом системы", "/administration/content");
            PushNavigationItem("Публикации", "Управление публикациями системы", "/administration/content/news", true);
            PushNavigationItem("Редактирование публикации " + article.Title, "", "/administration/content/edit-news/" + article.Id, false);

            return View("EditArticle", new ArticleModel(article));
        }

        /// <summary>
        /// Сохраняет изменения в существующей публикации или создает новую
        /// </summary>
        /// <param name="model">Модель данных для публикации</param>
        /// <returns>Переходит на список всех страниц</returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageContent)]
        [ValidateInput(false)]
        [Route("administration/content/save-news")]
        public ActionResult SaveArticle(ArticleModel model)
        {
            try
            {
                // Репозиторий
                var rep = Locator.GetService<IArticlesRepository>();

                // В зависимости от операции, создаем или редактируем публикацию
                if (model.Id <= 0)
                {
                    // Создаем публикацию
                    var newArticle = new Article()
                    {
                        Title = model.Title,
                        PublicationDate = model.PublicationDate,
                        ArticleType = model.ArticleType,
                        DateCreated = DateTimeZone.Now,
                        ShortContent = model.ShortContent,
                        FullContent = model.FullContent,
                        VideoLink = model.VideoLink,
                        CreatedBy = CurrentUser.Id,
                        ModifiedBy = CurrentUser.Id,
                        DateModified = DateTimeZone.Now
                    };

                    // Добавляем в базу
                    rep.Add(newArticle);
                    rep.SubmitChanges();

                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Создание новой публикации {0}", newArticle.Title));
                }
                else
                {
                    // Ищем и редактируем публикацию
                    var article = rep.Load(model.Id);
                    if (article == null)
                    {
                        throw new ObjectNotFoundException(string.Format("Публикация с идентификатором {0} не найдена", model.Id));
                    }

                    article.Title = model.Title;
                    article.PublicationDate = model.PublicationDate;
                    article.ArticleType = model.ArticleType;
                    article.ShortContent = model.ShortContent;
                    article.FullContent = model.FullContent;
                    article.VideoLink = model.VideoLink;
                    article.DateModified = DateTimeZone.Now;
                    article.ModifiedBy = CurrentUser.Id;

                    // Сохраняем изменения
                    rep.SubmitChanges();

                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Редактирование публикации {0}", article.Title));
                }

                // Выводим успешное сообщение
                UINotificationManager.Success("Изменения успешно сохранены");

                // Отображаем список страниц
                return RedirectToAction("News");
            }
            catch (Exception e)
            {
                UINotificationManager.Error(e.Message);
                return RedirectToAction("News");
            }
        }

        /// <summary>
        /// Удаляет публикации с указанными идентификаторами
        /// </summary>
        /// <param name="articleIds"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/delete-news")]
        public ActionResult DeleteArticles(string articleIds)
        {
            try
            {
                // Репозитории
                var articlesRep = Locator.GetService<IArticlesRepository>();
                var articles =
                    articleIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                        articlesRep.Load).Where(r => r != null).ToList();

                // Удаляем
                foreach (var article in articles)
                {
                    articlesRep.Delete(article);
                }
                articlesRep.SubmitChanges();

                UINotificationManager.Success("Изменения успешно сохранены");

                if (articles.Count > 0)
                {
                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Удаление {0} публикаций", articles.Count));
                }

                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при удалении публикаций для пользователя {0} - {1}",
                                        CurrentUser.Login, e.Message));
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        #endregion

        #region Элементы меню

        /// <summary>
        /// Отображает панеь управления элементами меню
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/menu")]
        public ActionResult Menu()
        {
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Контент", "Управление контентом системы", "/administration/content", true);
            PushNavigationItem("Меню", "Управление главным меню системы", "/administration/content/menu", false);

            // Выбираем элементы
            var items =
                Locator.GetService<IMenuItemsRepository>().FindAll().OrderBy(d => d.Position).ToList();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр панели управления главным меню");

            return View(items);
        }

        /// <summary>
        /// Возвращает аяксом информацию об указанном элементе меню
        /// </summary>
        /// <param name="id">Идентификатор элемента меню</param>
        /// <returns>Информация об элементе в JSON формате</returns>
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/get-menu-item-info/{id}")]
        public ActionResult GetMenuItemInfo(long id)
        {
            try
            {
                // Репозиторий
                var rep = Locator.GetService<IMenuItemsRepository>();

                // Ищем пользователя
                var item = rep.Load(id);
                if (item == null)
                {
                    throw new Exception("Элемент меню не найден");
                }

                // Отдаем информацию
                return Json(new
                {
                    success = true,
                    item.Id,
                    item.Title,
                    item.Href,
                    item.Position
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                var msg = String.Format("Ошибка при получении данных для элемента меню с идентификатором {0}: {1}", id, e.Message);
                Logger.Error(msg);
                Response.StatusCode = 500;
                return Json(new { success = false, message = msg });
            }
        }

        /// <summary>
        /// Сохраняет изменения в существующем элементе меню или создает новый
        /// </summary>
        /// <param name="model">Модель данных для элемента меню</param>
        /// <returns>Переходит на список всех элементов меню</returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageContent)]
        [ValidateInput(false)]
        [Route("administration/content/save-menu-item")]
        public ActionResult SaveMenuItem(MenuItemModel model)
        {
            try
            {
                // Репозиторий
                var rep = Locator.GetService<IMenuItemsRepository>();

                // В зависимости от операции, создаем или редактируем элемент меню
                if (model.Id <= 0)
                {
                    // Создаем элемент меню
                    var newItem = new MenuItem()
                    {
                        Title = model.Title,
                        Href = model.Href,
                        Position = model.Position,
                        DateCreated = DateTimeZone.Now,
                        CreatedBy = CurrentUser.Id,
                        ModifiedBy = CurrentUser.Id,
                        DateModified = DateTimeZone.Now
                    };

                    // Добавляем в базу
                    rep.Add(newItem);
                    rep.SubmitChanges();

                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Создание нового элемента меню {0}", newItem.Title));
                }
                else
                {
                    // Ищем и редактируем элемент меню
                    var item = rep.Load(model.Id);
                    if (item == null)
                    {
                        throw new ObjectNotFoundException(string.Format("Элемент меню с идентификатором {0} не найден", model.Id));
                    }

                    item.Title = model.Title;
                    item.Href = model.Href;
                    item.Position = model.Position;
                    item.DateModified = DateTimeZone.Now;
                    item.ModifiedBy = CurrentUser.Id;

                    // Сохраняем изменения
                    rep.SubmitChanges();

                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Редактирование элемента меню {0}", item.Title));
                }

                // Выводим успешное сообщение
                UINotificationManager.Success("Изменения успешно сохранены");

                // Отображаем список страниц
                return RedirectToAction("Menu");
            }
            catch (Exception e)
            {
                UINotificationManager.Error(e.Message);
                return RedirectToAction("Menu");
            }
        }

        /// <summary>
        /// Удаляет элементы меню с указанными идентификаторами
        /// </summary>
        /// <param name="itemIds"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/delete-menu-items")]
        public ActionResult DeleteMenuItems(string itemIds)
        {
            try
            {
                // Репозитории
                var menuItemsRep = Locator.GetService<IMenuItemsRepository>();
                var items =
                    itemIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                        menuItemsRep.Load).Where(r => r != null).ToList();

                // Удаляем
                foreach (var item in items)
                {
                    menuItemsRep.Delete(item);
                }
                menuItemsRep.SubmitChanges();

                UINotificationManager.Success("Изменения успешно сохранены");

                if (items.Count > 0)
                {
                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Удаление {0} элементов меню", items.Count));
                }

                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при удалении элементов меню для пользователя {0} - {1}",
                                        CurrentUser.Login, e.Message));
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        #endregion

        #region Баннерный блок

        /// <summary>
        /// Отображает панель управления баннерами
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/banners")]
        public ActionResult Banners()
        {
            // TODO: сделать получение разметки баннеров, хранимых гдето в базе данных
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Контент", "Управление контентом системы", "/administration/content", true);
            PushNavigationItem("Баннерный блок", "Управление баннерами на главной странице системы", "/administration/content/banners", false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр панели управления баннерами");

            // Формируем модель
            var model = new BannerContentModel()
                {
                    Markup = Locator.GetService<ISettingsRepository>().GetValue("s_banners")
                };

            return View(model);
        }

        /// <summary>
        /// Отображает панель управления баннерами
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/save-banners")]
        [ValidateInput(false)]
        public ActionResult SaveBanners(BannerContentModel model)
        {
            // Событие аудита
            PushAuditEvent(AuditEventTypes.Editing, "Изменение разметки баннеров");

            Locator.GetService<ISettingsRepository>().SetValue("s_banners", model.Markup);
            Locator.GetService<ISettingsRepository>().SubmitChanges();
            UINotificationManager.Success("Разметка баннеров была успешно изменена");

            return RedirectToAction("Banners");

        }

        #endregion

        #region Рассылки

        /// <summary>
        /// Отображает страницу с созданием новой рассылки
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/notifications")]
        public ActionResult SubscribeNotifications()
        {
            PushAdministrationNavigationItem();
            PushNavigationItem("Контент", "Управление контентом системы", "/administration/content", true);
            PushNavigationItem("Рассылки", "Создание рассылки", "/administration/content/notifications", false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр формы создания рассылки");

            return View();
        }

        /// <summary>
        /// Создает новую рассылку по указанным параметрам
        /// </summary>
        /// <param name="model">Модель данных</param>
        /// <param name="collection">Коллекция</param>
        /// <returns></returns>
        [ValidateInput(false)]
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/create-newsletters")]
        public ActionResult CreateNewsLetters(NewslettersModel model, FormCollection collection)
        {
            // Репозиторий пользователей
            var usersRep = Locator.GetService<IUsersRepository>();
            IList<User> recipients = null;

            switch (model.Recipients)
            {
                case 1:
                    recipients = usersRep.GetActiveUsers().ToList();
                    break;
                case 2:
                    recipients = usersRep.Search(u => u.Role.Name.ToLower().Contains("администратор")).ToList();
                    break;
                case 3:
                    recipients = usersRep.Search(u => u.Role.Name.ToLower().Contains("директор")).ToList();
                    break;
                case 4:
                    recipients = usersRep.Search(u => u.Role.Name.ToLower().Contains("агент")).ToList();
                    break;
                case 5:
                    var subjects = collection.AllKeys.Where(k => k.StartsWith("subject-")).Select(c => Convert.ToInt16(c.Split('-')[1])).ToList();
                    recipients = usersRep.GetActiveUsers().Where(activeUser => subjects.Any(a => (activeUser.Notifications & a) == a)).ToList();
                    break;
                default:
                    recipients = new List<User>();
                    break;
            }

            // Создаем рассылку
            foreach (var recipient in recipients)
            {
                MailNotificationManager.Notify(recipient, model.Subject, model.Content);
            }

            // Событие аудита
            //PushAuditEvent(AuditEventTypes.Editing, string.Format("Создание рассылки для {0} пользователей", recipients.Count));

            UINotificationManager.Success(string.Format("Рассылка для {0} пользователей была успешно создана и помещена в очередь отправки", recipients.Count));

            return RedirectToAction("SubscribeNotifications");
        }

        #endregion

        #region Магазин литературы

        /// <summary>
        /// Отображает страницу управления книгами системы
        /// </summary>
        /// <returns></returns>
        [Route("administration/content/books")]
        [AuthorizationCheck()]
        public ActionResult Books()
        {
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Книги", "Управление книгами учебного центра", "/administration/content/books", false);

            // Выбираем страницы
            var books =
                Locator.GetService<IBooksRepository>().FindAll().OrderByDescending(d => d.DateModified).
                    ThenByDescending(d => d.DateCreated).ToList();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр панели управления книгами учебного центра системы");

            return View(books);
        }

        /// <summary>
        /// Отображает форму создания новой книги
        /// </summary>
        /// <returns></returns>
        [Route("administration/content/add-book")]
        [AuthorizationCheck()]
        public ActionResult AddBook()
        {
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Контент", "Управление контентом системы", "/administration/content");
            PushNavigationItem("Новая книги", "Создание новой книги в системе", "/administration/content/add-book", false);

            return View("EditBook", new Book() { Id = -1 });
        }

        /// <summary>
        /// Отображает форму редактирования указанной книги
        /// </summary>
        /// <param name="id">Идентификатор статической страницы</param>
        /// <returns>Страница редактирования</returns>
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/edit-book/{id}")]
        public ActionResult EditBook(long id)
        {
            // Репозиторий
            var rep = Locator.GetService<IBooksRepository>();

            // Загружаем страницу
            var book = rep.Load(id);
            if (book == null)
            {
                UINotificationManager.Error(string.Format("Книга с идентификатором {0} не найдена", id));
                return RedirectToAction("Books");
            }

            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Контент", "Управление контентом системы", "/administration/content");
            PushNavigationItem("Редактирование книги " + book.Title, "", "/administration/content/edit-book/" + book.Id, false);

            return View("EditBook", book);
        }

        /// <summary>
        /// Сохраняет изменения в существующей книге или создает новую
        /// </summary>
        /// <param name="model">Модель данных для книги</param>
        /// <returns>Переходит на список всех книг</returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageContent)]
        [ValidateInput(false)]
        [Route("administration/content/save-book")]
        public ActionResult SaveBook(Book model)
        {
            try
            {
                // Репозиторий
                var rep = Locator.GetService<IBooksRepository>();
                var filesRep = Locator.GetService<IStoredFilesRepository>();

                var file = Request.Files["PictureFile"];
                bool fileSubmitted = false;
                string filename = null;
                if (file != null && file.ContentLength > 0 && file.ContentType.ToLower().Contains("image"))
                {
                    var savedFile = filesRep.SavePostedFile(file, "Books");
                    fileSubmitted = true;
                    filename = savedFile.GetURI();
                }

                // В зависимости от операции, создаем или редактируем страницу
                if (model.Id <= 0)
                {
                    model.DateCreated = DateTimeZone.Now;
                    if (fileSubmitted)
                    {
                        model.Picture = filename;
                    }

                    // Добавляем в базу
                    rep.Add(model);
                    rep.SubmitChanges();



                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Создание новой книги {0}", model.Title));
                }
                else
                {
                    // Ищем и редактируем страницу
                    var book = rep.Load(model.Id);
                    if (book == null)
                    {
                        throw new ObjectNotFoundException(string.Format("Книга с идентификатором {0} не найдена", model.Id));
                    }

                    book.Title = model.Title;
                    book.Publisher = model.Publisher;
                    book.Price = model.Price;
                    book.Author = model.Author;
                    book.Description = model.Description;
                    if (fileSubmitted)
                    {
                        book.Picture = filename;
                    }

                    book.DateModified = DateTimeZone.Now;

                    // Сохраняем изменения
                    rep.SubmitChanges();

                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Редактирование книги {0}", book.Title));
                }

                // Выводим успешное сообщение
                UINotificationManager.Success("Изменения успешно сохранены");

                // Отображаем список страниц
                return RedirectToAction("Books");
            }
            catch (Exception e)
            {
                UINotificationManager.Error(e.Message);
                return RedirectToAction("Books");
            }
        }

        /// <summary>
        /// Удаляет книги с указанными идентификаторами
        /// </summary>
        /// <param name="bookIds"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/delete-books")]
        public ActionResult DeleteBooks(string bookIds)
        {
            try
            {
                // Репозитории
                var pagesRep = Locator.GetService<IBooksRepository>();
                var books =
                    bookIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                        pagesRep.Load).Where(r => r != null).ToList();

                // Удаляем
                foreach (var book in books)
                {
                    pagesRep.Delete(book);
                }
                pagesRep.SubmitChanges();

                UINotificationManager.Success("Изменения успешно сохранены");

                if (books.Count > 0)
                {
                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Удаление {0} статических страниц", books.Count));
                }

                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при удалении книг для пользователя {0} - {1}",
                                        CurrentUser.Login, e.Message));
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        #endregion

        #region Партнеры

        /// <summary>
        /// Отображает страницу управления партнерами
        /// </summary>
        /// <returns></returns>
        [Route("administration/content/partners")]
        [AuthorizationCheck(Permission.ManageContent)]
        public ActionResult Partners()
        {
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Партнеры", "Управление партнерами", "/administration/content/partners", false);

            // Выбираем страницы
            var partners =
                Locator.GetService<IPartnersRepository>().FindAll().OrderByDescending(d => d.DateModified).
                    ThenByDescending(d => d.DateCreated).ToList();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр панели управления партнерами системы");

            return View(partners);
        }

        /// <summary>
        /// Отображает форму создания нового партнера
        /// </summary>
        /// <returns></returns>
        [Route("administration/content/add-partner")]
        [AuthorizationCheck(Permission.ManageContent)]
        public ActionResult AddPartner()
        {
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Контент", "Управление контентом системы", "/administration/content");
            PushNavigationItem("Новый партнер", "Создание нового партнера в системе", "/administration/content/add-partner", false);

            return View("EditPartner", new Partner() { Id = -1 });
        }

        /// <summary>
        /// Отображает форму редактирования указанной книги
        /// </summary>
        /// <param name="id">Идентификатор статической страницы</param>
        /// <returns>Страница редактирования</returns>
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/edit-partner/{id}")]
        public ActionResult EditPartner(long id)
        {
            // Репозиторий
            var rep = Locator.GetService<IPartnersRepository>();

            // Загружаем страницу
            var partner = rep.Load(id);
            if (partner == null)
            {
                UINotificationManager.Error(string.Format("Партнер с идентификатором {0} не найден", id));
                return RedirectToAction("Partners");
            }

            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Контент", "Управление контентом системы", "/administration/content");
            PushNavigationItem("Редактирование партнера " + partner.Name, "", "/administration/content/edit-partner/" + partner.Id, false);

            return View("EditPartner", partner);
        }

        /// <summary>
        /// Сохраняет изменения в существующем партнере либо создает нового
        /// </summary>
        /// <param name="model">Модель данных для книги</param>
        /// <returns>Переходит на список всех книг</returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageContent)]
        [ValidateInput(false)]
        [Route("administration/content/save-partner")]
        public ActionResult SavePartner(Partner model)
        {
            try
            {
                // Репозиторий
                var rep = Locator.GetService<IPartnersRepository>();
                var filesRep = Locator.GetService<IStoredFilesRepository>();

                var inactiveFile = Request.Files["InactiveImage"];
                bool inactiveSumitted = false;
                string inactiveFileName = null;
                if (inactiveFile != null && inactiveFile.ContentLength > 0 && inactiveFile.ContentType.ToLower().Contains("image"))
                {
                    var savedFile = filesRep.SavePostedFile(inactiveFile, "Partners");
                    inactiveSumitted = true;
                    inactiveFileName = savedFile.GetURI();
                }

                var activeFile = Request.Files["ActiveImage"];
                bool activeSubmitted = false;
                string activeFilename = null;
                if (activeFile != null && activeFile.ContentLength > 0 && activeFile.ContentType.ToLower().Contains("image"))
                {
                    var savedFile = filesRep.SavePostedFile(activeFile, "Partners");
                    activeSubmitted = true;
                    activeFilename = savedFile.GetURI();
                }

                // В зависимости от операции, создаем или редактируем страницу
                if (model.Id <= 0)
                {
                    model.DateCreated = DateTimeZone.Now;
                    if (inactiveSumitted)
                    {
                        model.InactiveImageUrl = inactiveFileName;
                    }
                    if (activeSubmitted)
                    {
                        model.ActiveImageUrl = activeFilename;
                    }

                    // Добавляем в базу
                    rep.Add(model);
                    rep.SubmitChanges();

                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Создание нового партнера {0}", model.Name));
                }
                else
                {
                    // Ищем и редактируем страницу
                    var partner = rep.Load(model.Id);
                    if (partner == null)
                    {
                        throw new ObjectNotFoundException(string.Format("Партнер с идентификатором {0} не найден", model.Id));
                    }

                    partner.Name = model.Name;
                    partner.Url = model.Url;
                    partner.Position = model.Position;
                    if (inactiveSumitted)
                    {
                        partner.InactiveImageUrl = inactiveFileName;
                    }
                    if (activeSubmitted)
                    {
                        partner.ActiveImageUrl = activeFilename;
                    }

                    partner.DateModified = DateTimeZone.Now;

                    // Сохраняем изменения
                    rep.SubmitChanges();

                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Редактирование партнера {0}", partner.Name));
                }

                // Выводим успешное сообщение
                UINotificationManager.Success("Изменения успешно сохранены");

                // Отображаем список страниц
                return RedirectToAction("Partners");
            }
            catch (Exception e)
            {
                UINotificationManager.Error(e.Message);
                return RedirectToAction("Partners");
            }
        }

        /// <summary>
        /// Удаляет книги с указанными идентификаторами
        /// </summary>
        /// <param name="partnerIds"></param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageContent)]
        [Route("administration/content/delete-partners")]
        public ActionResult DeletePartners(string partnerIds)
        {
            try
            {
                // Репозитории
                var pagesRep = Locator.GetService<IPartnersRepository>();
                var partners =
                    partnerIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                        pagesRep.Load).Where(r => r != null).ToList();

                // Удаляем
                foreach (var partner in partners)
                {
                    pagesRep.Delete(partner);
                }
                pagesRep.SubmitChanges();

                UINotificationManager.Success("Изменения успешно сохранены");

                if (partners.Count > 0)
                {
                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Удаление {0} партнеров", partners.Count));
                }

                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при удалении партнеров для пользователя {0} - {1}",
                                        CurrentUser.Login, e.Message));
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        #endregion

        #region Менеджер файлов

        /// <summary>
        /// Отображает страницу с менеджером файлов для хостинга файлов на сервере
        /// </summary>
        /// <returns></returns>
        [Route("administration/content/file-mgr")][AuthorizationCheck(Permission.ManageContent)]
        public ActionResult FileManager()
        {
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Контент", "Управление контентом системы", "/administration/content", false);
            PushNavigationItem("Файловый менеджер", "Управление контентом системы", "/administration/content/file-mgr", false);

            return View();
        }

        /// <summary>
        /// Коннектор для обработки запросов из поступившего ElFinder
        /// </summary>
        /// <returns></returns>
        [Route("administration/content/file-connector")][AuthorizationCheck(Permission.ManageContent)]
        public ActionResult FileManagerConnector()
        {
            return Connector.Process(this.HttpContext.Request);
        }

        /// <summary>
        /// Файловый коннектор
        /// </summary>
        private Connector _connector;

        /// <summary>
        /// Файловый коннектор для обеспечения связи
        /// </summary>
        public Connector Connector
        {
            get
            {
                if (_connector == null)
                {
                    FileSystemDriver driver = new FileSystemDriver();
                    DirectoryInfo thumbsStorage = new DirectoryInfo(Server.MapPath("~/Files/Thumbs"));
                    driver.AddRoot(new Root(new DirectoryInfo(Server.MapPath("~/Files")))
                    {
                        IsLocked = false,
                        IsReadOnly = false,
                        IsShowOnly = false,
                        ThumbnailsStorage = thumbsStorage,
                        ThumbnailsUrl = "/Administration/Thumbs/"
                    });
                    driver.AddRoot(new Root(new DirectoryInfo(Server.MapPath("~/Templates")))
                    {
                        IsLocked = false,
                        IsReadOnly = false,
                        IsShowOnly = false,
                        ThumbnailsStorage = thumbsStorage,
                        ThumbnailsUrl = "/Administration/Thumbs/"
                    });

                    _connector = new Connector(driver);
                }
                return _connector;
            }
        }

        /// <summary>
        /// Экшн вызываемый при клике на файле
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [Route("administration/content/select-file")]
        [AuthorizationCheck(Permission.ManageContent)]
        public ActionResult SelectFile(string target)
        {
            return Json(Connector.GetFileByHash(target).FullName);
        }

        /// <summary>
        /// Инструмент предварительного просмотра
        /// </summary>
        /// <param name="tmb"></param>
        /// <returns></returns>
        public ActionResult Thumbs(string tmb)
        {
            return Connector.GetThumbnail(Request, Response, tmb);
        }


        #endregion

        #endregion

        #region Аудит системы

        /// <summary>
        /// Отображает события аудита системы по указанным критериям
        /// </summary>
        /// <param name="eventTypes">Типы событий</param>
        /// <param name="users">Идентификаторы пользователей</param>
        /// <param name="companies">Идентификаторы компаний</param>
        /// <param name="startDate">Дата начала интервала событий отображения, по умолчанию показываются все события за прошедший месяц</param>
        /// <param name="endDate">Дата окончания интервала событий, по умолчанию отображаются все события за последний месяц</param>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageSystemEvents)]
        [Route("administration/audit")]
        public ActionResult Audit(string eventTypes = null, string users = null, string companies = null, string startDate = null, string endDate = null)
        {
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Аудит системы", "Страница с событиями аудита системы", "/administration/audit", false);

            // События аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр страница аудита системы");

            // Устанавливаем диапазон по датам
            DateTime _startDate, _endDate;
            if (startDate == null)
            {
                _startDate = DateTimeZone.Now.AddMonths(-1);
            }
            else
            {
                _startDate = DateTime.ParseExact(startDate, "dd.MM.yyyy", null);
            }
            if (endDate == null)
            {
                _endDate = DateTimeZone.Now;
            }
            else
            {
                _endDate = DateTime.ParseExact(endDate, "dd.MM.yyyy", null);
            }

            // Выбираем все события попадающие в указанный диапазон
            var events = AuditManager.GetAllEvents().Where(e => e.EventDate >= _startDate & e.EventDate <= _endDate);

            // Контейнеры отфильтрованных идентификаторов
            IList<AuditEventTypes> eventTypesIds = null;
            IList<long> companiesIds = null, usersIds = null;

            // Проверяем нужно ли фильтровать по типу события
            if (!String.IsNullOrEmpty(eventTypes))
            {
                eventTypesIds = eventTypes.Split(',').Where(s => !String.IsNullOrEmpty(s)).Select(e => Convert.ToInt16(e)).Select(i => (AuditEventTypes)i).ToList();
                events = events.Where(e => eventTypesIds.Contains(e.EventType));
            }

            // Проверяем, нужно ли фильтровать по компаниям
            if (!String.IsNullOrEmpty(companies))
            {
                companiesIds = companies.Split(',').Where(s => !String.IsNullOrEmpty(s)).Select(i => Convert.ToInt64(i)).ToList();
                events = events.Where(e => companiesIds.Contains(e.User.CompanyId));
            }

            // Проверяем, нужно ли фильтровать по пользователям
            if (!String.IsNullOrEmpty(users))
            {
                usersIds = users.Split(',').Where(s => !String.IsNullOrEmpty(s)).Select(i => Convert.ToInt64(i)).ToList();
                events = events.Where(e => usersIds.Contains(e.UserId));
            }

            // Формируем модель
            var model = new AuditViewModel()
                            {
                                Events = events.ToList(),
                                FilterPeriodStartDate = _startDate,
                                FilterPeriodEndDate = _endDate,
                                FilterEvents = eventTypesIds,
                                FilterCompaniesIds = companiesIds,
                                FilterUsersIds = usersIds,
                                AllUsers = Locator.GetService<IUsersRepository>().GetActiveUsers().ToList(),
                                AllCompanies = Locator.GetService<ICompaniesRepository>().GetActiveCompanies().ToList()
                            };

            // Отображаем вид
            return View(model);
        }

        /// <summary>
        /// Возвращает расширенную JSON информацию по указанному событию аудита
        /// </summary>
        /// <param name="id">Идентификатор события</param>
        /// <returns>JSON информация по указанному событию</returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ManageSystemEvents)]
        [Route("administration/audit/details/")]
        public ActionResult GetAuditEventDetails(long id)
        {
            try
            {
                // Ищем событие
                var auditEvent = AuditManager.FindEvent(id);
                if (auditEvent == null)
                {
                    throw new Exception(string.Format("Событие с идентификатором {0} не найдено", id));
                }

                // Форматируем
                var jObject = JObject.Parse(auditEvent.AdditionalInformation);

                // Событие аудита 
                PushAuditEvent(AuditEventTypes.ViewPage, String.Format("Просмотр подробностей про событие с идентификатором {0}", id));

                // Отдаем
                return Content(jObject.ToString(Formatting.Indented, null));
            }
            catch (Exception e)
            {
                var message = String.Format("Ошибка в ходе получения данных по событию аудита: {0}", e.Message);
                Logger.Error(message);
                return Content(message);
            }
        }

        #endregion

        #region Статистика

        /// <summary>
        /// Отображает страницу со статистической информацией по работе системы
        /// </summary>
        /// <returns></returns>
        [Route("administration/statistics")]
        [AuthorizationCheck(Permission.ManageStatistics)]
        public ActionResult Statistics()
        {
            // Навигационные элементы
            PushAdministrationNavigationItem();
            PushNavigationItem("Статистика", "Статистика по работе системы", "/administration/statistics", false);

            // Репозитории
            var usersRep = Locator.GetService<IUsersRepository>();
            var objectsRep = Locator.GetService<IEstateObjectsRepository>();
            var companiesRep = Locator.GetService<ICompaniesRepository>();
            var dictionariesRep = Locator.GetService<IDictionariesRepository>();
            var clientsRep = Locator.GetService<IClientsRepository>();
            var auditRep = Locator.GetService<IAuditEventsRepository>();
            var filesRep = Locator.GetService<IStoredFilesRepository>();
            var paymentsRep = Locator.GetService<IPaymentsRepository>();
            var servicesRep = Locator.GetService<IServiceTypesRepository>();

            // Собираем элементы статистики
            var model = new List<StatisticItem>()
                {
                    new StatisticItem("Объектов в системе",objectsRep.FindAll().Count().ToString()),
                    new StatisticItem("Компаний в системе",companiesRep.FindAll().Count().ToString()),
                    new StatisticItem("Пользователей в системе",usersRep.FindAll().Count().ToString()),
                    new StatisticItem("Клиентов в системе",clientsRep.FindAll().Count().ToString()),
                    new StatisticItem("Справочников",dictionariesRep.FindAll().Count().ToString()),
                    new StatisticItem("Значений в справочниках",dictionariesRep.DictionaryValuesRepository.FindAll().Count().ToString()),
                    new StatisticItem("Событий аудита",auditRep.FindAll().Count().ToString()),
                    new StatisticItem("Файлов в хранилище",filesRep.FindAll().Count().ToString()),
                    new StatisticItem("Суммарный размер файлов",filesRep.FindAll().Sum(f => f.ContentSize).ToFileSize()),
                    new StatisticItem("Среднее время продажи объекта, дней",objectsRep.Search(o => o.ObjectChangementProperties.DealDate.HasValue).Average(eo => (eo.ObjectChangementProperties.DealDate.Value - (eo.ObjectChangementProperties.DateRegisted??eo.DateCreated).Value).Days).ToString("0.00")),
                    new StatisticItem("Количество платных услуг",servicesRep.FindAll().Count().ToString()),
                    new StatisticItem("Количество активных услуг",servicesRep.Search(a => a.Tax > 0).Count().ToString()),
                    /*new StatisticItem("Всего платежей",paymentsRep.FindAll().Count(p => p.Payed).ToString()),
                    new StatisticItem("Начислений",paymentsRep.FindAll().Count(p => p.Payed && p.Direction == (short)PaymentDirection.Income).ToString()),
                    new StatisticItem("Списаний",paymentsRep.FindAll().Count(p => p.Payed && p.Direction == (short)PaymentDirection.Outcome).ToString()),
                    new StatisticItem("Зачислено на счета, рублей",paymentsRep.FindAll().Where(p => p.Payed && p.Direction == (short)PaymentDirection.Income).Sum(p => p.Amount).ToString("0.0")),
                    new StatisticItem("Потрачено пользователями, рублей",paymentsRep.FindAll().Where(p => p.Payed && p.Direction == (short)PaymentDirection.Outcome).Sum(p => p.Amount).ToString("0.0")),
                    new StatisticItem("Самое большое начисление, рублей",paymentsRep.FindAll().Where(p => p.Payed && p.Direction == (short)PaymentDirection.Income).Max(p => p.Amount).ToString("0.0")),
                    new StatisticItem("Самое маленькое начисление, рублей",paymentsRep.FindAll().Where(p => p.Payed && p.Direction == (short)PaymentDirection.Income).Min(p => p.Amount).ToString("0.0")),
                    new StatisticItem("Средний размер начисления, рублей",paymentsRep.FindAll().Where(p => p.Payed && p.Direction == (short)PaymentDirection.Income).Average(p => p.Amount).ToString("0.0")),*/
                };

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр статистики системы");

            return View(model);
        }

        #endregion

        #region Системные утилиты

        /// <summary>
        /// Отображает панель с системными инструментами, которые можно запустить либо вручную, либо по расписанию
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageTools)]
        [Route("administration/tools")]
        public ActionResult Tools()
        {
            // Навигационные элементы
            PushAdministrationNavigationItem();
            PushNavigationItem("Инструменты", "Системные инструменты, выполняющие системные операции в системе", "/administration/statistics", false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр системных инструментов");

            // Отображаем стандартный вью
            return View(Locator.GetService<IToolsManager>().Tools);
        }

        #endregion

        #region Настройки

        /// <summary>
        /// Отображает страницу с настройками работы системы
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManageSettings)]
        [Route("administration/settings")]
        public ActionResult Settings()
        {
            // Навигационные элементы
            PushAdministrationNavigationItem();
            PushNavigationItem("Настройки", "Настройки работы системы", "/administration/statistics", false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр настроек системы");

            // Отображаем стандартный вью
            return View();
        }

        #endregion

        #region Платежи

        /// <summary>
        /// Доступ к панели управления платежами системы.
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ManagePayments)]
        [Route("administration/payments")]
        public ActionResult Payments(string eventTypes = null, string users = null, string companies = null, string startDate = null, string endDate = null)
        {
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Платежи", "Страница с платежами в системе", "/administration/payments", false);

            // События аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр страница платежей системы");

            // Устанавливаем диапазон по датам
            DateTime _startDate, _endDate;
            if (startDate == null)
            {
                _startDate = DateTimeZone.Now.AddMonths(-1);
            }
            else
            {
                _startDate = DateTime.ParseExact(startDate, "dd.MM.yyyy", null);
            }
            if (endDate == null)
            {
                _endDate = DateTimeZone.Now;
            }
            else
            {
                _endDate = DateTime.ParseExact(endDate, "dd.MM.yyyy", null);
            }

            // Выбираем все события попадающие в указанный диапазон
            var events = Locator.GetService<IPaymentsRepository>().FindAll().Where(e => e.DateCreated >= _startDate & e.DateCreated <= _endDate);

            // Контейнеры отфильтрованных идентификаторов
            IList<PaymentDirection> directions = null;
            IList<long> companiesIds = null, usersIds = null;

            // Проверяем нужно ли фильтровать по типу события
            if (!String.IsNullOrEmpty(eventTypes))
            {
                directions = eventTypes.Split(',').Where(s => !String.IsNullOrEmpty(s)).Select(e => Convert.ToInt16(e)).Select(i => (PaymentDirection)i).ToList();
                events = events.Where(e => directions.Contains((PaymentDirection)e.Direction));
            }

            // Проверяем, нужно ли фильтровать по компаниям
            if (!String.IsNullOrEmpty(companies))
            {
                companiesIds = companies.Split(',').Where(s => !String.IsNullOrEmpty(s)).Select(i => Convert.ToInt64(i)).ToList();
                events = events.Where(e => companiesIds.Contains(e.CompanyId));
            }

            // Проверяем, нужно ли фильтровать по пользователям
            if (!String.IsNullOrEmpty(users))
            {
                usersIds = users.Split(',').Where(s => !String.IsNullOrEmpty(s)).Select(i => Convert.ToInt64(i)).ToList();
                events = events.Where(e => usersIds.Contains(e.UserId));
            }

            // Формируем модель
            var model = new PaymentsViewModel()
            {
                Events = events.ToList(),
                FilterPeriodStartDate = _startDate,
                FilterPeriodEndDate = _endDate,
                FilterEvents = directions,
                FilterCompaniesIds = companiesIds,
                FilterUsersIds = usersIds,
                AllUsers = Locator.GetService<IUsersRepository>().GetActiveUsers().ToList(),
                AllCompanies = Locator.GetService<ICompaniesRepository>().GetActiveCompanies().ToList()
            };

            // Отображаем вид
            return View(model);
        }

        #endregion

        #region Услуги

        /// <summary>
        /// Отображает страницу управления услуг
        /// </summary>
        /// <returns></returns>
        [Route("administration/services")]
        [AuthorizationCheck(Permission.ManageServices)]
        public ActionResult Services()
        {
            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Услуги", "Страница управления услуг", "/administration/services", false);

            // События аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр страницы управления услугами");

            // Репозиторий
            var rep = Locator.GetService<IServiceTypesRepository>();
            var services = rep.FindAll().OrderByDescending(d => d.DateModified).ThenBy(d => d.DateCreated).ToList();

            // Отдаем вид
            return View(services);
        }

        /// <summary>
        /// Отображает форму добавления новой услуги
        /// </summary>
        /// <returns></returns>
        [Route("administration/services/add")]
        [AuthorizationCheck(Permission.ManageServices)]
        public ActionResult AddServices()
        {

            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Услуги", "Страница управления услуг", "/administration/services", true);
            PushNavigationItem("Создание услуги", "Страница создания новой услуги", "/administration/services/add", false);

            // События аудита
            PushAuditEvent(AuditEventTypes.Editing, "Начало создания новой услуги");

            // Подгтавливаем пользователей и компании
            var companiesRep = Locator.GetService<ICompaniesRepository>();
            var usersRep = Locator.GetService<IUsersRepository>();
            var companies = companiesRep.FindAll().ToList();
            ViewBag.users = usersRep.GetActiveUsers().ToList();
            ViewBag.cities = Locator.GetService<IGeoManager>().CitiesRepository.FindAll().OrderBy(c => c.Name).ToList();

            // Отображаем вид
            return View();
        }

        /// <summary>
        /// Обрабатывает изменение услуги или создание новой услуги
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("administration/services/save-service")]
        [HttpPost]
        [AuthorizationCheck(Permission.ManageServices)]
        public ActionResult SaveService(ServiceType model)
        {
            // Репозитории
            var filesRep = Locator.GetService<IStoredFilesRepository>();
            var rep = Locator.GetService<IServiceTypesRepository>();
            var compRep = Locator.GetService<ICompaniesRepository>();

            // Сохраняем файлы
            var examplesFile = Request.Files["ExamplesImage"];
            var scanFile = Request.Files["ContractScanImage"];
            var exsubmitted = false;
            var exFilename = "";
            var scanSubmitted = false;
            var scanFilename = "";
            if (examplesFile != null && examplesFile.ContentLength > 0)
            {
                var file = filesRep.SavePostedFile(examplesFile, "ServiceExamples");
                exsubmitted = true;
                exFilename = file.GetURI();
            }
            if (scanFile != null && scanFile.ContentLength > 0)
            {
                var file = filesRep.SavePostedFile(examplesFile, "ServiceContactScans");
                scanSubmitted = true;
                scanFilename = file.GetURI();
            }

            // Проверяем что у нас
            if (model.Id <= 0)
            {
                // Создание
                model.DateCreated = DateTimeZone.Now;
                model.ServiceStatus = 1;
                model.CreatedBy = CurrentUser.Id;

                if (exsubmitted)
                {
                    model.Examples = exFilename;
                }
                if (scanSubmitted)
                {
                    model.ContractScan = scanFilename;
                }

                rep.Add(model);
                rep.SubmitChanges();

                UINotificationManager.Success("Услуга была успешно создана");

                // События аудита
                PushAuditEvent(AuditEventTypes.Editing, "Создание услуги");
            }
            else
            {
                // Редактирование
                var serv = rep.Load(model.Id);
                if (serv == null)
                {
                    UINotificationManager.Error("Услуга с таким идентификатором не найдена");
                    return RedirectToAction("Services");
                }

                // Редактируем
                serv.ServiceName = model.ServiceName;
                serv.Tax = model.Tax;
                if (model.ProvidedId != serv.ProvidedId)
                {
                    serv.Company = compRep.Load(model.ProvidedId);
                }
                serv.Measure = model.Measure;
                serv.Subject = model.Subject;
                serv.Description = model.Description;
                serv.Geo = model.Geo;
                serv.RDVShare = model.RDVShare;
                serv.ContractDate = model.ContractDate;
                serv.ContractNumber = model.ContractNumber;

                if (exsubmitted)
                {
                    serv.Examples = exFilename;
                }
                if (scanSubmitted)
                {
                    serv.ContractScan = scanFilename;
                }

                serv.DateModified = model.DateModified;
                serv.ModifiedBy = CurrentUser.Id;
                rep.SubmitChanges();

                UINotificationManager.Success("Услуга была успешно отредактирована");

                // События аудита
                PushAuditEvent(AuditEventTypes.Editing, "Редактирование услуги");
            }

            // Отдаем вид
            return RedirectToAction("Services");
        }

        /// <summary>
        /// Отображает форму редактирования указанной услуги
        /// </summary>
        /// <param name="id">Идентификатор услуги</param>
        /// <returns></returns>
        [Route("administration/services/edit/{id}")]
        [AuthorizationCheck(Permission.ManageServices)]
        public ActionResult EditService(long id)
        {
            // Репозиторий
            var rep = Locator.GetService<IServiceTypesRepository>();

            // Ищем
            var serv = rep.Load(id);
            if (serv == null)
            {
                return RedirectToAction("Services");
            }

            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Услуги", "Страница управления услуг", "/administration/services", true);
            PushNavigationItem("Редактирование услуги " + serv.ServiceName, "Страница редактировани услуги", "/administration/services/edit/" + id, false);

            // События аудита
            PushAuditEvent(AuditEventTypes.Editing, "Начало редактирования услуги " + serv.ServiceName);

            // Подгтавливаем пользователей и компании
            var companiesRep = Locator.GetService<ICompaniesRepository>();
            var usersRep = Locator.GetService<IUsersRepository>();
            var companies = companiesRep.FindAll().ToList();
            ViewBag.users = usersRep.GetActiveUsers().ToList();
            ViewBag.cities = Locator.GetService<IGeoManager>().CitiesRepository.FindAll().OrderBy(c => c.Name).ToList();

            // Отображаем вид
            return View(serv);
        }

        /// <summary>
        /// Обрабатывает изменение статуса у услуг
        /// </summary>
        /// <param name="serviceIds">Идентификаторы услуг</param>
        /// <param name="newStatus">Новый статус</param>
        /// <returns></returns>
        [Route("administration/service/change-status")]
        [HttpPost]
        [AuthorizationCheck(Permission.ManageServices)]
        public ActionResult ChangeServiceStatus(string serviceIds, short newStatus)
        {
            // Репозиторий
            var rep = Locator.GetService<IServiceTypesRepository>();

            // Выбираем список объектов
            var services =
                serviceIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => Convert.ToInt64(s)).
                    Select(rep.Load).Where(o => o != null).ToList();

            // Устанавливаем
            foreach (var service in services)
            {
                service.ServiceStatus = newStatus;
            }

            //  Сохраняет
            rep.SubmitChanges();

            // События аудита
            PushAuditEvent(AuditEventTypes.Editing, "Изменение статуса для " + services.Count + " услуг");

            UINotificationManager.Success("Изменения успешно сохранены");

            return RedirectToAction("Services");
        }

        /// <summary>
        /// Отображает таблицу логов по указанной услуге
        /// </summary>
        /// <param name="id">Идентификатор услуги</param>
        /// <returns></returns>
        [Route("administration/services/logs/{id}")]
        [AuthorizationCheck(Permission.ManageServices)]
        public ActionResult ServiceLogs(long id)
        {
            // Репозиторий
            var rep = Locator.GetService<IServiceTypesRepository>();

            // Ищем услугу
            var serv = rep.Load(id);
            if (serv == null)
            {
                return RedirectToAction("Services");
            }

            ViewBag.service = serv;

            // Отображаем хлебную крошку
            PushAdministrationNavigationItem();
            PushNavigationItem("Услуги", "Страница управления услуг", "/administration/services", true);
            PushNavigationItem("Просмотр логов услуги " + serv.ServiceName, "Просмотр логов услуги", "/administration/services/edit/" + id, false);

            // События аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр логов вызова услуги " + serv.ServiceName);

            return View(serv.ServiceLogItems.OrderByDescending(d => d.OrderDate).ToList());
        }

        /// <summary>
        /// Возвращает данные для автокомплита имен услуг
        /// </summary>
        /// <param name="term">Начальная фраза</param>
        /// <returns></returns>
        [Route("administration/services/names")]
        [AuthorizationCheck(Permission.ManageServices)]
        public ActionResult ServicesName(string term)
        {
            // Ищем
            var rep = Locator.GetService<IServiceTypesRepository>();

            var items =
                rep.Search(i => i.ServiceName.ToLower().Contains(term.ToLower())).Select(i => i.ServiceName).ToList();

            return Json(items, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
