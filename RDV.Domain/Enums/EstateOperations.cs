using System;

namespace RDV.Domain.Enums
{
    /// <summary>
    /// Операции, проводимые над объектами недвижимости
    /// </summary>
    [Flags]
    public enum EstateOperations: short
    {
        /// <summary>
        /// Продажа недвижимости
        /// </summary>
        [EnumDescription("Продажа")]
        Selling = 1,

        /// <summary>
        /// Покупка недвижимости
        /// </summary>
        [EnumDescription("Покупка")]
        Buying = 2,

        /// <summary>
        /// Сдача в аренду
        /// </summary>
        [EnumDescription("Сдача в аренду")]
        Lising = 4,

        /// <summary>
        /// Съем в аренду
        /// </summary>
        [EnumDescription("Съем в аренду")]
        Rent = 8,

        /// <summary>
        /// Обмен
        /// </summary>
        [EnumDescription("Обмен")]
        Exchange = 16
    }
    
    /// <summary>
    /// Операции, проводимые над объектами недвижимости
    /// </summary>
    [Flags]
    public enum EstateOperationsSingle: short
    {
        /// <summary>
        /// Продажа недвижимости
        /// </summary>
        [EnumDescription("Продажа")]
        Selling = 1,
    }
}