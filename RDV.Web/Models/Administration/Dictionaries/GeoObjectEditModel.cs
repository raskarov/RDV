using RDV.Domain.Enums;

namespace RDV.Web.Models.Administration.Dictionaries
{
    /// <summary>
    /// Модель сохранения изменений в географическом объекте
    /// </summary>
    public class GeoObjectEditModel
    {
        /// <summary>
        /// Идентификатор объекта
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Идентификатор родительского объекта
        /// </summary>
        public long ParentObjectId { get; set; }

        /// <summary>
        /// Тип создаваемого объекта
        /// </summary>
        public GeoLevels ObjectType { get; set; }

        /// <summary>
        /// Наименование объекта
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Координаты границ объектов, если у нас район или жилой массив
        /// </summary>
        public string Bounds { get; set; }
    }
}