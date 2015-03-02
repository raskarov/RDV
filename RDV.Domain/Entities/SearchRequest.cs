using System.Collections.Generic;
using System.Linq;
using RDV.Domain.Enums;

namespace RDV.Domain.Entities
{
    /// <summary>
    /// Поисковый запрос
    /// </summary>
    public partial class SearchRequest
    {
        /// <summary>
        /// Возвращает все объекты, подходящие под поисковый запрос
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SearchRequestObject> GetRequestObjects()
        {
            return SearchRequestObjects.OrderByDescending(d => d.DateMoved).ThenByDescending(d => d.DateCreated);
        }

        /// <summary>
        /// Возвращает количество новых объектов
        /// </summary>
        /// <returns></returns>
        public int GetNewObjectsCount()
        {
            return SearchRequestObjects.Count(c => c.Status == (short) SearchRequestObjectStatus.New);
        }

        /// <summary>
        /// Возвращает количество объектов в работе
        /// </summary>
        /// <returns></returns>
        public int GetWorkObjectsCount()
        {
            return SearchRequestObjects.Count(c => c.Status == (short)SearchRequestObjectStatus.Accepted);
        }

        /// <summary>
        /// Возвращает true если у поискового запроса есть новые объекты
        /// </summary>
        /// <returns></returns>
        public bool HasNewObjects()
        {
            return SearchRequestObjects.Any(c => c.Status == (short) SearchRequestObjectStatus.New);
        }
    }
}