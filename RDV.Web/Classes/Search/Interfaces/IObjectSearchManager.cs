using System.Collections.Generic;
using RDV.Domain.Entities;
using RDV.Web.Models.Search;

namespace RDV.Web.Classes.Search.Interfaces
{
    /// <summary>
    /// Абстрактный интерфейс менеджера по поиску объектов
    /// </summary>
    public interface IObjectSearchManager
    {
        /// <summary>
        /// Ищет объекты исходя из днных для поиска
        /// </summary>
        /// <param name="searchData">Данные поиска</param>
        /// <returns>Список найденных объектов</returns>
        IList<EstateObject> SearchObjects(IObjectSearchData searchData);

        /// <summary>
        /// Проверяет подходит ли указанный объект под указанные критерии поиска
        /// </summary>
        /// <param name="searchData">Данные по поиску</param>
        /// <param name="estateObject">Объект недвижимости</param>
        /// <returns></returns>
        bool CheckObject(IObjectSearchData searchData, EstateObject estateObject);

        /// <summary>
        /// Выполняет поиск в архиве партнерства на основе указанных дополнительных критериев в объекте поиска
        /// </summary>
        /// <param name="searchData">Данные поиска</param>
        /// <returns>Коллекция найденых объектов</returns>
        IList<EstateObject> SearchPartnershipArchive(IObjectSearchData searchData);
    }
}