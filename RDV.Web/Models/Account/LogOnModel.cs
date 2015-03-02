namespace RDV.Web.Models.Account
{
    /// <summary>
    /// Модель логина в личный кабинет
    /// </summary>
    public class LogOnModel
    {
        /// <summary>
        /// Email пользователя
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public string Password { get; set; }
    }
}