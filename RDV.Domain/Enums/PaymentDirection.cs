namespace RDV.Domain.Enums
{
    /// <summary>
    /// Направления платежей
    /// </summary>
    public enum PaymentDirection: short
    {
        /// <summary>
        /// Пополнение лицевого счета
        /// </summary>
        [EnumDescription("Пополнение")]
        Income = 1,

        /// <summary>
        /// Расход средств с лицевого счета
        /// </summary>
        [EnumDescription("Списание")]
        Outcome = 2,
    }
}