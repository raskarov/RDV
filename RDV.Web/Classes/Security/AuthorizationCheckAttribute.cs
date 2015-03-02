using System.Web.Mvc;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Controllers;

namespace RDV.Web.Classes.Security
{
    /// <summary>
    /// Аспект, валидирующий авторизованность пользователя на выполнение действия
    /// </summary>
    public class AuthorizationCheckAttribute: ActionFilterAttribute
    {
        /// <summary>
        /// Урл, куда происходит редирект если пользователь не авторизован
        /// </summary>
        public string RedirectUrl { get; private set; }

        /// <summary>
        /// Дополнительный пермишен, которым должен обладать пользователь для успешного продолжения авторизация
        /// </summary>
        public long RequiredPermission { get; private set; }

        /// <summary>
        /// Инициализирует новый атрибут, помещающий действие аспектом
        /// </summary>
        /// <param name="requiredPermission">Дополнительное разрешение, которым должен обладать пользователь чтобы пройти авторизацию</param>
        /// <param name="redirectUrl">Урл, куда редиректить неавторизованного пользователя</param>
        public AuthorizationCheckAttribute(long requiredPermission = -1 ,string redirectUrl = "/")
        {
            RequiredPermission = requiredPermission;
            RedirectUrl = redirectUrl;
        }

        /// <summary>
        /// Фильтруем действие перед его выполнение
        /// </summary>
        /// <param name="filterContext">Контекст действия</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var currentUser = (filterContext.Controller as BaseController).CurrentUser;
            if (currentUser == null)
            {
                // Если возвращаем на главную, то вернуть параметр returnUrl
                var url = this.RedirectUrl == "/" ? this.RedirectUrl + "?returnUrl=" + filterContext.RequestContext.HttpContext.Request.Url : this.RedirectUrl;
                filterContext.Result = new RedirectResult(url);
            } 
            else
            {
                // Проверка того, что пользователь забанен
                if (currentUser.Status == (int)UserStatuses.InActive)
                {
                    Locator.GetService<IUINotificationManager>().Error("Вы не активированы, поэтому не можете получить доступ к личному кабинету. Пожалуйста, свяжитесь с вашей компанией, и попросите уполномоченного сотрудника активировать вас.");
                    filterContext.Result = new RedirectResult("/");
                }
                else if (currentUser.Company != null && currentUser.Company.Inactive)
                {
                    Locator.GetService<IUINotificationManager>().Error("Ваша компания была заблокирована в нашей системе");
                    filterContext.Result = new RedirectResult("/");
                }
                else if (currentUser.Status == (int)UserStatuses.Blocked)
                {
                    Locator.GetService<IUINotificationManager>().Error("Вы были заблокированы администратором, поэтому больше не имеете права пользоваться функциями системы");
                    filterContext.Result = new RedirectResult("/");
                } else if (RequiredPermission != -1 && !currentUser.HasPermission(RequiredPermission))
                {
                    // Проверка того что пользователь имеет требуемые разрешения
                    var permission = Locator.GetService<IPermissionsRepository>().Load(RequiredPermission);
                    Locator.GetService<IUINotificationManager>().Error(string.Format("Отсутствует необходимая привелегия ({0}) для доступа к указанному разделу", permission.DisplayName));
                    filterContext.Result = new RedirectResult(this.RedirectUrl);    
                }
                else
                {
                    base.OnActionExecuting(filterContext);        
                }
            }
        }
    }
}