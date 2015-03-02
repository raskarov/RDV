using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория ролей
    /// </summary>
    public class RolesRepository: BaseRepository<Role>, IRolesRepository
    {
        /// <summary>
        /// Репозиторий пермишеннов
        /// </summary>
        private readonly IPermissionsRepository _permissionsRepository;

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext">Контекст доступа к данным</param>
        /// <param name="permissionsRepository">Репозиторий пермишеннов</param>
        public RolesRepository(RDVDataContext dataContext, IPermissionsRepository permissionsRepository) : base(dataContext)
        {
            _permissionsRepository = permissionsRepository;
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override Role Load(long id)
        {
            return Find(s => s.Id == id);
        }

        /// <summary>
        /// Репозиторий полномочий
        /// </summary>
        public IPermissionsRepository PermissionsRepository
        {
            get { return _permissionsRepository; }
        }

        /// <summary>
        /// Присваивает пользователю указанную роль
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="role">Роль</param>
        public void AssignRole(User user, Role role)
        {
            user.Role = role;
        }

        /// <summary>
        /// Присваивает пользователю одну из встроенных ролей
        /// </summary>
        /// <param name="user"></param>
        /// <param name="builtinRoles"></param>
        public void AssignRole(User user, BuiltinRoles builtinRoles)
        {
            // Определяем какую роль будем искать
            var roleToSearch = string.Empty;
            switch (builtinRoles)
            {
                case BuiltinRoles.Agent:
                    roleToSearch = "Агент";
                    break;
                case BuiltinRoles.Guest:
                    roleToSearch = "Гость";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("builtinRoles");
            }

            // Ищем роль с указанным именем
            var role = Find(r => r.Name == roleToSearch);
            if (role == null)
            {
                throw new ObjectNotFoundException(string.Format("Роль с указанным именем {0} не найдена", roleToSearch));
            }

            // Присваиваем
            AssignRole(user, role);
        }

        /// <summary>
        /// Возвращает список всех активных ролей в системе
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Role> GetActiveRoles()
        {
            return FindAll().OrderBy(r => r.Name);
        }

        /// <summary>
        /// Возвращает роль по ее имени
        /// </summary>
        /// <param name="roleName">Имя роли</param>
        /// <returns>Роль или Null если роль не найдена</returns>
        public Role GetRoleByName(string roleName)
        {
            return Find(r => r.Name.ToLower().Equals(roleName.ToLower()));
        }

        /// <summary>
        /// Добавляет к маппинг на пермишен к указанной роли
        /// </summary>
        /// <param name="role">Роль</param>
        /// <param name="rolePermission">Маппинг на пермишен</param>
        public void AddRolePermission(Role role, RolePermission rolePermission)
        {
            rolePermission.RoleId = role.Id;
            role.RolePermissions.Add(rolePermission);
            DataContext.RolePermissions.InsertOnSubmit(rolePermission);
        }

        /// <summary>
        /// Очищает контекстные операции у указанного мапинга на пермишен
        /// </summary>
        /// <param name="rolePermission">маппинг на пермишен</param>
        public void ClearRolePermissionContextOptions(RolePermission rolePermission)
        {
            if (rolePermission.RolePermissionOptions.Count > 0)
            {
                DataContext.RolePermissionOptions.DeleteAllOnSubmit(rolePermission.RolePermissionOptions);
                rolePermission.RolePermissionOptions.Clear();
            }
        }

        /// <summary>
        /// Добавляет контекстную операцию к указанному маппингу на пермишен
        /// </summary>
        /// <param name="rolePermission"></param>
        /// <param name="rolePermissionOption"></param>
        public void AddRolePermissionOption(RolePermission rolePermission, RolePermissionOption rolePermissionOption)
        {
            rolePermissionOption.RolePermissionId = rolePermission.Id;
            DataContext.RolePermissionOptions.InsertOnSubmit(rolePermissionOption);
        }

        /// <summary>
        /// Удаляет пермишенны у определенной роли
        /// </summary>
        /// <param name="role">Роль</param>
        /// <param name="permissions">Набор пермишеннов</param>
        public void DeleteRolePermissions(Role role, IEnumerable<RolePermission> permissions)
        {
            foreach (var rolePermission in permissions)
            {
                role.RolePermissions.Remove(rolePermission);
            }
            DataContext.RolePermissions.DeleteAllOnSubmit(permissions);
        }
    }
}