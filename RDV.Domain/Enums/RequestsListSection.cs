// 
// 
// Solution: RDV
// Project: RDV.Domain
// File: RequestsListSection.cs
// 
// Created by: ykors_000 at 26.03.2014 10:58
// 
// Property of SoftGears
// 
// ========
namespace RDV.Domain.Enums
{
    /// <summary>
    /// Секции раздела запросов
    /// </summary>
    public enum RequestsListSection: short
    {
        /// <summary>
        /// Мои запросы
        /// </summary>
        MyRequests = 1,

        /// <summary>
        /// Запросы компаний
        /// </summary>
        CompanyRequests = 2,

        /// <summary>
        /// Все запросы
        /// </summary>
        AllRequests = 3
    }
}