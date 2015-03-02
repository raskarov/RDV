using System.Collections.Generic;
using RDV.Domain.Entities;
using RDV.Domain.Enums;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий ролей
    /// </summary>
    public interface IRolesRepository: IBaseRepository<Role>
    {
        /// <summary>
        /// Репозиторий полномочий
        /// </summary>
        IPermissionsRepository PermissionsRepository { get; }

        /// <summary>
        /// Присваивает пользователю указанную роль
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="role">Роль</param>
        void AssignRole(User user, Role role);

        /// <summary>
        /// Присваивает пользователю одну из встроенных ролей
        /// </summary>
        /// <param name="user"></param>
        /// <param name="builtinRoles"></param>
        void AssignRole(User user, BuiltinRoles builtinRoles);

        /// <summary>
        /// Возвращает список всех активных ролей в системе
        /// </summary>
        /// <returns></returns>
        IEnumerable<Role> GetActiveRoles();

        /// <summary>
        /// Возвращает роль по ее имени
        /// </summary>
        /// <param name="roleName">Имя роли</param>
        /// <returns>Роль или Null если роль не найдена</returns>
        Role GetRoleByName(string roleName);

        /// <summary>
        /// Добавляет к маппинг на пермишен к указанной роли
        /// </summary>
        /// <param name="role">Роль</param>
        /// <param name="rolePermission">Маппинг на пермишен</param>
        void AddRolePermission(Role role, RolePermission rolePermission);

        /// <summary>
        /// Очищает контекстные операции у указанного мапинга на пермишен
        /// </summary>
        /// <param name="rolePermission">маппинг на пермишен</param>
        void ClearRolePermissionContextOptions(RolePermission rolePermission);

        /// <summary>
        /// Добавляет контекстную операцию к указанному маппингу на пермишен
        /// </summary>
        /// <param name="rolePermission"></param>
        /// <param name="rolePermissionOption"></param>
        void AddRolePermissionOption(RolePermission rolePermission, RolePermissionOption rolePermissionOption);

        /// <summary>
        /// Удаляет пермишенны у определенной роли
        /// </summary>
        /// <param name="role">Роль</param>
        /// <param name="permissions">Набор пермишеннов</param>
        void DeleteRolePermissions(Role role, IEnumerable<RolePermission> permissions);
    }
}