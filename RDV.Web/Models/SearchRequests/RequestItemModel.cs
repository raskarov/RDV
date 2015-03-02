// 
// 
// Solution: RDV
// Project: RDV.Web
// File: RequestItemModel.cs
// 
// Created by: ykors_000 at 26.03.2014 11:09
// 
// Property of SoftGears
// 
// ========

using System;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;

namespace RDV.Web.Models.SearchRequests
{
    /// <summary>
    /// Модель элемента для списка DevExpress
    /// </summary>
    public class RequestItemModel
    {
        /// <summary>
        /// Идентификатор запроса
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Имя запроса
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Тип объекта в запросе
        /// </summary>
        public string ObjectType { get; set; }

        /// <summary>
        /// Новых объектов в запросе
        /// </summary>
        public int NewObjects { get; set; }

        /// <summary>
        /// Объектов в работе
        /// </summary>
        public int AcceptedObjects { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RequestItemModel(SearchRequest request)
        {
            Id = request.Id;
            Name = request.Title;
            if (request.SearchRequestObjects.Count > 0)
            {
                ObjectType = ((EstateTypes) request.SearchRequestObjects.First().EstateObject.ObjectType).GetEnumMemberName();
            }
            NewObjects = request.SearchRequestObjects.Count(ro => ro.Status == (short)SearchRequestObjectStatus.New);
            AcceptedObjects = request.SearchRequestObjects.Count(ro => ro.Status == (short)SearchRequestObjectStatus.Accepted);
            DateCreated = request.DateCreated.Value;
        }
    }
}