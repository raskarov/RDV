using System;

namespace RDV.Web.Models.Account
{
    /// <summary>
    /// ������ ������������ ��� ����������� ������ ������������
    /// </summary>
    public class RegisterModel
    {
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// ������������� ������
        /// </summary>
        public string PasswordConfirm { get; set; }

        /// <summary>
        /// ���
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// ���������� �������
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// ��������������� �������
        /// </summary>
        public string Phone2 { get; set; }

        /// <summary>
        /// ����� ICQ
        /// </summary>
        public string ICQ { get; set; }

        /// <summary>
        /// ���� ��������
        /// </summary>
        public DateTime? Birthdate { get; set; }

        /// <summary>
        /// �������������� ���
        /// </summary>
        public int RegisterAs { get; set; }

        /// <summary>
        /// ������������� ��������
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        /// ��������� � ��������
        /// </summary>
        public string Appointment { get; set; }

        /// <summary>
        /// ���� ������ �����
        /// </summary>
        public DateTime? SeniorityStartDate { get; set; }

        /// <summary>
        /// ������������ ��������� ������������ ����������
        /// </summary>
        public bool AcceptAgreement { get; set; }

        /// <summary>
        /// ������ �� ��������� ������������
        /// </summary>
        public string ActivationLink { get; set; }

        /// <summary>
        /// ����������� �����������
        /// </summary>
        public RegisterModel()
        {
            Birthdate = new DateTime?();
            SeniorityStartDate = new DateTime?();
        }
    }
}