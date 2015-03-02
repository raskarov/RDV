// 
// 
// Solution: RDV
// Project: RDV.Web
// File: MatchedRequestsListModel.cs
// 
// Created by: ykors_000 at 17.01.2014 16:09
// 
// Property of SoftGears
// 
// ========

using System.Collections.Generic;
using RDV.Domain.Entities;
using RDV.Domain.Enums;

namespace RDV.Web.Models.SearchRequests
{
    /// <summary>
    /// Модель для построения грида подходящих поисковых запросов
    /// </summary>
    public class MatchedRequestsListModel
    {
        /// <summary>
        /// Статус рендеренных объектов
        /// </summary>
        public SearchRequestObjectStatus Status { get; set; }

        /// <summary>
        /// Массив данных
        /// </summary>
        public IList<EstateObjectMatchedSearchRequest> Data { get; set; }

        /// <summary>
        /// Идентификатор объекта
        /// </summary>
        public long ObjectId { get; set; }

        /// <summary>
        /// Сам объект недвижимости
        /// </summary>
        public EstateObject EstateObject { get; set; }
    }
}