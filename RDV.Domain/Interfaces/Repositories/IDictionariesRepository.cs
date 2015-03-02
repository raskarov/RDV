using System.Collections.Generic;
using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий справочников
    /// </summary>
    public interface IDictionariesRepository: IBaseRepository<Dictionary>
    {
        /// <summary>
        /// Репозиторий значений справочников
        /// </summary>
        IDictionaryValuesRepository DictionaryValuesRepository { get; }

        /// <summary>
        /// Возвращает все справочники системы
        /// </summary>
        /// <returns></returns>
        IEnumerable<Dictionary> GetAllDictionaries();

        /// <summary>
        /// Получает справочник по его системному имени
        /// </summary>
        /// <param name="systemName">Системное имя справочника</param>
        /// <returns>Справочник или null если справочник не найден</returns>
        Dictionary GetDictionaryByName(string systemName);
    }
}