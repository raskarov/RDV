namespace RDV.Domain.Enums
{
    /// <summary>
    /// Статусы объекта недвижимости
    /// </summary>
    public enum EstateStatuses: short
    {
        /// <summary>
        /// Объект находится в статусе черновик и виден только своему автору
        /// </summary>
        [EnumDescription("Черновик")]
        Draft = 0,

        /// <summary>
        /// Активный объект
        /// </summary>
        [EnumDescription("Активный")]
        Active = 1,

        /// <summary>
        /// Внесен аванс
        /// </summary>
        [EnumDescription("Аванс")]
        Advance = 2,

        /// <summary>
        /// Объект временно снят с продажи
        /// </summary>
        [EnumDescription("Временно снято с продажи")]
        TemporarilyWithdrawn = 3,

        /// <summary>
        /// Объект снят с продажи
        /// </summary>
        [EnumDescription("Снято с продажи")]
        Withdrawn = 4,

        /// <summary>
        /// Объект в статусе сделки
        /// </summary>
        [EnumDescription("Сделка")]
        Deal = 5
    }
}