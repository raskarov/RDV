using System;

namespace RDV.Web.Models.Account.Profile
{
    /// <summary>
    /// Модель для обновления данных в своем профиле
    /// </summary>
    public class ProfileUpdateModel
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Email пользователя
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Старый пароль
        /// </summary>
        public string OldPassword { get; set; }

        /// <summary>
        /// Новый пароль
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// Подтверждение нового пароля
        /// </summary>
        public string NewPasswordConfirm { get; set; }

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
        /// Должность в компании
        /// </summary>
        public string Appointment { get; set; }

        /// <summary>
        /// Дата начала стажа
        /// </summary>
        public DateTime? SeniorityStartDate { get; set; }

        /// <summary>
        /// Номер сертификата
        /// </summary>
        public string CertificateNumber { get; set; }

        /// <summary>
        /// Дата выдачи сертификата
        /// </summary>
        public DateTime? CertificationDate { get; set; }

        /// <summary>
        /// Общественная нагрузка
        /// </summary>
        public string PublicLoading { get; set; }

        /// <summary>
        /// Дополнительная информация
        /// </summary>
        public string AdditionalInformation { get; set; }
    }
}