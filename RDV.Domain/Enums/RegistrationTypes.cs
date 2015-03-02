namespace RDV.Domain.Enums
{
    /// <summary>
    /// типы регистрации пользователей
    /// </summary>
    public enum RegistrationTypes: int
    {
        /// <summary>
        /// Регистрируемся как гость
        /// </summary>
        Guest = 1,

        /// <summary>
        /// Регистрируемся как агент
        /// </summary>
        Agent = 2
    }
}