// 
// 
// Solution: RDV
// Project: RDV.Web
// File: ObjectItemModel.cs
// 
// Created by: ykors_000 at 29.11.2013 15:46
// 
// Property of SoftGears
// 
// ========

using RDV.Domain.Entities;
using RDV.Web.Classes.Caching;

namespace RDV.Web.Models.Objects
{
    /// <summary>
    /// МОдель списка объектов
    /// </summary>
    public class ObjectItemModel
    {
        /// <summary>
        /// Конструктор элемента списка
        /// </summary>
        /// <param name="estateObject">Объект недвижимости</param>
        public ObjectItemModel(EstateObject estateObject)
        {
            Id = estateObject.Id;
            City = estateObject.Address.GeoCity != null ? estateObject.Address.GeoCity.Name : "";
            District = GetShortDistrictName(estateObject.Address.GeoDistrict != null ? estateObject.Address.GeoDistrict.Name : "");
            Area = estateObject.Address.GeoResidentialArea != null ? estateObject.Address.GeoResidentialArea.Name : "";
            Street = estateObject.Address.GeoStreet != null ? estateObject.Address.GeoStreet.Name : "";
            House = estateObject.Address.House;
            RoomsCount = estateObject.ObjectAdditionalProperties.RoomsCount;
            HouseType = IdObjectsCache.GetShortDictionaryValue(estateObject.ObjectMainProperties.HouseType);
            FloorNumber = estateObject.ObjectMainProperties.FloorNumber;
            TotalFloors = estateObject.ObjectMainProperties.TotalFloors;
            TotalArea = estateObject.ObjectMainProperties.TotalArea;
            LivingArea = estateObject.ObjectMainProperties.ActualUsableFloorArea;
            KitchenArea = estateObject.ObjectMainProperties.KitchenFloorArea;
            Price = estateObject.ObjectMainProperties.Price;
			BuildingMaterial = IdObjectsCache.GetShortDictionaryValue(estateObject.ObjectMainProperties.BuildingMaterial);
            LandArea = estateObject.ObjectMainProperties.LandArea;
			Assigment = IdObjectsCache.GetShortDictionaryValue(estateObject.ObjectMainProperties.ObjectAssignment);
            Status = estateObject.Status;
            Legal = estateObject.ObjectMainProperties.Documents;
            UserName = estateObject.User.LastName;
            CompanyName = estateObject.User.Company != null
                ? estateObject.User.Company.ShortName ?? estateObject.User.Company.Name : "";
        }

		/// <summary>
		/// Возвращает краткое название районов
		/// </summary>
		/// <param name="district">Район</param>
		/// <returns></returns>
	    public static string GetShortDistrictName(string district)
	    {
			switch (district.ToLower())
			{
				case "индустриальный":
					return "ИНД";
				case "центральный":
					return "ЦЕНТР";
				case "кировский":
					return "КИР";
				case "железнодорожный":
					return "ЖД";
				case "краснофлотский":
					return "КР";
				default:
					return district;
			}
	    }

	    /// <summary>
        /// Идентификатор объекта
        /// </summary>
        public long Id { get; set; }

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
        /// Количество комнат
        /// </summary>
        public int? RoomsCount { get; set; }

        /// <summary>
        /// Тип дома
        /// </summary>
        public string HouseType { get; set; }

        /// <summary>
        /// Этаж
        /// </summary>
        public int? FloorNumber { get; set; }

        /// <summary>
        /// Этажность
        /// </summary>
        public int? TotalFloors { get; set; }

        /// <summary>
        /// Общая площадь
        /// </summary>
        public double? TotalArea { get; set; }

        /// <summary>
        /// Жилая площадь
        /// </summary>
        public double? LivingArea { get; set; }

        /// <summary>
        /// Кухня площадь
        /// </summary>
        public double? KitchenArea { get; set; }

        /// <summary>
        /// Цена
        /// </summary>
        public double? Price { get; set; }

        /// <summary>
        /// Материал постройки
        /// </summary>
        public string BuildingMaterial { get; set; }

        /// <summary>
        /// Площадь участка
        /// </summary>
        public double? LandArea { get; set; }

        /// <summary>
        /// Назначение
        /// </summary>
        public string Assigment { get; set; }

        /// <summary>
        /// Назначение
        /// </summary>
        public string Legal { get; set; }

        /// <summary>
        /// Статус объекта
        /// </summary>
        public short Status { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Имя компании
        /// </summary>
        public string CompanyName { get; set; }
    }
}