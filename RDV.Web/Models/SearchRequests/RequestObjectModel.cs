// 
// 
// Solution: RDV
// Project: RDV.Web
// File: RequestObjectModel.cs
// 
// Created by: ykors_000 at 27.03.2014 10:52
// 
// Property of SoftGears
// 
// ========

using System;
using System.Linq;
using System.Runtime.Serialization;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Web.Classes.Caching;
using RDV.Web.Classes.Extensions;
using RDV.Web.Models.Objects;

namespace RDV.Web.Models.SearchRequests
{
    /// <summary>
    /// Модель объекта поискового запроса
    /// </summary>
    public class RequestObjectModel
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Статус объекта
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Компания
        /// </summary>
        public string Company { get; set; }
        /// <summary>
        /// Город
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// район
        /// </summary>
        public string District { get; set; }

        /// <summary>
        /// Массив
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// Улица
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Номер дома
        /// </summary>
        public string House { get; set; }

        /// <summary>
        /// Цена
        /// </summary>
        public double? Price { get; set; }

        /// <summary>
        /// Дата регистрации
        /// </summary>
        public DateTime? RegistrationDate { get; set; }

        /// <summary>
        /// Дата 
        /// </summary>
        public DateTime? DatePriceChanged { get; set; }

        /// <summary>
        /// Старая цена
        /// </summary>
        public double? OldPrice { get; set; }

        /// <summary>
        /// Тип бонуса мультилистинга
        /// </summary>
        public string BonusType { get; set; }

        /// <summary>
        /// Размер бонуса мультилистинга
        /// </summary>
        public string BonusSize { get; set; }

        /// <summary>
        /// Дата отклонения
        /// </summary>
        public DateTime? DeclineDate { get; set; }

        /// <summary>
        /// Причина отклонения
        /// </summary>
        public string DeclineReason { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RequestObjectModel(SearchRequestObject requestObject)
        {
            Id = requestObject.Id;
            ObjectId = requestObject.EstateObjectId;
            Status = ((EstateStatuses)requestObject.EstateObject.Status).GetEnumMemberName();
            Company = requestObject.EstateObject.User.Company.ShortName ?? requestObject.EstateObject.User.Company.Name;
            City = requestObject.EstateObject.Address.GeoCity != null ? requestObject.EstateObject.Address.GeoCity.Name : "";
            District = ObjectItemModel.GetShortDistrictName(requestObject.EstateObject.Address.GeoDistrict != null ? requestObject.EstateObject.Address.GeoDistrict.Name : "");
            Area = requestObject.EstateObject.Address.GeoResidentialArea != null ? requestObject.EstateObject.Address.GeoResidentialArea.Name : "";
            Street = requestObject.EstateObject.Address.GeoStreet != null ? requestObject.EstateObject.Address.GeoStreet.Name : "";
            House = requestObject.EstateObject.Address.House;
            Price = requestObject.EstateObject.ObjectMainProperties.Price;
            RegistrationDate = requestObject.EstateObject.ObjectChangementProperties.DateRegisted;
            DatePriceChanged = requestObject.EstateObject.ObjectChangementProperties.PriceChanged;
            var priousPriceItm =
                requestObject.EstateObject.ObjectPriceChangements.OrderByDescending(p => p.DateChanged)
                    .Skip(1)
                    .FirstOrDefault();
            if (priousPriceItm != null)
            {
                OldPrice = priousPriceItm.Value;
            }
            BonusType = requestObject.EstateObject.ObjectAdditionalProperties.AgreementType == 354 ?
                IdObjectsCache.GetShortDictionaryValue(requestObject.EstateObject.ObjectMainProperties.MultilistingBonusType) : String.Empty;
            BonusSize = requestObject.EstateObject.ObjectMainProperties.MultilistingBonus.FormatString();
            DeclineDate = requestObject.DateMoved;
            DeclineReason = requestObject.DeclineReason;
        }

        /// <summary>
        /// Идентификатор объекта
        /// </summary>
        public long ObjectId { get; set; }
    }
}