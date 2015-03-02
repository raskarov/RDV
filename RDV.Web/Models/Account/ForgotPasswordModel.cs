using RDV.Domain.Entities;
using RDV.Domain.Infrastructure.Misc;

namespace RDV.Web.Models.Account
{
    /// <summary>
    /// Модель используемая для восстановления пароля
    /// </summary>
    public class ForgotPasswordModel
    {
        /// <summary>
        /// Имя
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// Электронный адрес
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Код на восстановление пароля
        /// </summary>
        public string PasswordCode { get; set; }

        /// <summary>
        /// Инициаилизирует модель из пользователя, чей пароль нужно восстановить
        /// </summary>
        public ForgotPasswordModel(User user)
        {
            FirstName = user.FirstName;
            LastName = user.LastName;
            SurName = user.SurName;
            Email = user.Email;
            var tmpCode = PasswordUtils.GenerateMD5PasswordHash(user.PasswordHash);
            PasswordCode = PasswordUtils.GenerateMD5PasswordHash(tmpCode);
        }
    }
}