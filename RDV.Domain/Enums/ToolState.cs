namespace RDV.Domain.Enums
{
    /// <summary>
    /// Статус системной утилиты
    /// </summary>
    public enum ToolState
    {
        /// <summary>
        /// Утилита не запущена и находиться в состоянии ожидания своего выполнения
        /// </summary>
        [EnumDescription("Ожидает")]
        Pending = 1,
        
        /// <summary>
        /// Утилита запущена и работает в данный момент
        /// </summary>
        [EnumDescription("Выполняется")]
        Running = 2
    }
}