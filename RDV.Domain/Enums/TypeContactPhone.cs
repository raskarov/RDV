namespace RDV.Domain.Enums
{
    /// <summary>
    /// Тип контактного телефона
    /// </summary>
    public enum TypeContactPhone: short
    {
        /// <summary>
        /// Телефон агента 1
        /// </summary>
        [EnumDescription("Телефон агента 1")]
        AgentPhone1 = 1,

        /// <summary>
        /// Телефон агента 2
        /// </summary>
        [EnumDescription("Телефон агента 2")]
        AgentPhone2 = 2,

        /// <summary>
        /// Телефон компании основной
        /// </summary>
        [EnumDescription("Основной телефон компании")]
        CompanyPhone1 = 3,

        /// <summary>
        /// Телефон компании дополнительный 1
        /// </summary>
        [EnumDescription("Дополнительный 1 телефон компании")]
        CompanyPhone2 = 4,

        /// <summary>
        /// Телефон компании дополнительный 2
        /// </summary>
        [EnumDescription("Дополнительный 2 телефон компании")]
        CompanyPhone3 = 5
    }
}