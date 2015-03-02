using System;
using System.Threading.Tasks;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Domain.Core;

namespace RDV.Domain.Infrastructure.Tools
{
    /// <summary>
    /// Базовый инструмент
    /// </summary>
    public abstract class BaseTool: ITool
    {
        /// <summary>
        /// Идентификатор утилиты
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Наименование утилиты
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Нить, в которой выполняется задача
        /// </summary>
        protected Task<bool> ExecutingThread { get; set; }

        /// <summary>
        /// Действие в нити должно быть прервано как можно скорее
        /// </summary>
        protected bool Breaking { get; set; }

        /// <summary>
        /// Интервал повторения выполнения
        /// </summary>
        public ToolLaunchInterval Interval { get; set; }

        /// <summary>
        /// Дата и время последнего запуска
        /// </summary>
        public DateTime? LastLaunch { get; set; }

        /// <summary>
        /// Дата и время последнего успешного завершения выполнения операций
        /// </summary>
        public DateTime? LastCompleted { get; set; }

        /// <summary>
        /// Начинает выполнение
        /// </summary>
        public void StartExecuting()
        {
            if (State == ToolState.Running)
            {
                return;
            }
            ExecutingThread.Start();
        }

        /// <summary>
        /// Прерывает выполнение (не всегда возможно)
        /// </summary>
        public void Break()
        {
            Breaking = true;
        }

        /// <summary>
        /// Текущий статус утилиты
        /// </summary>
        public ToolState State { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        protected BaseTool()
        {
            State = ToolState.Pending;
            Breaking = false;
            ExecutingThread = new Task<bool>(() =>
                {
                    State = ToolState.Running;
                    LastLaunch = DateTimeZone.Now;
                    Breaking = false;
                    Execute();
                    State = ToolState.Pending;
                    LastCompleted = DateTimeZone.Now;
                    SaveState();
                    return true;
                });
        }

        /// <summary>
        /// Действие, выполняемое непосредственно в задаче
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Сохраняет время завершения задачи в настройках системы
        /// </summary>
        private void SaveState()
        {
            if (LastCompleted.HasValue)
            {
                using (var scope = Locator.BeginNestedHttpRequestScope())
                {
                    var settingsRep = Locator.GetService<ISettingsRepository>();
                    var keyName = String.Format("tool_{0}_last_end", Id);
                    settingsRep.SetValue(keyName, LastCompleted.Value.ToString("dd.MM.yyyy hh:mm:ss"));
                    settingsRep.SubmitChanges();
                }    
            }
        }
    }
}