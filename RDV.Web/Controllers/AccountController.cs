using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using DevExpress.Office.Utils;
using NLog;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Misc;
using RDV.Domain.Infrastructure.Routing;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Classes.Caching;
using RDV.Web.Classes.Enums;
using RDV.Web.Classes.Extensions;
using RDV.Web.Classes.Forms;
using RDV.Web.Classes.Forms.Validators;
using RDV.Web.Classes.Notification.Mail;
using RDV.Web.Classes.Search.Interfaces;
using RDV.Web.Classes.Security;
using RDV.Web.Models;
using RDV.Web.Models.Account;
using RDV.Web.Models.Account.Company;
using RDV.Web.Models.Account.ObjectsList;
using RDV.Web.Models.Account.Profile;
using System.Web.UI.DataVisualization.Charting;
using RDV.Web.Models.Objects;
using RDV.Web.Models.Search;
using Chart = System.Web.UI.DataVisualization.Charting.Chart;
using RDV.Domain.Core;

namespace RDV.Web.Controllers
{
    /// <summary>
    /// Контроллер управления личным кабинетом
    /// </summary>
    public class AccountController : BaseController
    {
        /// <summary>
        /// Текущий логгер
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Common

        /// <summary>
        /// Логгер
        /// </summary>
        private Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Конструктор с инъекция репозитория объектов
        /// </summary>
        public AccountController()
            : base()
        {
            ObjectsRepository = Locator.GetService<IEstateObjectsRepository>();
        }

        /// <summary>
        /// Репозиторий объектов недвижимости
        /// </summary>
        private IEstateObjectsRepository ObjectsRepository { get; set; }

        /// <summary>
        /// Отображает главную страницу личного кабинета, как правило вкладку мои объекты
        /// </summary>
        /// <returns>Личный кабинет или страница регистрации если мы не авторизованы</returns>
        [Route("account")]
        [AuthorizationCheck(redirectUrl: "/account/register")]
        public ActionResult Index()
        {
            return RedirectToAction("MyObjects");
        }

        /// <summary>
        /// Кладет в стек навигационной цепочки корневой элемент личного кабинета
        /// </summary>
        private void PushAccountNavigationItem()
        {
            PushNavigationItem("Личный кабинет", "Корневая страница личного кабинета", "/account/");
        }

        #endregion

        #region Авторизация/регистрация

        [Route("account/check-email")]
        public ActionResult CheckEmail()
        {
            // Репозиторий пользователей
            var usersRep = Locator.GetService<IUsersRepository>();
            var email = Request["Email"];
            // Проверяем уникальность Email
            var exists = usersRep.Find(u => u.Email.ToLower() == email.ToLower()) != null;
            if (exists)
            {
                return Content("\"Такой Email уже используется\"");
            }
            else
            {
                return Content("true");
            }
        }

        /// <summary>
        /// Отображает форму регистрации
        /// </summary>
        /// <returns>Форма регистрации</returns>
        [Route("account/register")]
        public ActionResult Register()
        {
            // Если мы уже авторизованы то нам не нужно регистрироваться
            if (CurrentUser != null)
            {
                return RedirectToAction("Index");
            }

            // Получаем список компаний и передаем во вью
            var companies = Locator.GetService<ICompaniesRepository>().GetActiveCompanies().AsEnumerable();
            ViewBag.companies = companies;

            // Отображаем вид
            return View(new RegisterModel());
        }

        /// <summary>
        /// Обрабатывает форму регистрации нового пользователя
        /// </summary>
        /// <param name="model">Модель с данными по регистрации</param>
        /// <returns>Перенаправляет в личный кабинет в случае успеха. иначе на страницу с регистрации с объяснением ошибок</returns>
        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            // Репозитории
            var usersRepository = Locator.GetService<IUsersRepository>();
            var companiesRepository = Locator.GetService<ICompaniesRepository>();
            var rolesRepository = Locator.GetService<IRolesRepository>();

            // Сразу подгаталиваемся к отдаче данных во вью
            ViewBag.companies = companiesRepository.GetActiveCompanies();

            // Валидируем на сервере
            // проверяем уществование пользователя
            var existingUser = usersRepository.ExistsUserWithLogin(model.Email);
            if (existingUser)
            {
                Locator.GetService<IUINotificationManager>().Error(string.Format("Пользователем с email адресом {0} уже зарегистрирован в системе", model.Email));
                return View(model);
            }
            // Проверяем пароль и подтверждение пароля
            if (model.Password != model.PasswordConfirm)
            {
                Locator.GetService<IUINotificationManager>().Error("Пароль и его подтверждение не совпадают");
                return View(model);
            }
            // Проверяем что пользователь принял правила пользования системой
            if (!model.AcceptAgreement)
            {
                Locator.GetService<IUINotificationManager>().Error("Вы должны принять правила пользования системой перед регистрацией");
                return View(model);
            }
            // Проверяем что пользователь регистрируется за существующую компанию
            if (model.RegisterAs == (int)RegistrationTypes.Agent && !companiesRepository.ExistsCompanyWithId(model.CompanyId))
            {
                Locator.GetService<IUINotificationManager>().Error("Кажется вы пытаетесь зарегистрироваться как агент несуществующей компании");
                return View(model);
            }

            // Выполняем регистрацию
            var newUser = new User()
            {
                Activated = false,
                Birthdate = model.Birthdate,
                Blocked = false,
                Email = model.Email,
                FirstName = model.FirstName,
                ICQ = model.ICQ,
                LastName = model.LastName,
                Login = model.Email,
                /*Passport = new Passport()
                               {
                                   CreatedBy = -1,
                                   ModifiedBy = -1,
                                   DateCreated = DateTimeZone.Now
                               },*/
                Phone = model.Phone,
                Phone2 = model.Phone2,
                SurName = model.SurName,
                PasswordHash = PasswordUtils.GenerateMD5PasswordHash(model.Password),
                DateCreated = DateTimeZone.Now,
                Notifications = SubscribeUtils.All()
            };

            var registrationMessage = String.Empty; // Сообщение, которое будем выдалать пользователю при регистрации

            // Выполняем присвиение первоначальных ролй в зависимости от того, регистрируется пользователь как агент или нет
            if (model.RegisterAs == (int)RegistrationTypes.Agent)
            {
                // устанавливаем пользователь как члена компании
                var company = companiesRepository.Load(model.CompanyId);
                newUser.CompanyId = company.Id;
                newUser.Appointment = model.Appointment;
                newUser.SeniorityStartDate = model.SeniorityStartDate;
                newUser.Status = (int)UserStatuses.InActive;

                // устанавливаем роль пользователю по умолчанию
                rolesRepository.AssignRole(newUser, BuiltinRoles.Agent);

                // Устанавливаем сообщение
                registrationMessage = string.Format("Вы успешно зарегистрировались как агент компании \"{0}\"", company.Name);
            }
            else
            {
                // устанавливаем роль пользователя по умолчанию
                rolesRepository.AssignRole(newUser, BuiltinRoles.Guest);
                newUser.CompanyId = -1;

                // устанавливаем сообщение
                registrationMessage =
                    "Вы успешно зарегистрировались в системе как гость, и теперь можете оставлять сведения о своих объектах недвижимости и просматривать часть сведений о чужих";

                newUser.Activated = true;
                newUser.Status = (int)UserStatuses.Active;
            }

            // Сохраняем пользователя в системе
            try
            {
                usersRepository.Add(newUser);
                usersRepository.SubmitChanges();

                // похоже все в порядке - выдаем пользовтелю сообщение об успешной регистрации
                Locator.GetService<IUINotificationManager>().Success(registrationMessage);

                // Отправляем пользователю сообщение об успешной регистрации в системе
                Locator.GetService<IMailNotificationManager>().Notify(newUser, "Регистрация на сайте РДВ", MailTemplatesFactory.GetRegistrationTemplate(model).ToString());
                // Добавляем отправляем сообщение директору компании о том, что у него зарегистрировался новый агент
                if (newUser.Company != null && newUser.Company.Director != null)
                {
                    var badUser = usersRepository.Find(u =>
                                                       u.FirstName.ToLower() == model.FirstName.ToLower() &&
                                                       u.LastName.ToLower() == model.LastName.ToLower());
                    var badRegistration = badUser != null && badUser.CompanyId != newUser.Company.Id;

                    if (badRegistration)
                    {
                        // Похоже агент регистрируется повторно
                        Locator.GetService<IMailNotificationManager>().Notify(newUser.Company.Director, "Повторная регистрация пользователя", MailTemplatesFactory.GetBadRegistrationDirectorTemplate(new BadUserRegistrationModel(newUser.Company.Director, badUser)).ToString());
                    }
                    else
                    {
                        model.ActivationLink = string.Format("{0}/activate-user?id={1}&dId={2}&uHash={3}&dHash={4}",
                                                             "http://www.nprdv.ru", newUser.Id,
                                                             newUser.Company.Director.Id,
                                                             PasswordUtils.Hashify(newUser.PasswordHash, 5),
                                                             PasswordUtils.Hashify(
                                                                 newUser.Company.Director.PasswordHash, 5));
                        Locator.GetService<IMailNotificationManager>().Notify(newUser.Company.Director, "Новый пользователь в компании", MailTemplatesFactory.GetRegistrationDirectorTemplate(model).ToString());
                    }

                }

                // Авторизуем пользователя
                AuthorizeUser(newUser, true);

                // Добавляем событие аудита
                PushAuditEvent(AuditEventTypes.System, "Регистрация в системе");
            }
            catch (Exception e)
            {
                Locator.GetService<IUINotificationManager>().Error(string.Format("Ошибка в ходе создания пользователя: {0}", e.Message));
                _logger.Error(string.Format("Не удалось зарегистрировать пользователя: {0}", e.Message));
                return View(model);
            }

            // Переправляем пользователя в личный кабинет
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Обрабатывает форму авторизации в личный кабинет
        /// </summary>
        /// <param name="model">Модель с данными</param>
        /// <returns>Перенапрвляет в личный кабинет в случае успеха, иначе - на главную страницу</returns>
        [Route("account/logon")]
        public ActionResult LogOn(LogOnModel model, String ReturnUrl)
        {
            // репозиторий
            var rep = Locator.GetService<IUsersRepository>();

            // преобразуем пароль
            var pHash = PasswordUtils.GenerateMD5PasswordHash(model.Password);

            // Ищем пользователя
            var user = rep.GetUserByLoginAndPasswordHash(model.Email, pHash);

            // Проверяем найден ли пользователь
            if (user == null)
            {
                Locator.GetService<IUINotificationManager>().Error("Неправильная пара Email адреса и пароля");
                return RedirectToAction("Index", "Main");
            }

            // Авторизуем пользователя
            AuthorizeUser(user);
            user.LastLogin = DateTimeZone.Now;
            Locator.GetService<IUsersRepository>().SubmitChanges();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.System, "Авторизация в системе");

            if (String.IsNullOrEmpty(ReturnUrl))
            {
                // Перенаправляем в личный кабинет
                return RedirectToAction("Index");
            }
            else
            {
                // Перенаправляем на return url
                return Redirect(ReturnUrl);
            }
        }

        /// <summary>
        /// Выход из текущего сеанса
        /// </summary>
        /// <returns></returns>
        [Route("account/logoff")]
        public ActionResult LogOff()
        {
            // Событие аудита
            PushAuditEvent(AuditEventTypes.System, "Выход из системы");

            CloseAuthorization();
            return RedirectToAction("Index", "Main");
        }

        /// <summary>
        /// Отображает страницу восстановления забытого пароля
        /// </summary>
        /// <returns></returns>
        public ActionResult Forgot()
        {
            // Авторизованные нам не нужны
            if (CurrentUser != null)
            {
                return RedirectToAction("Index", "Main");
            }

            return View();
        }

        /// <summary>
        /// Обрабатывает страницу восстановления забытого пароля
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Forgot(string email)
        {
            // Авторизованные нам не нужны
            if (CurrentUser != null)
            {
                return RedirectToAction("Index", "Main");
            }

            // Ищем пользователя
            var usersRep = Locator.GetService<IUsersRepository>();
            var user = usersRep.Find(u => u.Email == email);
            if (user == null)
            {
                UINotificationManager.Error("Такой пользователь не найден");
                return RedirectToAction("Forgot");
            }
            else
            {
                MailNotificationManager.Notify(user, "Восстановление пароля. Шаг 1", MailTemplatesFactory.GetForgotPasswordTemplate(new ForgotPasswordModel(user)).ToString());
                UINotificationManager.Success("Код для восстановления пароля и дальнейшие инструкции были высланы на Email, указанный при регистрации. ");
                return RedirectToAction("Index", "Main");
            }
        }

        /// <summary>
        /// Отображает страницу сбрасывания забытого пароля
        /// </summary>
        /// <returns></returns>
        public ActionResult Remember(string email)
        {
            // Авторизованные нам не нужны
            if (CurrentUser != null)
            {
                return RedirectToAction("Index", "Main");
            }
            ViewBag.email = email;
            return View();
        }

        /// <summary>
        /// Обрабатывает страницу сбрасывания забытого пароля
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Remember(string email, string code)
        {
            // Авторизованные нам не нужны
            if (CurrentUser != null)
            {
                return RedirectToAction("Index", "Main");
            }

            // Ищем пользователя
            var usersRep = Locator.GetService<IUsersRepository>();
            var user = usersRep.Find(u => u.Email == email);
            if (user == null)
            {
                UINotificationManager.Error("Такой пользователь не найден");
                return RedirectToAction("Forgot");
            }
            else
            {
                // Проверяем правильность кода
                var validCode = PasswordUtils.GenerateMD5PasswordHash(user.PasswordHash);
                validCode = PasswordUtils.GenerateMD5PasswordHash(validCode);
                if (code != validCode)
                {
                    UINotificationManager.Error("Введен неправильный код восстановления");
                    return RedirectToAction("Remember", new { email = email });
                }
                else
                {
                    var newPassword = PasswordUtils.GeneratePassword(10);
                    user.PasswordHash = PasswordUtils.GenerateMD5PasswordHash(newPassword);
                    user.DateModified = DateTimeZone.Now;
                    user.ModifiedBy = user.Id;
                    usersRep.SubmitChanges();
                    PushAuditEvent(AuditEventTypes.Editing, "Восстановление забытого пароля");
                    MailNotificationManager.Notify(user, "Восстановление пароля. Шаг 2", MailTemplatesFactory.GetRememberPasswordTemplate(new RememberPasswordModel(user, newPassword)).ToString());
                    UINotificationManager.Success("Пароль был сброшен, на Email, указанный при регистрации было отправлено письмо, содержащее данные для входа в личный кабинет");
                    return RedirectToAction("Index", "Main");
                }
            }
        }

        /// <summary>
        /// Выполняет активацию указанного пользователя с помощью письма
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <param name="dId">Идентификатор директора компании</param>
        /// <param name="uHash">Секретный хеш пароля пользователя</param>
        /// <param name="dHash">Секретный хеш пароля директора компании</param>
        /// <returns></returns>
        [Route("activate-user")]
        public ActionResult ActivateUser(long id, long dId, string uHash, string dHash)
        {
            // Репозиторий
            var rep = Locator.GetService<IUsersRepository>();

            // Пользователи
            var user = rep.Load(id);
            var director = rep.Load(dId);

            try
            {
                // Первая проверка
                if (user == null | director == null)
                {
                    throw new ObjectNotFoundException("Не все пользователи найдены");
                }

                // Вторая проверка
                if (user.Company != null && user.Company.Id != director.CompanyId)
                {
                    throw new Exception("Неправильное соотношение компании пользователя и компании директора");
                }

                // Третья проверка
                var userHash = PasswordUtils.Hashify(user.PasswordHash, 5);
                var directorHash = PasswordUtils.Hashify(director.PasswordHash, 5);
                if (userHash != uHash | directorHash != dHash)
                {
                    throw new SecurityException("Ошибка безопасности - неправильные секретные коды");
                }

                // Активируем
                user.Status = (short)UserStatuses.Active;
                rep.SubmitChanges();
                UINotificationManager.Success(string.Format("Пользователь {0} был успешно активирован", user.ToString()));
            }
            catch (Exception e)
            {
                UINotificationManager.Error(e.Message);
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Профиль

        #region Кошелек

        /// <summary>
        /// Возвращает страницу с информацией о кошельке пользователя и движении денежных средств в нем
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/profile/payments")]
        public ActionResult Payments()
        {
            var payments = CurrentUser.Payments.OrderByDescending(d => d.DateCreated).ToList();

            PushAccountNavigationItem();
            PushNavigationItem("Профиль", "Профиль пользователя", "/account/profile", true);
            PushNavigationItem("Кошелек", "Личный кошелек пользователя", "/account/profile/payments", false);

            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр своего кошелька");

            return View(payments);
        }

        /// <summary>
        /// Подготавливает страницу к осуществлению нового платежа
        /// </summary>
        /// <param name="amount">Количество денег к зачислению</param>
        /// <returns>Подготовительная страница для введения платежа</returns>
        [AuthorizationCheck()]
        [Route("account/profile/new-payment")]
        [HttpPost]
        public ActionResult NewPayment(decimal amount)
        {
            // Создаем объект платежа в базе
            var newPayment = new Payment()
            {
                DateCreated = DateTimeZone.Now,
                Amount = amount,
                Direction = (short)PaymentDirection.Income,
                Payed = false,
                User = CurrentUser,
                CompanyId = -1,
                Description =
                    String.Format("Пополнение лицевого счета с помощью системы ROBOKassa на сумму {0} рублей",
                                  amount)
            };
            CurrentUser.Payments.Add(newPayment);
            Locator.GetService<IUsersRepository>().SubmitChanges();

            // Навигация
            PushAccountNavigationItem();
            PushNavigationItem("Профиль", "Профиль пользователя", "/account/profile", true);
            PushNavigationItem("Кошелек", "Личный кошелек пользователя", "/account/profile/payments", true);
            PushNavigationItem("Пополнение кошелька", "Страница пополнения своего кошелька", "/account/profile/new-payment", false);

            // Начало пополнения своего кошелька на 
            PushAuditEvent(AuditEventTypes.Editing, string.Format("Инициирование пополнения своего кошелька на {0} рублей", amount));

            return View(newPayment);
        }

        /// <summary>
        /// Выполняет обработку поступившего платежа
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("payment-result")]
        public ActionResult PaymentResult(decimal OutSum, long InvId, string SignatureValue)
        {
            // Репозиторий
            var paymentsRep = Locator.GetService<IPaymentsRepository>();

            // Обрабатываем
            var payment = paymentsRep.Load(InvId);
            if (payment == null)
            {
                Logger.Error(string.Format("Платеж с идентификатором {0} не найден", InvId));
                return Content("Error");
            }

            // Проверка запроса
            if (payment.Amount != OutSum)
            {
                Logger.Error(string.Format("По платежу с идентификатором {0} пришло меньше денег: {1} вместо {2}", InvId, OutSum, payment.Amount));
                return Content("Error");
            }

            // Помечаем запрос как оплаченный
            payment.Payed = true;
            payment.DatePayed = DateTimeZone.Now;
            paymentsRep.SubmitChanges();

            // Возвращаем
            return Content("OK");
        }

        /// <summary>
        /// Обрабатывает успешное завершение платежа
        /// </summary>
        /// <param name="OutSum">Сумма</param>
        /// <param name="InvId">Номер платежа</param>
        /// <param name="SignatureValue">Хеш</param>
        /// <returns></returns>
        [Route("payment-success")]
        public ActionResult PaymentSuccess(decimal OutSum, long InvId, string SignatureValue)
        {
            // Репозиторий
            var paymentsRep = Locator.GetService<IPaymentsRepository>();

            // Обрабатываем
            var payment = paymentsRep.Load(InvId);
            if (payment == null)
            {
                UINotificationManager.Error(string.Format("Платеж с идентификатором {0} не найден", InvId));
                View("PaymentNotFound");
            }

            // Навигация
            PushAccountNavigationItem();
            PushNavigationItem("Профиль", "Профиль пользователя", "/account/profile", true);
            PushNavigationItem("Кошелек", "Личный кошелек пользователя", "/account/profile/payments", true);
            PushNavigationItem("Успешное пополнение кошелька", "Страница пополнения своего кошелька", "", false);

            PushAuditEvent(AuditEventTypes.Editing, string.Format("Успешное пополнение счета на сумму {0}", OutSum));

            // Отображаем вид
            return View(payment);
        }

        /// <summary>
        /// Обрабатывает отказ от проведения платежа
        /// </summary>
        /// <param name="OutSum">Сумма</param>
        /// <param name="InvId">Номер платежа</param>
        /// <param name="SignatureValue">Хеш</param>
        /// <returns></returns>
        [Route("payment-fail")]
        public ActionResult PaymentFail(decimal OutSum, long InvId, string SignatureValue)
        {
            // Репозиторий
            var paymentsRep = Locator.GetService<IPaymentsRepository>();

            // Обрабатываем
            var payment = paymentsRep.Load(InvId);
            if (payment == null)
            {
                UINotificationManager.Error(string.Format("Платеж с идентификатором {0} не найден", InvId));
                View("PaymentNotFound");
            }

            // Навигация
            PushAccountNavigationItem();
            PushNavigationItem("Профиль", "Профиль пользователя", "/account/profile", true);
            PushNavigationItem("Кошелек", "Личный кошелек пользователя", "/account/profile/payments", true);
            PushNavigationItem("Успешное пополнение кошелька", "Страница пополнения своего кошелька", "", false);

            PushAuditEvent(AuditEventTypes.Editing, string.Format("Отказ от проведения платежа {0}", InvId));

            // Отображаем вид
            return View(payment);
        }


        #endregion

        #region Учебные программы

        /// <summary>
        /// Отображает таблицу для редактирования учебных программ в профиле пользователя
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/profile/training-programs")]
        public ActionResult TrainingPrograms()
        {
            PushAccountNavigationItem();
            PushNavigationItem("Профиль", "Профиль текущего авторизованного пользователя", "/account/profile/");
            PushNavigationItem("Учебные программы", "Учебные программы, в которых участвовал текущий пользователь", "/account/profile/training-programs", false);

            // Получаем список учебных программ текущего пользователя
            var trainingPrograms = CurrentUser.TrainingPrograms.OrderBy(p => p.TrainingDate).ToList();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка своих учебных программ");

            return View(trainingPrograms);
        }

        /// <summary>
        /// Сохраняет изменения или создает новую тренировочную программу
        /// </summary>
        /// <param name="model">Модель данных</param>
        /// <returns>JSON ответ</returns>
        [AuthorizationCheck()]
        [HttpPost]
        [Route("account/profile/save-training-program")]
        public ActionResult SaveTrainingProgram(TrainingProgramModel model)
        {
            // Репозитории
            var usersRepository = Locator.GetService<IUsersRepository>();

            var filesRep = Locator.GetService<IStoredFilesRepository>();

            var fileSubmitted = false;
            string filename = "";

            // Сохраняем файл
            var file = Request.Files[0];
            if (file != null && file.ContentLength > 0 && file.ContentType.Contains("image"))
            {
                var serverFile = filesRep.SavePostedFile(file, "TrainingPrograms");
                fileSubmitted = true;
                filename = serverFile.GetURI();
            }

            // Проверяем безопасность
            if (CurrentUser.Id != model.UserId)
            {
                throw new Exception("Неверный идентификатор пользователя");
            }

            // Подгатавливаемся
            var currentUser = usersRepository.Load(model.UserId);
            TrainingProgram trainingProgram = null;
            if (model.Id <= 0)
            {
                trainingProgram = new TrainingProgram()
                {
                    CreatedBy = model.UserId,
                    DateCreated = DateTimeZone.Now,
                    UserId = model.UserId
                };
                usersRepository.AddTrainingProgram(currentUser, trainingProgram);
            }
            else
            {
                trainingProgram = currentUser.TrainingPrograms.FirstOrDefault(p => p.Id == model.Id);
                if (trainingProgram == null)
                {
                    throw new Exception(string.Format("Не удается найти программу с идентификатором {0} у текущего пользователя", model.Id));
                }
                trainingProgram.DateModified = DateTimeZone.Now;
                trainingProgram.ModifiedBy = currentUser.Id;
            }

            // Сохраняем изменения
            trainingProgram.TrainingDate = model.TrainingDate;
            trainingProgram.ProgramName = model.ProgramName;
            trainingProgram.Organizer = model.Organizer;
            trainingProgram.TrainingPlace = model.TrainingPlace;
            if (fileSubmitted)
            {
                trainingProgram.CertificateFile = filename;
            }
            usersRepository.SubmitChanges();

            Locator.GetService<IUINotificationManager>().Success("Изменения успешно сохранены");

            // Событие аудита
            PushAuditEvent(AuditEventTypes.Editing, model.Id <= 0 ? "Добавление новой учебной программы " + model.ProgramName : "Редактирование учебной программы " + model.ProgramName);

            // Отадаем успешный результат
            return RedirectToAction("TrainingPrograms");
        }

        /// <summary>
        /// Обрабатывает удаление учебных программ
        /// </summary>
        /// <param name="programIds">Идентификаторы программ, разделенные запятой</param>
        /// <param name="userId">Идентификатор пользователя выполняющего операцию</param>
        /// <returns>JSON ответ</returns>
        [AuthorizationCheck()]
        [HttpPost]
        [Route("account/profile/delete-training-program")]
        public JsonResult DeleteTrainingProgram(string programIds, long userId)
        {
            try
            {
                // Репозитории
                var usersRepository = Locator.GetService<IUsersRepository>();

                // Проверяем безопасность
                if (CurrentUser.Id != userId)
                {
                    throw new Exception("Неверный идентификатор пользователя");
                }

                // Подгатавливаемся
                var currentUser = usersRepository.Load(userId);
                var programs =
                    programIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                        i => currentUser.TrainingPrograms.FirstOrDefault(p => p.Id == i)).Where(p => p != null).ToList();

                // Удаляем
                usersRepository.DeleteTrainingPrograms(currentUser, programs);
                usersRepository.SubmitChanges();

                Locator.GetService<IUINotificationManager>().Success("Изменения успешно сохранены");

                // Событие аудита
                if (programs.Count > 0)
                {
                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Удаление {0} учебных программ", programs.Count));
                }


                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                _logger.Error(String.Format("Ошибка при удалении учебной программы для пользователя {0} - {1}",
                                            CurrentUser.Login, e.Message));
                Response.StatusCode = 500;
                Locator.GetService<IUINotificationManager>().Error("Ошибка при удалении учебной программы: " + e.Message);
                return Json(new { success = false, message = e.Message });
            }
        }

        #endregion

        #region Достижения

        /// <summary>
        /// Отображает таблицу для редактирования достижений в профиле пользователя
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/edit-achivements")]
        public ActionResult Achievments()
        {
            PushAccountNavigationItem();
            PushNavigationItem("Профиль", "Профиль текущего авторизованного пользователя", "/account/profile/");
            PushNavigationItem("Достижения", "Достижения, которые достиг текущий пользователь", "/account/profile/edit-achievments", false);

            // Получаем список учебных программ текущего пользователя
            var archivements = CurrentUser.Achievments.OrderBy(p => p.ReachDate).ToList();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка своих достижений");

            return View(archivements);
        }

        /// <summary>
        /// Сохраняет изменения или создает новую тренировочную программу
        /// </summary>
        /// <param name="model">Модель данных</param>
        /// <returns>JSON ответ</returns>
        [AuthorizationCheck()]
        [HttpPost]
        [Route("account/save-achievment")]
        public ActionResult SaveAchievment(Achievment model)
        {
            // Репозитории
            var usersRepository = Locator.GetService<IUsersRepository>();
            var filesRep = Locator.GetService<IStoredFilesRepository>();

            var fileSubmitted = false;
            string filename = "";

            // Сохраняем файл
            var file = Request.Files[0];
            if (file != null && file.ContentLength > 0 && file.ContentType.Contains("image"))
            {
                var serverFile = filesRep.SavePostedFile(file, "Achievments");
                fileSubmitted = true;
                filename = serverFile.GetURI();
            }

            // Проверяем безопасность
            if (CurrentUser.Id != model.UserId)
            {
                throw new Exception("Неверный идентификатор пользователя");
            }

            // Подгатавливаемся
            Achievment achievment = null;
            if (model.Id <= 0)
            {
                achievment = new Achievment()
                {
                    DateCreated = DateTimeZone.Now,
                    User = CurrentUser
                };
                CurrentUser.Achievments.Add(achievment);
            }
            else
            {
                achievment = CurrentUser.Achievments.FirstOrDefault(p => p.Id == model.Id);
                if (achievment == null)
                {
                    UINotificationManager.Error(string.Format("Не удается найти достижение с идентификатором {0} у текущего пользователя", model.Id));
                    return RedirectToAction("Achievments");
                }
                achievment.DateModified = DateTimeZone.Now;
            }

            // Сохраняем изменения
            achievment.ReachDate = model.ReachDate;
            achievment.Title = model.Title;
            achievment.Organizer = model.Organizer;
            if (fileSubmitted)
            {
                achievment.ScanUrl = filename;
            }
            usersRepository.SubmitChanges();

            Locator.GetService<IUINotificationManager>().Success("Изменения успешно сохранены");

            // Событие аудита
            PushAuditEvent(AuditEventTypes.Editing, model.Id <= 0 ? "Добавление нового достижения " + model.Title : "Редактирование достижения " + model.Title);

            // Отадаем успешный результат
            return RedirectToAction("Achievments");
        }

        /// <summary>
        /// Обрабатывает удаление учебных программ
        /// </summary>
        /// <param name="achievmentIds">Идентификаторы программ, разделенные запятой</param>
        /// <param name="userId">Идентификатор пользователя выполняющего операцию</param>
        /// <returns>JSON ответ</returns>
        [AuthorizationCheck()]
        [HttpPost]
        [Route("account/delete-achievments")]
        public JsonResult DeleteAchievments(string achievmentIds, long userId)
        {
            try
            {
                // Репозитории
                var usersRepository = Locator.GetService<IUsersRepository>();

                // Проверяем безопасность
                if (CurrentUser.Id != userId)
                {
                    throw new Exception("Неверный идентификатор пользователя");
                }

                // Подгатавливаемся
                var currentUser = CurrentUser;
                var achievments =
                    achievmentIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                        i => currentUser.Achievments.FirstOrDefault(p => p.Id == i)).Where(p => p != null).ToList();

                // Удаляем
                foreach (var achievment in achievments)
                {
                    currentUser.Achievments.Remove(achievment);
                }
                usersRepository.SubmitChanges();

                Locator.GetService<IUINotificationManager>().Success("Изменения успешно сохранены");

                // Событие аудита
                if (achievments.Count > 0)
                {
                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Удаление {0} достижений", achievments.Count));
                }


                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                _logger.Error(String.Format("Ошибка при удалении достижений для пользователя {0} - {1}",
                                            CurrentUser.Login, e.Message));
                Response.StatusCode = 500;
                Locator.GetService<IUINotificationManager>().Error("Ошибка при удалении достижений: " + e.Message);
                return Json(new { success = false, message = e.Message });
            }
        }

        #endregion

        #region Отзывы клиенты

        /// <summary>
        /// Отображает таблицу для редактирования достижений в профиле пользователя
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/edit-client-reviews")]
        public ActionResult ClientReviews()
        {
            PushAccountNavigationItem();
            PushNavigationItem("Профиль", "Профиль текущего авторизованного пользователя", "/account/profile/");
            PushNavigationItem("Отзывы клиентов", "Отзывы по клиентам", "/account/profile/edit-clients-review", false);

            // Получаем список учебных программ текущего пользователя
            var reviews = CurrentUser.ClientReviews.OrderBy(p => p.ReviewDate).ToList();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка отзывов по клиентам");

            return View(reviews);
        }

        [Route("account/copy-blob-storage")]
        public ActionResult CopyBlobStorage()
        {
            var filesRep = Locator.GetService<IStoredFilesRepository>();
            filesRep.CopyBlobStorage();

            return Json("Ok", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Сохраняет изменения или создает новую тренировочную программу
        /// </summary>
        /// <param name="model">Модель данных</param>
        /// <returns>JSON ответ</returns>
        [AuthorizationCheck()]
        [HttpPost]
        [Route("account/save-client-review")]
        public ActionResult SaveClientReview(ClientReview model)
        {
            // Репозитории
            var usersRepository = Locator.GetService<IUsersRepository>();
            var filesRep = Locator.GetService<IStoredFilesRepository>();

            var fileSubmitted = false;
            string filename = "";

            // Сохраняем файл
            var file = Request.Files[0];
            if (file != null && file.ContentLength > 0 && file.ContentType.Contains("image"))
            {
                var serverFile = filesRep.SavePostedFile(file, "ClientReviews");
                fileSubmitted = true;
                filename = serverFile.GetURI();
            }

            // Проверяем безопасность
            if (CurrentUser.Id != model.UserId)
            {
                throw new Exception("Неверный идентификатор пользователя");
            }

            // Подгатавливаемся
            ClientReview review = null;
            if (model.Id <= 0)
            {
                review = new ClientReview()
                {
                    DateCreated = DateTimeZone.Now,
                    User = CurrentUser
                };
                CurrentUser.ClientReviews.Add(review);
            }
            else
            {
                review = CurrentUser.ClientReviews.FirstOrDefault(p => p.Id == model.Id);
                if (review == null)
                {
                    UINotificationManager.Error(string.Format("Не удается найти отзыв по клиенту с идентификатором {0} у текущего пользователя", model.Id));
                    return RedirectToAction("Achievments");
                }
                review.DateModified = DateTimeZone.Now;
            }

            // Сохраняем изменения
            review.ReviewDate = model.ReviewDate;
            review.Description = model.Description;
            if (fileSubmitted)
            {
                review.ScanUrl = filename;
            }
            usersRepository.SubmitChanges();

            Locator.GetService<IUINotificationManager>().Success("Изменения успешно сохранены");

            // Событие аудита
            PushAuditEvent(AuditEventTypes.Editing, model.Id <= 0 ? "Добавление отзыва по объекту" + model.ObjectId : "Редактирование отзыв по объекту" + model.ObjectId);

            // Отадаем успешный результат
            return RedirectToAction("ClientReviews");
        }

        /// <summary>
        /// Обрабатывает удаление учебных программ
        /// </summary>
        /// <param name="clientReviewIds">Идентификаторы программ, разделенные запятой</param>
        /// <param name="userId">Идентификатор пользователя выполняющего операцию</param>
        /// <returns>JSON ответ</returns>
        [AuthorizationCheck()]
        [HttpPost]
        [Route("account/delete-client-reviews")]
        public JsonResult DeleteClientReviews(string clientReviewIds, long userId)
        {
            try
            {
                // Репозитории
                var usersRepository = Locator.GetService<IUsersRepository>();

                // Проверяем безопасность
                if (CurrentUser.Id != userId)
                {
                    throw new Exception("Неверный идентификатор пользователя");
                }

                // Подгатавливаемся
                var currentUser = CurrentUser;
                var reviews =
                    clientReviewIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                        i => currentUser.ClientReviews.FirstOrDefault(p => p.Id == i)).Where(p => p != null).ToList();

                // Удаляем
                foreach (var review in reviews)
                {
                    currentUser.ClientReviews.Remove(review);
                }
                usersRepository.SubmitChanges();

                Locator.GetService<IUINotificationManager>().Success("Изменения успешно сохранены");

                // Событие аудита
                if (reviews.Count > 0)
                {
                    PushAuditEvent(AuditEventTypes.Editing, string.Format("Удаление {0} отзывов", reviews.Count));
                }


                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                _logger.Error(String.Format("Ошибка при удалении отзывов для пользователя {0} - {1}",
                                            CurrentUser.Login, e.Message));
                Response.StatusCode = 500;
                Locator.GetService<IUINotificationManager>().Error("Ошибка при удалении достижений: " + e.Message);
                return Json(new { success = false, message = e.Message });
            }
        }

        #endregion

        #region Непосредственно профиль

        /// <summary>
        /// Отображает страницу профиля пользователя
        /// </summary>
        /// <returns>Страница редактирования реквизитов пользователя</returns>
        [AuthorizationCheck()]
        public ActionResult Profile()
        {
            PushAccountNavigationItem();
            PushNavigationItem("Профиль", "Профиль текущего авторизованного пользователя", "/account/profile/");

            // Создаем модель для передачи ее в вид
            var profileModel = new ProfileInfoModel(CurrentUser);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр своего профиля");

            // отображаем
            return View(profileModel);
        }

        /// <summary>
        /// Сохраняет изменения в профиле пользователя
        /// </summary>
        /// <param name="model">Модель данных</param>
        /// <returns>Страница профиля в любом случае</returns>
        [AuthorizationCheck()]
        [HttpPost]
        [ValidateInput(false)]
        [Route("account/profile/save")]
        public ActionResult SaveProfile(ProfileUpdateModel model, FormCollection collection)
        {
            // Репозитории
            var usersRepository = Locator.GetService<IUsersRepository>();
            var filesRepository = Locator.GetService<IStoredFilesRepository>();

            // Проверяем совпадают ли формы
            if (CurrentUser.Id != model.Id)
            {
                Locator.GetService<IUINotificationManager>().Error("Несовпадают идентификаторы текущего пользователя и пользователя, от которого прошел запрос на изменение данных");
                return RedirectToAction("Profile");
            }
            var currentUser = usersRepository.Load(CurrentUser.Id);

            // Проверяем, прислана ли нам картинка
            var userPhotoFile = Request.Files["UserPhoto"];
            bool imageSubmitted = false;
            string submittedImageUrl = String.Empty;
            if (userPhotoFile != null && userPhotoFile.ContentLength > 0)
            {
                // Похоже картинка прислана, проверяем подходит ли она для аватарки
                if (!userPhotoFile.ContentType.ToLower().Contains("image"))
                {
                    Locator.GetService<IUINotificationManager>().Warning("К сожалению, загруженный вами файл не является изображением и не может быть использован в качестве фотографии");
                }
                else
                {
                    var savedFile = filesRepository.SavePostedFile(userPhotoFile, "UserPhotos");
                    submittedImageUrl = savedFile.GetURI();
                    imageSubmitted = true;
                }
            }

            // Сохраняем изменения в профиле пользователе
            currentUser.FirstName = model.FirstName;
            currentUser.SurName = model.SurName;
            currentUser.LastName = model.LastName;
            currentUser.Phone = model.Phone;
            currentUser.Phone2 = model.Phone2;
            currentUser.ICQ = model.ICQ;
            currentUser.Birthdate = model.Birthdate;
            currentUser.Appointment = model.Appointment;
            currentUser.SeniorityStartDate = model.SeniorityStartDate;
            currentUser.CertificationDate = model.CertificationDate;
            currentUser.CertificateNumber = model.CertificateNumber;
            currentUser.PublicLoading = model.PublicLoading;
            currentUser.AdditionalInformation = model.AdditionalInformation;
            currentUser.ModifiedBy = currentUser.Id;
            currentUser.DateModified = DateTimeZone.Now;
            if (imageSubmitted)
            {
                currentUser.PhotoUrl = submittedImageUrl;
            }
            currentUser.Notifications = collection.AllKeys.Where(k => k.StartsWith("subject-")).Select(c => Convert.ToInt16(c.Split('-')[1])).Aggregate((short)0, (current, next) => current |= next);

            // Проверяем был ли изменен пароль, если был то отправляем пользователю нотификацию
            if (!String.IsNullOrEmpty(model.OldPassword))
            {
                // Проверяем совпадает ли старый пароль
                var encoded = PasswordUtils.GenerateMD5PasswordHash(model.OldPassword);
                if (currentUser.PasswordHash.Equals(encoded))
                {
                    // Меняем пользователю пароль, авториуем его и отправляем ему письмо
                    currentUser.PasswordHash = PasswordUtils.GenerateMD5PasswordHash(model.NewPassword);
                    AuthorizeUser(currentUser);
                    Locator.GetService<IMailNotificationManager>().Notify(currentUser, "Изменение пароля", MailTemplatesFactory.GetPasswordChangeTemplate(model).ToString());
                }
                else
                {
                    Locator.GetService<IUINotificationManager>().Error("Не удалось изменить пароль т.к. введенный вами старый пароль не совпал с текущим паролем");
                }
            }

            // Пытаемся сохранить
            try
            {
                usersRepository.SubmitChanges();
            }
            catch (Exception e)
            {
                // Дерьмо случилось
                var errorMsg = string.Format("Не удалось сохранить изменения в профиле пользователя: {0}", e.Message);
                _logger.Error(errorMsg);
                Locator.GetService<IUINotificationManager>().Error(errorMsg);
                return RedirectToAction("Profile");
            }

            Locator.GetService<IUINotificationManager>().Success("Изменения в профиле успешно сохранены");

            // Событие аудита
            PushAuditEvent(AuditEventTypes.Editing, "Редактирование своего профиля");

            return RedirectToAction("Profile");
        }

        /// <summary>
        /// Отображает статистику пользователя
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/profile/statistics")]
        public ActionResult ProfileStatistics()
        {
            PushAccountNavigationItem();
            PushNavigationItem("Профиль", "Профиль текущего авторизованного пользователя", "/account/profile/");
            PushNavigationItem("Статистика", "Статистика пользователя", "/account/profile/statistics", false);

            // Формируем список элементов статистики
            var items = new List<StatisticItem>
                {
                    new StatisticItem("Дата регистрации", CurrentUser.DateCreated.FormatDate()),
                    new StatisticItem("Количество объектов",
                                      CurrentUser.EstateObjects.Count.ToString(CultureInfo.InvariantCulture)),
                    new StatisticItem("Количество учебных программ",
                                      CurrentUser.TrainingPrograms.Count.ToString(CultureInfo.InvariantCulture)),
                    new StatisticItem("Количество активных объектов",CurrentUser.EstateObjects.Count(e => e.Status == (short)EstateStatuses.Active).ToString()),
                    new StatisticItem("Количество черновиков",CurrentUser.EstateObjects.Count(e => e.Status == (short)EstateStatuses.Draft).ToString()),
                    new StatisticItem("Количество достижений",CurrentUser.Achievments.Count.ToString()),
                    new StatisticItem("Количество отзывов",CurrentUser.ClientReviews.Count.ToString()),
                    new StatisticItem("Количество объектов с фотографиями",CurrentUser.EstateObjects.Count(eo =>eo.GetObjectsMedia(true).Count > 0).ToString()),
                    new StatisticItem("Количество объектов без договора",CurrentUser.EstateObjects.Count(eo =>eo.Client == null).ToString()),
                    new StatisticItem("Количество объектов договорных объектов",CurrentUser.EstateObjects.Count(eo =>eo.Client != null).ToString())
                };

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр своей статистики");

            // Отдаем во вью
            return View(items);
        }

        /// <summary>
        /// Отображает страницу активности пользователя
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/profile/activity")]
        public ActionResult ProfileActivity()
        {
            PushAccountNavigationItem();
            PushNavigationItem("Профиль", "Профиль текущего авторизованного пользователя", "/account/profile/");
            PushNavigationItem("Активность", "Активность пользователя, действия которые он совершал", "/account/profile/activity", false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр своей активности");

            // Отображает только события за последний месяц
            var activity = AuditManager.GetEventsForUser(CurrentUser).Where(e => e.EventDate >= DateTimeZone.Now.AddMonths(-1)).ToList();

            return View(activity);
        }

        /// <summary>
        /// Сохраняет фотографию пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveProfilePhoto(long id)
        {
            try
            {
                var usersRep = Locator.GetService<IUsersRepository>();
                var filesRep = Locator.GetService<IStoredFilesRepository>();

                // Ищем пользователя
                var user = usersRep.Load(id);
                if (user == null)
                {
                    throw new ObjectNotFoundException("user not found");
                }

                // Сохраняем файл
                var file = Request.Files[0];
                if (file == null || file.ContentLength == 0 || !file.ContentType.Contains("image"))
                {
                    throw new Exception("Invalid file format");
                }
                var savedFile = filesRep.SavePostedFile(file, "UserPhotos");

                user.PhotoUrl = savedFile.GetURI();
                usersRep.SubmitChanges();

                return Content("OK");
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }
        }

        #endregion

        #endregion

        #region Профиль компании

        [AuthorizationCheck()]
        [Route("account/company")]
        public ActionResult CompanyProfile()
        {
            PushAccountNavigationItem();
            PushNavigationItem("Компания", "Сведения о компании, в которой состоит текущий пользователь", "/account/company/", false);

            // Нет компании, так быть не должно
            if (CurrentUser.Company == null)
            {
                return RedirectToAction("Index");
            }

            // Создаем модель для передачи ее в вид
            var companyProfileModel = new CompanyProfileModel(CurrentUser.Company);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр профиля своей компании");

            return View(companyProfileModel);
        }

        /// <summary>
        /// Отображает страницу редактирования сведений о компании при условии наличия у пользователя на это прав
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.EditOwnCompanyInfo, "/account/company")]
        [Route("account/company/edit")]
        public ActionResult EditCompanyProfile()
        {
            PushAccountNavigationItem();
            PushNavigationItem("Компания", "Сведения о компании, в которой состоит текущий пользователь", "/account/company/");
            PushNavigationItem("Редактирование", "Редактирование сведений о компании", "/account/company/edit", false);

            // Нет компании, так быть не должно
            if (CurrentUser.Company == null)
            {
                return RedirectToAction("Index");
            }

            // Создаем модель для передачи ее в вид
            var companyProfileModel = new CompanyProfileModel(CurrentUser.Company);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр формы редактирования профиля своей компании");

            return View(companyProfileModel);
        }

        /// <summary>
        /// Обрабатывает сохранение изменений в профиле компании
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck(Permission.EditOwnCompanyInfo, "/account/company")]
        [Route("account/company/save")]
        public ActionResult SaveCompanyProfile(CompanyProfileModel model)
        {
            // Проверяем что есть компания
            if (CurrentUser.Company == null)
            {
                return RedirectToAction("CompanyProfile");
            }

            // репозиторий
            var companyRep = Locator.GetService<ICompaniesRepository>();
            var filesRep = Locator.GetService<IStoredFilesRepository>();
            var company = companyRep.Load(CurrentUser.CompanyId);

            // сохраняем картинки если они были загружены
            bool logoSubmitted = false, schemeSubmitted = false;
            string logoServerUrl = null, schemeServerUrl = null;
            HttpPostedFileBase logoFile = Request.Files["CompanyLogo"], schemeFile = Request.Files["SchemeFile"];
            if (logoFile != null && logoFile.ContentLength > 0 && logoFile.ContentType.Contains("image"))
            {
                // Сохраняем файл логотипа
                var logoServerFile = filesRep.SavePostedFile(logoFile, "CompanyLogos");
                logoServerUrl = logoServerFile.GetURI();
                logoSubmitted = true;
            }
            if (schemeFile != null && schemeFile.ContentLength > 0 && schemeFile.ContentType.Contains("image"))
            {
                // сохраняем файл схемы проезда
                var schemeServerFile = filesRep.SavePostedFile(schemeFile, "CompanyLocationSchemes");
                schemeServerUrl = schemeServerFile.GetURI();
                schemeSubmitted = true;
            }

            // Выполняем редактирование
            company.Name = model.Name;
            company.ShortName = model.ShortName;
            company.Description = model.Description;
            company.Address = model.Address;
            company.Phone1 = model.Phone1;
            company.Phone2 = model.Phone2;
            company.Phone3 = model.Phone3;
            company.Email = model.Email;
            company.ContactPerson = model.ContactPerson;
            company.Branch = model.Branch;
            company.DateModified = DateTimeZone.Now;
            company.ModifiedBy = CurrentUser.Id;
            if (logoSubmitted)
            {
                company.LogoImageUrl = logoServerUrl;
            }
            if (schemeSubmitted)
            {
                company.LocationSchemeUrl = schemeServerUrl;
            }

            // сохраняем
            companyRep.SubmitChanges();
            Locator.GetService<IUINotificationManager>().Success("Изменения в профиле компании были успешно сохранены");

            // Событие аудита
            PushAuditEvent(AuditEventTypes.Editing, "Редактирование профиля своей компании");

            return RedirectToAction("CompanyProfile");
        }

        /// <summary>
        /// Отображает статистику пользователя
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/company/statistics")]
        public ActionResult CompanyStatistics()
        {
            PushAccountNavigationItem();
            PushNavigationItem("Компания", "Сведения о компании, в которой состоит текущий пользователь", "/account/company/");
            PushNavigationItem("Статистика", "Статистика компании", "/account/company/statistics", false);

            // Формируем список элементов статистики
            var items = new List<StatisticItem>
                {
                    new StatisticItem("Дата регистрации", CurrentUser.Company.DateCreated.FormatDate()),
                    new StatisticItem("Количество объектов",
                                      CurrentUser.EstateObjects.Count.ToString(CultureInfo.InvariantCulture)),
                    new StatisticItem("Количество учебных программ",
                                      CurrentUser.TrainingPrograms.Count.ToString(CultureInfo.InvariantCulture))
                };

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр статистики своей компании");

            // Отдаем во вью
            return View(items);
        }

        /// <summary>
        /// Отображает страницу активности пользователя
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/company/activity")]
        public ActionResult CompanyActivity()
        {
            PushAccountNavigationItem();
            PushNavigationItem("Компания", "Сведения о компании, в которой состоит текущий пользователь", "/account/company/");
            PushNavigationItem("Активность", "Активность пользователей компании", "/account/company/activity", false);

            if (CurrentUser.Company == null)
            {
                return RedirectToAction("CompanyProfile");
            }

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр активности пользователей своей компании");

            // Отображает только события за последний месяц
            var activity = AuditManager.GetEventsForCompany(CurrentUser.Company).Where(e => e.EventDate >= DateTimeZone.Now.AddMonths(-1)).ToList();

            return View(activity);
        }

        /// <summary>
        /// Сохраняет логотип компаний
        /// </summary>
        /// <param name="id">Идентификатор компании</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveCompanyLogo(long id)
        {
            try
            {
                var comprep = Locator.GetService<ICompaniesRepository>();
                var filesRep = Locator.GetService<IStoredFilesRepository>();

                // Ищем компанию
                var comp = comprep.Load(id);
                if (comp == null)
                {
                    throw new ObjectNotFoundException("Company not found");
                }

                // Сохраняем файл
                var file = Request.Files[0];
                if (file == null || file.ContentLength == 0 || !file.ContentType.Contains("image"))
                {
                    throw new Exception("Invalid file format");
                }
                var savedFile = filesRep.SavePostedFile(file, "CompanyLogos");

                comp.LogoImageUrl = savedFile.GetURI();
                comprep.SubmitChanges();

                return Content("OK");
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }
        }

        #region Кошелек компании

        /// <summary>
        /// Кошелек компании
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.AccessCompanyPayments)]
        [Route("account/company/payments/")]
        public ActionResult CompanyPayments()
        {
            var payments = CurrentUser.Company.Payments.OrderByDescending(d => d.DateCreated).ToList();

            PushAccountNavigationItem();
            PushNavigationItem("Профиль", "Профиль пользователя", "/account/profile", true);
            PushNavigationItem("Компания", "Профиль компании", "/account/company/", true);
            PushNavigationItem("Кошелек компании", "Личный кошелек пользователя", "/account/company/payments", false);

            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр своего кошелька");

            return View(payments);
        }

        /// <summary>
        /// Подготавливает страницу к осуществлению нового платежа
        /// </summary>
        /// <param name="amount">Количество денег к зачислению</param>
        /// <returns>Подготовительная страница для введения платежа</returns>
        [AuthorizationCheck(Permission.AccessCompanyPayments)]
        [Route("account/company/payments/new-payment")]
        [HttpPost]
        public ActionResult NewCompanyPayment(decimal amount)
        {
            // Создаем объект платежа в базе
            var newPayment = new Payment()
            {
                DateCreated = DateTimeZone.Now,
                Amount = amount,
                Direction = (short)PaymentDirection.Income,
                Payed = false,
                UserId = -1,
                CompanyId = CurrentUser.Company.Id,
                Description =
                    String.Format("Пополнение лицевого счета компании с помощью системы ROBOKassa на сумму {0} рублей со внешнего счета пользователя {1}",
                                  amount,CurrentUser.ToString())
            };
            CurrentUser.Company.Payments.Add(newPayment);
            Locator.GetService<IUsersRepository>().SubmitChanges();

            // Навигация
            PushAccountNavigationItem();
            PushNavigationItem("Профиль", "Профиль пользователя", "/account/profile", true);
            PushNavigationItem("Компания", "Профиль компании", "/account/company/", true);
            PushNavigationItem("Кошелек компании", "Личный кошелек пользователя", "/account/company/payments", true);
            PushNavigationItem("Пополнение кошелька компании", "Страница пополнения кошелька компании", "account/company/payments/new-payment", false);

            // Начало пополнения своего кошелька на 
            PushAuditEvent(AuditEventTypes.Editing, string.Format("Инициирование пополнения кошелька компании {1} на {0} рублей", amount,CurrentUser.Company.Name));

            return View(newPayment);
        }

        #endregion

        #endregion

        #region Сотрудники компании

        /// <summary>
        /// Отображает страницу со списком агентов в компании
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/company/members")]
        public ActionResult CompanyMembers()
        {
            if (CurrentUser.Company == null)
            {
                RedirectToAction("Index");
            }

            PushAccountNavigationItem();
            PushNavigationItem("Компания", "Сведения о компании, в которой состоит текущий пользователь", "/account/company/");
            PushNavigationItem("Сотрудники", "Сотрудники, рабюотающие в текущей компании", "/account/company/members", false);


            // Извлекаем список агентов компании
            var agents = CurrentUser.Company.Users.OrderBy(u => u.Id).ToList();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка сотрудников своей компании");

            ViewBag.availableRoles =
                Locator.GetService<IRolesRepository>().GetActiveRoles().Where(r => r.Id != 4).ToList();

            // Передаем его во вью
            return View(agents);
        }

        /// <summary>
        /// Возвращает кусок HTML кода, содержащий данные указанного пользователя
        /// </summary>
        /// <param name="id">Идентификатор пользователя</param>
        /// <returns></returns>
        [Route("account/user-info/{id}")]
        public ActionResult UserInfoAjax(long id)
        {
            // Проверяем авторизованность пользователя
            if (CurrentUser == null)
            {
                return Content("Access Denied");
            }

            // Ищем пользователя
            var repository = Locator.GetService<IUsersRepository>();
            var user = repository.Load(id);
            if (user == null)
            {
                return Content(string.Format("Не удалось найти пользователя с идентификатором {0}", id));
            }

            if (user.CompanyId != CurrentUser.CompanyId)
            {
                return Content("Access Denied");
            }

            // Отдаем частичный вид
            return PartialView(new ProfileInfoModel(user));
        }

        /// <summary>
        /// Обрабатывает запрос на изменение статуса пользователям
        /// </summary>
        /// <param name="userIds">строка со с идентификаторами пользователей</param>
        /// <param name="newUsersStatus">Новый статус пользователей</param>
        /// <returns>Переходит на страницу списка сотрудников компании</returns>
        [HttpPost]
        [AuthorizationCheck(Permission.EditCompanyAgents)]
        [Route("account/company/change-user-status")]
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
                            rep.Load).Where(r => r != null).Where(u => u.CompanyId == CurrentUser.CompanyId).ToList();

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
            return RedirectToAction("CompanyMembers");
        }

        /// <summary>
        /// Обрабатывает запрос на изменение статуса пользователям
        /// </summary>
        /// <param name="userIds">строка со с идентификаторами пользователей</param>
        /// <param name="newUsersRole">Новый статус пользователей</param>
        /// <returns>Переходит на страницу списка сотрудников компании</returns>
        [HttpPost]
        [AuthorizationCheck(Permission.EditCompanyAgents)]
        [Route("account/company/change-user-role")]
        public ActionResult ChangeUsersRole(string userIds, int newUsersRole)
        {
            try
            {
                if (newUsersRole != -1)
                {
                    // Пытаем изменить статус пользователей
                    var rep = Locator.GetService<IUsersRepository>();

                    // Находим пользователей
                    var users =
                        userIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                            rep.Load).Where(r => r != null).Where(u => u.CompanyId == CurrentUser.CompanyId).ToList();

                    // Изменяем и сохраняем
                    foreach (var user in users)
                    {
                        user.RoleId = newUsersRole;
                    }
                    rep.SubmitChanges();

                    Locator.GetService<IUINotificationManager>().Success("Изменения в ролях пользователей успешно сохранены");

                    if (users.Count > 0)
                    {
                        PushAuditEvent(AuditEventTypes.Editing, string.Format("Изменение ролей {0} пользователей", users.Count));
                    }
                }
            }
            catch (Exception e)
            {
                var msg = String.Format("Ошибка при измении ролей пользователей {0}: {1}", userIds, e.Message);
                Logger.Error(msg);
                Locator.GetService<IUINotificationManager>().Error(msg);
            }
            return RedirectToAction("CompanyMembers");
        }

        /// <summary>
        /// Изменяет должность указанного сотрудника
        /// </summary>
        /// <param name="userId">Идентификатор пользователя в компании</param>
        /// <param name="appointment">Должность пользователя</param>
        /// <returns></returns>
        [HttpPost]
        [Route("account/company/change-user-appointment")]
        [AuthorizationCheck(Permission.EditUserAppointment)]
        public ActionResult ChangeUserAppointment(long userId, string appointment)
        {
            // Репозиторий
            var rep = Locator.GetService<IUsersRepository>();

            // ищем пользователя
            var user = CurrentUser.Company.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                UINotificationManager.Error("Такой пользователь не найден в вашей компании");
                return RedirectToAction("CompanyMembers");
            }

            // изменяем пользователя
            user.Appointment = appointment;
            user.DateModified = DateTimeZone.Now;
            rep.SubmitChanges();

            PushAuditEvent(AuditEventTypes.Editing, string.Format("Редактирование должности пользователя {0} на {1}", user.ToString(), appointment));
            UINotificationManager.Success("Пользователь был успешно изменен");

            return RedirectToAction("CompanyMembers");
        }

        /// <summary>
        /// Возвращает данные о сертификации указанного пользователя
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("account/company/get-user-certification-data/{id}")]
        public ActionResult GetUserCertificationData(long id)
        {
            // Репозиторий
            var rep = Locator.GetService<IUsersRepository>();

            // ищем пользователя
            var user = CurrentUser.Company.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return Content("Пользователь не найден");
            }

            // Отдаем результат
            return Json(new
            {
                userId = user.Id,
                certificationStartDate = user.CertificationDate.FormatDate(),
                certificationEndDate = user.CertificateEndDate.FormatDate(),
                certificateNumber = user.CertificateNumber
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Обрабатывает изменение данных сертификации указанного пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="certificationStartDate">Дата выдачи сертификата</param>
        /// <param name="certificationEndDate">Дата окончания сертификата</param>
        /// <param name="certificateNumber">Номер сертификата</param>
        /// <returns></returns>
        [HttpPost]
        [Route("account/company/change-user-certification")]
        [AuthorizationCheck(Permission.EditUserCertification)]
        public ActionResult ChangeUserCertificationData(long userId, DateTime? certificationStartDate,
                                                        DateTime? certificationEndDate, string certificateNumber)
        {
            // Репозиторий
            var rep = Locator.GetService<IUsersRepository>();

            // ищем пользователя
            var user = CurrentUser.Company.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                UINotificationManager.Error("Такой пользователь не найден в вашей компании");
                return RedirectToAction("CompanyMembers");
            }

            // изменяем пользователя
            user.CertificationDate = certificationStartDate;
            user.CertificateEndDate = certificationEndDate;
            user.CertificateNumber = certificateNumber;
            user.DateModified = DateTimeZone.Now;
            rep.SubmitChanges();

            PushAuditEvent(AuditEventTypes.Editing, string.Format("Редактирование данных о сертификации пользователя {0}", user.ToString()));
            UINotificationManager.Success("Пользователь был успешно изменен");

            return RedirectToAction("CompanyMembers");
        }

        #endregion

        #region Клиенты компании

        /// <summary>
        /// Отображает страницу со списком клиентов компании
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ViewCompanyClients)]
        [Route("account/company/clients")]
        public ActionResult CompanyClients()
        {
            if (CurrentUser.Company == null)
            {
                RedirectToAction("Index");
            }

            PushAccountNavigationItem();
            PushNavigationItem("Компания", "Сведения о компании, в которой состоит текущий пользователь", "/account/company/");
            PushNavigationItem("Клиенты", "Клиенты, работающие с компанией", "/account/company/clients", false);

            // Репозиторий
            var rep = Locator.GetService<IClientsRepository>();

            // Ищем всех клиентов, которые относятся к текущей компании
            var clients =
                rep.Search(c => c.CompanyId == CurrentUser.CompanyId).OrderByDescending(d => d.DateModified).ThenByDescending(d => d.DateCreated).ToList();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка клиентов своей компании");

            // Отдаем данные во вью
            return View(clients);
        }

        /// <summary>
        /// Отображает страницу добавления нового клиента в базу
        /// </summary>
        /// <returns></returns>
        [Route("account/company/clients/add")]
        [AuthorizationCheck(Permission.ViewCompanyClients)]
        public ActionResult AddClient()
        {
            if (CurrentUser.Company == null)
            {
                RedirectToAction("Index");
            }

            // Навигационная цепочка
            PushAccountNavigationItem();
            PushNavigationItem("Компания", "Сведения о компании, в которой состоит текущий пользователь", "/account/company/");
            PushNavigationItem("Клиенты", "Клиенты, работающие с компанией", "/account/company/clients", true);
            PushNavigationItem("Новый клиент", "", "", false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Начало добавления клиента в список клиентов компании");

            return View("EditClient", new EditClientModel());
        }

        /// <summary>
        /// Отображает форму редатирования клиента
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>
        /// <returns>Форма редактирования клиента</returns>
        [Route("account/company/clients/edit/{id}")]
        [AuthorizationCheck(Permission.ViewCompanyClients)]
        public ActionResult EditClient(long id)
        {
            if (CurrentUser.Company == null)
            {
                RedirectToAction("Index");
            }

            // Репозиторий
            var rep = Locator.GetService<IClientsRepository>();

            // Ищем клиента
            var client = rep.Load(id);
            if (client == null)
            {
                UINotificationManager.Error(String.Format("Клиент с идентификатором {0} не найден", id));
                return RedirectToAction("CompanyClients");
            }

            // Навигационная цепочка
            PushAccountNavigationItem();
            PushNavigationItem("Компания", "Сведения о компании, в которой состоит текущий пользователь", "/account/company/");
            PushNavigationItem("Клиенты", "Клиенты, работающие с компанией", "/account/company/clients", true);
            PushNavigationItem("Просмотр данных клиента " + client.ToString(), "", "", false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Начало добавления клиента в список клиентов компании");

            return View("EditClient", new EditClientModel(client));
        }

        /// <summary>
        /// Обрабатывает создание или сохранение изменений в клиенте
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("account/company/clients/save")]
        [AuthorizationCheck(Permission.ViewCompanyClients)]
        public ActionResult SaveClient(EditClientModel model)
        {
            if (CurrentUser.Company == null)
            {
                RedirectToAction("Index");
            }

            // Репозиторий
            var rep = Locator.GetService<IClientsRepository>();

            // Проверяем что у нас, создание или редактирование
            if (model.Id <= 0)
            {
                // Создаем нового клиента
                var newClient = new Client()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName ?? "",
                    SurName = model.SurName,
                    Email = model.Email,
                    ICQ = model.ICQ,
                    Phone = model.Phone,
                    Address = model.Address,
                    ClientType = (short)model.ClientType,
                    CompanyId = CurrentUser.CompanyId,
                    DateCreated = DateTimeZone.Now,
                    CreatedBy = CurrentUser.Id,
                    ModifiedBy = -1,
                    Notes = model.Notes
                };
                rep.Add(newClient);
                rep.SubmitChanges();

                // Событие аудита
                PushAuditEvent(AuditEventTypes.Editing, "Создание нового клиента " + newClient.ToString());
            }
            else
            {
                // Ищем клиента
                var client = rep.Load(model.Id);
                if (client == null)
                {
                    UINotificationManager.Error(String.Format("Клиент с идентификатором {0} не найден", model.Id));
                    return RedirectToAction("CompanyClients");
                }

                // Обновляем информацию
                client.FirstName = model.FirstName;
                client.LastName = model.LastName;
                client.SurName = model.SurName;
                client.Email = model.Email;
                client.ICQ = model.ICQ;
                client.Phone = model.Phone;
                client.Address = model.Address;
                client.ClientType = (short)model.ClientType;
                client.PaymentConditions = model.PaymentCondition;
                //client.CompanyId = CurrentUser.CompanyId;
                client.DateModified = DateTimeZone.Now;
                client.ModifiedBy = CurrentUser.Id;
                rep.SubmitChanges();

                // Событие аудита
                PushAuditEvent(AuditEventTypes.Editing, "Редактирование клиента " + client.ToString());
            }

            UINotificationManager.Success("Изменения успешно сохранены");
            return RedirectToAction("CompanyClients");
        }

        /// <summary>
        /// Обрабатывает удаление клиентов из системы
        /// </summary>
        /// <param name="clientIds">Идентификаторы клиенто</param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck(Permission.ViewCompanyClients)]
        [Route("account/company/clients/delete")]
        public ActionResult DeleteClients(string clientIds)
        {
            try
            {
                // Репозитории
                var repository = Locator.GetService<IClientsRepository>();

                var clients =
                    clientIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).Select(
                        repository.Load).Where(r => r != null).ToList();

                // Удаляем
                var deleted = 0;
                foreach (var client in clients)
                {
                    try
                    {
                        repository.Delete(client);
                        repository.SubmitChanges();
                        deleted++;
                    }
                    catch (Exception e)
                    {
                        UINotificationManager.Error(string.Format("Не удалось удалить клиента {0}. Возможно с клиентом связан какой либо объект.", client.ToString()));
                        continue;
                    }

                }
                repository.SubmitChanges();

                Locator.GetService<IUINotificationManager>().Success("Изменения успешно сохранены");

                if (deleted > 0)
                {
                    // Событие аудита
                    PushAuditEvent(AuditEventTypes.Editing, String.Format("Удаление {0} клиентов", deleted));
                }

                // Возвращаем результат
                return Json(new { success = true });
            }
            catch (Exception e)
            {
                Logger.Error(String.Format("Ошибка при удалении клиентов: {0}", e.Message));
                Response.StatusCode = 500;
                return Json(new { success = false, message = e.Message });
            }
        }

        /// <summary>
        /// Возвращает на клиент список всех клиентов пользовательской компании или список всех клиентов системы
        /// </summary>
        /// <param name="term">Часть строки для поиска</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/company/clients-autocomplete")]
        public ActionResult CompanyClientsAjax(string term)
        {
            // Репозиторий
            var clientsRep = Locator.GetService<IClientsRepository>();

            // Подготовка
            term = term.ToLower();
            /*var clients = CurrentUser.HasPermission(Permission.EditAllObjects)
                              ? clientsRep.FindAll()
                              : CurrentUser.Company.Clients;*/
            // Подгатавливаем список клиента объектов текущего пользователя

            List<Client> clients = null;

            if (CurrentUser.RoleId == 6 || CurrentUser.RoleId == 7)
            {
                clients = CurrentUser.Company.Clients.ToList();
            }
            else
            {
                clients =
                CurrentUser.EstateObjects.Select(o => o.ClientId)
                           .Select(i => clientsRep.Load(i))
                           .Where(c => c != null)
                           .ToList();
                var statusClients = from obj in CurrentUser.EstateObjects
                                    from item in obj.ObjectHistoryItems
                                    let client = clientsRep.Load(item.ClientId)
                                    where client != null
                                    select client;
                clients.AddRange(statusClients);

                // Добавляем клиентов, которых пользователь создал
                var createdClients = from client in CurrentUser.Company.Clients
                                     where client.CreatedBy == CurrentUser.Id
                                     select client;
                clients.AddRange(createdClients);    
            }
            

            // Удаляем дубликаты если есть
            foreach (var dublicateClient in clients.ToList().Select(client1 => clients.Where(c => c.Id == client1.Id).ToList()).Where(dublicateClients => dublicateClients.Count >= 2).SelectMany(dublicateClients => dublicateClients.Skip(1)))
            {
                clients.Remove(dublicateClient);
            }

            // Новый клиент
            var newClient = new
            {
                name = "Новый клиент",
                id = "-1"
            };

            // Фильтруем клиентов
            var fetchedClients = clients.Where(c => c.ToString().ToLower().Contains(term)).Select(c => new
            {
                name = String.Format("{0} ({1})", c.ToString(), c.Phone.FormatPhoneNumber()),
                id = c.Id.ToString()
            }).ToList();
            fetchedClients.Insert(0, newClient);

            // Отдаем список
            return Json(fetchedClients, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Возвращает на клиент список всех клиентов пользовательской компании или список всех клиентов системы
        /// </summary>
        /// <param name="term">Часть строки для поиска</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/company/non-rdv-agents-autocomplete")]
        public ActionResult NonRdvAgentsAjax(string term)
        {
            // Репозиторий
            var agentsRep = Locator.GetService<INonRdvAgentsRepository>();

            // Подготовка
            term = term.ToLower();

            List<NonRdvAgent> agents = null;

            agents = agentsRep.FindAll().ToList();

            // Новый агент
            var newAgent = new
            {
                name = "Новый агент",
                id = "-1"
            };

            // Фильтруем клиентов
            var fetchedAgents = agents.Where(c => c.ToString().ToLower().Contains(term)).Select(c => new
            {
                name = String.Format("{0} ({1})", c.ToString(), c.Phone.FormatPhoneNumber()),
                id = c.Id.ToString()
            }).ToList();
            fetchedAgents.Insert(0, newAgent);

            // Отдаем список
            return Json(fetchedAgents, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Обрабатывает аяксовое создание нового клиента и возвращает его данные на клиент
        /// </summary>
        /// <param name="model">Модель данных по клиенту</param>
        /// <returns></returns>
        [Route("account/company/clients/new-ajax")]
        [HttpPost]
        [AuthorizationCheck()]
        public JsonResult CreateCompanyClientAjax(AjaxNewClientModel model)
        {
            // Репозиторий
            var rep = Locator.GetService<IClientsRepository>();

            // Проверяем клиента по имени, фамилии, отчеству и номеру телефона
            var existedClients = rep.Search(c => (c.FirstName ?? "").Trim().ToLower() == (model.FirstName ?? "").Trim().ToLower() &&
                                                (c.SurName ?? "").Trim().ToLower() == (model.SurName ?? "").Trim().ToLower() &&
                                                (c.LastName ?? "").Trim().ToLower() == (model.LastName ?? "").Trim().ToLower()).ToList();
            if (existedClients.Count > 0)
            {
                // Ищем совпадение по номеру телефона
                var existedClient =
                    existedClients.FirstOrDefault(c => c.Phone.FormatPhoneNumber() == model.Phone.FormatPhoneNumber());
                if (existedClient != null)
                {
                    // Отдаем данные
                    return Json(new { success = true, id = existedClient.Id, name = String.Format("{0} ({1})", existedClient.ToString(), existedClient.Phone.FormatPhoneNumber()) });
                }
            }


            // Создаем нового клиента
            var newClient = new Client()
            {
                FirstName = model.FirstName,
                LastName = model.LastName ?? "",
                SurName = model.SurName,
                Email = model.Email,
                ICQ = model.ICQ,
                Phone = model.Phone,
                Address = model.Address,
                ClientType = model.ClientType,
                AgreementType = model.AgreementType,
                AgencyPayment = model.Payment,
                AgreementDate = model.AgreementStartDate,
                AgreementEndDate = model.AgreementEndDate,
                AgreementNumber = model.AgreementNumber,
                Birthday = model.Birthdate,
                Commision = model.Comission,
                PaymentConditions = model.PaymentCondition,
                Blacklisted = model.Blacklisted,
                CompanyId = CurrentUser.CompanyId,
                DateCreated = DateTimeZone.Now,
                CreatedBy = CurrentUser.Id,
                ModifiedBy = -1,
                Notes = model.Notes
            };
            rep.Add(newClient);
            rep.SubmitChanges();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.Editing, "Создание нового клиента " + newClient.ToString());

            // Отдаем данные
            return Json(new { success = true, id = newClient.Id, name = String.Format("{0} ({1})", newClient.ToString(), newClient.Phone.FormatPhoneNumber()) });
        }

        /// <summary>
        /// Обрабатывает аяксовое создание нового клиента и возвращает его данные на клиент
        /// </summary>
        /// <param name="model">Модель данных по клиенту</param>
        /// <returns></returns>
        [Route("account/new-non-rdv-agent")]
        [HttpPost]
        [AuthorizationCheck()]
        public JsonResult CreateNonRdvAgentAjax(AjaxNewClientModel model)
        {
            // Репозиторий
            var rep = Locator.GetService<INonRdvAgentsRepository>();

            // Проверяем клиента по имени, фамилии, отчеству и номеру телефона
            var existedAgents = rep.Search(c => (c.FirstName ?? "").Trim().ToLower() == (model.FirstName ?? "").Trim().ToLower() &&
                                                (c.SurName ?? "").Trim().ToLower() == (model.SurName ?? "").Trim().ToLower() &&
                                                (c.LastName ?? "").Trim().ToLower() == (model.LastName ?? "").Trim().ToLower()).ToList();
            if (existedAgents.Count > 0)
            {
                // Ищем совпадение по номеру телефона
                var existedAgent =
                    existedAgents.FirstOrDefault(c => c.Phone.FormatPhoneNumber() == model.Phone.FormatPhoneNumber());
                if (existedAgent != null)
                {
                    // Отдаем данные
                    return Json(new { success = true, id = existedAgent.Id, name = String.Format("{0} ({1})", existedAgent.ToString(), existedAgent.Phone.FormatPhoneNumber()) });
                }
            }


            // Создаем нового клиента
            var newAgent = new NonRdvAgent()
            {
                FirstName = model.FirstName,
                LastName = model.LastName ?? "",
                SurName = model.SurName,
                Phone = model.Phone,
                DateCreated = DateTimeZone.Now
            };
            rep.Add(newAgent);
            rep.SubmitChanges();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.Editing, "Создание нового агента не члена РДВ " + newAgent.ToString());

            // Отдаем данные
            return Json(new { success = true, id = newAgent.Id, name = String.Format("{0} ({1})", newAgent.ToString(), newAgent.Phone.FormatPhoneNumber()) });
        }

        #endregion

        #region Списки объектов

        /// <summary>
        /// Фильтрует объекты на основании привилегий пользователя для просмотра объектов в соответствующем разделе
        /// </summary>
        /// <param name="inputList">Входной список</param>
        /// <param name="listLocation">Раздел, где просматриваем объекты</param>
        /// <returns>Отфильтрованный список</returns>
        private IList<EstateObject> FilterObjects(IEnumerable<EstateObject> inputList, ObjectsListLocation listLocation)
        {
            // Перебираем объекты, создавая новый список в памяти
            var resultList = new List<EstateObject>();
            foreach (var estateObject in inputList)
            {
                // Общий результат решения, относительно данного объекта
                bool needToAdd = true;

                // Критерии, зависящие от того, в каком разделе происходит выборка
                switch (listLocation)
                {
                    case ObjectsListLocation.MyObjects:
                        needToAdd = CurrentUser.HasObjectRelatedPermission(Permission.EditOwnObjects, estateObject.Operation, estateObject.ObjectType);
                        break;
                    case ObjectsListLocation.Favourites:
                        needToAdd = true; // Можно просматривать все избранные объекты
                        break;
                    case ObjectsListLocation.CompanyObjects:
                        needToAdd = CurrentUser.HasObjectRelatedPermission(Permission.EditCompanyObjects, estateObject.Operation, estateObject.ObjectType);
                        break;
                    case ObjectsListLocation.AllObjects:
                        needToAdd = CurrentUser.HasObjectRelatedPermission(Permission.EditAllObjects, estateObject.Operation, estateObject.ObjectType);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("listLocation");
                }

                // Если решили добавить то добавляем
                if (needToAdd)
                {
                    resultList.Add(estateObject);
                }
            }

            // Отдаем отфильтрованный список
            return resultList;
        }

        /// <summary>
        /// Возвращает HTML с детальной информационей об указанном объекте с расположением объекта на карте
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/objects/get-details/{id}")]
        public ActionResult GetObjectDetails(long id)
        {
            try
            {
                // Репозиторий
                var objectsRep = Locator.GetService<IEstateObjectsRepository>();

                // Ищем объект
                var obj = objectsRep.Load(id);
                if (obj == null)
                {
                    return Content("Объект не найден");
                }

                // Отдаем частичный вид
                return PartialView("ObjectDetails", obj);
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }
        }

        /// <summary>
        /// Отображает страницу со списком своих объектов
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.EditOwnObjects)]
        [Route("account/objects/my")]
        public ActionResult MyObjects()
        {
            return RedirectToAction("Index", "ObjectsList", new {section = ObjectsListSection.MyObjects});


            PushAccountNavigationItem();
            PushNavigationItem("Мои объекты", "Список добавленных мной объектов", "/account/objects/my", false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка своих объектов");

            // Формируем список
            var objects = ObjectsRepository.GetActiveObjects().Where(o => o.UserId == CurrentUser.Id);
            IList<EstateObject> filtredObjects = FilterObjects(objects, ObjectsListLocation.MyObjects);

            // Достаем первые 50 объектов для формирования списка, остальные подгружаются аяксом при прокрутке
            var model = new ObjectsListViewModel()
            {
                ListLocation = ObjectsListLocation.MyObjects,
                TotalObjectsCount = filtredObjects.Count,
                LoadedObjects = filtredObjects.OrderByDescending(o => o.DateModified).ThenByDescending(d => d.DateCreated).Select(o => new EstateObjectModel(o)).ToList(),
                CompanyAgents = CurrentUser.Company != null ? CurrentUser.Company.Users.Where(u => u.Status == (short)UserStatuses.Active).ToList() : new List<User>()
            };

            // Отображаем вид
            return View(model);
        }

        /// <summary>
        /// Отображает страницу со списком объектов, добавленных в избранное
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ViewObjects)]
        [Route("account/objects/favourites")]
        public ActionResult FavouriteObjects()
        {
            PushAccountNavigationItem();
            PushNavigationItem("Избранные объекты", "Список избранных объектов системы", "/account/objects/favourite", false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка избранных объектов");

            // Формируем список
            var objects = ObjectsRepository.GetActiveObjects().Where(o => o.UserId == CurrentUser.Id);
            IList<EstateObject> filtredObjects = FilterObjects(objects, ObjectsListLocation.Favourites);

            // Достаем первые 50 объектов для формирования списка, остальные подгружаются аяксом при прокрутке
            var model = new ObjectsListViewModel()
            {
                ListLocation = ObjectsListLocation.Favourites,
                TotalObjectsCount = filtredObjects.Count,
                LoadedObjects = filtredObjects.OrderByDescending(o => o.DateModified).ThenByDescending(d => d.DateCreated).Select(o => new EstateObjectModel(o)).ToList(),
                CompanyAgents = CurrentUser.Company != null ? CurrentUser.Company.Users.Where(u => u.Status == (short)UserStatuses.Active).ToList() : new List<User>()
            };

            // Отображаем вид
            return View(model);
        }

        /// <summary>
        /// Отображает страницу со списком объектов, добавленных всеми риелторами компании
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ViewObjects)]
        [Route("account/objects/company")]
        public ActionResult CompanyObjects()
        {
            return RedirectToAction("Index", "ObjectsList", new { section = ObjectsListSection.CompanyObjects });

            if (CurrentUser.Company == null)
            {
                return RedirectToAction("Index");
            }

            PushAccountNavigationItem();
            PushNavigationItem("Объекты компании", "Список избранных объектов компании", "/account/objects/company", false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка объектов своей компании");

            // Формируем список
            var objects = ObjectsRepository.GetActiveObjects().Where(o => o.User.Company != null && o.User.Company.Id == CurrentUser.CompanyId);
            IList<EstateObject> filtredObjects = FilterObjects(objects, ObjectsListLocation.CompanyObjects);

            // Достаем первые 50 объектов для формирования списка, остальные подгружаются аяксом при прокрутке
            var model = new ObjectsListViewModel()
            {
                ListLocation = ObjectsListLocation.CompanyObjects,
                TotalObjectsCount = filtredObjects.Count,
                LoadedObjects = filtredObjects.OrderByDescending(o => o.DateModified).ThenByDescending(d => d.DateCreated).Select(o => new EstateObjectModel(o)).ToList(),
                CompanyAgents = CurrentUser.Company != null ? CurrentUser.Company.Users.Where(u => u.Status == (short)UserStatuses.Active).ToList() : new List<User>()
            };

            // Отображаем вид
            return View(model);
        }

        /// <summary>
        /// Отображает страницу со списком всех объектов системы
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ViewObjects)]
        [Route("account/objects/all")]
        public ActionResult AllObjects()
        {
            return RedirectToAction("Index", "ObjectsList", new { section = ObjectsListSection.AllObjects });

            PushAccountNavigationItem();
            PushNavigationItem("Все объекты", "Список всех объектов системы", "/account/objects/all", false);

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка всех объектов системы");

            // Формируем список
            var objects = ObjectsRepository.GetActiveObjects();
            IList<EstateObject> filtredObjects = FilterObjects(objects, ObjectsListLocation.AllObjects);

            // Достаем первые 50 объектов для формирования списка, остальные подгружаются аяксом при прокрутке
            var model = new ObjectsListViewModel()
            {
                ListLocation = ObjectsListLocation.AllObjects,
                TotalObjectsCount = filtredObjects.Count,
                LoadedObjects = filtredObjects.OrderByDescending(o => o.DateModified).ThenByDescending(d => d.DateCreated).Select(o => new EstateObjectModel(o)).ToList(),
                CompanyAgents = CurrentUser.Company != null ? CurrentUser.Company.Users.Where(u => u.Status == (short)UserStatuses.Active).ToList() : new List<User>()
            };

            // Отображаем вид
            return View(model);
        }

        /// <summary>
        /// Возвращает отфильтрованный список для секции согласно указанным условиям
        /// </summary>
        /// <param name="section">Секция, для которой требуется построить фильтр</param>
        /// <param name="filter">Строка содержащая данные для фильтрации</param>
        /// <returns></returns>
        public ActionResult ObjectsFilter(ObjectsListLocation section, string filter, short statusFilter)
        {
            // Строим критерии для фильтрации
            var convertedFilter = new StringBuilder(filter);
            convertedFilter.Replace("selling", ((short)EstateOperations.Selling).ToString(CultureInfo.InvariantCulture))
                .Replace("buying", ((short)EstateOperations.Buying).ToString(CultureInfo.InvariantCulture))
                .Replace("lising", ((short)EstateOperations.Lising).ToString(CultureInfo.InvariantCulture))
                .Replace("rent", ((short)EstateOperations.Rent).ToString(CultureInfo.InvariantCulture))
                .Replace("rooms", ((short)EstateTypes.Room).ToString(CultureInfo.InvariantCulture))
                .Replace("flat", ((short)EstateTypes.Flat).ToString(CultureInfo.InvariantCulture))
                .Replace("houses", ((short)EstateTypes.House).ToString(CultureInfo.InvariantCulture))
                .Replace("land", ((short)EstateTypes.House).ToString(CultureInfo.InvariantCulture))
                .Replace("office", ((short)EstateTypes.House).ToString(CultureInfo.InvariantCulture))
                .Replace("garages", ((short)EstateTypes.House).ToString(CultureInfo.InvariantCulture));
            var criterias = convertedFilter.ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            // Проводим изначальную фильтрацию объектов
            IEnumerable<EstateObject> objects = null;
            switch (section)
            {
                case ObjectsListLocation.MyObjects:
                    objects = ObjectsRepository.GetActiveObjects().Where(o => o.UserId == CurrentUser.Id);
                    break;
                case ObjectsListLocation.Favourites:
                    objects = ObjectsRepository.GetActiveObjects().Where(o => o.UserId == CurrentUser.Id);
                    break;
                case ObjectsListLocation.CompanyObjects:
                    objects = ObjectsRepository.GetActiveObjects().Where(o => o.UserId == CurrentUser.Id);
                    break;
                case ObjectsListLocation.AllObjects:
                    objects = ObjectsRepository.GetActiveObjects();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("section");
            }

            // Проводим вторичную фильтрацию объектов на основании пользовательских привилегий
            var filteredObjects = FilterObjects(objects, section);

            // Проводим третичную фильтрацию объект на основании указанного пользователем фильтра
            // Подгатавливаем критерии для фильтрации
            IList<Func<EstateObject, bool>> critesriasFuncs = new List<Func<EstateObject, bool>>();
            foreach (var criteria in criterias.Where(c => c.Contains("_")))
            {
                // Извлекаем основные параметры фильтрации
                var parts = criteria.Split('_');
                var requiredOperation = Convert.ToInt16(parts[0]);
                var requiredType = Convert.ToInt16(parts[1]);
                if (parts.Length == 2)
                {
                    Func<EstateObject, bool> criteriaFunc = (estateObject) => estateObject.Operation == requiredOperation &&
                                                                              estateObject.ObjectType == requiredType;
                    critesriasFuncs.Add(criteriaFunc);
                }
                else
                {
                    var roomsRequired = Convert.ToInt32(parts[2]);
                    if (roomsRequired >= 4)
                    {
                        Func<EstateObject, bool> criteriaFunc = (estateObject) => estateObject.Operation == requiredOperation &&
                                                                                  estateObject.ObjectType == requiredType && estateObject.ObjectAdditionalProperties.RoomsCount.HasValue && estateObject.ObjectAdditionalProperties.RoomsCount.Value >= 4;
                        critesriasFuncs.Add(criteriaFunc);
                    }
                    else
                    {
                        Func<EstateObject, bool> criteriaFunc = (estateObject) => estateObject.Operation == requiredOperation &&
                                                                                  estateObject.ObjectType == requiredType && estateObject.ObjectAdditionalProperties.RoomsCount.HasValue && estateObject.ObjectAdditionalProperties.RoomsCount.Value == roomsRequired;
                        critesriasFuncs.Add(criteriaFunc);
                    }
                }
            }
            // Прогоняем недавно отфильтрованный список объектов через наши критерии
            IList<EstateObject> finalList = new List<EstateObject>();
            if (filter.Contains("all"))
            {
                finalList = filteredObjects;
            }
            else
            {
                foreach (var filteredObject in filteredObjects)
                {
                    var accept = false;
                    foreach (var critesriasFunc in critesriasFuncs)
                    {
                        accept = critesriasFunc(filteredObject);
                        if (accept)
                        {
                            finalList.Add(filteredObject);
                            break;
                        }
                    }
                }
            }

            // Отдаем отфильтрованный список
            var model = new ObjectsListViewModel()
            {
                ListLocation = section,
                TotalObjectsCount = finalList.Count,
                LoadedObjects = finalList.Select(o => new EstateObjectModel(o)).ToList(),
                CompanyAgents = CurrentUser.Company != null ? CurrentUser.Company.Users.Where(u => u.Status == (short)UserStatuses.Active).ToList() : new List<User>(),
                CustomStatusFilter = (EstateStatuses)statusFilter
            };

            // Отображаем вид
            return PartialView("ObjectsTableContent", model);
        }

        /// <summary>
        /// Обрабатывает изменение статуса у объекта
        /// </summary>
        /// <param name="objectIds">Идентификаторы объектов</param>
        /// <param name="newStatus">Новый статус</param>
        /// <param name="removalReason">Причина снятия с продажи для статусов снято с продажи или временно снято с продажи</param>
        /// <param name="delayDate">Дата до которой объект временно снят с продажи</param>
        /// <param name="location">Локация, куда потом переходить</param>
        /// <param name="counteragentType">Тип контрагент для статуса аванс и сделка </param>
        /// <param name="rdvCounterAgentCompany">Идентификатор компании котрагента - члена РДВ </param>
        /// <param name="counterAgentCompany">Имя контрагента не члена РДВ </param>
        /// <param name="clientId">Идентификатор физического лица - клиента </param>
        /// <param name="dealDate">Дата заключения сделки </param>
        /// <param name="advanceDate">Дата внесения аванса</param>
        /// <param name="realPrice">Реальная цена </param>
        /// <param name="mortageBank">Ипотека банка</param>
        /// <param name="statusSection">Секция, куда перейти потом</param>
        /// <returns>Переходит обратно на список откуда была вызвана</returns>
        [AuthorizationCheck()]
        [Route("account/objects/change-status")]
        public ActionResult ChangeObjectsStatus(string objectIds, short newStatus, long removalReason, DateTime? delayDate, ObjectsListSection location, int counteragentType, long rdvCounterAgentCompany, string counterAgentCompany, long clientId, DateTime? dealDate, DateTime? advanceDate, double? realPrice, long mortageBank, string statusSection, long rdvAgentId, long nonRdvAgentId, DateTime? advanceEndDate, EstateTypes estateType)
        {
            // Репозитории
            var objectsRep = Locator.GetService<IEstateObjectsRepository>();

            // Выбираем список объектов
            var objects =
                objectIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => Convert.ToInt64(s)).
                    Select(objectsRep.Load).Where(o => o != null).ToList();

            // Активированные объекты
            var activatedObjects = new List<EstateObject>();

            ObjectHistoryItem historyItem = null; // Элемент истории

            // Перебираем объекты и поочереди устанавливаем статус
            foreach (var estateObject in objects)
            {
                // Проверяем, не стоит ли наш объект в статусе сделка
                if (estateObject.Status == (short)EstateStatuses.Deal && !CurrentUser.IsAdministrator)
                {
                    UINotificationManager.Error(string.Format("Объект №{0} находится в статусе Сделка, поэтому дальнейшее изменение его статусов невозможно", estateObject.Id));
                    continue;
                }

                // Определяем тип требуемого пермишенна
                long targetPermission;
                if (estateObject.UserId == CurrentUser.Id)
                {
                    targetPermission = Permission.ChangeOwnObjectsStatus;
                }
                else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
                {
                    targetPermission = Permission.ChangeCompanyObjectsStatus;
                }
                else
                {
                    targetPermission = Permission.ChangeAllObjectsStatus;
                }

                // Проверяем, есть ли у нас пермишены
                if (newStatus > (short)EstateStatuses.Active && !PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
                {
                    UINotificationManager.Error(String.Format("Недостаточно правилегий на изменение статуса объекта №{0}", estateObject.Id));
                    continue;
                }

                // Проверяем не находится ли объект в статусе сделка
                if (estateObject.Status == (short) EstateStatuses.Deal)
                {

                    UINotificationManager.Error(String.Format("Объект №{0} не может изменить статус, поскольку он не находится в статусе \"Сделка\"", estateObject.Id));
                    continue;
                }

                switch ((EstateStatuses)newStatus)
                {
                    case EstateStatuses.Draft:
                        // Нельзя возвращать зарегистрированные объекты обратно в статус черновик
                        if (estateObject.Status > (short)EstateStatuses.Draft && !CurrentUser.IsAdministrator)
                        {
                            UINotificationManager.Error(String.Format("Объект №{0} уже был зарегистрирован как активный, и не может быть возвращен в статус черновик", estateObject.Id));
                            continue;
                        }
                        estateObject.Status = (short)EstateStatuses.Draft;
                        UINotificationManager.Success(string.Format("Объекту №{0} успешно установлен статус Черновик", estateObject.Id));
                        break;
                    case EstateStatuses.Active:
                        if (estateObject.Status == (short)EstateStatuses.Draft)
                        {
                            // NOTE: проводиться валидация объекта
                            var validationError = ActiveStatusValidator.Validate(estateObject, CurrentUser);
                            var valid = validationError.Count == 0;
                            if (valid)
                            {
                                estateObject.Status = (short)EstateStatuses.Active;
                                historyItem = new ObjectHistoryItem()
                                {
                                    ClientId = -1,
                                    CompanyId = -1,
                                    DateCreated = DateTimeZone.Now,
                                    CreatedBy = CurrentUser.Id,
                                    EstateObject = estateObject,
                                    HistoryStatus = (int)EstateStatuses.Active
                                };
                                estateObject.ObjectHistoryItems.Add(historyItem);
                                UINotificationManager.Success(string.Format("Объекту №{0} успешно установлен статус Активный", estateObject.Id));
                                estateObject.ObjectChangementProperties.DateRegisted = DateTimeZone.Now;

                                // Добавляем в список активированных объектов
                                activatedObjects.Add(estateObject);
                            }
                            else
                            {
                                UINotificationManager.Error(string.Format("Невозможно установить статус Активный для объекта №{0}, поскольку: {1}", estateObject.Id, String.Join("; ", validationError)));
                                continue;
                            }
                        }
                        else
                        {
                            // Идет проверка, есть у пользователя право на выполнение этой операции
                            var allow = false;
                            switch (location)
                            {
                                case ObjectsListSection.MyObjects:
                                    allow = CurrentUser.HasPermission(StandartPermissions.ChangeOwnObjectsStatus);
                                    break;
                                case ObjectsListSection.CompanyObjects:
                                    allow = CurrentUser.HasPermission(StandartPermissions.ChangeCompanyObjectsStatus);
                                    break;
                                case ObjectsListSection.AllObjects:
                                    allow = CurrentUser.HasPermission(StandartPermissions.ChangeAllObjectsStatus);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException("location");
                            }
                            if (allow)
                            {
                                estateObject.Status = (short)EstateStatuses.Active;
                                historyItem = new ObjectHistoryItem()
                                {
                                    ClientId = -1,
                                    CompanyId = -1,
                                    DateCreated = DateTimeZone.Now,
                                    CreatedBy = CurrentUser.Id,
                                    EstateObject = estateObject,
                                    HistoryStatus = (int)EstateStatuses.Active
                                };
                                estateObject.ObjectHistoryItems.Add(historyItem);
                                UINotificationManager.Success(string.Format("Объекту №{0} успешно установлен статус Активный", estateObject.Id));
                                estateObject.ObjectChangementProperties.DateRegisted = DateTimeZone.Now;

                                // Добавляем в список активированных объектов
                                activatedObjects.Add(estateObject);
                            }
                            else
                            {
                                UINotificationManager.Error(string.Format("У вас недостаточно прав для того чтобы присвоить объекту №{0} статус Активный", estateObject.Id));
                            }
                        }
                        break;
                    case EstateStatuses.Advance:
                        if (estateObject.Status != (short) EstateStatuses.Active)
                        {
                            UINotificationManager.Error(String.Format("Объект №{0} не может получить статус \"Аванс\", поскольку он не находится в статусе \"Активный\"", estateObject.Id));
                            continue;
                        }
                        // Создаем запись в истории
                        historyItem = new ObjectHistoryItem()
                        {
                            ClientId = -1,
                            CompanyId = -1,
                            DateCreated = DateTimeZone.Now,
                            CreatedBy = CurrentUser.Id,
                            EstateObject = estateObject,
                            HistoryStatus = (int)EstateStatuses.Advance,
                            AdvanceEndDate = advanceEndDate
                        };
                        estateObject.ObjectHistoryItems.Add(historyItem);
                        switch (counteragentType)
                        {
                            case 1:
                                historyItem.CompanyId = rdvCounterAgentCompany;
                                historyItem.RDVAgentId = rdvAgentId;
                                break;
                            case 2:
                                historyItem.CustomerName = counterAgentCompany;
                                historyItem.NonRDVAgentId = nonRdvAgentId;
                                break;
                            case 3:
                                historyItem.ClientId = clientId;
                                break;
                        }
                        // Устанавливаем дополнительные поля
                        estateObject.ObjectChangementProperties.AdvanceDate = advanceDate;
                        estateObject.ObjectMainProperties.RealPrice = realPrice;
                        estateObject.ObjectMainProperties.MortgageBank = mortageBank != -1 ? mortageBank : new long?();
                        // Устанавливаем статус
                        estateObject.Status = (short)EstateStatuses.Advance;
                        UINotificationManager.Success(string.Format("Объекту №{0} успешно установлен статус Аванс", estateObject.Id));
                        break;
                    case EstateStatuses.TemporarilyWithdrawn:
                        // Устанавливаем атрибуты снятия
                        estateObject.ObjectChangementProperties.DelayToDate = delayDate;
                        estateObject.Status = (short)EstateStatuses.TemporarilyWithdrawn;
                        historyItem = new ObjectHistoryItem()
                        {
                            ClientId = -1,
                            CompanyId = -1,
                            DateCreated = DateTimeZone.Now,
                            CreatedBy = CurrentUser.Id,
                            EstateObject = estateObject,
                            HistoryStatus = (int)EstateStatuses.TemporarilyWithdrawn,
                            DelayDate = delayDate
                        };
                        estateObject.ObjectHistoryItems.Add(historyItem);
                        UINotificationManager.Success(string.Format("Объект №{0} временно снят с продажи до {1}", estateObject.Id, delayDate.FormatDate()));
                        break;
                    case EstateStatuses.Withdrawn:
                        // Устанавливаем атрибуты снятия
                        estateObject.ObjectMainProperties.RemovalReason = removalReason;
                        estateObject.ObjectChangementProperties.DelayToDate = null;
                        estateObject.Status = (short)EstateStatuses.Withdrawn;
                        historyItem = new ObjectHistoryItem()
                        {
                            ClientId = -1,
                            CompanyId = -1,
                            DateCreated = DateTimeZone.Now,
                            CreatedBy = CurrentUser.Id,
                            EstateObject = estateObject,
                            HistoryStatus = (int)EstateStatuses.Withdrawn,
                            DelayReason = removalReason
                        };
                        estateObject.ObjectHistoryItems.Add(historyItem);
                        UINotificationManager.Success(string.Format("Объект №{0} снят с продажи по причине: {1}", estateObject.Id, IdObjectsCache.GetDictionaryValue(removalReason)));
                        break;
                    case EstateStatuses.Deal:
                        if (estateObject.Status != (short)EstateStatuses.Advance)
                        {
                            UINotificationManager.Error(String.Format("Объект №{0} не может получить статус \"Сделка\", поскольку он не находится в статусе \"Аванс\"", estateObject.Id));
                            continue;
                        }
                        // Создаем запись в истории
                        historyItem = new ObjectHistoryItem()
                        {
                            ClientId = -1,
                            CompanyId = -1,
                            DateCreated = DateTimeZone.Now, //dealDate.HasValue ? dealDate.Value : DateTimeZone.Now,
                            CreatedBy = CurrentUser.Id,
                            EstateObject = estateObject,
                            HistoryStatus = (int)EstateStatuses.Deal
                        };
                        estateObject.ObjectHistoryItems.Add(historyItem);
                        switch (counteragentType)
                        {
                            case 1:
                                historyItem.CompanyId = rdvCounterAgentCompany;
                                historyItem.RDVAgentId = rdvAgentId;
                                break;
                            case 2:
                                historyItem.CustomerName = counterAgentCompany;
                                historyItem.NonRDVAgentId = nonRdvAgentId;
                                break;
                            case 3:
                                historyItem.ClientId = clientId;
                                break;
                        }
                        // Устанавливаем дополнительные поля
                        estateObject.ObjectChangementProperties.DealDate = advanceDate;
                        historyItem.AdvanceEndDate = advanceEndDate;
                        estateObject.ObjectMainProperties.RealPrice = realPrice;
                        estateObject.ObjectMainProperties.MortgageBank = mortageBank != -1 ? mortageBank : new long?();
                        // Устанавливаем статус
                        estateObject.Status = (short)EstateStatuses.Deal;
                        UINotificationManager.Success(string.Format("Объекту №{0} успешно установлен статус Сделка", estateObject.Id));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("newStatus");
                }

                estateObject.DateModified = DateTimeZone.Now;
                estateObject.ObjectChangementProperties.DateMoved = DateTimeZone.Now;
                estateObject.ObjectChangementProperties.DateModified = DateTimeZone.Now;
            }

            // Сохраняем изменения
            objectsRep.SubmitChanges();

            // Уведомляем систему триггеров о том что объекты были активированы
            var triggerManager = Locator.GetService<IObjectsTriggerManager>();
            foreach (var activated in activatedObjects)
            {
                triggerManager.ObjectActivated(activated);
            }

            // Возвращаемся обратно
            Session["estateType"] = estateType;
            switch (location)
            {
                case ObjectsListSection.MyObjects:
                    return Redirect("/account/objects/my#" + statusSection);
                case ObjectsListSection.CompanyObjects:
                    return Redirect("/account/objects/company#" + statusSection);
                case ObjectsListSection.AllObjects:
                    return Redirect("/account/objects/all#" + statusSection);
                default:
                    throw new ArgumentOutOfRangeException("location");
            }
        }

        /// <summary>
        /// Изменяет агентов у указанных объект ов недвижимости. Данные объекты появляются в списке моих объектов нового агента
        /// </summary>
        /// <param name="objectIds">Идентификатор объектов</param>
        /// <param name="newAgentId">Идентификатор нового агента</param>
        /// <param name="location">Локация куда потом вернуться после операции</param>
        /// <param name="statusSection">Секция, куда перейти потом</param>
        /// <returns>Возвращает на тот список откуда была вызвана изначально</returns>
        [AuthorizationCheck(Permission.ChangeObjectAgent)]
        [Route("account/objects/change-agent")]
        public ActionResult ChangeObjectsAgent(string objectIds, long newAgentId, ObjectsListSection location, string statusSection, EstateTypes estateType)
        {
            // Репозитории
            var objectsRep = Locator.GetService<IEstateObjectsRepository>();
            var usersRep = Locator.GetService<IUsersRepository>();

            // Выбираем список объектов
            var objects =
                objectIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => Convert.ToInt64(s)).
                    Select(objectsRep.Load).Where(o => o != null).ToList();

            var newAgent = usersRep.Load(newAgentId);
            if (newAgent != null)
            {
                // Перебираем объекты и поочереди устанавливаем агента
                foreach (var estateObject in objects)
                {
                    // Проверяем, не стоит ли наш объект в статусе сделка
                    if (estateObject.Status == (short)EstateStatuses.Deal)
                    {
                        UINotificationManager.Error(string.Format("Объект №{0} находится в статусе Сделка, поэтому дальнейшее изменение его агента невозможно", estateObject.Id));
                        continue;
                    }

                    // Проверяем, есть ли у нас пермишены
                    if (!CurrentUser.HasPermission(Permission.ChangeObjectAgent))
                    {
                        UINotificationManager.Error(String.Format("Недостаточно правилегий на изменение агента объекта №{0}", estateObject.Id));
                        continue;
                    }
                    estateObject.User = newAgent;
                    UINotificationManager.Success(String.Format("Объекту №{0} установлен агент {1}", estateObject.Id, newAgent.ToString()));
                }

                // Сохраняем изменения
                objectsRep.SubmitChanges();
            }
            else
            {
                UINotificationManager.Error(string.Format("Агент с идентификатором {0} не найден", newAgentId));
            }


            // Возвращаемся обратно
            Session["estateType"] = estateType;
            switch (location)
            {
                case ObjectsListSection.MyObjects:
                    return Redirect("/account/objects/my#" + statusSection);
                case ObjectsListSection.CompanyObjects:
                    return Redirect("/account/objects/company#" + statusSection);
                case ObjectsListSection.AllObjects:
                    return Redirect("/account/objects/all#" + statusSection);
                default:
                    throw new ArgumentOutOfRangeException("location");
            }
        }

        /// <summary>
        /// Отображает страницу сравнения нескольких объектов
        /// </summary>
        /// <param name="objectIds">Идентификаторы объектов</param>
        /// <returns>Страница содержащая сводную таблицу характеристик объектов</returns>
        [AuthorizationCheck(Permission.ViewObjects)]
        [Route("account/objects/compare")]
        public ActionResult CompareObjects(string objectIds)
        {
            // Репозитории
            var objectsRep = Locator.GetService<IEstateObjectsRepository>();

            // Выбираем список объектов
            var objects =
                objectIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => Convert.ToInt64(s)).
                    Select(objectsRep.Load).Where(o => o != null).ToList();

            // Проверяем, что у нас объекты имеют одинаковый тип и одинаковую операцию
            if (objects.Count > 0)
            {
                var etalon = objects.First();
                var etalonOperation = etalon.Operation;
                var etalonType = etalon.ObjectType;
                if (!(objects.All(o => o.Operation == etalonOperation) & objects.All(o => o.ObjectType == etalonType)))
                {
                    UINotificationManager.Error("Допускается сравнивать лишь объекты одинакового типа с одинаковой операцией");
                    return RedirectToAction("MyObjects");
                }
            }
            else
            {
                UINotificationManager.Error("Не выбраны объекты для сравнения");
                return RedirectToAction("MyObjects");
            }

            // Проверяем корректное количество
            if (objects.Count > 4)
            {
                UINotificationManager.Error("Сравнивать можно не более 4х объектов");
                return RedirectToAction("MyObjects");
            }

            // Формируем модель объектов для сравнения
            var model = objects.Take(4).Select(obj => new ObjectComparationModel(obj, CurrentUser)).ToList();

            return View(model);
        }

        /// <summary>
        /// Обрабатывает удаление объекта
        /// </summary>
        /// <param name="objectIds">Идентификаторы объектов</param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck()]
        [Route("account/objects/delete")]
        public ActionResult DeleteObjects(string objectIds, EstateTypes estateType = EstateTypes.Flat)
        {
            try
            {
                // Репозитории
                var objectsRep = Locator.GetService<IEstateObjectsRepository>();

                // Выбираем список объектов
                var objects =
                    objectIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => Convert.ToInt64(s)).
                        Select(objectsRep.Load).Where(o => o != null).ToList();

                // Проверяем, что у нас объекты имеют одинаковый тип и одинаковую операцию
                if (objects.Count > 0)
                {
                    var success = 0;
                    foreach (var estateObject in objects)
                    {
                        // Проверяем, находиться ли объект в статусе черновик
                        if (estateObject.Status != (short)EstateStatuses.Draft)
                        {
                            UINotificationManager.Error(String.Format("Объект №{0} не находится в статусе черновик, поэтому его удаление невозможно", estateObject.Id));
                            continue;
                        }

                        // Определяем тип требуемого пермишенна
                        long targetPermission;
                        if (estateObject.UserId == CurrentUser.Id)
                        {
                            targetPermission = Permission.DeleteOwnObjects;
                        }
                        else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
                        {
                            targetPermission = Permission.DeleteCompanyObjects;
                        }
                        else
                        {
                            targetPermission = Permission.DeleteAllObjects;
                        }

                        // Проверяем, есть ли у нас пермишены
                        if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
                        {
                            UINotificationManager.Error(String.Format("Недостаточно правилегий на удаление объекта №{0}", estateObject.Id));
                            continue;
                        }
                        try
                        {
                            objectsRep.Delete(estateObject);
                            objectsRep.SubmitChanges();
                        }
                        catch (Exception e)
                        {
                            UINotificationManager.Error(string.Format("Не удалось удалить объект с идентификатором {0}:{1}", estateObject.Id, e.Message));
                        }

                        PushAuditEvent(AuditEventTypes.Editing, string.Format("Удаление объекта с идентификатором {0}", estateObject.Id));
                        success++;
                    }
                    UINotificationManager.Success(string.Format("Успешно удалено {0} объектов", success));
                    Session["estateType"] = estateType;
                }
                else
                {
                    UINotificationManager.Error("Не указаны объекты для удаления");
                }

                return Json(new { success = true });
            }
            catch (Exception e)
            {
                var message = String.Format("Ошибка при удалении объектов для пользователя {0}:{1}",
                                            CurrentUser.ToString(), e.Message);
                Logger.Error(message);
                return Json(new { success = false, message });
            }
        }

        /// <summary>
        /// Обрабатывает обноление объекта
        /// </summary>
        /// <param name="objectIds">Идентификаторы объектов</param>
        /// <returns></returns>
        [HttpPost]
        [AuthorizationCheck()]
        [Route("account/objects/refresh")]
        public ActionResult RefreshObjects(string objectIds, EstateTypes estateType = EstateTypes.Flat)
        {
            try
            {
                // Репозитории
                var objectsRep = Locator.GetService<IEstateObjectsRepository>();

                // Выбираем список объектов
                var objects =
                    objectIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(s => Convert.ToInt64(s)).
                        Select(objectsRep.Load).Where(o => o != null).ToList();

                Int32 section = 0;

                // Проверяем, что у нас объекты имеют одинаковый тип и одинаковую операцию
                if (objects.Count > 0)
                {
                    var success = 0;
                    foreach (var estateObject in objects)
                    {
                        // Проверяем, находиться ли объект в статусе черновик
                        if (estateObject.Status != (short)EstateStatuses.Active)
                        {
                            UINotificationManager.Error(String.Format("Объект №{0} не находится в статусе Активный, поэтому его обновление невозможно", estateObject.Id));
                            continue;
                        }

                        // Определяем тип требуемого пермишенна
                        long targetPermission;
                        if (estateObject.UserId == CurrentUser.Id)
                        {
                            targetPermission = Permission.EditOwnObjects;
                            section = 1;
                        }
                        else if (estateObject.User.Company != null && CurrentUser.Company != null && estateObject.User.CompanyId == CurrentUser.CompanyId)
                        {
                            targetPermission = Permission.EditCompanyAgents;
                            section = 2;
                        }
                        else
                        {
                            targetPermission = Permission.EditAllObjects;
                            section = 3;
                        }

                        // Проверяем, есть ли у нас пермишены
                        if (!PermissionUtils.CheckUserObjectPermission(estateObject, CurrentUser, targetPermission))
                        {
                            UINotificationManager.Error(String.Format("Недостаточно правилегий на обновление объекта №{0}", estateObject.Id));
                            continue;
                        }
                        try
                        {
							estateObject.ModifiedBy = CurrentUser.Id;
							estateObject.DateModified = DateTimeZone.Now;
							estateObject.ObjectChangementProperties.ChangedBy = CurrentUser.Id;
							estateObject.ObjectChangementProperties.DateModified = DateTimeZone.Now;
                            objectsRep.SubmitChanges();
                        }
                        catch (Exception e)
                        {
                            UINotificationManager.Error(string.Format("Не удалось обновить объект с идентификатором {0}:{1}", estateObject.Id, e.Message));
                        }

                        PushAuditEvent(AuditEventTypes.Editing, string.Format("Обновление объекта с идентификатором {0}", estateObject.Id));
                        success++;
                    }
                    UINotificationManager.Success(string.Format("Успешно обновлено {0} объектов", success));
                    Session["estateType"] = estateType;
                }
                else
                {
                    UINotificationManager.Error("Не указаны объекты для обновления");
                }

                return Json(new { success = true, section = section, estateType = (short)estateType });
            }
            catch (Exception e)
            {
                var message = String.Format("Ошибка при обновлении объектов для пользователя {0}:{1}",
                                            CurrentUser.ToString(), e.Message);
                Logger.Error(message);
                return Json(new { success = false, message });
            }
        }
        
        /// <summary>
        /// Формируем набор данных компаний контрагентов и отправляет его в автокомплит
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.ViewObjects)]
        [Route("account/clients/counter-agent-companies")]
        public JsonResult CounterAgentCompanies(string term)
        {
            // Репозиторий объектов
            var objectsRep = Locator.GetService<IEstateObjectsRepository>();
            var counterAgentsRep = Locator.GetService<ICounterAgentsRepository>();

            // Приводим запрос к нижнему регистру
            term = term.ToLower();

            // Хранилище результатов
            var result = counterAgentsRep.SearchCounteragents(term);

            // Убираем дубликаты
            var res = new List<string>();
            foreach (var name in result.Where(name => !res.Contains(name)))
            {
                res.Add(name);
            }

            // Отдаем результат
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Отображает список агентов членов указанной компании
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("account/rdv-agents-list")]
        public ActionResult RdvAgentsList(long id)
        {
            // Грузим компанию
            var compRep = Locator.GetService<ICompaniesRepository>();
            var comp = compRep.Load(id);

            // Получаем всех агентов компании
            var agents = comp.Users.Where(u => u.Status == (short) UserStatuses.Active).ToList();
            return PartialView(agents);
        }

        #endregion

        #region Поисковые запросы

        /// <summary>
        /// Отображает страницу с сохраненными поисковыми запросами текущего пользователя
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/search-requests")]
        public ActionResult SearchRequests()
        {
            var items =
                CurrentUser.SearchRequests.OrderByDescending(r => r.TimesUsed).ThenBy(r => r.DateCreated).ToList();

            PushAccountNavigationItem();
            PushNavigationItem("Поисковые запросы", "Список сохраненных поисковых запросов", "/account/search-requests/");

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка поисковых запросов");

            return View(items);
        }

        /// <summary>
        /// Выполняет переход на указанный поисковый запрос
        /// </summary>
        /// <param name="id">Идентификатор запроса</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/search-request/{id}")]
        public ActionResult SearchRequest(long id)
        {
            // Ищем запрос
            var request = Locator.GetService<ISearchRequestsRepository>().Load(id);
            if (request == null)
            {
                UINotificationManager.Error("Такой поисковый запрос не найден");
                return RedirectToAction("Index","SearchRequests");
            }

            // Исполняем запрос
            request.TimesUsed += 1;
            request.DateModified = DateTimeZone.Now;
            Locator.GetService<IUsersRepository>().SubmitChanges();

            // Событие аудита
            PushAuditEvent(AuditEventTypes.ViewPage, string.Format("Использование поискового запроса {0}", request.Title));

            // Формируем запрос
            var query = String.Format("/search/results{0}", request.SearchUrl);
            return Redirect(query);
        }

        /// <summary>
        /// Удаляет указанный поисковый запрос
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/search-requests/delete/{id}")]
        public ActionResult DeleteSearchRequest(long id)
        {
            // Ищем запрос
            var request = CurrentUser.SearchRequests.FirstOrDefault(s => s.Id == id);
            if (request == null)
            {
                UINotificationManager.Error("Такой поисковый запрос не найден");
                return RedirectToAction("SearchRequest");
            }

            // Событие аудита
            PushAuditEvent(AuditEventTypes.Editing, string.Format("Удаление поискового запроса {0}", request.Title));

            // Исполняем запрос
            CurrentUser.SearchRequests.Remove(request);
            Locator.GetService<IUsersRepository>().SubmitChanges();

            UINotificationManager.Success("Поисковый запрос был успешно удален");

            return RedirectToAction("SearchRequests");
        }

        /// <summary>
        /// Отображает список объектов подходящих под указанный поисковый запрос
        /// </summary>
        /// <param name="id">Идентификатор запроса</param>
        /// <returns></returns>
        [AuthorizationCheck()]
        [Route("account/search-requests/get-details/{id}")]
        public ActionResult SearchRequestDetails(long id)
        {
            try
            {
                // Получаем запрос
                var request = CurrentUser.SearchRequests.FirstOrDefault(r => r.Id == id);
                if (request == null)
                {
                    throw new ObjectNotFoundException("Такой поисковый запрос не найден");
                }

                // Отдаем частичный вид
                return PartialView(request);
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }
        }

        /// <summary>
        /// ВЫполняет операции над объектом в поисковом запросе
        /// </summary>
        /// <param name="id">идентификатор</param>
        /// <param name="op">операции</param>
        /// <returns></returns>
        [AuthorizationCheck()][Route("account/search-requests/move-object")]
        public ActionResult SearchRequestMoveObject(long id, string op, string reason = null, bool declinePrice = false)
        {
            // Репозиторий
            var rep = Locator.GetService<ISearchRequestObjectsRepository>();

            // Ищем
            var obj = rep.Load(id);
            if (obj == null)
            {
                return Json(new {success = false, msg = "Объект не найден"});
            }

            // Проверяем
            if (obj.SearchRequest.UserId != CurrentUser.Id)
            {
                return Json(new {success = false, msg = "Ошибка доступа"});
            }

            // Изменяем
            switch (op)
            {
                case "accept":
                    obj.Status = (short) SearchRequestObjectStatus.Accepted;
                    break;
                case "decline":
                    obj.Status = (short)SearchRequestObjectStatus.Declined;
                    obj.DeclineReason = reason;
                    obj.DeclineReasonPrice = declinePrice;
                    obj.OldPrice = obj.EstateObject.ObjectMainProperties.Price;
                    break;
            }
            obj.DateMoved = DateTimeZone.Now;
            rep.SubmitChanges();

            // Отдаем
            return Json(new {success = true, obj = new
                {
                    id = obj.Id,
                    objectId = obj.EstateObject.Id,
                    dateCreated = obj.DateCreated.FormatDate(),
                    reason = obj.DeclineReason,
                    compId = obj.EstateObject.User.Company != null ? obj.EstateObject.User.CompanyId : -1,
                    compTitle = obj.EstateObject.User.Company != null ? obj.EstateObject.User.Company.Name : "Вне компании",
                    compName = obj.EstateObject.User.Company != null ? obj.EstateObject.User.Company.ShortName ?? obj.EstateObject.User.Company.Name : "Вне компании",
                    address = obj.EstateObject.Address.ToShortAddressString(),
                    price = obj.EstateObject.ObjectMainProperties.Price.FormatPrice(),
                    dateRegistred = obj.EstateObject.GetRegistrationDate().FormatDate(),
                    dateMoved = obj.DateMoved.FormatDate(),
                    datePriceChanged = obj.EstateObject.GetPriceChangementDate().FormatDate(),
                    oldPrice = obj.OldPrice.FormatPrice(),
                    declinePrice = obj.DeclineReasonPrice,
                    bonus = obj.EstateObject.ObjectMainProperties.MultilistingBonus.FormatString() ?? "",
                    bonusType = obj.EstateObject.ObjectAdditionalProperties.AgreementType.HasValue && obj.EstateObject.ObjectAdditionalProperties.AgreementType.Value == 354 ? IdObjectsCache.GetDictionaryValue(obj.EstateObject.ObjectMainProperties.MultilistingBonusType) : ""
                }});
        }

        #endregion

        #region Архив партнерства

        /// <summary>
        /// Отображает корневую страницу архива партнерства
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.PartnershipArchive)]
        [Route("account/partnership-archive")]
        public ActionResult PartnershipArchive()
        {
            PushAccountNavigationItem();
            PushNavigationItem("Профиль", "Профиль пользователя", "/account/profile", true);
            PushNavigationItem("Архив партнерства", "Просмотр главного экрана архива партнерства", "/account/partnership-archive", false);

            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр своего кошелька");

            return View();
        }

        /// <summary>
        /// Загружает поля, которые используются при формировании запросов к архиву партнерства
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.PartnershipArchive)]
        [Route("account/partnership-archive/params-select")]
        public ActionResult PartnershipArchiveParams()
        {
            // Формируем список имен полей
            var fieldNames = new string[]
                {
                    "agent-email", "currency", "trade-possibility", "latitude", "longitude", "date-modified",
                    "date-price-modified", "date-moved", "add_commerce_structures", "first-floor-downset", "title",
                    "price-changement", "object-id", "agent-id", "windows-count", "famalies-count", "phone-lines-count",
                    "levels-count", "facade-windows-count", "rent-with-services", "rent-overpayments",
                    "short-description", "furniture", "agent-icq", "release-conditions", "delay-to-date",
                    "status-changed-by", "development-period", "bigroom-area", "land-factical-floor", "full-description"
                    , "prepay-required", "notes", "removal-reason", "residents", "livers-description", "entrance",
                    "metrage-description", "advertising-text-1", "advertising-text-2", "advertising-text-3",
                    "advertising-text-4", "advertising-text-5", "land-relief", "special-offer", "special-offer-text",
                    "ready-percent", "agent-phone-1", "agent-phone-2", "exchange-description", "exchange-required",
                    "foundation", "price-zone", "electric-power", "window-view", "rooms-flat-count", "bedrooms-count",
                    "erkers-count", "roof", "basement", "flat-arrangement", "window-arrangement", "windows", "land-form"
                    , "phones", "entrance-door", "kitchen-description", "ladder", "windows-description",
                    "maintenance-rooms", "floor-rating", "ceiling-rating", "sanfurniture-rating", "wc-rooms",
                    "common-state", "walls-rating", "doors-rating", "tambur-rating"
                };

            // Выбираем список всех полей, которые используются в архиве партнерства
            var fields = FieldsCache.AllSearchFields.Value.Where(f => !fieldNames.Contains(f.Name));

            // Формируем вид
            return PartialView(fields);

        }

        /// <summary>
        /// Выполняет обработку поискового запроса по архиву партнерства
        /// </summary>
        /// <returns></returns>
        [AuthorizationCheck(Permission.PartnershipArchive)]
        [Route("account/partnership-archive/fetch")][HttpPost]
        public ActionResult PartnershipArchiveFetch(long cityId, string districts, string areas, FormCollection collection)
        {
            try
            {
                // Пытаемся выдрать данные
                var searchFormModel = new SearchFormModel();

                // Критерии местоположения
                searchFormModel.CityId = cityId;
                searchFormModel.DistrictIds = districts;
                searchFormModel.AreaIds = areas;


                // Дополнительные критерии
                foreach (var criteriaName in collection.AllKeys.Where(k => k.StartsWith("af_")))
                {
                    var field = FieldsCache.AllSearchFields.Value.FirstOrDefault(f => f.Name == criteriaName.Replace("af_", ""));
                    if (field != null)
                    {
                        searchFormModel.AdditionalCriterias.Add(field);
                        field.Value = collection[criteriaName];
                    }
                }

                // Фильтры дополнительных критериев
                foreach (var criteriaName in collection.AllKeys.Where(k => k.StartsWith("filter")))
                {
                    var field =
                        FieldsCache.AllSearchFields.Value.FirstOrDefault(f => f.Name == criteriaName.Replace("filter_", ""));
                    if (field != null)
                    {
                        searchFormModel.FieldsFilters[field.Name] = collection[criteriaName];
                    }
                }

                // Выполняем поиск
                var foundedObjects = Locator.GetService<IObjectSearchManager>()
                                            .SearchPartnershipArchive(searchFormModel);

                // Проверяем что для всех дополнительных критериев установлены фильтры
                foreach (var searchCriteria in searchFormModel.AdditionalCriterias.Where(searchCriteria => !searchFormModel.FieldsFilters.ContainsKey(searchCriteria.Name)))
                {
                    searchFormModel.FieldsFilters[searchCriteria.Name] = "=";
                }

                ViewBag.searchData = searchFormModel;

                // Отдаем вид
                return PartialView(foundedObjects);
            }
            catch (Exception e)
            {
                return Content(String.Format("Ошибка во время поиска по архиву партнерства: {0}",e.Message));    
            }
            
        }

        #endregion

        #region Изменение цены на объект

        /// <summary>
        /// Запрашивает цену на указанный объект недвижимости
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <returns></returns>
        [HttpPost][Route("account/get-object-price")][AuthorizationCheck()]
        public ActionResult GetObjectPrice(long id)
        {
            // Репозиторий
            var rep = ObjectsRepository;
            var obj = rep.Load(id);
            if (obj == null)
            {
                throw new ObjectNotFoundException(string.Format("Объект с идентификаторо {0} не найден", id));
            }

            return Json(new
                {
                    id = obj.Id,
                    ownerPrice = obj.ObjectMainProperties.OwnerPrice.FormatString(),
                    price = obj.ObjectMainProperties.Price.FormatString()
                });
        }

        /// <summary>
        /// Обрабатывает изменение цены на указанный объект
        /// </summary>
        /// <param name="objectId">Идентификатор объекта</param>
        /// <param name="ownerPrice">Цена хозяина</param>
        /// <param name="price">Цена на объект</param>
        /// <param name="location">Куда переходить</param>
        /// <param name="statusSection">Секция, куда нужно перейти</param>
        /// <returns></returns>
        [Route("account/change-object-price")][AuthorizationCheck()][HttpPost]
        public ActionResult ChangeObjectPrice(long objectId, double ownerPrice, double price, ObjectsListLocation location, string statusSection, EstateTypes estateType = EstateTypes.Flat)
        {
            // Репозиторий
            var rep = ObjectsRepository;
            var estateObject = rep.Load(objectId);
            if (estateObject == null)
            {
                UINotificationManager.Error(string.Format("Объект с идентификаторо {0} не найден", objectId));
                goto finish;
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
                goto finish;
            }

            // Не даем изменять статусы или поля у объектов статусе сделка
            if (estateObject.Status == (short)EstateStatuses.Deal)
            {
                UINotificationManager.Error(string.Format("Объект №{0} имеет статуса Сделка, поэтому не может быть отредактирован", estateObject.Id));
                goto finish;
            }

            // Проверяем было ли проведено изменение в цене
            var priceChanged = false;
            double? oldPrice = estateObject.ObjectMainProperties.Price;

            // Изменяем цены
            estateObject.ObjectMainProperties.OwnerPrice = ownerPrice;
            estateObject.ObjectMainProperties.Price = price;
            estateObject.DateModified = DateTimeZone.Now;
            estateObject.ObjectChangementProperties.PriceChanged = DateTimeZone.Now;
            estateObject.ObjectChangementProperties.DateModified = DateTimeZone.Now;
            rep.SubmitChanges();

            UINotificationManager.Success(string.Format("Цена на объект №{0} была успешно изменена", objectId));

            // Устанавливаем изменение цены
            priceChanged = oldPrice != estateObject.ObjectMainProperties.Price;
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
                if (estateObject.Status >= (short)EstateStatuses.Active)
                {
                    var triggerManager = Locator.GetService<IObjectsTriggerManager>();
                    triggerManager.ObjectPriceChanged(estateObject, estateObject.ObjectMainProperties.Price);
                }
            }

        finish:
            // Возвращаемся обратно
            Session["estateType"] = estateType;
            switch (location)
            {
                case ObjectsListLocation.MyObjects:
                    return Redirect("/account/objects/my#" + statusSection);
                case ObjectsListLocation.Favourites:
                    return Redirect("/account/objects/favourites#" + statusSection);
                case ObjectsListLocation.CompanyObjects:
                    return Redirect("/account/objects/company#" + statusSection);
                case ObjectsListLocation.AllObjects:
                    return Redirect("/account/objects/all#" + statusSection);
                default:
                    throw new ArgumentOutOfRangeException("location");
            }
        }

        #endregion

        #region Мои услуги

        /// <summary>
        /// Отображает страницу с историей вызовов услуг компании
        /// </summary>
        /// <returns></returns>
        [Route("account/company/services")]
        public ActionResult MyServices()
        {
            PushAccountNavigationItem();
            PushNavigationItem("Компания", "Сведения о компании, в которой состоит текущий пользователь", "/account/company/");
            PushNavigationItem("Услуги компании", "Клиенты, работающие с компанией", "/account/company/services", false);

            var services = from service in CurrentUser.Company.ServiceTypes
                           from callItem in service.ServiceLogItems
                           orderby callItem.OrderDate descending
                           select callItem;

            return View(services.ToList());
        }


        #endregion
    }
}
