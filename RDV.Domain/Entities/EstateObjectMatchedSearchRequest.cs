// 
// 
// Solution: RDV
// Project: RDV.Domain
// File: EstateObjectMatchedSearchRequest.cs
// 
// Created by: ykors_000 at 17.01.2014 16:00
// 
// Property of SoftGears
// 
// ========

using RDV.Domain.Infrastructure.Misc;

namespace RDV.Domain.Entities
{
    /// <summary>
    /// Подходящий под объект поисковый запрос
    /// </summary>
    public partial class EstateObjectMatchedSearchRequest
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName
        {
            get
            {
                return User.ToString();
            }
        }

        /// <summary>
        /// Телефон пользователя
        /// </summary>
        public string Phone
        {
            get
            {
                return User.Phone.FormatPhoneNumber();
            }
        }

        /// <summary>
        /// Email пользователя
        /// </summary>
        public string Email
        {
            get
            {
                return User.Email;
            }
        }

        /// <summary>
        /// Компания пользователя
        /// </summary>
        public string Company
        {
            get
            {
                return User.Company.ShortName ?? User.Company.Name;
            }
        }
    }
}