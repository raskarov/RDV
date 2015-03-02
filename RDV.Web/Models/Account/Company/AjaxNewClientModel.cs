using System;

namespace RDV.Web.Models.Account.Company
{
    /// <summary>
    /// Модель клиента используемая для создания клиента через аякс диалоги
    /// </summary>
    public class AjaxNewClientModel
    {
        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public long Id { get; set; }

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
        /// Телефон
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// ICQ
        /// </summary>
        public string ICQ { get; set; }

        /// <summary>
        /// Адрес клиента
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// Примечания по клиенту
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Тип клиента
        /// </summary>
        public short ClientType { get; set; }

        /// <summary>
        /// Тип договора
        /// </summary>
        public short AgreementType { get; set; }

        /// <summary>
        /// Номер договора
        /// </summary>
        public string AgreementNumber { get; set; }

        /// <summary>
        /// Дата заключения договора
        /// </summary>
        public DateTime? AgreementStartDate { get; set; }

        /// <summary>
        /// Дата завершения договора
        /// </summary>
        public DateTime? AgreementEndDate { get; set; }

        /// <summary>
        /// Комиссия
        /// </summary>
        public string Comission { get; set; }

        /// <summary>
        /// Оплата услуг агенства
        /// </summary>
        public bool Payment { get; set; }

        /// <summary>
        /// Клиент в черном списке
        /// </summary>
        public bool Blacklisted { get; set; }

        /// <summary>
        /// Условия оплаты услуг агенства
        /// </summary>
        public string PaymentCondition { get; set; }
    }
}