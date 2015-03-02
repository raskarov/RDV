using System;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.Core;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория нотификаций объектов
    /// </summary>
    public class ObjectNotificationsRepository: BaseRepository<ObjectManagerNotification>, IObjectNotificationsRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext">Контекст доступа к данным</param>
        public ObjectNotificationsRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override ObjectManagerNotification Load(long id)
        {
            return Find(n => n.Id == id);
        }

        /// <summary>
        /// Проверяет, была ли отослана объекту указанная нотификация
        /// </summary>
        /// <param name="estateObject">Объект</param>
        /// <param name="notificationType">Тип нотификации</param>
        /// <returns>true если была</returns>
        public bool HasObjectNotification(EstateObject estateObject, ObjectNotificationTypes notificationType)
        {
            return estateObject.ObjectManagerNotifications.All(n => n.NotificationType != (short) notificationType);
        }

        /// <summary>
        /// Добавляет к объекту указанную нотификацию
        /// </summary>
        /// <param name="estateObject">Объект</param>
        /// <param name="notificationType">Нотификация</param>
        public void AddObjectNotification(EstateObject estateObject, ObjectNotificationTypes notificationType)
        {
            var newNotification = new ObjectManagerNotification()
                {
                    DateCreated = DateTimeZone.Now,
                    ObjectId = estateObject.Id,
                    EstateObject = estateObject,
                    NotificationType = (short) notificationType
                };
            estateObject.ObjectManagerNotifications.Add(newNotification);
        }
    }
}