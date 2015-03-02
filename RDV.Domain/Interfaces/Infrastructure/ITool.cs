using System;
using RDV.Domain.Enums;

namespace RDV.Domain.Interfaces.Infrastructure
{
    /// <summary>
    /// Абстратный системная утилита, работающая в фоновом режиме
    /// </summary>
    public interface ITool
    {
        /// <summary>
        /// Идентификатор утилиты
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Наименование утилиты
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Начинает выполнение
        /// </summary>
        void StartExecuting();

        /// <summary>
        /// Прерывает выполнение (не всегда возможно)
        /// </summary>
        void Break();

        /// <summary>
        /// Текущий статус утилиты
        /// </summary>
        ToolState State { get; }

        /// <summary>
        /// Интервал повторения выполнения
        /// </summary>
        ToolLaunchInterval Interval { get; set; }

        /// <summary>
        /// Дата и время последнего запуска
        /// </summary>
        DateTime? LastLaunch { get; set; }

        /// <summary>
        /// Дата и время последнего успешного завершения выполнения операций
        /// </summary>
        DateTime? LastCompleted { get; set; }
    }
}