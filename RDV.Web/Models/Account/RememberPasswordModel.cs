using RDV.Domain.Entities;
using RDV.Domain.Infrastructure.Misc;

namespace RDV.Web.Models.Account
{
    public class RememberPasswordModel
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
        /// Новый пароль
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// Инициаилизирует модель из пользователя, чей пароль нужно восстановить
        /// </summary>
        public RememberPasswordModel(User user, string newPassword)
        {
            NewPassword = newPassword;
            FirstName = user.FirstName;
            LastName = user.LastName;
            SurName = user.SurName;
            Email = user.Email;
        } 
    }
}