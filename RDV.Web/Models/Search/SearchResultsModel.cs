using System.Collections.Generic;
using RDV.Domain.Entities;

namespace RDV.Web.Models.Search
{
    /// <summary>
    /// Модель с результатами поиска
    /// </summary>
    public class SearchResultsModel
    {
        /// <summary>
        /// Модель с данными для поиска
        /// </summary>
        public SearchFormModel SearchData { get; set; }

        /// <summary>
        /// Найденные объекты
        /// </summary>
        public IList<EstateObject> SearchResults { get; set; }
    }
}