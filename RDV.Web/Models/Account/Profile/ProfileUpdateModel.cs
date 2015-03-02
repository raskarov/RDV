using System;

namespace RDV.Web.Models.Account.Profile
{
    /// <summary>
    /// ������ ��� ���������� ������ � ����� �������
    /// </summary>
    public class ProfileUpdateModel
    {
        /// <summary>
        /// ������������� ������������
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Email ������������
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// ������ ������
        /// </summary>
        public string OldPassword { get; set; }

        /// <summary>
        /// ����� ������
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// ������������� ������ ������
        /// </summary>
        public string NewPasswordConfirm { get; set; }

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
        /// ��������� � ��������
        /// </summary>
        public string Appointment { get; set; }

        /// <summary>
        /// ���� ������ �����
        /// </summary>
        public DateTime? SeniorityStartDate { get; set; }

        /// <summary>
        /// ����� �����������
        /// </summary>
        public string CertificateNumber { get; set; }

        /// <summary>
        /// ���� ������ �����������
        /// </summary>
        public DateTime? CertificationDate { get; set; }

        /// <summary>
        /// ������������ ��������
        /// </summary>
        public string PublicLoading { get; set; }

        /// <summary>
        /// �������������� ����������
        /// </summary>
        public string AdditionalInformation { get; set; }
    }
}