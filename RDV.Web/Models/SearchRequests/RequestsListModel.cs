// 
// 
// Solution: RDV
// Project: RDV.Web
// File: RequestsListModel.cs
// 
// Created by: ykors_000 at 26.03.2014 11:39
// 
// Property of SoftGears
// 
// ========

using System.Collections;
using RDV.Domain.Enums;

namespace RDV.Web.Models.SearchRequests
{
    /// <summary>
    /// Модель для данных списка поисковых запросов
    /// </summary>
    public class RequestsListModel
    {
        /// <summary>
        /// Секция запросов
        /// </summary>
        public RequestsListSection Section { get; set; }

        /// <summary>
        /// Набор данных
        /// </summary>
        public IEnumerable Data { get; set; }
    }
}