using System.Collections.Generic;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;

namespace RDV.Web.Classes.Extensions
{
    /// <summary>
    /// Статический класс содержащий расширения для географических объектов
    /// </summary>
    public static class GeoExtensions
    {
        /// <summary>
        /// Выполняет конвертацию объекта страны в справочник, пригодный к JSON сериализации и использовании в компоненте JS Tree
        /// </summary>
        /// <param name="country">Страна</param>
        /// <param name="ajaxChildren">Дети загружаются аяксом</param>
        /// <returns></returns>
        public static Dictionary<string, object> ConvertToJSTreeJson(this GeoCountry country, bool ajaxChildren = false)
        {
            return new Dictionary<string, object>()
                             {
                                 {"data", country.Name},
                                 {
                                     "attr", new Dictionary<string, object>()
                                                 {
                                                     {"id", string.Format("country-{0}", country.Id)},
                                                     {"real-id", country.Id},
                                                     {"real-name", country.Name},
                                                     {"level", (short) GeoLevels.Country},
                                                     {"ajax", ajaxChildren},
                                                 }
                                     },
                                 {
                                     "children",
                                     ajaxChildren
                                         ? (object) new object[] {}
                                         : country.GeoRegions.OrderBy(r => r.Name).Select(r => r.ConvertToJSTreeJson()).ToList()
                                     },
                                 {"state", "closed"}
                             };
        }

        /// <summary>
        /// Выполняет конвертацию объекта региона в справочник, пригодный к JSON сериализации и использовании в компоненте JS Tree
        /// </summary>
        /// <param name="region">Регион</param>
        /// <param name="ajaxChildren">Дети подгружаются аяксом</param>
        /// <returns></returns>
        public static Dictionary<string, object> ConvertToJSTreeJson(this GeoRegion region, bool ajaxChildren = false)
        {
            return new Dictionary<string, object>()
                             {
                                 {"data", region.Name},
                                 {
                                     "attr", new Dictionary<string, object>()
                                                 {
                                                     {"id", string.Format("regionDistrict-{0}", region.Id)},
                                                     {"real-id", region.Id},
                                                     {"real-name", region.Name},
                                                     {"level", (short) GeoLevels.Region},
                                                     {"country-id", region.CountryId},
                                                     {"ajax", ajaxChildren},
                                                 }
                                     },
                                 {
                                     "children",
                                     ajaxChildren
                                         ? (object) new object[] {}
                                         : region.GeoRegionDistricts.OrderBy(c => c.Name).Select(c => c.ConvertToJSTreeJson()).ToList()
                                     },
                                 {"state", "closed"}
                             };
        }

        /// <summary>
        /// Выполняет конвертацию объекта района региона в справочник, пригодный к JSON сериализации и использовании в компоненте JS Tree
        /// </summary>
        /// <param name="regionDistrict">Регион</param>
        /// <param name="ajaxChildren">Дети подгружаются аяксом</param>
        /// <returns></returns>
        public static Dictionary<string, object> ConvertToJSTreeJson(this GeoRegionDistrict regionDistrict, bool ajaxChildren = false)
        {
            return new Dictionary<string, object>()
                             {
                                 {"data", regionDistrict.Name},
                                 {
                                     "attr", new Dictionary<string, object>()
                                                 {
                                                     {"id", string.Format("regionDistrict-{0}", regionDistrict.Id)},
                                                     {"real-id", regionDistrict.Id},
                                                     {"real-name", regionDistrict.Name},
                                                     {"level", (short) GeoLevels.RegionDistrict},
                                                     {"region-id", regionDistrict.RegionId},
                                                     {"ajax", ajaxChildren},
                                                 }
                                     },
                                 {
                                     "children",
                                     ajaxChildren
                                         ? (object) new object[] {}
                                         : regionDistrict.GeoCities.OrderBy(c => c.Name).Select(c => c.ConvertToJSTreeJson()).ToList()
                                     },
                                 {"state", "closed"}
                             };
        }

        /// <summary>
        /// Выполняет конвертацию объекта города в справочник, пригодный к JSON сериализации и использовании в компоненте JS Tree
        /// </summary>
        /// <param name="city">Город</param>
        /// <param name="ajaxChildren">Дети подгружаются аяксом</param>
        /// <returns></returns>
        public static Dictionary<string, object> ConvertToJSTreeJson(this GeoCity city, bool ajaxChildren = false)
        {
            return new Dictionary<string, object>()
                             {
                                 {"data", city.Name},
                                 {
                                     "attr", new Dictionary<string, object>()
                                                 {
                                                     {"id", string.Format("city-{0}", city.Id)},
                                                     {"real-id", city.Id},
                                                     {"real-name", city.Name},
                                                     {"level", (short) GeoLevels.City},
                                                     {"regionDistrict-id", city.RegionDistrictId},
                                                     {"ajax", ajaxChildren},
                                                 }
                                     },
                                 {
                                     "children",
                                     ajaxChildren
                                         ? (object) new object[] {}
                                         : city.GeoDistricts.OrderBy(r => r.Name).Select(r => r.ConvertToJSTreeJson(true)).ToList()
                                     },
                                 {"state", "closed"}
                             };
        }

        /// <summary>
        /// Выполняет конвертацию объекта района в справочник, пригодный к JSON сериализации и использовании в компоненте JS Tree
        /// </summary>
        /// <param name="district">Район</param>
        /// <param name="ajaxChildren">Дети подгружаются аяксом</param>
        /// <returns></returns>
        public static Dictionary<string, object> ConvertToJSTreeJson(this GeoDistrict district, bool ajaxChildren = false)
        {
            return new Dictionary<string, object>()
                             {
                                 {"data", district.Name},
                                 {
                                     "attr", new Dictionary<string, object>()
                                                 {
                                                     {"id", string.Format("district-{0}", district.Id)},
                                                     {"real-id", district.Id},
                                                     {"real-name", district.Name},
                                                     {"level", (short) GeoLevels.District},
                                                     {"city-id", district.CityId},
                                                     {"bounds", district.Bounds},
                                                     {"ajax", ajaxChildren},
                                                 }
                                     },
                                 {
                                     "children",
                                     ajaxChildren
                                         ? (object) new object[] {}
                                         : district.GeoResidentialAreas.OrderBy(r => r.Name).Select(r => r.ConvertToJSTreeJson()).ToList()
                                     },
                                 {"state", "closed"}
                             };
        }

        /// <summary>
        /// Выполняет конвертацию объекта жилмассива в справочник, пригодный к JSON сериализации и использовании в компоненте JS Tree
        /// </summary>
        /// <param name="residentialArea">Жилой массив</param>
        /// <param name="ajaxChildren">Дети подгружаются аяксом</param>
        /// <returns></returns>
        public static Dictionary<string, object> ConvertToJSTreeJson(this GeoResidentialArea residentialArea, bool ajaxChildren = false)
        {
            return new Dictionary<string, object>()
                             {
                                 {"data", residentialArea.Name},
                                 {
                                     "attr", new Dictionary<string, object>()
                                                 {
                                                     {"id", string.Format("area-{0}", residentialArea.Id)},
                                                     {"real-id", residentialArea.Id},
                                                     {"real-name", residentialArea.Name},
                                                     {"level", (short) GeoLevels.ResidentialArea},
                                                     {"district-id", residentialArea.DistrictId},
                                                     {"bounds", residentialArea.Bounds},
                                                     {"ajax", ajaxChildren},
                                                 }
                                     },
                                 {
                                     "children",
                                     ajaxChildren
                                         ? (object) new object[] {}
                                         : residentialArea.GeoStreets.OrderBy(r => r.Name).Select(r => r.ConvertToJSTreeJson(true)).ToList()
                                     },
                                 {"state", "closed"}
                             };
        }

        /// <summary>
        /// Выполняет конвертацию объекта улицы в справочник, пригодный к JSON сериализации и использовании в компоненте JS Tree
        /// </summary>
        /// <param name="street">Улица</param>
        /// <param name="ajaxChildren">Дети подгружаются аяксом</param>
        /// <returns></returns>
        public static Dictionary<string, object> ConvertToJSTreeJson(this GeoStreet street, bool ajaxChildren = false)
        {
            return new Dictionary<string, object>()
                             {
                                 {"data", street.Name},
                                 {
                                     "attr", new Dictionary<string, object>()
                                                 {
                                                     {"id", string.Format("street-{0}", street.Id)},
                                                     {"real-name", street.Name},
                                                     {"real-id", street.Id},
                                                     {"level", (short) GeoLevels.Street},
                                                     {"area-id", street.AreaId},
                                                     {"ajax", ajaxChildren},
                                                 }
                                     },
                                 {
                                     "children",
                                     ajaxChildren
                                         ? (object) new object[] {}
                                         : street.GeoObjects.OrderBy(r => r.Name).Select(r => r.ConvertToJSTreeJson(true)).ToList()
                                     },
                                 {"state", "closed"}
                             };
        }

        /// <summary>
        /// Выполняет конвертацию объекта на улице в справочник, пригодный к JSON сериализации и использовании в компоненте JS Tree
        /// </summary>
        /// <param name="geoObject">Гео объект</param>
        /// <param name="ajaxChildren">Дети подгружаются аяксом</param>
        /// <returns></returns>
        public static Dictionary<string, object> ConvertToJSTreeJson(this GeoObject geoObject, bool ajaxChildren = false)
        {
            return new Dictionary<string, object>()
                             {
                                 {"data", geoObject.Name},
                                 {
                                     "attr", new Dictionary<string, object>()
                                                 {
                                                     {"id", string.Format("object-{0}", geoObject.Id)},
                                                     {"real-id", geoObject.Id},
                                                     {"real-name", geoObject.Name},
                                                     {"level", (short) GeoLevels.Object},
                                                     {"street-id", geoObject.StreetId},
                                                     {"latitude", geoObject.Latitude},
                                                     {"logitude", geoObject.Longitude},
                                                     {"ajax", ajaxChildren},
                                                 }
                                 }
                             };
        }

    }
}