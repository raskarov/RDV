using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Domain.Core;

namespace RDV.Domain.Infrastructure.Tools
{
    /// <summary>
    /// Менеджер утилит
    /// </summary>
    public class ToolsManager: IToolsManager
    {
        /// <summary>
        /// Список системных утилит, зарегистрированных в системе
        /// </summary>
        public IList<ITool> Tools { get; private set; }

        /// <summary>
        /// Таймер, который срабатывает каждую минуту и запускает задачи по необходимости
        /// </summary>
        public System.Threading.Timer MinuteTimer { get; private set; }

        /// <summary>
        /// Инициализирует подсистему утилит
        /// </summary>
        public void Init()
        {
            using (var scope = Locator.BeginNestedHttpRequestScope())
            {
                // Репозиторий настроек для восстановления статуса утилит
                var settingsRep = Locator.GetService<ISettingsRepository>();
                
                // Инициализируем список утилит и ищем всех наследников
                Tools = new List<ITool>();
                var types = from type in Assembly.GetExecutingAssembly().GetTypes()
                            where type.IsSubclassOf(typeof(BaseTool)) && !type.IsAbstract
                            select type;
                foreach (ITool instance in types.Select(Activator.CreateInstance))
                {
                    Tools.Add(instance);
                    // Восстанавливаем дату последнего запуска
                    var keyName = String.Format("tool_{0}_last_end", instance.Id);
                    if (settingsRep.HasSetting(keyName))
                    {
                        var lastLaunchDateTime = settingsRep.GetValue<DateTime>(keyName);
                        instance.LastLaunch = lastLaunchDateTime;
                        instance.LastCompleted = lastLaunchDateTime;
                    }
                }
            }

            // Инициализация таймера
            MinuteTimer = new Timer(state => MinuteTick(),null,60000,60000);
        }

        /// <summary>
        /// Уведомляет все события о том что прошла минута и в случае необходимости запускает события
        /// </summary>
        public void MinuteTick()
        {
            foreach (var tool in Tools.Where(t => t.State == ToolState.Pending).ToList())
            {
                // Запускаем утилиту если она еще не была запущена или ее последнее выполнение завершилось с ошибкой
                if (!tool.LastLaunch.HasValue || !tool.LastCompleted.HasValue)
                {
                    tool.StartExecuting();
                    continue;
                }

                // Проверяем период который прошел с момента завершения выполнения утилиты
                double waitedPeriod = (DateTimeZone.Now - tool.LastCompleted.Value).TotalMinutes;
                int requiredPeriod = tool.Interval.IntervalMinutes();
                if (waitedPeriod >= requiredPeriod)
                {
                    tool.StartExecuting();
                }
            }
        }
    }
}