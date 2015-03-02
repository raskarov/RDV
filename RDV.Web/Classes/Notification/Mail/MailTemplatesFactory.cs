using System;
using System.IO;
using System.Web;
using RDV.Domain.Entities;
using RDV.Domain.Infrastructure.Mailing.Templates;
using RDV.Web.Models;
using RDV.Web.Models.Account;
using RDV.Web.Models.Account.Profile;
using RDV.Web.Models.Administration.Users;
using RDV.Web.Models.MainPage;

namespace RDV.Web.Classes.Notification.Mail
{
    /// <summary>
    /// Фабрика по производству шаблонов к рассылке писем
    /// </summary>
    public static class MailTemplatesFactory
    {
        /// <summary>
        /// Создает объект шаблона для уведомления пользователя о его регистрации на сайте РДВ
        /// </summary>
        /// <param name="model">Модель регистрации</param>
        /// <returns></returns>
        public static BaseTemplate GetRegistrationTemplate(RegisterModel model)
        {
            return new ParametrizedFileTemplate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Templates","Mail","Register.htm"),model);
        }

        /// <summary>
        /// Создает объект шаблона для уведомления пользователя о восстановлении его пароля в личный кабинет
        /// </summary>
        /// <param name="model">Модель восстановления</param>
        /// <returns></returns>
        public static BaseTemplate GetForgotPasswordTemplate(ForgotPasswordModel model)
        {
            return new ParametrizedFileTemplate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "ForgotPassword.htm"), model);
        }

        /// <summary>
        /// Создает объект шаблона уведомления пользователя о смене его пароля в профиле
        /// </summary>
        /// <param name="model">Модель с данными</param>
        /// <returns></returns>
        public static BaseTemplate GetPasswordChangeTemplate(ProfileUpdateModel model)
        {
            return new ParametrizedFileTemplate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "ChangePassword.htm"), model);
        }

        /// <summary>
        /// Создает объект шаблона для уведомления пользователя о том что администратор создал для него аккаунт
        /// </summary>
        /// <param name="model">Модель</param>
        /// <returns>Объект шаблона</returns>
        public static BaseTemplate GetUserCreationTemplate(SaveUserModel model)
        {
            return new ParametrizedFileTemplate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "UserCreation.htm"), model);
        }

        /// <summary>
        /// Создает объект шаблона для уведомления пользователя о том что администратор сменил ему пароль
        /// </summary>
        /// <param name="model">Модель</param>
        /// <returns>Объект шаблона</returns>
        public static BaseTemplate GetForcePasswordChangeTemplate(SaveUserModel model)
        {
            return new ParametrizedFileTemplate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "ForceChangePassword.htm"), model);
        }

        /// <summary>
        /// Создает объект шаблона для восстановления пароля пользователя
        /// </summary>
        /// <param name="model">Модель восстановления</param>
        /// <returns></returns>
        public static BaseTemplate GetRememberPasswordTemplate(RememberPasswordModel model)
        {
            return new ParametrizedFileTemplate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "RememberPassword.htm"), model);
        }

        /// <summary>
        /// Создает объект шаблона для нотитфицирования агента о заказе звонка
        /// </summary>
        /// <param name="model">Модель</param>
        /// <returns></returns>
        public static BaseTemplate GetCallbackTemplate(object model)
        {
            return new ParametrizedFileTemplate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "Callback.htm"), model);
        }

        /// <summary>
        /// Создает объект шаблона для нотификации агента об заявке на смотр
        /// </summary>
        /// <param name="model">Модель</param>
        /// <returns></returns>
        public static BaseTemplate GetViewRequestTemplate(object model)
        {
            return new ParametrizedFileTemplate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "ViewRequest.htm"), model);
        }

        /// <summary>
        /// Возвращает шаблно для нотификации директора о том, что кто то зарегистрировался в его компании
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static BaseTemplate GetRegistrationDirectorTemplate(RegisterModel model)
        {
            return new ParametrizedFileTemplate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "RegisterDirector.htm"), model);
        }

        /// <summary>
        /// Возвращает шаблон для уведомления о новом сообщении на сайте РДВ
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static BaseTemplate GetFeedbackTemplate(FeedbackModel model)
        {
            model.Message = HttpUtility.HtmlEncode(model.Message);
            return new ParametrizedFileTemplate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "Feedback.htm"), model);
        }

        /// <summary>
        /// Возвращает шаблон для уведомления директора о том что в его компанию регистрируется сотрудник, ранее зарегистрированный где то еще
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static BaseTemplate GetBadRegistrationDirectorTemplate(BadUserRegistrationModel model)
        {
            return new ParametrizedFileTemplate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "BadUserRegistration.htm"), model);
        }

        /// <summary>
        /// Возвращает шаблон для уведомления об ошибке на сайте
        /// </summary>
        /// <param name="model">Модель данных</param>
        /// <returns></returns>
        public static BaseTemplate GetBugReportTemplate(BugReportModel model)
        {
            return new ParametrizedFileTemplate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "BugReport.htm"), model);
        }
    }
}