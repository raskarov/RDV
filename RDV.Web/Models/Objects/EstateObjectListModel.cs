using System;
using System.Globalization;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Misc;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Classes.Caching;
using RDV.Web.Classes.Extensions;

namespace RDV.Web.Models.Objects
{
    /// <summary>
    /// Модель новинки
    /// </summary>
    public class EstateObjectListModel
    {
        /// <summary>
        /// Заголовок новинки
        /// </summary>
        public string TitleLine { get; set; }

        /// <summary>
        /// Линия количества комнат
        /// </summary>
        public string SizeLine { get; set; }

        /// <summary>
        /// Линия района
        /// </summary>
        public string DistrictLine { get; set; }

        /// <summary>
        /// Линия улицы
        /// </summary>
        public string StreetLine { get; set; }

        /// <summary>
        /// Линия цены
        /// </summary>
        public string PriceLine { get; set; }

        /// <summary>
        /// Ссылка на изображения
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Идентификатор объекта
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Конструктор на основе доменного объекта
        /// </summary>
        /// <param name="obj"></param>
        public EstateObjectListModel(EstateObject obj)
        {
            Id = obj.Id;
            // Формируем строку заголовка и размер
            var operation = "";
            var objectType = "";
            switch ((EstateOperations)obj.Operation)
            {
                case EstateOperations.Selling:
                    operation = "продам";
                    break;
                case EstateOperations.Buying:
                    operation = "куплю";
                    break;
                case EstateOperations.Lising:
                    operation = "сдам";
                    break;
                case EstateOperations.Rent:
                    operation = "сниму";
                    break;
                case EstateOperations.Exchange:
                    operation = "обменяю";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch ((EstateTypes)obj.ObjectType)
            {
                case EstateTypes.Room:
                    objectType = "комнату";
                    SizeLine = String.Format("Площадь: {0}",
                                             obj.ObjectMainProperties.TotalArea.HasValue
                                                 ? obj.ObjectMainProperties.TotalArea.Value.ToString(CultureInfo.InvariantCulture)
                                                 : "не указана");
                    break;
                case EstateTypes.Flat:
                    objectType = "квартиру";
                    SizeLine = String.Format("Количество комнат: {0}",
                                             obj.ObjectAdditionalProperties.RoomsCount.HasValue
                                                 ? obj.ObjectAdditionalProperties.RoomsCount.Value.ToString(CultureInfo.InvariantCulture)
                                                 : "-");
                    break;
                case EstateTypes.House:
                    objectType = "коттедж";
                    SizeLine = String.Format("Количество комнат: {0}",
                                             obj.ObjectAdditionalProperties.RoomsCount.HasValue
                                                 ? obj.ObjectAdditionalProperties.RoomsCount.Value.ToString(CultureInfo.InvariantCulture)
                                                 : "-");
                    break;
                case EstateTypes.Land:
                    objectType = "земель. участок";
                    SizeLine = String.Format("Площадь: {0}",
                                             obj.ObjectMainProperties.TotalArea.HasValue
                                                 ? obj.ObjectMainProperties.TotalArea.Value.ToString(CultureInfo.InvariantCulture)
                                                 : "не указана");
                    break;
                case EstateTypes.Office:
                    objectType = "офис";
                    SizeLine = String.Format("Площадь: {0}",
                                             obj.ObjectMainProperties.TotalArea.HasValue
                                                 ? obj.ObjectMainProperties.TotalArea.Value.ToString(CultureInfo.InvariantCulture)
                                                 : "не указана");
                    break;
                case EstateTypes.Garage:
                    objectType = "гараж";
                    SizeLine = String.Format("Площадь: {0}",
                                             obj.ObjectMainProperties.TotalArea.HasValue
                                                 ? obj.ObjectMainProperties.TotalArea.Value.ToString(CultureInfo.InvariantCulture)
                                                 : "не указана");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            TitleLine = String.Format("{0} {1}", operation, objectType);
            if (!String.IsNullOrEmpty(obj.ObjectMainProperties.Title))
            {
                TitleLine = obj.ObjectMainProperties.Title;
            }

            // Формируем район
            string districtName;
            if (obj.Address != null && obj.Address.GeoDistrict != null)
            {
                districtName = obj.Address.GeoDistrict.Name;
            }
            else
            {
                districtName = "не указан";
            }
            DistrictLine = String.Format("Район: {0}", districtName);

            // Формируем улицу
            string streetName;
            if (obj.Address != null && obj.Address.GeoStreet != null)
            {
                streetName = obj.Address.GeoStreet.Name;
            }
            else
            {
                streetName = "не указана";
            }
            StreetLine = String.Format("Улица: {0}", streetName);

            // Формируем цену
            PriceLine = String.Format("{0} {1}", obj.ObjectMainProperties.Price.FormatPrice(),
                                      IdObjectsCache.GetDictionaryValue(obj.ObjectMainProperties.Currency));

            // Получаем изображение
            var firstPhoto = obj.ObjectMedias.FirstOrDefault(m => m.MediaType == (short) ObjectMediaTypes.Photo);
            if (firstPhoto != null)
            {
                var imagesRep = Locator.GetService<IStoredFilesRepository>();
                ImageUrl = imagesRep.ResolveFileUrl(firstPhoto.MediaUrl);
            }
        }
    }
}