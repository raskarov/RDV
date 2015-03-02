using System.Collections.Generic;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория справочников
    /// </summary>
    public class DictionariesRepository: BaseRepository<Dictionary>, IDictionariesRepository
    {
        /// <summary>
        /// Репозиторий значений справочников
        /// </summary>
        private readonly IDictionaryValuesRepository _dictionaryValuesRepository;

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext">Контекст доступа к данным</param>
        /// <param name="dictionaryValuesRepository">Репозиторий значений справочников</param>
        public DictionariesRepository(RDVDataContext dataContext, IDictionaryValuesRepository dictionaryValuesRepository) : base(dataContext)
        {
            _dictionaryValuesRepository = dictionaryValuesRepository;
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override Dictionary Load(long id)
        {
            return Find(d => d.Id == id);
        }

        /// <summary>
        /// Репозиторий значений справочников
        /// </summary>
        public IDictionaryValuesRepository DictionaryValuesRepository
        {
            get { return _dictionaryValuesRepository; }
        }

        /// <summary>
        /// Возвращает все справочники системы
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Dictionary> GetAllDictionaries()
        {
            return FindAll().OrderBy(d => d.SystemName);
        }

        /// <summary>
        /// Получает справочник по его системному имени
        /// </summary>
        /// <param name="systemName">Системное имя справочника</param>
        /// <returns>Справочник или null если справочник не найден</returns>
        public Dictionary GetDictionaryByName(string systemName)
        {
            var dict = Find(d => d.SystemName.ToLower() == systemName.ToLower());
            return dict;
        }
    }
}