using RDV.Domain.Enums;

namespace RDV.Web.Models.Administration.Dictionaries
{
    /// <summary>
    /// Модель удаления географического элемента
    /// </summary>
    public class GeoObjectDeleteModel
    {
        /// <summary>
        /// Идентификатор элемента
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Тип гео объекта
        /// </summary>
        public GeoLevels ObjectType { get; set; }
    }
}