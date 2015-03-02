namespace RDV.Web.Models.Administration.Roles
{
    /// <summary>
    /// Модель используемая при сохранении прав для определенной роли
    /// </summary>
    public class SaveRolePermissionModel
    {
        /// <summary>
        /// Идентификатор роли
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// Идентификатор редактирумой записи у роли
        /// </summary>
        public long RolePermissionId { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Илентификатор права у роли
        /// </summary>
        public long PermissionId { get; set; }
    }
}