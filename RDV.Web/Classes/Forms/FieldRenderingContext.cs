using RDV.Domain.Entities;
using RDV.Domain.Enums;

namespace RDV.Web.Classes.Forms
{
    /// <summary>
    /// Контекст рендеринга полей
    /// </summary>
    public class FieldRenderingContext
    {
        /// <summary>
        /// Объект для которого происходит рендеринг
        /// </summary>
        public EstateObject EstateObject { get; set; }

        /// <summary>
        /// Тип объекта
        /// </summary>
        public EstateTypes EstateType { get; set; }

        /// <summary>
        /// Операция проводимая по объекту
        /// </summary>
        public EstateOperations EstateOperation { get; set; }

        /// <summary>
        /// Текущий статус объекта
        /// </summary>
        public EstateStatuses EstateStatus { get; set; }

        /// <summary>
        /// Текущий авторизованный пользователь
        /// </summary>
        public User CurrentUser { get; set; }

        /// <summary>
        /// Инициализирует контекст рендеринга
        /// </summary>
        /// <param name="estateObject">Объект недвижимости</param>
        /// <param name="currentUser">Текущий пользователь</param>
        public FieldRenderingContext(EstateObject estateObject, User currentUser)
        {
            EstateObject = estateObject;
            CurrentUser = currentUser;
            EstateOperation = (EstateOperations) estateObject.Operation;
            EstateStatus = (EstateStatuses) estateObject.Status;
            EstateType = (EstateTypes) estateObject.ObjectType;
        }
    }
}