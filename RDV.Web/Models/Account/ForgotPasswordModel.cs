using RDV.Domain.Entities;
using RDV.Domain.Infrastructure.Misc;

namespace RDV.Web.Models.Account
{
    /// <summary>
    /// ������ ������������ ��� �������������� ������
    /// </summary>
    public class ForgotPasswordModel
    {
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
        /// ����������� �����
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// ��� �� �������������� ������
        /// </summary>
        public string PasswordCode { get; set; }

        /// <summary>
        /// ��������������� ������ �� ������������, ��� ������ ����� ������������
        /// </summary>
        public ForgotPasswordModel(User user)
        {
            FirstName = user.FirstName;
            LastName = user.LastName;
            SurName = user.SurName;
            Email = user.Email;
            var tmpCode = PasswordUtils.GenerateMD5PasswordHash(user.PasswordHash);
            PasswordCode = PasswordUtils.GenerateMD5PasswordHash(tmpCode);
        }
    }
}