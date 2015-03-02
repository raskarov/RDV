using System;

namespace RDV.Web.Models.Administration.Users
{
    /// <summary>
    /// Модель сохранения изменений или создания нового пользователя через панель управления
    /// </summary>
    public class SaveUserModel
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public long Id { get; set; }

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
        /// Идентификатор роли
        /// </summary>
        public long RoleId { get; set; }

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
        /// Номер сертификата
        /// </summary>
        public string CertificateNumber { get; set; }

        /// <summary>
        /// Дата выдачи сертификата
        /// </summary>
        public DateTime? CertificationDate { get; set; }

        /// <summary>
        /// Дата окончания действия сертификата
        /// </summary>
        public DateTime? CertificateEndDate { get; set; }

        /// <summary>
        /// Общественные нагрузки
        /// </summary>
        public string PublicLoading { get; set; }

        /// <summary>
        /// Дополнительная информация
        /// </summary>
        public string AdditionalInformation { get; set; }
    }
}