namespace RDV.Domain.Enums
{
    /// <summary>
    /// Статусы объектов в избранном
    /// </summary>
    public enum SearchRequestObjectStatus: short
    {
        /// <summary>
        /// Новый объект
        /// </summary>
        [EnumDescription("Новый")]
        New = 1,

        /// <summary>
        /// Объект принят и находится в работе
        /// </summary>
        [EnumDescription("В работе")]
        Accepted = 2,

        /// <summary>
        /// Объект отклонен
        /// </summary>
        [EnumDescription("Отклонен")]
        Declined = 3
    }
}