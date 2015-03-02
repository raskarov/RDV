using System.Collections.Generic;

namespace RDV.Domain.Interfaces.Infrastructure
{
    /// <summary>
    /// Абстрактный менеджер системных утилит
    /// </summary>
    public interface IToolsManager
    {
        /// <summary>
        /// Список системных утилит, зарегистрированных в системе
        /// </summary>
        IList<ITool> Tools { get; }

        /// <summary>
        /// Инициализирует подсистему утилит
        /// </summary>
        void Init();
    }
}