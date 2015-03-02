namespace RDV.Domain.Enums
{
    /// <summary>
    /// Тип события которое вызвало помещение объекта под поисковый запрос
    /// </summary>
    public enum SearchRequestTriggerEvent: short
    {
        /// <summary>
        /// Объекту был присвоем статус активный
        /// </summary>
        [EnumDescription("Активация объекта")]
        Activation = 1,

        /// <summary>
        /// Была изменена цена на объект
        /// </summary>
        [EnumDescription("Изменение цены")]
        PriceChanging = 2,

        /// <summary>
        /// Был создан поисковый запрос
        /// </summary>
        [EnumDescription("Создание запроса")]
        RequestCreation = 3
    }
}