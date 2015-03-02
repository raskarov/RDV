// 
// 
// Solution: RDV
// Project: RDV.Web
// File: ObjectListModel.cs
// 
// Created by: ykors_000 at 29.11.2013 16:11
// 
// Property of SoftGears
// 
// ========

using System.Collections;
using RDV.Domain.Enums;

namespace RDV.Web.Models.Objects
{
    /// <summary>
    /// Модель частичного вида
    /// </summary>
    public class ObjectListModel
    {
        /// <summary>
        /// Статус объектов
        /// </summary>
        public EstateStatuses Status { get; set; }

        /// <summary>
        /// Секция объектов
        /// </summary>
        public ObjectsListSection Section { get; set; }

        /// <summary>
        /// Тип объектов
        /// </summary>
        public EstateTypes EstateType { get; set; }

        /// <summary>
        /// Массив данных
        /// </summary>
        public IEnumerable Data { get; set; }
    }
}