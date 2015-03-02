using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий настроек
    /// </summary>
    public interface ISettingsRepository: IBaseRepository<Setting>
    {
        /// <summary>
        /// Получает настройку в виде строки
        /// </summary>
        /// <param name="name">Имя-ключ настройки</param>
        /// <returns></returns>
        string GetValue(string name);

        /// <summary>
        /// Получает настройку, конвертирую ее в указанный тип
        /// </summary>
        /// <typeparam name="T">Тип настройки, к которому конвертировать</typeparam>
        /// <param name="name">Имя-ключ настройки</param>
        /// <returns></returns>
        T GetValue<T>(string name);

        /// <summary>
        /// Устанавливает настройку, конвертирую ее в строку
        /// </summary>
        /// <param name="name">Имя-ключ настройки</param>
        /// <param name="value">Значениен настройки</param>
        void SetValue(string name, object value);

        /// <summary>
        /// Проверяет, была ли когда-либо установлена указанная настройка
        /// </summary>
        /// <param name="name">Наименование настройки</param>
        /// <returns>true если была, иначе false</returns>
        bool HasSetting(string name);
    }
}