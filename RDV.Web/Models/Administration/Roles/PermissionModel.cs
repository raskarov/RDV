using System.Linq;
using RDV.Domain.Entities;

namespace RDV.Web.Models.Administration.Roles
{
    /// <summary>
    /// Модель права, которое может быть у роли
    /// </summary>
    public class PermissionModel
    {
        /// <summary>
        /// Идентификатор разрешения у роли
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Идентификатор права
        /// </summary>
        public long PermissionId { get; set; }

        /// <summary>
        /// Системное имя права
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Отображаемое имя права
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Группа разрешений
        /// </summary>
        public string PermissionGroup { get; set; }

        /// <summary>
        /// Контекст операции
        /// </summary>
        public bool OperationContext { get; set; }

        /// <summary>
        /// Опции данной роли
        /// </summary>
        public string Options { get; set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public PermissionModel()
        {
        }

        /// <summary>
        /// Конструктор на основе маппинга права на роль
        /// </summary>
        /// <param name="model">Маппинг</param>
        public PermissionModel(RolePermission model)
        {
            Id = model.Id;
            PermissionId = model.PermissionId;
            SystemName = model.Permission.SystemName;
            DisplayName = model.Permission.DisplayName;
            PermissionGroup = model.Permission.PermissionGroup;
            OperationContext = model.Permission.OperationContext;
            Options = string.Join(",",
                                  model.RolePermissionOptions.Select(
                                      po => string.Format("{0}_{01}", po.ObjectType, po.ObjectOperation)));
        }

        /// <summary>
        /// Конструктор на основе прав
        /// </summary>
        /// <param name="model">Право</param>
        public PermissionModel(Permission model)
        {
            Id = model.Id;
            SystemName = model.SystemName;
            DisplayName = model.DisplayName;
            PermissionGroup = model.PermissionGroup;
            OperationContext = model.OperationContext;
        }
    }
}