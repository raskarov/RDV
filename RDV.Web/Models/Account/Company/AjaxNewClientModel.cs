using System;

namespace RDV.Web.Models.Account.Company
{
    /// <summary>
    /// ������ ������� ������������ ��� �������� ������� ����� ���� �������
    /// </summary>
    public class AjaxNewClientModel
    {
        /// <summary>
        /// ������������� �������
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// ���
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// �������
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
        /// ����� �������
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// ���� ��������
        /// </summary>
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// ���������� �� �������
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// ��� �������
        /// </summary>
        public short ClientType { get; set; }

        /// <summary>
        /// ��� ��������
        /// </summary>
        public short AgreementType { get; set; }

        /// <summary>
        /// ����� ��������
        /// </summary>
        public string AgreementNumber { get; set; }

        /// <summary>
        /// ���� ���������� ��������
        /// </summary>
        public DateTime? AgreementStartDate { get; set; }

        /// <summary>
        /// ���� ���������� ��������
        /// </summary>
        public DateTime? AgreementEndDate { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public string Comission { get; set; }

        /// <summary>
        /// ������ ����� ��������
        /// </summary>
        public bool Payment { get; set; }

        /// <summary>
        /// ������ � ������ ������
        /// </summary>
        public bool Blacklisted { get; set; }

        /// <summary>
        /// ������� ������ ����� ��������
        /// </summary>
        public string PaymentCondition { get; set; }
    }
}