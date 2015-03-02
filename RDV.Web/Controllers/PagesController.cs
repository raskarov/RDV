using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Routing;
using RDV.Domain.Interfaces.Repositories.Content;
using RDV.Domain.IoC;
using RDV.Web.Classes.Extensions;

namespace RDV.Web.Controllers
{
    /// <summary>
    /// Контроллер отображения динамически создаваемых страниц через панель адмиистрирования
    /// </summary>
    public class PagesController : BaseController
    {
        /// <summary>
        /// Репозиторий статичных страниц
        /// </summary>
        public IStaticPagesRepository StaticPagesRepository { get; private set; }

        /// <summary>
        /// Конструктор с инъекцией
        /// </summary>
        public PagesController(): base()
        {
            this.StaticPagesRepository = Locator.GetService<IStaticPagesRepository>();
        }

        #region Публикации

        [Route("articles")]
        public ActionResult Articles()
        {
            return RedirectToAction("AllNews");
        }

        /// <summary>
        /// Отображает страницу с указанным идентификатором
        /// </summary>
        /// <param name="id">Идентификатор страницы</param>
        /// <returns>HTML содержимое, встроенное в стандартный шаблон</returns>
        public ActionResult ViewPage(long id)
        {
            // Загружаем страницу
            var page = StaticPagesRepository.Load(id);
            if (page == null)
            {
                return View("Error404NotFound");
            }

            // Увеличиваем счетчик просмотров
            page.Views = page.Views + 1;
            StaticPagesRepository.SubmitChanges();

            PushAuditEvent(AuditEventTypes.ViewPage, String.Format("Просмотр страницы \"{0}\"", page.Title));

            return View("StaticPage",page);
        }

        /// <summary>
        /// Отображает указанную публикацию
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns></returns>
        [Route("articles/view/{id}")]
        public ActionResult ViewArticle(long id)
        {
            // Репозиторий
            var rep = Locator.GetService<IArticlesRepository>();
            var art = rep.Load(id);
            if (art == null)
            {
                return RedirectToAction("Index", "Main");
            }
            art.Views = art.Views + 1;
            rep.SubmitChanges();

            PushNavigationItem("Публикации","Все публикации системы","/articles/");
            PushNavigationItem(art.Title,"","/articles/view/"+art.Id,false);
            PushAuditEvent(AuditEventTypes.ViewPage, String.Format("Просмотр публикации \"{0}\"", art.Title));

            return View(art);
        }

        /// <summary>
        /// Отображает список всех новостей системы
        /// </summary>
        /// <returns></returns>
        [Route("articles/news")]
        public ActionResult AllNews()
        {
            var rep = Locator.GetService<IArticlesRepository>();
            var articles = rep.Search(a => a.ArticleType == ArticleTypes.News).OrderByDescending(d => d.DateCreated);

            PushNavigationItem("Публикации", "Все публикации системы", "/articles/");
            PushNavigationItem("Новости", "Все новости системы", "/articles/news");
            
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка всех новостей системы");

            return View(articles);
        }

        /// <summary>
        /// Отображает список всех новостей системы
        /// </summary>
        /// <returns></returns>
        [Route("articles/events")]
        public ActionResult AllEvents(DateTime? date = null)
        {
            var rep = Locator.GetService<IArticlesRepository>();
            IEnumerable<Article> articles = rep.Search(a => a.ArticleType == ArticleTypes.CalendarEvent).OrderByDescending(d => d.DateCreated);

            PushNavigationItem("Публикации", "Все публикации системы", "/articles/");
            PushNavigationItem("События", "Все события системы", "/articles/events");

            if (date.HasValue)
            {
                articles =
                    articles.Where(a => a.ArticleType == ArticleTypes.CalendarEvent && a.PublicationDate == date.Value).OrderByDescending(d => d.DateCreated);
                PushAuditEvent(AuditEventTypes.ViewPage, string.Format("Просмотр списка всех событий системы за {0}", date.Value.FormatDate()));
            }
            else
            {
                articles = rep.Search(a => a.ArticleType == ArticleTypes.CalendarEvent).OrderByDescending(d => d.DateCreated);
                PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка всех событий системы");
            }
            
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка всех событий системы");

            return View(articles);
        }

        /// <summary>
        /// Отображает список всех видеорепортажей
        /// </summary>
        /// <returns></returns>
        [Route("articles/video")]
        public ActionResult AllVideos()
        {
            var rep = Locator.GetService<IArticlesRepository>();
            var articles = rep.Search(a => a.ArticleType == ArticleTypes.VideoInterview || a.ArticleType == ArticleTypes.VideoNews || a.ArticleType == ArticleTypes.VideoPresentation).OrderByDescending(d => d.DateCreated);

            PushNavigationItem("Публикации", "Все публикации системы", "/articles/");
            PushNavigationItem("Видеорепортажи", "Все видеорепортажи системы", "/articles/video");
            
            PushAuditEvent(AuditEventTypes.ViewPage, "Просмотр списка всех видеорепортажей системы");

            return View(articles);
        }

        #endregion
    }
}
