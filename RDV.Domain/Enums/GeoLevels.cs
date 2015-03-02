namespace RDV.Domain.Enums
{
    /// <summary>
    /// Уровни, обозначающие привязки географических объектов
    /// </summary>
    public enum GeoLevels: short
    {
        /// <summary>
        /// Страна
        /// </summary>
        [EnumDescription("Страна")]
        Country = 1,

        /// <summary>
        /// Регион
        /// </summary>
        [EnumDescription("Регион")]
        Region = 2,

        /// <summary>
        /// Район региона
        /// </summary>
        [EnumDescription("Район региона")]
        RegionDistrict = 3,

        /// <summary>
        /// Город
        /// </summary>
        [EnumDescription("Город")]
        City = 4,

        /// <summary>
        /// Район города
        /// </summary>
        [EnumDescription("Район города")]
        District = 5,

        /// <summary>
        /// Жилой массив
        /// </summary>
        [EnumDescription("Жилой массив")]
        ResidentialArea = 6,

        /// <summary>
        /// Улица
        /// </summary>
        [EnumDescription("Улица")]
        Street = 7,

        /// <summary>
        /// Объект
        /// </summary>
        [EnumDescription("Объект")]
        Object = 8
    }
}