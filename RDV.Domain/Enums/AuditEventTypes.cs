namespace RDV.Domain.Enums
{
    /// <summary>
    /// Типы событий аудита
    /// </summary>
    public enum AuditEventTypes: short
    {
        /// <summary>
        /// Просмотр страниц
        /// </summary>
        [EnumDescription("Просмотр раздела")]
        ViewPage = 1,

        /// <summary>
        /// Изменение какого либо объекта
        /// </summary>
        [EnumDescription("Редактирование объекта")]
        Editing = 2,

        /// <summary>
        /// Системное событие
        /// </summary>
        [EnumDescription("Системное событие")]
        System = 3
    }
}