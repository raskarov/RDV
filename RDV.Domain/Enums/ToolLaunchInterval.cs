using System;

namespace RDV.Domain.Enums
{
    /// <summary>
    /// Интервал для повторения выполнения операции
    /// </summary>
    public enum ToolLaunchInterval
    {
        /// <summary>
        /// Процедура запускается заново спустя минуту после первого завершения
        /// </summary>
        [EnumDescription("Каждую минуту")]
        EveryMinute = 1,

        /// <summary>
        /// Процедура запускается заново спустя 10 минут после завершения
        /// </summary>
        [EnumDescription("Каждые 10 минут")]
        Evert10Minutes = 2,

        /// <summary>
        /// Процедура запускается заново каждые пол часа
        /// </summary>
        [EnumDescription("Каждые пол часа")]
        EveryHalfHour = 3,

        /// <summary>
        /// Процедура запускается заново каждый час
        /// </summary>
        [EnumDescription("Каждый час")]
        EveryHour = 4,

        /// <summary>
        /// Процедура запускается заново каждые 12 часов
        /// </summary>
        [EnumDescription("Каждые 12 часов")]
        Every12Hour = 5,

        /// <summary>
        /// Процедура запускается заного каждый день
        /// </summary>
        [EnumDescription("Каждый день")]
        EveryDay = 6,

        /// <summary>
        /// Процедура выполняется только раз в неделю
        /// </summary>
        [EnumDescription("Каждую неделю")]
        EveryWeek = 7,

        /// <summary>
        /// Процедура выполняется каждый месяц
        /// </summary>
        [EnumDescription("Каждый месяц")]
        EveryMonth = 8
    }

    /// <summary>
    /// Класс, содержащий расширения для интервала запуска
    /// </summary>
    public static class LaunchIntervalExtension
    {
        /// <summary>
        /// Возвращает количество минут, которые должны пройти в течении интервала
        /// </summary>
        /// <param name="launchInterval">Интервал запуска</param>
        /// <returns></returns>
        public static int IntervalMinutes(this ToolLaunchInterval launchInterval)
        {
            switch (launchInterval)
            {
                case ToolLaunchInterval.EveryMinute:
                    return 1;
                    break;
                case ToolLaunchInterval.Evert10Minutes:
                    return 10;
                    break;
                case ToolLaunchInterval.EveryHalfHour:
                    return 30;
                    break;
                case ToolLaunchInterval.EveryHour:
                    return 60;
                    break;
                case ToolLaunchInterval.Every12Hour:
                    return 60*12;
                    break;
                case ToolLaunchInterval.EveryDay:
                    return 60*24;
                    break;
                case ToolLaunchInterval.EveryWeek:
                    return 60*24*7;
                    break;
                case ToolLaunchInterval.EveryMonth:
                    return 60*24*7*31;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("launchInterval");
            }
        }
    }
}