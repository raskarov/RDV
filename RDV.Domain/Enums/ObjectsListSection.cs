// 
// 
// Solution: RDV
// Project: RDV.Domain
// File: ObjectsListSection.cs
// 
// Created by: ykors_000 at 29.11.2013 14:41
// 
// Property of SoftGears
// 
// ========
namespace RDV.Domain.Enums
{
    /// <summary>
    /// Место расположения в списке объектов
    /// </summary>
    public enum ObjectsListSection: short
    {
        /// <summary>
        /// Список моих объектов
        /// </summary>
        [EnumDescription("Мои объекты")]
        MyObjects = 1,

        /// <summary>
        /// Список объектов компании
        /// </summary>
        [EnumDescription("Объекты компании")]
        CompanyObjects = 2,

        /// <summary>
        /// Список всех объектов
        /// </summary>
        [EnumDescription("Все объекты")]
        AllObjects = 3
    }
}