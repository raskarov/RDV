using System;

namespace RDV.Web.Models.Account
{
    /// <summary>
    /// Модель используемая для регистрации нового пользователя
    /// </summary>
    public class RegisterModel
    {
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Подтверждение пароля
        /// </summary>
        public string PasswordConfirm { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Контактный телефон
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Вспомогательный телефон
        /// </summary>
        public string Phone2 { get; set; }

        /// <summary>
        /// номер ICQ
        /// </summary>
        public string ICQ { get; set; }

        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// Регистрируется как
        /// </summary>
        public int RegisterAs { get; set; }

        /// <summary>
        /// Идентификатор компании
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        /// Должность в компании
        /// </summary>
        public string Appointment { get; set; }

        /// <summary>
        /// Дата начала стажа
        /// </summary>
        public DateTime? SeniorityStartDate { get; set; }

        /// <summary>
        /// Пользователь принимает лицензионное соглашение
        /// </summary>
        public bool AcceptAgreement { get; set; }

        /// <summary>
        /// Ссылка на активацию пользователя
        /// </summary>
        public string ActivationLink { get; set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public RegisterModel()
        {
            Birthdate = new DateTime?();
            SeniorityStartDate = new DateTime?();
        }
    }
}