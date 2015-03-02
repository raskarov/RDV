namespace RDV.Domain.Enums
{
    /// <summary>
    /// Типы договоров с клиентом
    /// </summary>
    public enum ClientAgreementTypes:short
    {
        /// <summary>
        /// Обычный договор
        /// </summary>
        [EnumDescription("Обычный")]
        Normal = 1,

        /// <summary>
        /// Эксклюзивный договор
        /// </summary>
        [EnumDescription("Эксклюзивный")]
        Exclusive = 2
    }
}