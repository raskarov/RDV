namespace RDV.Domain.Enums
{
    /// <summary>
    /// Типы нотификаций для менеджера объектов
    /// </summary>
    public enum ObjectNotificationTypes
    {
        /// <summary>
        /// Забытый объект. Прошло два месяца
        /// </summary>
        [EnumDescription("Забытый объект. Первое предупреждение")]
        ForgotObject1 = 1,

        /// <summary>
        /// Забытый объект. Прошло два месяца и одна неделя
        /// </summary>
        [EnumDescription("Забытый объект. Второе предупреждение")]
        ForgotObject2 = 2,

        /// <summary>
        /// Забытый обхект. Снят с продажи
        /// </summary>
        [EnumDescription("Забытый объект. Снят с продажи")]
        ForgotObject3 = 3,

        /// <summary>
        /// Временно снято с продажи. Первое предупреждение
        /// </summary>
        [EnumDescription("Временно снято с продажи. Первое предупреждение")]
        TemporalyWithdraw1 = 4,

        /// <summary>
        /// Временно снято с продажи. Первое предупреждение
        /// </summary>
        [EnumDescription("Временно снято с продажи. Второе предупреждение")]
        TemporalyWithdraw2 = 5,

        /// <summary>
        /// Временно снято с продажи. Уведомление о том, что объект снят с продажи
        /// </summary>
        [EnumDescription("Временно снято с продажи. Снят с продажи")]
        TemporalyWithdraw3 = 5,
    }
}