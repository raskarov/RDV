using RDV.Domain.Entities;

namespace RDV.Web.Classes.Security
{
    /// <summary>
    /// Класс со вспомогательными методами при работе с пермишеннами
    /// </summary>
    public static class PermissionUtils
    {
        /// <summary>
        /// Проверяет с учетом контекста, имеется ли у пользователя привилегии на выполнение какой либо операции
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <param name="user">Пользователь</param>
        /// <param name="rootPermission">Корневой пермишен</param>
        /// <returns>true если есть, иначе false</returns>
        public static bool CheckUserObjectPermission(EstateObject obj, User user, long rootPermission)
        {
            return user.IsAdministrator || user.HasObjectRelatedPermission(rootPermission, obj.Operation, obj.ObjectType);
        }
    }
}