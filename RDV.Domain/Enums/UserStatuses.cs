namespace RDV.Domain.Enums
{
    /// <summary>
    /// Статусы, которые может иметь пользователь
    /// </summary>
    public enum UserStatuses: short
    {
        /// <summary>
        /// Пользователь активен
        /// </summary>
        [EnumDescription("Активен")]
        Active = 1,

        /// <summary>
        /// Пользователь неактивен
        /// </summary>
        [EnumDescription("Неактивен")]
        InActive = 2,

        /// <summary>
        /// Пользователь заблокирован
        /// </summary>
        [EnumDescription("Заблокирован")]
        Blocked = 3
    }
}