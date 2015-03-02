using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Mailing.Templates;
using RDV.Domain.Infrastructure.Routing;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.Interfaces.Repositories.Content;
using RDV.Domain.IoC;
using RDV.Web.Classes.Notification.Mail;
using RDV.Web.Models.Account.Company;
using RDV.Web.Models.Account.Profile;
using RDV.Web.Models.MainPage;
using RDV.Web.Models.Objects;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Blob;
using RDV.Domain.Core;

namespace RDV.Web.Controllers
{
    /// <summary>
    /// Контроллер выполняющий всю логику взаимодействия пользователя с главной страницей
    /// </summary>
    public class MainController : BaseController
    {
        /// <summary>
        /// Отображает непосредственно главную страницу
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(String returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            // Репозитории
            var objectsRep = Locator.GetService<IEstateObjectsRepository>();
            var articlesRep = Locator.GetService<IArticlesRepository>();
            var partnersRep = Locator.GetService<IPartnersRepository>();
            var settingsRep = Locator.GetService<ISettingsRepository>();

            // Формируем модель
            var model = new MainPageModel()
                {
                    NewEstateObjects =
                        objectsRep.Search(d =>d.Status == (short)EstateStatuses.Active && d.ObjectMedias.Count > 0 && d.DateCreated > DateTimeZone.Now.AddDays(-14)).OrderBy(o => Guid.NewGuid()).
                            Take(4).Select(o => new EstateObjectListModel(o)).ToList(),
                    Articles = articlesRep.Search(a => a.ArticleType != ArticleTypes.CalendarEvent).OrderByDescending(a => a.PublicationDate).ToList(),
                    CalendarEvents = articlesRep.Search(a => a.ArticleType == ArticleTypes.CalendarEvent && a.PublicationDate.Date > DateTimeZone.Now.Date).ToList(),
                    Partners = partnersRep.Search(p => p.ActiveImageUrl != null || p.InactiveImageUrl != null).OrderBy(p => p.Position).ToList(),
                    BannerHtml = settingsRep.GetValue("s_banners")
                };
            return View(model);
        }

        /// <summary>
        /// Отображает правила пользования системой
        /// </summary>
        /// <returns></returns>
        [Route("rules")]
        public ActionResult Rules()
        {
            return View();
        }

        public ActionResult CopyFileToAzureBlob()
        {
            var storedFilesRep = Locator.GetService<IStoredFilesRepository>();

            var files = storedFilesRep.FindAll().ToList();

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            //foreach (var item in files)
            for (var i = 12317; i < files.Count; i++)
            {
                //i++;//10730
                var item = files[i];
                Boolean isExist = System.IO.File.Exists(@"C:/Users/rassu_000/Desktop/Проекты/rdv/RDV.Web/Files/" + item.ServerFilename);

                if (isExist)
                {
                    using (FileStream fileStream = System.IO.File.Open(@"C:/Users/rassu_000/Desktop/Проекты/rdv/RDV.Web/Files/" + item.ServerFilename, FileMode.Open))
                    {
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                        String containerName = item.ServerFilename.Substring(0, item.ServerFilename.IndexOf('\\'));
                        String fileName = item.ServerFilename.Substring(item.ServerFilename.IndexOf('\\') + 1);

                        CloudBlobContainer container = blobClient.GetContainerReference(containerName.ToLower());

                        CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
                        if (!blockBlob.Exists())
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                fileStream.CopyTo(memoryStream);
                                var bytes = memoryStream.ToArray();

                                blockBlob.UploadFromByteArray(bytes, 0, bytes.Length);
                            }
                        }
                    }
                }
            }

            return Json("success", JsonRequestBehavior.AllowGet);
        }

        #region Члены РДВ

        /// <summary>
        /// Отображает страницу со списком компаний - членов РДВ
        /// </summary>
        /// <returns></returns>
        [Route("members")]
        public ActionResult Members()
        {
            // Получаем списко компаний
            var rep = Locator.GetService<ICompaniesRepository>();
            var comp =
                rep.GetActiveCompanies().AsEnumerable().OrderByDescending(c => c.GetObjectsCount()).ThenBy(c => c.Name).
                    ToList();

            PushNavigationItem("Члены РДВ","Все члены РДВ","/members/");
            PushNavigationItem("Агентства","Все компании члены РДВ","/members/",false);

            // Отдаем вид
            return View(comp);
        }

        /// <summary>
        /// Отображает страницу с информацией о компании
        /// </summary>
        /// <param name="id">Идентификатор компании</param>
        /// <returns></returns>
        [Route("member/{id}")]
        public ActionResult MemberInfo(long id)
        {
            // Репозиторий
            var rep = Locator.GetService<ICompaniesRepository>();
            var comp = rep.Load(id);
            if (comp == null)
            {
                UINotificationManager.Error(String.Format("Компания с идентификатором {0} не найдена",id));
                return RedirectToAction("Members");
            }

            PushNavigationItem("Члены РДВ", "Все члены РДВ", "/members/");
            PushNavigationItem("Агентства", "Все компании члены РДВ", "/members/", true);
            PushNavigationItem(comp.Name,comp.Description,"/member/"+id,false);

            return View(new CompanyProfileModel(comp));
        }

        /// <summary>
        /// Отображает страницу с информацией об агенте
        /// </summary>
        /// <param name="id">Идентификатор агента</param>
        /// <returns></returns>
        [Route("members/users/{id}")]
        public ActionResult AgentInfo(long id)
        {
            // Репозиторий
            var rep = Locator.GetService<IUsersRepository>();
            var user = rep.Load(id);
            if (user == null)
            {
                UINotificationManager.Error(String.Format("Пользователь с идентификатором {0} не найден", id));
                return RedirectToAction("Members");
            }

            PushNavigationItem("Члены РДВ", "Все члены РДВ", "/members/");
            PushNavigationItem("Агентства", "Все компании члены РДВ", "/members/", false);
            PushNavigationItem(user.Company.Name, user.Company.Description, "/member/" + user.CompanyId);
            PushNavigationItem(user.ToString(),"", "/members/user/" + id,false);

            return View(new ProfileInfoModel(user));
        }

        /// <summary>
        /// Обрабатывает заказ звонка на указанный номер
        /// </summary>
        /// <param name="name">Имя</param>
        /// <param name="phone">Телефон</param>
        /// <param name="id">Идентификатор агента</param>
        /// <returns></returns>
        [HttpPost][Route("members/callback")]
        public ActionResult AgentCallback(string name, string phone, long id)
        {
            try
            {
                var rep = Locator.GetService<IUsersRepository>();
                var agent = rep.Load(id);
                if (agent == null)
                {
                    throw new Exception("Агент не найден");
                }
                Locator.GetService<ISMSNotificationManager>().Notify(agent, String.Format("{0} заказал звонок на номер {1} из вашего профиля", name, phone));
                return Content("OK");
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                return Content(e.Message);
            }
        }

        /// <summary>
        /// Обрабатывает заказ звонка на указанный номер
        /// </summary>
        /// <param name="name">Имя</param>
        /// <param name="email">Телефон</param>
        /// <param name="id">Идентификатор агента</param>
        /// <param name="question">Текст вопроса</param>
        /// <returns></returns>
        [HttpPost][Route("members/question")]
        public ActionResult AgentQuestion(string name, string email, long id, string question)
        {
            try
            {
                var rep = Locator.GetService<IUsersRepository>();
                var agent = rep.Load(id);
                if (agent == null)
                {
                    throw new Exception("Агент не найден");
                }
                var message = String.Format(
                    "{0} ({1}) задал вам вопрос на сайте НП Риэлторы Дальнего Востока:<br/>{2}", name, email,
                    Server.HtmlEncode(question));
                Locator.GetService<IMailNotificationManager>().Notify(agent,"Вопрос на сайте НП РДВ",message);
                return Content("OK");
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                return Content(e.Message);
            }
        }

        #endregion

        #region Feedback

        /// <summary>
        /// Обрабатывает сообщение формы обратной связи
        /// </summary>
        /// <param name="model">Модель данных</param>
        /// <returns></returns>
        [HttpPost][Route("feedback")]
        public ActionResult Feedback(FeedbackModel model)
        {
            try
            {
                // Получатель
                var targetEmail = ConfigurationManager.AppSettings["FeedbackEmail"];
                

                // Отправляем
                Locator.GetService<IMailNotificationManager>().Notify(targetEmail,"Сообщение на сайте НП РДВ",MailTemplatesFactory.GetFeedbackTemplate(model).ToString());

                // Сообщение
                return Content("success");
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                return Content("Fail");
            }
        }
        
        /// <summary>
        /// Отправляет отчет об ошибке
        /// </summary>
        /// <param name="model">Модель данных</param>
        /// <returns></returns>
        [HttpPost][Route("bug-report")]
        public ActionResult BugReport(BugReportModel model)
        {
            try
            {
                // Получатель
                var targetEmail = ConfigurationManager.AppSettings["BugReportEmail"];
                var secondBugReportEmail = ConfigurationManager.AppSettings["CopyBugReportEmail"];

                // Отправляем
                Locator.GetService<IMailNotificationManager>().Notify(targetEmail, "Сообщение сайте НП РДВ: "+model.Subject, MailTemplatesFactory.GetBugReportTemplate(model).ToString());
                Locator.GetService<IMailNotificationManager>().Notify(secondBugReportEmail, "Сообщение сайте НП РДВ: " + model.Subject, MailTemplatesFactory.GetBugReportTemplate(model).ToString());

                // Сообщение
                return Content("success");
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                return Content("Fail");
            }
        }

        #endregion

        #region О нас

        #region О нас

        /// <summary>
        /// Отображает страницу о нас
        /// </summary>
        /// <returns></returns>
        [Route("about")]
        public ActionResult About()
        {
            PushNavigationItem("О нас","Информация о партнерстве","/about/",false);
            return View();
        }

        /// <summary>
        /// Информация для риелторов
        /// </summary>
        /// <returns></returns>
        [Route("about/rieltors")]
        public ActionResult Rieltros()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Риэлторам", "Информация для риэлторов", "/about/rieltors", false);
            return View();
        }

        /// <summary>
        /// Информация для клиентов
        /// </summary>
        /// <returns></returns>
        [Route("about/clients")]
        public ActionResult Clients()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Клиентам", "Информация для риелторов", "/about/clients", false);
            return View();
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/legal")]
        public ActionResult Legal()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Государственным структурам", "Информация для государственных структур", "/about/legal", false);
            return View();
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/banks")]
        public ActionResult Banks()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Банкам", "Информация для банков", "/about/banks", false);
            return View();
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/ensurance")]
        public ActionResult Ensurance()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Страховым компаниям", "Информация для страховых компаний", "/about/ensurance", false);
            return View();
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/builders")]
        public ActionResult Builders()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Застройщикам", "Информация для застройщиков", "/about/builders", false);
            return View();
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/pricers")]
        public ActionResult Pricers()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Оценщикам", "Информация для оценщиков", "/about/pricers", false);
            return View();
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/smi")]
        public ActionResult SMI()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("СМИ", "Информация для smi", "/about/smi", false);
            return View();
        }

        #endregion

        #region Структура

        [Route("about/structure")]
        public ActionResult Structure()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/");
            PushNavigationItem("Структура", "Структура партнерства", "/about/structure/", false);
            return View();
        }

        [Route("about/structure/common")]
        public ActionResult CommonStructure()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/");
            PushNavigationItem("Структура", "Структура партнерства", "/about/structure/");
            PushNavigationItem("Общее собрание", "Структура партнерства", "/about/structure/common", false);
            return View("StructureMembers",new StructureMembersModel()
                {
                    Header = "Верховный коллегиальный орган управления, отвечающий за стратегическое развитие Партнерства",
                    Members = Enumerable.Empty<User>(),
                    Title = "Общее собрание"
                });
        }



        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/structure/president")]
        public ActionResult StructurePresident()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/");
            PushNavigationItem("Структура", "Структура партнерства", "/about/structure/");
            PushNavigationItem("Президент", "Президент партнерства", "/about/structure/president");
            return View("StructureMembers", new StructureMembersModel()
            {
                Header = "Высшее должностное лицо Партнерства, возглавляющее Президентский Совет, подотчетное Общему собранию членов Партнерства и отвечающее за текущее руководство деятельностью Партнерства",
                Members = Locator.GetService<IUsersRepository>().Search(u => u.Id == 61),
                Title = "Президент РДВ"
            });
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/structure/budget")]
        public ActionResult StructureBudget()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/");
            PushNavigationItem("Структура", "Структура партнерства", "/about/structure/");
            PushNavigationItem("Комитет по бюджетной и финансовой деятельности", "Комитет по бюджетной и финансовой деятельности", "/about/structure/budget", false);
            return View("StructureMembers", new StructureMembersModel()
            {
                Header = "Контролирует финансовую деятельность РДВ и дочерних предприятий.",
                Members = Locator.GetService<IUsersRepository>().Search(u => u.Id == 72),
                Title = "Комитет по бюджетной и финансовой деятельности"
            });
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/structure/development")]
        public ActionResult StructureRieltyDevelopment()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/");
            PushNavigationItem("Структура", "Структура партнерства", "/about/structure/");
            PushNavigationItem("Комитет по риэлторской деятельности, образованию и сертификации", "", "/about/structure/development", false);
            return View("StructureMembers", new StructureMembersModel()
            {
                Header = "Разрабатывает нормативные документы, касающиеся риэлторской деятельности, ведет разъяснительную работу среди профессиональных участников рынка в отношении соблюдения ими правил риэлторской деятельности. Занимается вопросами обучения и сертификации.",
                Members = Locator.GetService<IUsersRepository>().Search(u => u.Id == 92),
                Title = "Комитет по риэлторской деятельности, образованию и сертификации"
            });
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/structure/etics")]
        public ActionResult StructureEtics()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/");
            PushNavigationItem("Структура", "Структура партнерства", "/about/structure/");
            PushNavigationItem("Комитет по этике", "", "/about/structure/etics", false);
            return View("StructureMembers", new StructureMembersModel()
            {
                Header = "Специальный орган РДВ, действующий на основании Положения «О комиссии по профессиональной этике», рассматривающий жалобы (споры) на неправомерные действия риэлторов, возникающие при оказании риэлторских услуг либо при совершении сделок с недвижимостью между участниками рынка недвижимости.", 
                Members = Locator.GetService<IUsersRepository>().Search(u => u.Id == 92),
                Title = "Комитет по этике"
            });
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/structure/membership")]
        public ActionResult StructureMembership()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/");
            PushNavigationItem("Структура", "Структура партнерства", "/about/structure/");
            PushNavigationItem("Комиссия по членству", "", "/about/structure/membership", false);
            return View("StructureMembers", new StructureMembersModel()
            {
                Header = "Осуществляет мероприятия направленные на привлечение новых членов ,контролирует членов на периоде кандидатского стажа, дает рекомендации для перевода в действительные члены.",
                Members = Locator.GetService<IUsersRepository>().Search(u => u.Id == 40),
                Title = "Комиссия по членству"
            });
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/structure/ross")]
        public ActionResult StructureROSS()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/");
            PushNavigationItem("Структура", "Структура партнерства", "/about/structure/");
            PushNavigationItem("РОСС", "", "/about/structure/ross", false);
            return View("StructureMembers", new StructureMembersModel()
            {
                Header = "Специальный орган ,утвержденный РГР. Занимается вопросами сертификации фирм , а также созданием и деятельностью саморегулируемой организации риэлторов на Дальнем Востоке.",
                Members = Locator.GetService<IUsersRepository>().Search(u => u.Id == 93),
                Title = "РОСС"
            });
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/structure/it")]
        public ActionResult StructureIT()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/");
            PushNavigationItem("Структура", "Структура партнерства", "/about/structure/");
            PushNavigationItem("Комитет по ИТ", "Комитет по IT", "/about/structure/it");
            return View("StructureMembers", new StructureMembersModel()
            {
                Header = "Отвечает за техническое развитие и реализацию IT проектов партнерства.",
                Members = Locator.GetService<IUsersRepository>().Search(u => u.Id == 6),
                Title = "Комитет по ИТ"
            });
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/structure/it/system")]
        public ActionResult StructureITSystem()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/");
            PushNavigationItem("Структура", "Структура партнерства", "/about/structure/");
            PushNavigationItem("Комитет по ИТ", "Комитет по IT", "/about/structure/it");
            PushNavigationItem("Комиссия по разработка сайта НП РДВ", "Комитет по IT", "/about/structure/it/system");
            return View("StructureMembers", new StructureMembersModel()
            {
                Header = "Отвечает за разработку и развитие информационной системы партнерства",
                Members = Locator.GetService<IUsersRepository>().Search(u => u.Id == 6 | u.Id == 119),
                Title = "Комиссия по разработке сайта НП РДВ"
            });
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/structure/partners")]
        public ActionResult StructurePartners()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/");
            PushNavigationItem("Структура", "Структура партнерства", "/about/structure/");
            PushNavigationItem("Комитет по работе с партнерами", "Комитет по работе с партнерами", "/about/structure/partners");
            return View("StructureMembers", new StructureMembersModel()
            {
                Header = "Осуществляет взаимодействие с муниципальными и областными органами власти и официальными организациями, обслуживающими рынок недвижимости ,взаимодействие с банками. Разрабатывает и внедряет бонусные программы для сотрудников и для клиентов.",
                Members = Locator.GetService<IUsersRepository>().Search(u => u.Id == 123),
                Title = "Комиссия по работе с партнерами"
            });
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/structure/partners/banks")]
        public ActionResult StructurePartnersBanks()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/");
            PushNavigationItem("Структура", "Структура партнерства", "/about/structure/");
            PushNavigationItem("Комитет по работе с партнерами", "Комитет по работе с партнерами", "/about/structure/partners");
            PushNavigationItem("Комиссия по работе с банками", "Комиссия по работе с банками", "/about/structure/partners/banks");
            return View("StructureMembers", new StructureMembersModel()
            {
                Header = "Осуществляет взаимодействие с банками.",
                Members = Locator.GetService<IUsersRepository>().Search(u => u.Id == 29),
                Title = "Комиссия по работе с партнерами"
            });
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/structure/partners/bonus")]
        public ActionResult StructurePartnersBonus()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/");
            PushNavigationItem("Структура", "Структура партнерства", "/about/structure/");
            PushNavigationItem("Комитет по работе с партнерами", "Комитет по работе с партнерами", "/about/structure/partners");
            PushNavigationItem("Комиссия по работе с партнерами \"Бонус\"", "Комиссия по работе с партнерами", "/about/structure/partners/bonus");
            return View("StructureMembers", new StructureMembersModel()
            {
                Header = "Разрабатывает и внедряет бонусные программы для клиентов и для сотрудников.",
                Members = Locator.GetService<IUsersRepository>().Search(u => u.Id == 16),
                Title = "Комиссия по работе с партнерами (Бонус клиенту)"
            });
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/structure/partners/legal")]
        public ActionResult StructurePartnersLegal()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/");
            PushNavigationItem("Структура", "Структура партнерства", "/about/structure/");
            PushNavigationItem("Комитет по работе с партнерами", "Комитет по работе с партнерами", "/about/structure/parnets");
            PushNavigationItem("Комиссия по работе с гос органами", "Комиссия по работе с партнерами", "/about/structure/partners/legal");
            return View("StructureMembers", new StructureMembersModel()
            {
                Header = "Осуществляет взаимодействие с муниципальными и областными органами власти и официальными организациями, обслуживающими рынок недвижимости .",
                Members = Locator.GetService<IUsersRepository>().Search(u => u.Id == 148),
                Title = "Комиссия по работе с гос органами"
            });
        }

        #region Учебный центр

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/structure/training")]
        public ActionResult TrainingCenter()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            return View();
        }

        /// <summary>
        /// Отображает страницу со списком новостей учебного центра
        /// </summary>
        /// <returns></returns>
        [Route("training-center/news")]
        public ActionResult TrainingCenterNews()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", true);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", true);
            PushNavigationItem("Новости", "Список новостей учебного центра", "/training-center/news", true);

            // Выбираем статьи для отображения
            var rep = Locator.GetService<IArticlesRepository>();
            var articles =
                rep.Search(a => a.ArticleType == ArticleTypes.TraningCenterNews).AsEnumerable().Where(p => (DateTimeZone.Now - p.PublicationDate).Days < 366).OrderByDescending(
                    d => d.PublicationDate).ToList();

            // Отображаем вид
            return View(articles);

        }

        /// <summary>
        /// Отображает страницу указанной новости учебного центра
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("traning-center/view-news/{id}")]
        public ActionResult TrainingCenterViewNews(long id)
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", true);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", true);
            PushNavigationItem("Новости", "Список новостей учебного центра", "/training-center/news", true);

            // Загружаем статью
            var rep = Locator.GetService<IArticlesRepository>();
            var item = rep.Load(id);
            if (item == null)
            {
                return RedirectToAction("TrainingCenterNews");
            }

            PushNavigationItem(item.Title, "Просмотр новости учебного центра", "/training-center/view-news/"+item.Id, false);

            // Увеличиваем счетчик просмотров
            item.Views += 1;
            rep.SubmitChanges();

            // Отдаем вид
            return View(item);
        }

        /// <summary>
        /// Отображает страницу со списком новостей учебного центра
        /// </summary>
        /// <returns></returns>
        [Route("training-center/news/archive")]
        public ActionResult TrainingCenterNewsArchive()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", true);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", true);
            PushNavigationItem("Новости", "Список новостей учебного центра", "/training-center/news", true);
            PushNavigationItem("Архив", "Архивные новости учебного центра", "/training-center/news/archive", true);

            // Выбираем статьи для отображения
            var rep = Locator.GetService<IArticlesRepository>();
            var articles =
                rep.Search(a => a.ArticleType == ArticleTypes.TraningCenterNews).AsEnumerable().Where(p => (DateTimeZone.Now - p.PublicationDate).Days >= 366).OrderByDescending(
                    d => d.PublicationDate).ToList();

            // Отображаем вид
            return View("TrainingCenterNews",articles);

        }

        /// <summary>
        /// Отображает страницу истории и миссии учебного центра
        /// </summary>
        /// <returns></returns>
        [Route("training-center/history")]
        public ActionResult TrainingCenterHistory()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            PushNavigationItem("История, миссия УЧ", "Информация по учебному центру", "/training-center/history", false);
            return View();
        }

        /// <summary>
        /// Отображает страницу с планами учебного центра
        /// </summary>
        /// <returns></returns>
        [Route("training-center/plans")]
        public ActionResult TrainingCenterPlans()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            PushNavigationItem("Планы и достижения", "Информация по учебному центру", "/training-center/plans", false);

            return View();
        }

        /// <summary>
        /// Отображает страницу с документами по учебному центру
        /// </summary>
        /// <returns></returns>
        [Route("training-center/docs")]
        public ActionResult TraningCenterDocs()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            PushNavigationItem("Документы", "Документы учебного центра", "/training-center/docs", false);

            return View();
        }

        /// <summary>
        /// Отображает страницу с програмами для риелторов
        /// </summary>
        /// <returns></returns>
        [Route("training-center/study/rieltor")]
        public ActionResult TraningCenterStudyPrograms()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            PushNavigationItem("Учебные курсы, тренинги и семинары", "Учебные курсы, тренинги и семинары", "/training-center/study/", false);
            PushNavigationItem("Программы для риэлторов", "Программы для риэлторов", "/training-center/study/rieltor", false);

            return View();
        }

        /// <summary>
        /// Отображает страницу с програмами для риелторов
        /// </summary>
        /// <returns></returns>
        [Route("training-center/study/monitoring")]
        public ActionResult TraningCenterStudyMonitoring()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            PushNavigationItem("Учебные курсы, тренинги и семинары", "Учебные курсы, тренинги и семинары", "/training-center/study/", false);
            PushNavigationItem("Росфинмониторинг", "Росфинмониторинг", "/training-center/study/monitoring", false);

            return View();
        }

        /// <summary>
        /// Отображает страницу с програмами для риелторов
        /// </summary>
        /// <returns></returns>
        [Route("training-center/study/brifings")]
        public ActionResult TraningCenterStudyBrigings()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            PushNavigationItem("Учебные курсы, тренинги и семинары", "Учебные курсы, тренинги и семинары", "/training-center/study/", false);
            PushNavigationItem("Целевой инструктаж", "Целевой инструктаж", "/training-center/study/brifings", false);

            return View();
        }

        /// <summary>
        /// Отображает страницу с програмами для риелторов
        /// </summary>
        /// <returns></returns>
        [Route("training-center/study/list")]
        public ActionResult TraningCenterStudyList()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            PushNavigationItem("Учебные курсы, тренинги и семинары", "Учебные курсы, тренинги и семинары", "/training-center/study/", false);
            PushNavigationItem("Тренинги, семинары, мастерклассы", "Тренинги, семинары, мастерклассы", "/training-center/study/list", false);

            return View();
        }

        /// <summary>
        /// Разработка обучающих программ
        /// </summary>
        /// <returns></returns>
        [Route("training-center/study/development")]
        public ActionResult TraningCenterStudyDevelopment()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            PushNavigationItem("Учебные курсы, тренинги и семинары", "Учебные курсы, тренинги и семинары", "/training-center/study/", false);
            PushNavigationItem("Разработка обучающих программ", "Разработка обучающих программ", "/training-center/study/development", false);

            return View();
        }

        /// <summary>
        /// выездное обучение
        /// </summary>
        /// <returns></returns>
        [Route("training-center/study/outside")]
        public ActionResult TraningCenterStudyOutside()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            PushNavigationItem("Учебные курсы, тренинги и семинары", "Учебные курсы, тренинги и семинары", "/training-center/study/", false);
            PushNavigationItem("Выездое обучение", "Выездное обучение", "/training-center/study/outside", false);

            return View();
        }

        /// <summary>
        /// Помощь в трудоустройстве
        /// </summary>
        /// <returns></returns>
        [Route("training-center/study/help")]
        public ActionResult TraningCenterStudyHelp()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            PushNavigationItem("Учебные курсы, тренинги и семинары", "Учебные курсы, тренинги и семинары", "/training-center/study/", false);
            PushNavigationItem("Помощь в трудоустройстве", "Помощь в трудоустройстве", "/training-center/study/help", false);

            return View();
        }

        /// <summary>
        /// Преподователи учебного центра
        /// </summary>
        /// <returns></returns>
        [Route("training-center/teachers")]
        public ActionResult TraningCenterTeachers()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            PushNavigationItem("Преподователи", "Информация о преподователях учебного центра", "/training-center/teachers", false);

            return View();
        }

        /// <summary>
        /// Преподователи учебного центра
        /// </summary>
        /// <returns></returns>
        [Route("training-center/class")]
        public ActionResult TraningCenterClass()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            PushNavigationItem("Учебный класс", "Информация по учебному классу", "/training-center/class", false);

            return View();
        }

        /// <summary>
        /// Преподователи учебного центра
        /// </summary>
        /// <returns></returns>
        [Route("training-center/rent")]
        public ActionResult TraningCenterRent()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            PushNavigationItem("Аренда класса", "Информация по учебному классу", "/training-center/rent", false);

            return View();
        }

        /// <summary>
        /// Преподователи учебного центра
        /// </summary>
        /// <returns></returns>
        [Route("training-center/shop")]
        public ActionResult TraningCenterBooks()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Учебный центр", "Информация по учебному центру", "/about/structure/training", false);
            PushNavigationItem("Магазин профессиональной литературы", "Магазин профессиональной литературы", "/training-center/books", false);

            var rep = Locator.GetService<IBooksRepository>();
            var books = rep.FindAll().ToList();

            return View(books);
        }

        /// <summary>
        /// Обрабатывает форму заказа книги
        /// </summary>
        /// <param name="title">Заголовок книги</param>
        /// <param name="fio">ФИО</param>
        /// <param name="email">Email</param>
        /// <param name="phone">Phone</param>
        /// <returns></returns>
        [Route("order-book")][HttpPost]
        public ActionResult OrderBook(string title, string fio, string email, string phone)
        {
            var template =
                new ParametrizedFileTemplate(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "Book.html"), new
                        {
                            Title = title,
                            Fio = fio,
                            Email = email,
                            Phone = phone
                        }).ToString();

            Locator.GetService<IMailNotificationManager>().Notify("rdv.partner@mail.ru","Заказ книги",template);
            Locator.GetService<IUINotificationManager>().Success("Заказ на книгу был успешно принят. Менеджер свяжется с вами в ближайшее время");

            return RedirectToAction("TraningCenterBooks");
        }

        /// <summary>
        /// Обрабатывает форму учебного класса
        /// </summary>
        /// <param name="date">Дата аренды</param>
        /// <param name="fio">ФИО</param>
        /// <param name="email">Email</param>
        /// <param name="phone">Phone</param>
        /// <returns></returns>
        [Route("order-class")][HttpPost]
        public ActionResult OrderClass(string date, string fio, string email, string phone, string message)
        {
            var template =
                new ParametrizedFileTemplate(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "ClassRent.html"), new
                        {
                            Message = message,
                            Fio = fio,
                            Email = email,
                            Date = date,
                            Phone = phone
                        }).ToString();

            Locator.GetService<IMailNotificationManager>().Notify("uc.rdv@yandex.ru", "Заказ аренды учебного класса", template);
            Locator.GetService<IUINotificationManager>().Success("Заявка успешно отправлена. Менеджер свяжется с вами в ближайшее время");

            return RedirectToAction("TraningCenterClass");
        }

        #endregion

        #endregion

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/docs")]
        public ActionResult Docs()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Нормативные документы", "Нормативные документы", "/about/docs", false);
            return View();
        }

        /// <summary>
        /// Информация для государственных структур
        /// </summary>
        /// <returns></returns>
        [Route("about/contacts")]
        public ActionResult Contacts()
        {
            PushNavigationItem("О нас", "Информация о партнерстве", "/about/", false);
            PushNavigationItem("Контакты", "Контактная информация", "/about/structure/training", false);
            return View();
        }

        #endregion

        #region Шляпа

        /// <summary>
        /// Возвращает кусок разметки с ближайшим событием в указанный день
        /// </summary>
        /// <param name="date">Дата</param>
        /// <returns></returns>
        [Route("change-calendar")]
        public ActionResult ChangeCalendar(string date)
        {
            var dateFormat = DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.CurrentUICulture);
            var rep = Locator.GetService<IArticlesRepository>();
            var eventsInDay =
                rep.Search(e => e.ArticleType == ArticleTypes.CalendarEvent && e.PublicationDate.Date == dateFormat.Date)
                    .ToList();
            ViewBag.date = dateFormat;
            return PartialView(eventsInDay);
        }

        #endregion
    }
}
