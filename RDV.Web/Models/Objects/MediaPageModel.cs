using System.Collections.Generic;
using System.Linq;
using RDV.Domain.Entities;

namespace RDV.Web.Models.Objects
{
    /// <summary>
    /// Модель используемая на странице отображения списка загруженных к объектов фото и видео материалов
    /// </summary>
    public class MediaPageModel
    {
        /// <summary>
        /// Объект недвижимости
        /// </summary>
        public EstateObject EstateObject { get; private set; }

        /// <summary>
        /// Список медиа элементов, загруженных для данного объекта недвижимости
        /// </summary>
        public IList<ObjectMediaModel> MediaList { get; private set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="estateObject">Объект</param>
        public MediaPageModel(EstateObject estateObject)
        {
            EstateObject = estateObject;
            MediaList =
                estateObject.ObjectMedias.OrderByDescending(m => m.DateCreated).Select(m => new ObjectMediaModel(m)).
                    ToList();
        }
    }
}