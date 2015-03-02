using System;

namespace RDV.Web.Models.Account.Profile
{
    /// <summary>
    /// ������ ������� ���������
    /// </summary>
    public class TrainingProgramModel
    {
        /// <summary>
        /// ������������� ��� ��������������
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// ���� ����������
        /// </summary>
        public DateTime TrainingDate { get; set; }

        /// <summary>
        /// ������������ ���������
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// �����������
        /// </summary>
        public string Organizer { get; set; }

        /// <summary>
        /// ����� ����������
        /// </summary>
        public string TrainingPlace { get; set; }

        /// <summary>
        /// ������������� ������������
        /// </summary>
        public long UserId { get; set; }
    }
}