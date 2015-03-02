using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;

namespace RDV.Web.Models.Account.Profile
{
    /// <summary>
    /// ������ ��� �������� � ����� ������ ��� �������� �������������� ������� ������������
    /// </summary>
    public class ProfileInfoModel: User
    {
        /// <summary>
        /// �������� ����� ���������� ������������, ��������������� �� ���� ���������� � ������� �������� ������
        /// </summary>
        public string RealImageUrl { get; set; }

        /// <summary>
        /// ��������� ��������� ������������
        /// </summary>
        public IList<TrainingProgram> Programs { get; set; }

        /// <summary>
        /// �������� 
        /// </summary>
        public short Notifications { get; set; }

        /// <summary>
        /// ����������� �����������
        /// </summary>
        public ProfileInfoModel()
        {
        }

        /// <summary>
        /// ����������� �� ������ ��������� �������
        /// </summary>
        /// <param name="user"></param>
        public ProfileInfoModel(User user)
        {
            this.Id = user.Id;
            this.Activated = user.Activated;
            this.Birthdate = user.Birthdate;
            this.Blocked = user.Blocked;
            this.CertificateNumber = user.CertificateNumber;
            this.CertificationDate = user.CertificationDate;
            this.CertificateEndDate = user.CertificateEndDate;
            this.CompanyId = user.CompanyId;
            this.Company = user.Company;
            this.CreatedBy = user.CreatedBy;
            this.DateCreated = user.DateCreated;
            this.DateModified = user.DateModified;
            this.FirstName = user.FirstName;
            this.ICQ = user.ICQ;
            this.LastLogin = user.LastLogin;
            this.LastName = user.LastName;
            this.Login = user.Login;
            this.ModifiedBy = user.ModifiedBy;
            this.Passport = user.Passport;
            this.PassportId = user.PassportId;
            this.Phone = user.Phone;
            this.Phone2 = user.Phone2;
            this.PhotoUrl = user.PhotoUrl;
            this.Role = user.Role;
            this.RoleId = user.RoleId;
            this.SeniorityStartDate = user.SeniorityStartDate;
            this.SurName = user.SurName;
            this.Appointment = user.Appointment;
            this.Email = user.Email;
            this.PublicLoading = user.PublicLoading;
            this.AdditionalInformation = user.AdditionalInformation;
            this.Status = user.Status;
            this.RealImageUrl = !string.IsNullOrEmpty(user.PhotoUrl) ? Locator.GetService<IStoredFilesRepository>().ResolveFileUrl(user.PhotoUrl) : string.Empty;
            this.Programs = user.TrainingPrograms.OrderBy(d => d.TrainingDate).ToList();
            this.Notifications = user.Notifications;
            this.Achievments = user.Achievments;
            this.ClientReviews = user.ClientReviews;
        }

        /// <summary>
        /// ���������� ������ ���� ���������� ������������
        /// </summary>
        /// <returns></returns>
        public IList<Achievment> GetAchievments()
        {
            return Achievments.ToList();
        }

        /// <summary>
        /// ���������� ������ ���� ������� �������� � �������������
        /// </summary>
        /// <returns></returns>
        public IList<ClientReview> GetClientReviews()
        {
            return ClientReviews.ToList();
        }
    }
}