using System;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Classes.Caching;

namespace RDV.Web.Models.Objects
{
    /// <summary>
    /// Модель медиа элемента загруженного к объекту
    /// </summary>
    public class ObjectMediaModel
    {
        /// <summary>
        /// Идентификатор медиа объекта
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Тип медиа объекта
        /// </summary>
        public ObjectMediaTypes MediaType { get; set; }

        /// <summary>
        /// Заголовок медиа объекта
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Ссылка на превью для медиа объекта во внутренней файловой системе
        /// </summary>
        public string PreviewUrl { get; set; }

        /// <summary>
        /// Разрезолвенная ссылка на превью медиа объекта
        /// </summary>
        public string PreviewFullUrl { get; set; }

        /// <summary>
        /// Ссылка на файл медиа объекта
        /// </summary>
        public StoredFile PreviewFile { get; set; }

        /// <summary>
        /// Ссылка на сам объект медиа в файловой системе
        /// </summary>
        public string MediaUrl { get; set; }

        /// <summary>
        /// Разрезолвенная ссылка на медиа объект
        /// </summary>
        public string MediaFullUrl { get; set; }

        /// <summary>
        /// Медиа файл
        /// </summary>
        public StoredFile MediaFile { get; set; }

        /// <summary>
        /// Количество просмотров медиа элемента
        /// </summary>
        public int Views { get; set; }

        /// <summary>
        /// Дата загрузки медиа элемента
        /// </summary>
        public DateTime? DateUploaded { get; set; }

        /// <summary>
        /// Кем загружен медиа элемент
        /// </summary>
        public string UploadedBy { get; set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public ObjectMediaModel()
        {
        }

        /// <summary>
        /// Конструктор на основе доменного объекта
        /// </summary>
        public ObjectMediaModel(ObjectMedia media)
        {
            // Файловый репозиторий
            var rep = Locator.GetService<IStoredFilesRepository>();

            Id = media.Id;
            Title = media.Title;
            MediaType = (ObjectMediaTypes) media.MediaType;
            PreviewUrl = media.PreviewUrl;
            if (!String.IsNullOrEmpty(PreviewUrl))
            {
                PreviewFile = rep.GetFile(PreviewUrl);
                PreviewFullUrl = rep.ResolveFileUrl(PreviewUrl);
            }
            MediaUrl = media.MediaUrl;
            if (!String.IsNullOrEmpty(MediaUrl))
            {
                MediaFile = rep.GetFile(MediaUrl);
                MediaFullUrl = rep.ResolveFileUrl(MediaUrl);
            }
            Views = media.Views;
            DateUploaded = media.DateCreated;
            UploadedBy = IdObjectsCache.GetUserLogin(media.CreatedBy);
            Position = media.Position;
            IsMain = media.IsMain;
        }

        /// <summary>
        /// Фотография является главной
        /// </summary>
        public bool IsMain { get; set; }

        /// <summary>
        /// Индекс следования фотографии для сортировки
        /// </summary>
        public int Position { get; set; }
    }
}