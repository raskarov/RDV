using System;
using System.Text;
using RDV.Domain.Entities;
using RDV.Domain.Enums;

namespace RDV.Web.Models.Account.Company
{
    /// <summary>
    /// Модель клиента компании
    /// </summary>
    public class EditClientModel
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
        /// Тип клиента
        /// </summary>
        public ClientTypes ClientType { get; set; }

        /// <summary>
        /// Примечания по клиенту
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Тип договора с клиентом
        /// </summary>
        public int AgreementType { get; set; }

        /// <summary>
        /// Номер договора с клиентом
        /// </summary>
        public string AgreementNumber { get; set; }

        /// <summary>
        /// Дата заключения договора с клиентом
        /// </summary>
        public DateTime? AgreementStartDate { get; set; }

        /// <summary>
        /// Дата окончания договора с клиентом
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
        /// Условия оплаты
        /// </summary>
        public string PaymentCondition { get; set; }

        /// <summary>
        /// Стандартный конструкор
        /// </summary>
        public EditClientModel()
        {
        }

        /// <summary>
        /// Конструктор на основе доменной модели
        /// </summary>
        /// <param name="client">Доменный объект</param>
        public EditClientModel(Client client)
        {
            Id = client.Id;
            FirstName = client.FirstName;
            LastName = client.LastName;
            SurName = client.SurName;
            Phone = client.Phone;
            Email = client.Email;
            ICQ = client.ICQ;
            Birthdate = client.Birthday;
            Address = client.Address;
            ClientType = (ClientTypes)client.ClientType;
            AgreementType = client.AgreementType;
            AgreementStartDate = client.AgreementDate;
            AgreementEndDate = client.AgreementEndDate;
            AgreementNumber = client.AgreementNumber;
            Comission = client.Commision;
            Payment = client.AgencyPayment;
            PaymentCondition = client.PaymentConditions;
            Notes = client.Notes;
        }

        /// <summary>
        /// Преобразует модель в строку, выдавая имя и фамилию
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1}", LastName, FirstName);
            if (!string.IsNullOrEmpty(SurName))
            {
                sb.AppendFormat(" {0}", SurName);
            }
            return sb.ToString();
        }
    }
}