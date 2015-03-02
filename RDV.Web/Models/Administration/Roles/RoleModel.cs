using System.Collections.Generic;

namespace RDV.Web.Models.Administration.Roles
{
    /// <summary>
    /// Модель роли в которой может состаять пользователь
    /// </summary>
    public class RoleModel
    {
        /// <summary>
        /// Идентификатор роли
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public RoleModel()
        {

        }

        /// <summary>
        /// Имя роли
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Права, которые есть у данной роли
        /// </summary>
        public IList<PermissionModel> Permissions { get; set; }
    }
}