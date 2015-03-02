using System.Collections.Generic;

namespace RDV.Domain.Entities
{
    /// <summary>
    /// Роль в которой находиться пользователь или группа пользователей
    /// </summary>
    public partial class Role
    {
        private static readonly List<string> SystemRoles = new List<string>(){"агент","администратор","гость"};
        
        /// <summary>
        /// Возвращает количество пользователей, находящихся в этой роли
        /// </summary>
        /// <returns></returns>
        public int GetUsersCount()
        {
            return Users.Count;
        }

        /// <summary>
        /// Возвращает true если данная роль является системной и не может быть отредактирована или изменена
        /// </summary>
        /// <returns></returns>
        public bool IsSystemRole()
        {
            return SystemRoles.Contains(this.Name.ToLower());
        }
    }
}