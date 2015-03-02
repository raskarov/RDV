using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Базовый интерфейс всех репозиториев системы
    /// </summary>
    /// <typeparam name="T">Тип сущности в репозитории</typeparam>
    public interface IBaseRepository<T>
    {
        /// <summary>
        /// Добавляет указанную сущность в репозиторий
        /// </summary>
        /// <param name="entity"></param>
        void Add(T entity);

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        T Load(long id);

        /// <remarks>
        /// Возвращает первую сущность, удовлетворяющую условию
        /// </remarks>
        /// <summary>
        /// Ищет единственную сущность в репозитории удовлетворяющую указанному условию
        /// </summary>
        /// <param name="predicate">Предикат с условием поиска</param>
        /// <returns>Сущность удовлетворяющая условию или null</returns>
        T Find(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Ищет в репозитории сущности удовлетворяющие указанному условию
        /// </summary>
        /// <param name="predicate">Предикат с условием поиска</param>
        /// <returns>Коллекция сущностей удовлетворяющих условию или пустой список</returns>
        IEnumerable<T> Search(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Возвращает все сущности, хранимые в указанном репозитории
        /// </summary>
        /// <returns>Коллекция сущностей</returns>
        IEnumerable<T> FindAll();

        /// <summary>
        /// Удаляет сущность из репозитория
        /// </summary>
        /// <param name="entity">Сущность для удаления</param>
        void Delete(T entity);

        /// <summary>
        /// Сохраняет изменения в указанной сущности в репозитории
        /// </summary>
        /// <param name="entity">Сущность для сохранения изменений</param>
        void Save(T entity);

        /// <summary>
        /// Отправляет все накопившиеся изменения в хранилище внутри репозитория
        /// </summary>
        void SubmitChanges();
    }
}