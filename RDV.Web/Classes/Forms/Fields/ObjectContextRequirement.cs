using RDV.Domain.Enums;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// Класс, описывающий требование к текущему контексту объекта, чтобы поле было отображено
    /// </summary>
    public class ObjectContextRequirement
    {
        /// <summary>
        /// Требуемый тип объекта недвижимости
        /// </summary>
        public EstateTypes RequiredEstateType { get; set; }

        /// <summary>
        /// Требуемая операция для объекта
        /// </summary>
        public EstateOperations RequiredOperation { get; set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="requiredEstateType">Требуемый тип объекта</param>
        /// <param name="requiredOperation">Требуемая операция</param>
        public ObjectContextRequirement(EstateTypes requiredEstateType, EstateOperations requiredOperation)
        {
            RequiredEstateType = requiredEstateType;
            RequiredOperation = requiredOperation;
        }
    }
}