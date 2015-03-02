namespace RDV.Domain.Enums
{
    /// <summary>
    /// Тип недвижимости
    /// </summary>
    public enum EstateTypes:short
    {
        /// <summary>
        /// Квартира
        /// </summary>
        [EnumDescription("Квартира")]
        Flat = 2,
        
        /// <summary>
        /// Комната
        /// </summary>
        [EnumDescription("Комната")]
        Room = 1,

        /// <summary>
        /// Дом
        /// </summary>
        [EnumDescription("Дом")]
        House = 3,

        /// <summary>
        /// Земля
        /// </summary>
        [EnumDescription("Земля")]
        Land = 4,

        /// <summary>
        /// Коммерческая недвижимость
        /// </summary>
        [EnumDescription("Коммерческая недвижимость")]
        Office = 5,

        /// <summary>
        /// Гараж/парковочное место
        /// </summary>
        [EnumDescription("Парковочное место/гараж")]
        Garage = 6
    }
}