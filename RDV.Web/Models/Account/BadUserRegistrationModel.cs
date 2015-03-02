using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Classes.Extensions;

namespace RDV.Web.Models.Account
{
    /// <summary>
    /// Модель регистрации плохого пользователя
    /// </summary>
    public class BadUserRegistrationModel
    {
        /// <summary>
        /// Имя директора
        /// </summary>
        public string DirectorFirstName { get; set; }

        /// <summary>
        /// Фамилия директора
        /// </summary>
        public string DirectorLastName { get; set; }

        /// <summary>
        /// Отчество Директора
        /// </summary>
        public string DirectorSurName { get; set; }

        /// <summary>
        /// Имя плохого пользователя
        /// </summary>
        public string BadUserFirstName { get; set; }

        /// <summary>
        /// Отчество плохого пользователя
        /// </summary>
        public string BadUserSurName { get; set; }

        /// <summary>
        /// Фамилия плохого пользователя
        /// </summary>
        public string BadUserLastName { get; set; }

        /// <summary>
        /// Дата рождения плохого пользователя
        /// </summary>
        public string BadUserBirthDate { get; set; }

        /// <summary>
        /// Ссылка на фото плохого пользователя
        /// </summary>
        public string BadUserPhoto { get; set; }

        /// <summary>
        /// Описание плохого пользователя
        /// </summary>
        public string BadUserDescription { get; set; }

        /// <summary>
        /// Ссылка на страницу профиля плохого пользователя
        /// </summary>
        public string BadUserProfileUrl { get; set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public BadUserRegistrationModel(User director, User badUser)
        {
            DirectorFirstName = director.FirstName;
            DirectorLastName = director.LastName;
            DirectorSurName = director.SurName;
            BadUserSurName = badUser.SurName;
            BadUserFirstName = badUser.FirstName;
            BadUserLastName = badUser.LastName;
            BadUserBirthDate = badUser.Birthdate.FormatDate();
            BadUserPhoto = !string.IsNullOrEmpty(badUser.PhotoUrl)
                               ? string.Format("http://www.nprdv.ru{0}",
                                               Locator.GetService<IStoredFilesRepository>()
                                                      .ResolveFileUrl(badUser.PhotoUrl))
                               : "#";
            BadUserDescription = badUser.AdditionalInformation;
            BadUserProfileUrl = string.Format("http://www.nprdv.ru/members/users/{0}", badUser.Id);
        }
    }
}