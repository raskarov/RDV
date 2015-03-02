using System.Collections.Generic;
using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Infrastructure
{
    /// <summary>
    /// Менеджер управляющий системой уведомлений на основании поисковых запросов
    /// </summary>
    public interface IObjectsTriggerManager
    {
        /// <summary>
        /// Вызывают инвокацию события о том, что указанный объект был активирован т.е. переведен в статус активный
        /// </summary>
        /// <param name="obj"></param>
        void ObjectActivated(EstateObject obj);

        /// <summary>
        /// Вызывает инвокацию события о том, что у указанного объекта изменилась цена
        /// </summary>
        /// <param name="obj">Объект недвижимости</param>
        /// <param name="newPrice">Новая цена</param>
        void ObjectPriceChanged(EstateObject obj, double? newPrice);

        /// <summary>
        /// Возвращает список поисковых запросов, под который попадает указанный объект
        /// </summary>
        /// <param name="obj">Объект недвижимости</param>
        /// <returns></returns>
        IList<SearchRequest> GetMatchedRequests(EstateObject obj);

        /// <summary>
        /// Возвращает список поисковых запросов, под которые попадает указанный объект исключая определенные поисковые запросы
        /// </summary>
        /// <param name="estateObject">Объект недвижимости</param>
        /// <param name="skipIds">Перечисление, содержащее идентификаторы элементов, который нужно пропустить</param>
        /// <returns></returns>
        IList<SearchRequest> GetMatchedRequests(EstateObject estateObject, IEnumerable<long> skipIds);
    }
}