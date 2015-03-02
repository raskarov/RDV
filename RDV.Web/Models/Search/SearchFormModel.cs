using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RDV.Web.Classes.Extensions;
using RDV.Web.Classes.Forms;
using RDV.Web.Classes.Search.Interfaces;

namespace RDV.Web.Models.Search
{
    /// <summary>
    /// Модель, используемая для формы поиска
    /// </summary>
    public class SearchFormModel : IObjectSearchData
    {
        /// <summary>
        /// Идентификатор объекта
        /// </summary>
        public long ObjID { get; set; }

        /// <summary>
        /// Тип объекта, если -1 то любой
        /// </summary>
        public short ObjectType { get; set; }

        /// <summary>
        /// Операция, проводимая над объектом
        /// </summary>
        public short Operation { get; set; }

        /// <summary>
        /// Цена от
        /// </summary>
        public double? PriceFrom { get; set; }

        /// <summary>
        /// Цена до
        /// </summary>
        public double? PriceTo { get; set; }

        /// <summary>
        /// Площадь от
        /// </summary>
        public double? SquareFrom { get; set; }

        /// <summary>
        /// Площадь до
        /// </summary>
        public double? SquareTo { get; set; }

        /// <summary>
        /// Идентификаторы компании-риэлторов
        /// </summary>
        public string CompanyIds { get; set; }

        /// <summary>
        /// Идентификаторы агентов
        /// </summary>
        public string AgentIds { get; set; }

        /// <summary>
        /// Идентификатор города, в котором происходит поиск объектов
        /// </summary>
        public long CityId { get; set; }

        /// <summary>
        /// Строка с идентификаторами выбраных районов
        /// </summary>
        public string DistrictIds { get; set; }

        /// <summary>
        /// Строка с идентификаторами выбранных жилых массивов в районе
        /// </summary>
        public string AreaIds { get; set; }

        /// <summary>
        /// Выбранные улицы города для поиска
        /// </summary>
        public string StreetIds { get; set; }

        /// <summary>
        /// Список дополнительных критериев для поиска
        /// </summary>
        public FieldsList AdditionalCriterias { get; set; }

        /// <summary>
        /// Список настреок фильтра
        /// </summary>
        public Dictionary<string, string> FieldsFilters { get; set; }

        /// <summary>
        /// Наименование поискового запроса для сохранения
        /// </summary>
        public string SearchRequestName { get; set; }

        /// <summary>
        /// Количество комнат - 1
        /// </summary>
        public bool? CountRoom1 { get; set; }

        /// <summary>
        /// Количество комнат - 2
        /// </summary>
        public bool? CountRoom2 { get; set; }

        /// <summary>
        /// Количество комнат - 3
        /// </summary>
        public bool? CountRoom3 { get; set; }

        /// <summary>
        /// Количество комнат - 4 и более
        /// </summary>
        public bool? CountRoom4 { get; set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public SearchFormModel()
        {
            ObjID = 0;
            ObjectType = -1;
            Operation = -1;
            CityId = 4;
            DistrictIds = string.Empty;
            AreaIds = string.Empty;
            StreetIds = string.Empty;
            AdditionalCriterias = new FieldsList();
            FieldsFilters = new Dictionary<string, string>();

            CompanyIds = string.Empty;
            AgentIds = string.Empty;
        }

        /// <summary>
        /// Считывает состояние модели из коллекции форм
        /// </summary>
        public void ReadFromFormCollection(HttpRequestBase request)
        {
            // Основные критерии поиска
            ObjID = Convert.ToInt64(request["objId"]);
            ObjectType = Convert.ToInt16(request["objectType"]);
            Operation = Convert.ToInt16(request["operation"]);
            PriceFrom = request["priceFrom"].ConvertToNullableDouble();
            PriceTo = request["priceTo"].ConvertToNullableDouble();
            SquareFrom = request["squareFrom"].ConvertToNullableDouble();
            SquareTo = request["squareTo"].ConvertToNullableDouble();
            CityId = Convert.ToInt64(request["cityId"]);
            DistrictIds = request["districtIds"];
            AreaIds = request["areaIds"];
            StreetIds = request["streetIds"];
            SearchRequestName = request["requestName"];
            CountRoom1 = request["countRooms1"] != null && request["countRooms1"] == "on" ? (Boolean?)true : null;
            CountRoom2 = request["countRooms2"] != null && request["countRooms2"] == "on" ? (Boolean?)true : null;
            CountRoom3 = request["countRooms3"] != null && request["countRooms3"] == "on" ? (Boolean?)true : null;
            CountRoom4 = request["countRooms4"] != null && request["countRooms4"] == "on" ? (Boolean?)true : null;

            CompanyIds = request["companyIds"] != null ? request["companyIds"] : string.Empty;
            AgentIds = request["agentIds"] != null ? request["agentIds"] : string.Empty;
            
            // Дополнительные критерии
            foreach (var criteriaName in request.QueryString.AllKeys.Where(k => k.StartsWith("af_")))
            {
                var field = FieldsCache.AllSearchFields.Value.FirstOrDefault(f => f.Name == criteriaName.Replace("af_", ""));
                if (field != null)
                {
                    AdditionalCriterias.Add(field);
                    field.Value = request[criteriaName];
                }
            }

            // Фильтры дополнительных критериев
            foreach (var criteriaName in request.QueryString.AllKeys.Where(k => k.StartsWith("filter")))
            {
                var field =
                    FieldsCache.AllSearchFields.Value.FirstOrDefault(f => f.Name == criteriaName.Replace("filter_", ""));
                if (field != null)
                {
                    FieldsFilters[field.Name] = request[criteriaName];
                }
            }

            // Выполняем проверку есть ли у нас поиск по количеству комнат
            var roomsFrom = request["roomsFrom"];
            var roomsTo = request["roomsTo"];
            if (!String.IsNullOrEmpty(roomsFrom) | !String.IsNullOrEmpty(roomsTo))
            {
                var roomsCriteria = AdditionalCriterias.FirstOrDefault(c => c.Name == "rooms-count");
                if (roomsCriteria == null)
                {
                    roomsCriteria = FieldsCache.AllSearchFields.Value.FirstOrDefault(f => f.Name == "rooms-count");
                    AdditionalCriterias.Add(roomsCriteria);
                }
                if (!String.IsNullOrEmpty(roomsFrom) && !String.IsNullOrEmpty(roomsTo))
                {
                    roomsCriteria.Value = String.Format("{0}-{1}", roomsFrom, roomsTo);
                    FieldsFilters["rooms-count"] = "range";
                }
                else if (!String.IsNullOrEmpty(roomsFrom) && String.IsNullOrEmpty(roomsTo))
                {
                    roomsCriteria.Value = String.Format("{0}", roomsFrom);
                    FieldsFilters["rooms-count"] = ">";
                }
                else if (String.IsNullOrEmpty(roomsFrom) && !String.IsNullOrEmpty(roomsTo))
                {
                    roomsCriteria.Value = String.Format("{0}", roomsTo);
                    FieldsFilters["rooms-count"] = "<";
                }    
            }
        }

        public void ReadFromFormCollection(HttpRequest request)
        {
            // NOTE: Придумать как переписать этот фрагмент

            // Основные критерии поиска
            ObjID = Convert.ToInt32(request["objId"]);
            ObjectType = Convert.ToInt16(request["objectType"]);
            Operation = Convert.ToInt16(request["operation"]);
            PriceFrom = request["priceFrom"].ConvertToNullableDouble();
            PriceTo = request["priceTo"].ConvertToNullableDouble();
            SquareFrom = request["squareFrom"].ConvertToNullableDouble();
            SquareTo = request["squareTo"].ConvertToNullableDouble();
            CityId = Convert.ToInt64(request["cityId"]);
            DistrictIds = request["districtIds"];
            AreaIds = request["areaIds"];
            StreetIds = request["streetIds"];
            SearchRequestName = request["requestName"];
            CountRoom1 = request["countRooms1"] != null && request["countRooms1"] == "on" ? (Boolean?)true : null;
            CountRoom2 = request["countRooms2"] != null && request["countRooms2"] == "on" ? (Boolean?)true : null;
            CountRoom3 = request["countRooms3"] != null && request["countRooms3"] == "on" ? (Boolean?)true : null;
            CountRoom4 = request["countRooms4"] != null && request["countRooms4"] == "on" ? (Boolean?)true : null;

            CompanyIds = request["companyIds"] != null ? request["companyIds"] : string.Empty;
            AgentIds = request["agentIds"] != null ? request["agentIds"] : string.Empty;

            // Дополнительные критерии
            foreach (var criteriaName in request.QueryString.AllKeys.Where(k => k.StartsWith("af_")))
            {
                var field = FieldsCache.AllSearchFields.Value.FirstOrDefault(f => f.Name == criteriaName.Replace("af_", ""));
                if (field != null)
                {
                    AdditionalCriterias.Add(field);
                    field.Value = request[criteriaName];
                }
            }

            // Фильтры дополнительных критериев
            foreach (var criteriaName in request.QueryString.AllKeys.Where(k => k.StartsWith("filter")))
            {
                var field =
                    FieldsCache.AllSearchFields.Value.FirstOrDefault(f => f.Name == criteriaName.Replace("filter_", ""));
                if (field != null)
                {
                    FieldsFilters[field.Name] = request[criteriaName];
                }
            }

            // Выполняем проверку есть ли у нас поиск по количеству комнат
            var roomsFrom = request["roomsFrom"];
            var roomsTo = request["roomsTo"];
            if (!String.IsNullOrEmpty(roomsFrom) | !String.IsNullOrEmpty(roomsTo))
            {
                var roomsCriteria = AdditionalCriterias.FirstOrDefault(c => c.Name == "rooms-count");
                if (roomsCriteria == null)
                {
                    roomsCriteria = FieldsCache.AllSearchFields.Value.FirstOrDefault(f => f.Name == "rooms-count");
                    AdditionalCriterias.Add(roomsCriteria);
                }
                if (!String.IsNullOrEmpty(roomsFrom) && !String.IsNullOrEmpty(roomsTo))
                {
                    roomsCriteria.Value = String.Format("{0}-{1}", roomsFrom, roomsTo);
                    FieldsFilters["rooms-count"] = "range";
                }
                else if (!String.IsNullOrEmpty(roomsFrom) && String.IsNullOrEmpty(roomsTo))
                {
                    roomsCriteria.Value = String.Format("{0}", roomsFrom);
                    FieldsFilters["rooms-count"] = ">";
                }
                else if (String.IsNullOrEmpty(roomsFrom) && !String.IsNullOrEmpty(roomsTo))
                {
                    roomsCriteria.Value = String.Format("{0}", roomsTo);
                    FieldsFilters["rooms-count"] = "<";
                }
            }
        }
    }
}