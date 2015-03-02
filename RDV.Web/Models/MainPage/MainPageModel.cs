using System.Collections.Generic;
using RDV.Domain.Entities;
using RDV.Web.Models.Objects;
using System;

namespace RDV.Web.Models.MainPage
{
    /// <summary>
    /// Модель данных на главной странице
    /// </summary>
    public class MainPageModel
    {
        /// <summary>
        /// Содержимое блока новинки
        /// </summary>
        public IList<EstateObjectListModel> NewEstateObjects { get; set; }

        /// <summary>
        /// Все статьи и публикации
        /// </summary>
        public IList<Article> Articles { get; set; }

        /// <summary>
        /// События календаря
        /// </summary>
        public IList<Article> CalendarEvents { get; set; }

        /// <summary>
        /// Список партнеров системы
        /// </summary>
        public IList<Partner> Partners { get; set; }

        /// <summary>
        /// HTML баннера на главной странице
        /// </summary>
        public String BannerHtml { get; set; }
    }
}