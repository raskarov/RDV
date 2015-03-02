using System.Collections.Generic;
using RDV.Web.Classes.Forms;

namespace RDV.Web.Classes.Search.Interfaces
{
    /// <summary>
    /// Интерфейс инкапсулирующий данные используемые для поиска объектов
    /// </summary>
    public interface IObjectSearchData
    {
        /// <summary>
        /// Идентификатор объекта
        /// </summary>
        long ObjID { get; set; }

        /// <summary>
        /// Тип объекта, если -1 то любой
        /// </summary>
        short ObjectType { get; set; }

        /// <summary>
        /// Операция, проводимая над объектом
        /// </summary>
        short Operation { get; set; }

        /// <summary>
        /// Цена от
        /// </summary>
        double? PriceFrom { get; set; }

        /// <summary>
        /// Цена до
        /// </summary>
        double? PriceTo { get; set; }

        /// <summary>
        /// Площадь от
        /// </summary>
        double? SquareFrom { get; set; }

        /// <summary>
        /// Площадь до
        /// </summary>
        double? SquareTo { get; set; }

        /// <summary>
        /// Идентификатор города, в котором происходит поиск объектов
        /// </summary>
        long CityId { get; set; }

        /// <summary>
        /// Строка с идентификаторами выбраных районов
        /// </summary>
        string DistrictIds { get; set; }

        /// <summary>
        /// Строка с идентификаторами выбранных жилых массивов в районе
        /// </summary>
        string AreaIds { get; set; }

        /// <summary>
        /// Строка, с идентификаторами выбранных улиц
        /// </summary>
        string StreetIds { get; set; }

        /// <summary>
        /// Список дополнительных критериев для поиска
        /// </summary>
        FieldsList AdditionalCriterias { get; set; }

        /// <summary>
        /// Фильтры полей
        /// </summary>
        Dictionary<string, string> FieldsFilters { get; set; }

        /// <summary>
        /// Количество комнат - 1
        /// </summary>
        bool? CountRoom1 { get; set; }

        /// <summary>
        /// Количество комнат - 2
        /// </summary>
        bool? CountRoom2 { get; set; }

        /// <summary>
        /// Количество комнат - 3
        /// </summary>
        bool? CountRoom3 { get; set; }

        /// <summary>
        /// Количество комнат - 4 и более
        /// </summary>
        bool? CountRoom4 { get; set; }

        /// <summary>
        /// Идентификаторы компаний
        /// </summary>
        string CompanyIds { get; set; }

        /// <summary>
        /// Идентификаторы агентов
        /// </summary>
        string AgentIds { get; set; }
    }
}