using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Mailing;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Classes.Navigation;
using RDV.Domain.Core;

namespace RDV.Web.Controllers
{
    /// <summary>
    /// Базовый контроллер содержащий общую для всех контроллеров функциональность
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        protected BaseController()
        {
            // Хранилище для хлебной крошки
            NavigationChainItems = new List<NavigationChainItem>();
            PushNavigationItem("Главная","Главная страница сайта","/",true);
            AuditManager = Locator.GetService<IAuditManager>();
            UINotificationManager = Locator.GetService<IUINotificationManager>();
            MailNotificationManager = Locator.GetService<IMailNotificationManager>();
        }

        #region Основные менеджеры

        /// <summary>
        /// Менеджер оповещений по электронной почте
        /// </summary>
        protected IMailNotificationManager MailNotificationManager { get; set; }

        /// <summary>
        /// Менеджер оповещений через UI
        /// </summary>
        protected IUINotificationManager UINotificationManager { get; set; }

        /// <summary>
        /// Менеджер аудита
        /// </summary>
        protected IAuditManager AuditManager { get; set; }

        #endregion

        #region Сессии пользователей

        /// <summary>
        /// Хранение текущего пользователя
        /// </summary>
        private User _user { get; set; }

        /// <summary>
        /// Текущий авторизованный пользователь
        /// </summary>
        public User CurrentUser
        {
            get
            {
                object fromSess = Session["CurrentUser"];
                if (fromSess == null)
                {
                    return null;
                }
                var userId = (long) fromSess;
                if (_user == null)
                {
                    _user = Locator.GetService<IUsersRepository>().Load(userId);
                }
                return _user;
            }
            set
            {
                Session["CurrentUser"] = value != null ? (object) value.Id : null;
                if (value == null)
                {
                    Session.Remove("CurrentUser");
                }
                _user = value;
            }
        }

        /// <summary>
        /// Является ли текущий пользователь авторизованным
        /// </summary>
        public bool IsAuthentificated
        {
            get { return CurrentUser != null; }
        }

        /// <summary>
        /// Авторизирует текущего пользователя
        /// </summary>
        /// <param name="user">Пользователь которого установить как текущего</param>
        /// <param name="remember">Запомнить ли пользователя</param>
        public void AuthorizeUser(User user, bool remember = true)
        {
            CurrentUser = user;
            if (remember)
            {
                // Устанавливаем собственные авторизационные куки
                var authCookie = new HttpCookie("auth");
                authCookie.Values["identity"] = user.Login;
                authCookie.Values["pass"] = user.PasswordHash;
                authCookie.Expires = DateTimeZone.Now.AddDays(7);
                Response.Cookies.Add(authCookie);
            }
        }

        /// <summary>
        /// Убирает авторизацию текущего пользователя и убирает авторизационные куки если они есть
        /// </summary>
        public void CloseAuthorization()
        {
            CurrentUser = null;

            // убираем куки если они есть
            var authCookie = Response.Cookies["auth"];
            if (authCookie != null)
            {
                authCookie = new HttpCookie("auth")
                                 {
                                     Expires = DateTimeZone.Now.AddDays(-1)
                                 };
                Response.Cookies.Add(authCookie);
            }
        }

        #endregion

        #region Навигационная цепочка - хлебная крошка

        /// <summary>
        /// Элементы навигационной цепочки
        /// </summary>
        public IList<NavigationChainItem> NavigationChainItems { get; private set; }

        /// <summary>
        /// Добавляет новый элемент в навигационную цепочку - хлебную крошку
        /// </summary>
        /// <param name="title">Заголовок</param>
        /// <param name="description">Описание</param>
        /// <param name="url">Ссылка куда ведет</param>
        /// <param name="active">Активна ли ссылка для кликанья</param>
        public void PushNavigationItem(string title, string description, string url, bool active = true)
        {
            NavigationChainItems.Add(new NavigationChainItem()
                {
                    Active = active,
                    Description = description,
                    Title = title,
                    Url = url
                });
        }

        #endregion

        #region Аудит

        /// <summary>
        /// Добавляет событие аудита от текущего пользователя
        /// </summary>
        /// <param name="eventType">Тип события</param>
        /// <param name="message">Сообщение</param>
        protected void PushAuditEvent(AuditEventTypes eventType, string message)
        {
            if (IsAuthentificated)
            {
                var request = System.Web.HttpContext.Current.Request;
                try
                {
                    AuditManager.PushEvent(CurrentUser, eventType, message, httpRequest: request);
                }
                catch (Exception e)
                {
                    // TODO: придумать что сделать чтобы обойти встроенную валидацию запроса в HTTP контексте
                }
                
            }
        }

        #endregion
    }
}
