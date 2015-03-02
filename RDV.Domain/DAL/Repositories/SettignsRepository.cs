using System;
using System.ComponentModel;
using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.Core;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория настроек
    /// </summary>
    public class SettignsRepository: BaseRepository<Setting>, ISettingsRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext">Контекст доступа к данным</param>
        public SettignsRepository(RDVDataContext dataContext) : base(dataContext)
        {

        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override Setting Load(long id)
        {
            return Find(s => s.Id == id);
        }

        /// <summary>
        /// Получает настройку в виде строки
        /// </summary>
        /// <param name="name">Имя-ключ настройки</param>
        /// <returns></returns>
        public string GetValue(string name)
        {
            var set = Find(s => s.Name.ToLower() == name.ToLower());
            if (set == null)
            {
                return null;
            }
            else
            {
                return set.Value;
            }
        }

        /// <summary>
        /// Получает настройку, конвертирую ее в указанный тип
        /// </summary>
        /// <typeparam name="T">Тип настройки, к которому конвертировать</typeparam>
        /// <param name="name">Имя-ключ настройки</param>
        /// <returns></returns>
        public T GetValue<T>(string name)
        {
            string val = GetValue(name);
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFrom(val);
        }

        /// <summary>
        /// Устанавливает настройку, конвертирую ее в строку
        /// </summary>
        /// <param name="name">Имя-ключ настройки</param>
        /// <param name="value">Значениен настройки</param>
        public void SetValue(string name, object value)
        {
            var set = Find(s => s.Name.ToLower() == name.ToLower());
            if (set == null)
            {
                set = new Setting()
                          {
                              Name = name,
                              DateCreated = DateTimeZone.Now,
                              Value = value.ToString()
                          };
                Add(set);
            }
            else
            {
                set.Value = value.ToString();
                set.DateModified = DateTimeZone.Now;
            }
        }

        /// <summary>
        /// Проверяет, была ли когда-либо установлена указанная настройка
        /// </summary>
        /// <param name="name">Наименование настройки</param>
        /// <returns>true если была, иначе false</returns>
        public bool HasSetting(string name)
        {
            return Find(s => s.Name.ToLower() == name.ToLower()) != null;
        }
    }
}