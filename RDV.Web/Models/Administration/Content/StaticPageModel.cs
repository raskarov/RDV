using RDV.Domain.Entities;

namespace RDV.Web.Models.Administration.Content
{
    /// <summary>
    /// Модель создания или редактирования статической страницы
    /// </summary>
    public class StaticPageModel
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public StaticPageModel()
        {
        }

        /// <summary>
        /// Конструктор на основе доменного объекта
        /// </summary>
        /// <param name="page">Статическая страница</param>
        public StaticPageModel(StaticPage page)
        {
            Id = page.Id;
            Title = page.Title;
            Route = page.Route;
            Content = page.Content;
        }

        /// <summary>
        /// Идентификатор страницы
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Заголовок страницы
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Путь к странице в системе
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Содержимое страницы
        /// </summary>
        public string Content { get; set; }
    }
}