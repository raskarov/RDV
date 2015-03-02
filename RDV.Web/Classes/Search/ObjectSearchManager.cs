using System;
using System.Collections.Generic;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Classes.Forms.Fields;
using RDV.Web.Classes.Search.Interfaces;

namespace RDV.Web.Classes.Search
{
    /// <summary>
    /// Реализация менеджера по поиску объектов
    /// </summary>
    public class ObjectSearchManager: IObjectSearchManager
    {
        /// <summary>
        /// Репозиторий объектов недвижимости
        /// </summary>
        private IEstateObjectsRepository EstateObjectsRepository { get; set; }

        /// <summary>
        /// Стандартный конструктор с инъекцией параметров
        /// </summary>
        /// <param name="estateObjectsRepository">Репозиторий объектов недвижимости</param>
        public ObjectSearchManager(IEstateObjectsRepository estateObjectsRepository)
        {
            EstateObjectsRepository = estateObjectsRepository;
        }

        /// <summary>
        /// Ищет объекты исходя из днных для поиска
        /// </summary>
        /// <param name="searchData">Данные поиска</param>
        /// <returns>Список найденных объектов</returns>
        public IList<EstateObject> SearchObjects(IObjectSearchData searchData)
        {
            // Список куда помещать
            var resultList = new List<EstateObject>();

            // Простой фильтр по основным критериям поиска (тип, операция, цена, площадь)
            IEnumerable<EstateObject> query =
                EstateObjectsRepository.Search(o => o.Status == (short) EstateStatuses.Active);

            if (searchData.ObjID > 0)
            {
                query = query.Where(x => x.Id == searchData.ObjID);
                resultList = query.ToList();
                return resultList;
            }

            if (searchData.ObjectType != -1)
            {
                query = query.Where(o => o.ObjectType == searchData.ObjectType);
            }
            if (searchData.Operation != -1)
            {
                query = query.Where(o => o.Operation == searchData.Operation);
            }
            if (searchData.PriceFrom.HasValue)
            {
                query =
                    query.Where(
                        o =>
                        o.ObjectMainProperties.Price.HasValue && o.ObjectMainProperties.Price.Value >= searchData.PriceFrom.Value);
            }
            if (searchData.PriceTo.HasValue)
            {
                query = query.Where(
                        o =>
                        o.ObjectMainProperties.Price.HasValue && o.ObjectMainProperties.Price.Value <= searchData.PriceTo.Value);
            }
            if (searchData.SquareFrom.HasValue)
            {
                query =
                    query.Where(
                        o =>
                        o.ObjectMainProperties.TotalArea.HasValue && o.ObjectMainProperties.TotalArea.Value >= searchData.SquareFrom.Value);
            }
            if (searchData.SquareTo.HasValue)
            {
                query = query.Where(
                        o =>
                        o.ObjectMainProperties.TotalArea.HasValue && o.ObjectMainProperties.TotalArea.Value <= searchData.SquareTo.Value);
            }

            var searchByCountRoom1 = searchData.CountRoom1.HasValue && searchData.CountRoom1.Value;
            var searchByCountRoom2 = searchData.CountRoom2.HasValue && searchData.CountRoom2.Value;
            var searchByCountRoom3 = searchData.CountRoom3.HasValue && searchData.CountRoom3.Value;
            var searchByCountRoom4 = searchData.CountRoom4.HasValue && searchData.CountRoom4.Value;

            query = query.Where(
                    o => (searchByCountRoom1 && o.ObjectAdditionalProperties.RoomsCount == 1) ||
                         (searchByCountRoom2 && o.ObjectAdditionalProperties.RoomsCount == 2) ||
                         (searchByCountRoom3 && o.ObjectAdditionalProperties.RoomsCount == 3) ||
                         (searchByCountRoom4 && o.ObjectAdditionalProperties.RoomsCount >= 4) ||
                         (!searchByCountRoom1 && !searchByCountRoom2 && !searchByCountRoom3 && !searchByCountRoom4));

            if (!String.IsNullOrEmpty(searchData.StreetIds))
            {
                var streetIds = searchData.StreetIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(
                    i => Convert.ToInt64(i)).ToList();
                if (streetIds.Count > 0)
                {
                    query = query.Where(o => streetIds.Contains(o.Address.StreetId.HasValue ? o.Address.StreetId.Value : -1));
                }
            }

            // Фильтр по координатам
            if (searchData.CityId != -1)
            {
                // Фильтр по городам
                query = query.Where(o => o.Address.CityId == searchData.CityId);
                if (!string.IsNullOrEmpty(searchData.DistrictIds))
                {
                    // Фильтр по районам
                    var districtIds =
                        searchData.DistrictIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(
                            i => Convert.ToInt64(i)).ToList();
                    if (districtIds.Count > 0)
                    {
                        query = query.Where(o => districtIds.Contains(o.Address.CityDistrictId.HasValue ? o.Address.CityDistrictId.Value : -1));
                    }

                    // Фильтр по жил. массивам
                    if (districtIds.Count == 1)
                    {
                        var areaIds = searchData.AreaIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(
                            i => Convert.ToInt64(i)).ToList();
                        if (areaIds.Count > 0)
                        {
                            query = query.Where(o => areaIds.Contains(o.Address.DistrictResidentialAreaId.HasValue ? o.Address.DistrictResidentialAreaId.Value : -1));
                        }
                    }

                    // Фильтр по улицам
                    if (!String.IsNullOrEmpty(searchData.StreetIds))
                    {
                        var streetIds = searchData.StreetIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(
                            i => Convert.ToInt64(i)).ToList();
                        if (streetIds.Count > 0)
                        {
                            query = query.Where(o => streetIds.Contains(o.Address.StreetId.HasValue ? o.Address.StreetId.Value : -1));
                        }
                    }
                }
            }

            if (!String.IsNullOrEmpty(searchData.CompanyIds))
            {
                var companyIds = searchData.CompanyIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(
                    i => Convert.ToInt64(i)).ToList();
                if (companyIds.Count > 0)
                {
                    query = query.Where(o => companyIds.Contains(o.User.CompanyId));
                }
            }

            if (!String.IsNullOrEmpty(searchData.AgentIds))
            {
                var agentIds = searchData.AgentIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(
                    i => Convert.ToInt64(i)).ToList();
                if (agentIds.Count > 0)
                {
                    query = query.Where(o => agentIds.Contains(o.UserId));
                }
            }

            // Делаем выборку
            resultList.AddRange(query.AsEnumerable());

            // Ищем по дополнительным критериям поиска
            if (searchData.AdditionalCriterias.Count > 0)
            {
                // Новый список с результатами
                var newList = new List<EstateObject>(); //

                // Перебираем список объектов
                foreach (var estateObject in resultList)
                {
                    // bool need to add
                    var adding = false;

                    adding = CheckAdditionalCriteries(searchData, estateObject);

                    // Добавляем объект если он нужен
                    if (adding)
                    {
                        newList.Add(estateObject);
                    }
                }

                // Подменяем результаты
                resultList = newList.Where(u => u.User != null && u.User.Company != null).ToList();
            }

            // Отдаем результат
            return resultList;
        }

        /// <summary>
        /// Проверяет подходит ли указанный объект по дополнительным критериям поиска
        /// </summary>
        /// <param name="searchData">Данные для поиска</param>
        /// <param name="estateObject">Объект</param>
        /// <returns></returns>
        public bool CheckAdditionalCriteries(IObjectSearchData searchData, EstateObject estateObject)
        {
            var adding = false;
            
            // Перебираем все критерии
            foreach (var additionalCriteria in searchData.AdditionalCriterias)
            {
                // Значения
                var criteriaValue = additionalCriteria.Value;
                object objectValue = additionalCriteria.GetValueFromObject(estateObject);
                string strValue = objectValue != null ? objectValue.ToString() : String.Empty;

                // Проверяем что у нас критерии, требующие множественного сравнения
                if (additionalCriteria is ObjectFormDictionaryField |
                    additionalCriteria is ObjectFormMultiDictionaryField |
                    additionalCriteria is ObjectFormMultiDropDownField)
                {
                    // Object ids
                    var requiredIds =
                        criteriaValue.Split(',')
                                     .Where(s => !String.IsNullOrEmpty(s))
                                     .Select(s => Convert.ToInt64(s))
                                     .ToList();
                    var objectIds =
                        strValue.Split(',')
                                .Where(s => !String.IsNullOrEmpty(s))
                                .Select(s => Convert.ToInt64(s))
                                .ToList();

                    // Сравниваем две последовательности
                    var matched = objectIds.Intersect(requiredIds).ToList();
                    adding = matched.Count > 0;
                }
                else if (additionalCriteria is ObjectFormDropDownField)
                {
                    var requiredId = criteriaValue;
                    var objectId = objectValue;

                    adding = Convert.ToInt64(requiredId) == Convert.ToInt64(objectId);
                }
                // Проеряем что у нас критерии, требующие сравнения по булевому типу
                else if (additionalCriteria is ObjectFormCheckboxField)
                {
                    var criteriaBoolValue = criteriaValue.Contains("true");
                    var objectBoolValue = strValue.ToLower().Contains("true") || strValue.Contains("1");
                    adding = objectBoolValue == criteriaBoolValue;
                }
                // Все прочие типы критериев
                else
                {
                    // Использование фильтров
                    var fieldFilter = searchData.FieldsFilters.ContainsKey(additionalCriteria.Name)
                                          ? searchData.FieldsFilters[additionalCriteria.Name]
                                          : "";
                    var filterOperation = fieldFilter;
                    if (filterOperation == "=")
                    {
                        adding = criteriaValue.Trim().ToLower() == strValue.Trim().ToLower();
                    }
                    else if (filterOperation == "range")
                    {
                        if (additionalCriteria.IsNumeric)
                        {
                            var parts = criteriaValue.Split('-').ToList();
                            if (parts.Count < 2)
                            {
                                continue;
                            }
                            double min, max, value;
                            if (!double.TryParse(parts[0].ToString(), out min) |
                                !double.TryParse(parts[1].ToString(), out max) |
                                !double.TryParse(strValue, out value))
                            {
                                continue;
                            }
                            adding = value >= min && value <= max;
                        }
                    }
                    else if (filterOperation == "%")
                    {
                        adding = criteriaValue.Trim().ToLower().Contains(strValue.Trim().ToLower());
                    }
                    else
                    {
                        // Если поле является числовым
                        if (additionalCriteria.IsNumeric)
                        {
                            double objectNumeric, criteriaNumeric;
                            if (!double.TryParse(strValue, out objectNumeric) |
                                !double.TryParse(criteriaValue.ToString(), out criteriaNumeric))
                            {
                                continue;
                            }
                            switch (filterOperation)
                            {
                                case "!=":
                                    adding = objectNumeric != criteriaNumeric;
                                    break;
                                case ">":
                                    adding = objectNumeric > criteriaNumeric;
                                    break;
                                case "<":
                                    adding = objectNumeric < criteriaNumeric;
                                    break;
                            }
                        }
                    }
                }

                if (!adding)
                {
                    return false;
                }
            }

            return adding;
        }

        /// <summary>
        /// Проверяет подходит ли указанный объект под указанные критерии поиска
        /// </summary>
        /// <param name="searchData">Данные по поиску</param>
        /// <param name="estateObject">Объект недвижимости</param>
        /// <returns></returns>
        public bool CheckObject(IObjectSearchData searchData, EstateObject estateObject)
        {
            // Проверка на соответствие типу
            if (estateObject.ObjectType != searchData.ObjectType && searchData.ObjectType > 0)
            {
                return false;
            }

            // Проверка на соответствие операции
            if (estateObject.Operation != searchData.Operation && searchData.Operation > 0)
            {
                return false;
            }

            // Проверка на соответствие цены
            if (searchData.PriceFrom.HasValue)
            {
                if (estateObject.ObjectMainProperties.Price.HasValue == false)
                {
                    return false;
                }
                if (estateObject.ObjectMainProperties.Price.Value < searchData.PriceFrom.Value)
                {
                    return false;
                }
            }
            if (searchData.PriceTo.HasValue)
            {
                if (estateObject.ObjectMainProperties.Price.HasValue == false)
                {
                    return false;
                }
                if (estateObject.ObjectMainProperties.Price.Value > searchData.PriceTo.Value)
                {
                    return false;
                }
            }

            //Проверка на соответствие площади
            if (searchData.SquareFrom.HasValue)
            {
                if (estateObject.ObjectMainProperties.TotalArea.HasValue == false)
                {
                    return false;
                }
                if (estateObject.ObjectMainProperties.TotalArea.Value < searchData.SquareFrom.Value)
                {
                    return false;
                }
            }
            if (searchData.SquareTo.HasValue)
            {
                if (estateObject.ObjectMainProperties.TotalArea.HasValue == false)
                {
                    return false;
                }
                if (estateObject.ObjectMainProperties.TotalArea.Value > searchData.SquareTo.Value)
                {
                    return false;
                }
            }

            // Проверка на соответствие географическому адресу
            if (searchData.CityId != -1)
            {
                if (estateObject.Address.CityId != searchData.CityId)
                {
                    return false;
                }
                else
                {
                    // Фильтр по районам
                    var districtIds =
                        searchData.DistrictIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(
                            i => Convert.ToInt64(i)).ToList();
                    if (districtIds.Count > 0)
                    {
                        if (!districtIds.Contains(estateObject.Address.CityDistrictId.HasValue ? estateObject.Address.CityDistrictId.Value : -1))
                        {
                            return false;
                        }
                    }

                    // Фильтр по жил. массивам
                    if (districtIds.Count == 1)
                    {
                        var areaIds = searchData.AreaIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(
                            i => Convert.ToInt64(i)).ToList();
                        if (areaIds.Count > 0)
                        {
                            if (!areaIds.Contains(estateObject.Address.DistrictResidentialAreaId.HasValue ? estateObject.Address.DistrictResidentialAreaId.Value : -1))
                            {
                                return false;
                            }
                        }
                    }

                    // Фильтр по улицам
                    if (!String.IsNullOrEmpty(searchData.StreetIds))
                    {
                        var streetIds = searchData.StreetIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(
                            i => Convert.ToInt64(i)).ToList();
                        if (streetIds.Count > 0)
                        {
                            if (!streetIds.Contains(estateObject.Address.StreetId.HasValue ? estateObject.Address.StreetId.Value : -1))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            // фильтр по дополнительным критериям
            if (searchData.AdditionalCriterias.Count > 0)
            {
                var additionalMatch = CheckAdditionalCriteries(searchData, estateObject);
                if (!additionalMatch)
                {
                    return false; // Проверку не прошли
                }    
            }

            // В самом низу
            return true;
        }

        /// <summary>
        /// Выполняет поиск в архиве партнерства на основе указанных дополнительных критериев в объекте поиска
        /// </summary>
        /// <param name="searchData">Данные поиска</param>
        /// <returns>Коллекция найденых объектов</returns>
        public IList<EstateObject> SearchPartnershipArchive(IObjectSearchData searchData)
        {
            // Список куда помещать
            var resultList = new List<EstateObject>();

            // Фильтруем все объекты, находящиеся в статусе сделка
            IEnumerable<EstateObject> query =
                EstateObjectsRepository.Search(o => o.Status == (short)EstateStatuses.Deal);

            // Фильтр по местоположению
            if (searchData.CityId != -1)
            {
                // Фильтр по городам
                query = query.Where(o => o.Address.CityId == searchData.CityId);
                if (!string.IsNullOrEmpty(searchData.DistrictIds))
                {
                    // Фильтр по районам
                    var districtIds =
                        searchData.DistrictIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(
                            i => Convert.ToInt64(i)).ToList();
                    if (districtIds.Count > 0)
                    {
                        query = query.Where(o => districtIds.Contains(o.Address.CityDistrictId.HasValue ? o.Address.CityDistrictId.Value : -1));
                    }

                    // Фильтр по жил. массивам
                    if (districtIds.Count == 1)
                    {
                        var areaIds = searchData.AreaIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(
                            i => Convert.ToInt64(i)).ToList();
                        if (areaIds.Count > 0)
                        {
                            query = query.Where(o => areaIds.Contains(o.Address.DistrictResidentialAreaId.HasValue ? o.Address.DistrictResidentialAreaId.Value : -1));
                        }
                    }

                    // Фильтр по улицам
                    if (!String.IsNullOrEmpty(searchData.StreetIds))
                    {
                        var streetIds = searchData.StreetIds.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(
                            i => Convert.ToInt64(i)).ToList();
                        if (streetIds.Count > 0)
                        {
                            query = query.Where(o => streetIds.Contains(o.Address.StreetId.HasValue ? o.Address.StreetId.Value : -1));
                        }
                    }
                }
            }

            // Критерии должны быть заданы
            if (searchData.AdditionalCriterias.Count == 0)
            {
                return query.ToList();
            }

            // Фильтруем объекты по дополнительным критериям
            resultList.AddRange(query.AsEnumerable().Where(foundedObject => CheckAdditionalCriteries(searchData, foundedObject)));

            // Отдаем результат
            return resultList;
        }
    }
}