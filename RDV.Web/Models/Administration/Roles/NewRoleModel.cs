namespace RDV.Web.Models.Administration.Roles
{
    /// <summary>
    /// Модель создания новой роли
    /// </summary>
    public class NewRoleModel
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Имя новой роли
        /// </summary>
        public string Name { get; set; }
    }
}