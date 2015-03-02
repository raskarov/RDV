using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NLog;
using RDV.Domain.DAL;
using RDV.Domain.Enums;
using RDV.Domain.ImportExport;
using RDV.Domain.Infrastructure;
using RDV.Domain.Infrastructure.Routing;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.Interfaces.Repositories.Content;
using RDV.Domain.IoC;
using RDV.Web.Classes;
using RDV.Domain.Core;

namespace RDV.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication: HttpApplication
    {
        /// <summary>
        /// Текущий логгер
        /// </summary>
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Регистрация глобальных фильтров
        /// </summary>
        /// <param name="filters">Коллекция фильтров</param>
        private static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        /// <summary>
        /// Регистрация роутов
        /// </summary>
        /// <param name="routes">Роуты</param>
        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Регистрирует роуты экшенов
            RoutesManager.RegisterActionRoutes();

            // Регистрируем роуты статических страниц
            using (var scope = Locator.BeginNestedHttpRequestScope())
            {
                // Перебираем все статические страницы и регистрируем роуты в них
                foreach (var staticPage in Locator.GetService<IStaticPagesRepository>().FindAll())
                {
                    RoutesManager.RegisterRoute("Static-page-"+staticPage.Id,staticPage.Route,new {controller = "Pages", Action = "ViewPage", id = staticPage.Id});
                }
            }

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Main", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        /// <summary>
        /// Старт приложения
        /// </summary>
        protected void Application_Start()
        {
            IoCInitialization();
            MvcInitialization();
        }

        /// <summary>
        /// Начала сессии пользователя
        /// </summary>
        protected void Session_Start()
        {
            using (var httpRequestScope = Locator.BeginNestedHttpRequestScope())
            {
                // Контекст
                var context = HttpContext.Current;

                // Ищем авторизационную куку
                var authCookie = context.Request.Cookies["auth"];
                if (authCookie != null)
                {
                    var identity = authCookie["identity"];
                    var pass = authCookie["pass"];

                    var repository = Locator.GetService<IUsersRepository>();
                    var user = repository.GetUserByLoginAndPasswordHash(identity, pass);
                    if (user != null)
                    {
                        context.Session["CurrentUser"] = user.Id;
                        user.LastLogin = DateTimeZone.Now;
                        repository.SubmitChanges();
                        _logger.Info(String.Format("Начало сессии пользователя {0}", identity));
                        Locator.GetService<IAuditManager>().PushEvent(user,AuditEventTypes.System, string.Format("Начало сессии пользователем {0}", identity),httpRequest: context.Request);
                    }
                    else
                    {
                        _logger.Warn(String.Format("Попытка взлома сессии пользователя {0}", identity));
                        context.Response.Cookies.Remove("auth");
                    }
                }    
            }
        }

        /// <summary>
        /// Инициализация Mvc инфраструктуры
        /// </summary>
        private void MvcInitialization()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        /// <summary>
        /// Иницализация IoC контейнера
        /// </summary>
        private void IoCInitialization()
        {
            Locator.Init(new DataAccessLayer(),new InfrastructureLayer(),new WebLayer(),new ImportExportLayer());
            Locator.GetService<IMailNotificationManager>().Init();
            Locator.GetService<ISMSNotificationManager>().Init();
            Locator.GetService<IToolsManager>().Init();
        }
    }
}