namespace RDV.Domain.Enums
{
    /// <summary>
    /// Типы клиента
    /// </summary>
    public enum ClientTypes:short
    {
        /// <summary>
        /// Обычный клиент
        /// </summary>
        [EnumDescription("Обычный")]
        Normal = 1,

        /// <summary>
        /// Важный клиент
        /// </summary>
        [EnumDescription("ВИП персона")]
        VIP = 2
    }
}