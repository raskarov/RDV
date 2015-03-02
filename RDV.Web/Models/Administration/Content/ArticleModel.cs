using System;
using RDV.Domain.Entities;
using RDV.Domain.Enums;

namespace RDV.Web.Models.Administration.Content
{
    public class ArticleModel
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public ArticleModel()
        {
        }

        /// <summary>
        /// Конструктор на основе доменного объекта
        /// </summary>
        /// <param name="article">Статическая страница</param>
        public ArticleModel(Article article)
        {
            Id = article.Id;
            Title = article.Title;
            ShortContent = article.ShortContent;
            FullContent = article.FullContent;
            ArticleType = article.ArticleType;
            PublicationDate = article.PublicationDate;
            VideoLink = article.VideoLink;
        }

        /// <summary>
        /// Идентификатор публикации
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Заголовок публикации
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Тип публикации
        /// </summary>
        public ArticleTypes ArticleType { get; set; }

        /// <summary>
        /// Дата публикации
        /// </summary>
        public DateTime PublicationDate { get; set; }

        /// <summary>
        /// Краткая аннотация к публикации
        /// </summary>
        public string ShortContent { get; set; }

        /// <summary>
        /// Содержимое публикации
        /// </summary>
        public string FullContent { get; set; }

        /// <summary>
        /// Ссылка на видео, если публикация являеется видеорепортажем
        /// </summary>
        public string VideoLink { get; set; }
    }
}