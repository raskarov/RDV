using System.Collections.Generic;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;

namespace RDV.Web.Models.Account.Company
{
    /// <summary>
    /// Модель с данными компании
    /// </summary>
    public class CompanyProfileModel
    {
        public long Id { get; set; }

        /// <summary>
        /// наименование компании
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Городгде находится
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Адрес
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Телефон 1
        /// </summary>
        public string Phone1 { get; set; }

        /// <summary>
        /// Телефон 2
        /// </summary>
        public string Phone2 { get; set; }

        /// <summary>
        /// Телефон 3
        /// </summary>
        public string Phone3 { get; set; }

        /// <summary>
        /// Ссылка на логотип
        /// </summary>
        public string LogoUrl { get; set; }

        /// <summary>
        /// Ссылка на схему проезда
        /// </summary>
        public string SchemeUrl { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// сведения о филиалах
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// контактное лицо
        /// </summary>
        public string ContactPerson { get; set; }

        /// <summary>
        /// Директор
        /// </summary>
        public string Director { get; set; }

        /// <summary>
        /// Email адрес для компании
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Список агентов компании
        /// </summary>
        public IList<User> Agents { get; private set; }

        /// <summary>
        /// Краткое наименованеи компании
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public CompanyProfileModel()
        {
        }

        /// <summary>
        /// Конструктор на основе доменного объекта
        /// </summary>
        public CompanyProfileModel(RDV.Domain.Entities.Company company)
        {
            // Репозиторий файлов
            var rep = Locator.GetService<IStoredFilesRepository>();

            Id = company.Id;
            Name = company.Name;
            ShortName = company.ShortName;
            City = company.GeoCity != null ? company.GeoCity.Name : "Не установлен";
            Address = company.Address;
            Phone1 = company.Phone1;
            Phone2 = company.Phone2;
            Phone3 = company.Phone3;
            LogoUrl = rep.ResolveFileUrl(company.LogoImageUrl);
            SchemeUrl = rep.ResolveFileUrl(company.LocationSchemeUrl);
            Description = company.Description;
            Branch = company.Branch;
            ContactPerson = company.ContactPerson;
            Director = company.Director != null ? company.Director.ToString() : "Директор не указан";
            Email = company.Email;

            Agents = company.Users.Where(ua => ua.Status == (int) UserStatuses.Active || ua.Status == (int)UserStatuses.Blocked).AsEnumerable().OrderByDescending(u => u.GetObjectsCount()).ThenBy(u => u.ToString()).ToList();
        }
    }
}