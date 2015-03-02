// 
// 
// Solution: RDV
// Project: RDV.Web
// File: RequestObjectsListModel.cs
// 
// Created by: ykors_000 at 27.03.2014 11:17
// 
// Property of SoftGears
// 
// ========

using System.Collections;
using RDV.Domain.Entities;
using RDV.Domain.Enums;

namespace RDV.Web.Models.SearchRequests
{
    /// <summary>
    /// Модель для грида объектов поискового запроса
    /// </summary>
    public class RequestObjectsListModel
    {
        /// <summary>
        /// Поисковый запрос
        /// </summary>
        public SearchRequest Request { get; set; }

        /// <summary>
        /// Статус объектов в запросе
        /// </summary>
        public SearchRequestObjectStatus Status { get; set; }

        /// <summary>
        /// Массив данных
        /// </summary>
        public IEnumerable Data { get; set; }
    }
}