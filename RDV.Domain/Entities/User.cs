using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDV.Domain.Enums;

namespace RDV.Domain.Entities
{
    /// <summary>
    /// Пользователь, зарегистрированный в системе
    /// </summary>
    public partial class User
    {
        /// <summary>
        /// Возвращает фамилию, имя и отчество пользователя
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1}",LastName,FirstName);
            if (!string.IsNullOrEmpty(SurName))
            {
                sb.AppendFormat(" {0}", SurName);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Возвращает количество объектов недвижимости созданных этим пользователем
        /// </summary>
        /// <returns></returns>
        public int GetObjectsCount()
        {
            return this.EstateObjects.Count;
        }

        /// <summary>
        /// Проверяет, есть ли у пользователя указанное разрешение
        /// </summary>
        /// <param name="permissionId">Идентификатор разрешения</param>
        /// <returns></returns>
        public bool HasPermission(long permissionId)
        {
            return this.Role.RolePermissions.Any(rp => rp.PermissionId == permissionId);
        }

        /// <summary>
        /// Проверяет, есть ли у пользователя указанное разрешение
        /// </summary>
        /// <param name="permission">Тип разрешения</param>
        /// <returns></returns>
        public bool HasPermission(StandartPermissions permission)
        {
            return Role.RolePermissions.Any(rp => rp.PermissionId == (long) permission);
        }

        /// <summary>
        /// Проверяет, есть ли у пользователя какое либо разрешение на доступ к панели управления
        /// </summary>
        /// <returns></returns>
        public bool HasAnyAdministrativePermission()
        {
            return Role.RolePermissions.Any(rp => rp.Permission.PermissionGroup == "Администрирование");
        }

        /// <summary>
        /// Проверяет, есть ли у пользователя контекстно-зависимые от объектов права на определенную операцию для определенного типа объектов
        /// </summary>
        /// <param name="permissionId">Идентификатор разрешения</param>
        /// <param name="operation">Операция</param>
        /// <param name="objectType">Тип объекта на операции</param>
        /// <returns></returns>
        public bool HasObjectRelatedPermission(long permissionId, short operation, short objectType)
        {
            // Ищем само разрешение
            var permission = Role.RolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
            if (permission == null)
            {
                return false;
            }
            // Проверяем есть ли в разрешении права на определенный объект и определенную операцию
            return
                permission.RolePermissionOptions.Any(
                    op => op.ObjectOperation == operation && op.ObjectType == objectType);
        }

        /// <summary>
        /// Является ли пользователь администратором
        /// </summary>
        public bool IsAdministrator
        {
            get { return RoleId == 4; }
        }

        /// <summary>
        /// Возвращает список всех объектов недвижимости, которые назначены на объект
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EstateObject> GetAllEstateObjects()
        {
            return EstateObjects;
        }

        /// <summary>
        /// Возвращает баланс лицевого счета пользователя в системе
        /// </summary>
        /// <returns></returns>
        public decimal GetAccountBalance()
        {
            if (Payments.Count == 0)
            {
                return 0;
            }
            var income = Payments.Where(p => p.Payed && p.Direction == (short)PaymentDirection.Income).Sum(p => p.Amount);
            var outcome = Payments.Where(p => p.Direction == (short)PaymentDirection.Outcome).Sum(p => p.Amount);
            return income - outcome;
        }
    }
}