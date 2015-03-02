using RDV.Domain.Entities;
using RDV.Domain.Enums;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий нотификаций менеджера объектов
    /// </summary>
    public interface IObjectNotificationsRepository: IBaseRepository<ObjectManagerNotification>
    {
        /// <summary>
        /// Проверяет, была ли отослана объекту указанная нотификация
        /// </summary>
        /// <param name="estateObject">Объект</param>
        /// <param name="notificationType">Тип нотификации</param>
        /// <returns>true если была</returns>
        bool HasObjectNotification(EstateObject estateObject, ObjectNotificationTypes notificationType);

        /// <summary>
        /// Добавляет к объекту указанную нотификацию
        /// </summary>
        /// <param name="estateObject">Объект</param>
        /// <param name="notificationType">Нотификация</param>
        void AddObjectNotification(EstateObject estateObject, ObjectNotificationTypes notificationType);
    }
}