using RDV.Domain.Entities;
using RDV.Web.Classes.Forms;

namespace RDV.Web.Models.Objects
{
    /// <summary>
    /// Модель используемая во всех формах редактирования объекта
    /// </summary>
    public class ObjectFormModel
    {
        /// <summary>
        /// Текущий редактируемый объект
        /// </summary>
        public EstateObject EstateObject { get; private set; }

        /// <summary>
        /// Список редактируемых полей у объекта
        /// </summary>
        public FieldsList Fields { get; private set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="estateObject"></param>
        /// <param name="fields"></param>
        public ObjectFormModel(EstateObject estateObject, FieldsList fields)
        {
            EstateObject = estateObject;
            Fields = fields;
        }
    }
}