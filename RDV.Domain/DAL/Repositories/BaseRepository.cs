using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий системы для работы с сущностью указанного типа
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseRepository<T>: IBaseRepository<T> where T : class
    {
        /// <summary>
        /// Контекст данных
        /// </summary>
        public RDVDataContext DataContext { get; private set; }

        /// <summary>
        /// Таблица в которой непосредственно хранятся сериализованные сущности
        /// </summary>
        protected Table<T> DataTable { get; set; }

        /// <summary>
        /// Инициализирует новый инстанс абстрактного репозитория для указанного типа
        /// </summary>
        /// <param name="dataContext"></param>
        protected BaseRepository(RDVDataContext dataContext)
        {
            DataContext = dataContext;
            DataTable = DataContext.GetTable<T>();
        }

        /// <summary>
        /// Добавляет указанную сущность в репозиторий
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Add(T entity)
        {
            DataTable.InsertOnSubmit(entity);
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public abstract T Load(long id);

        /// <remarks>
        /// Возвращает первую сущность, удовлетворяющую условию
        /// </remarks>
        /// <summary>
        /// Ищет единственную сущность в репозитории удовлетворяющую указанному условию
        /// </summary>
        /// <param name="predicate">Предикат с условием поиска</param>
        /// <returns>Сущность удовлетворяющая условию или null</returns>
        public virtual T Find(Expression<Func<T, bool>> predicate)
        {
            return DataTable.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Ищет в репозитории сущности удовлетворяющие указанному условию
        /// </summary>
        /// <param name="predicate">Предикат с условием поиска</param>
        /// <returns>Коллекция сущностей удовлетворяющих условию или пустой список</returns>
        public virtual IEnumerable<T> Search(Expression<Func<T, bool>> predicate)
        {
            return DataTable.Where(predicate);
        }

        /// <summary>
        /// Возвращает все сущности, хранимые в указанном репозитории
        /// </summary>
        /// <returns>Коллекция сущностей</returns>
        public virtual IEnumerable<T> FindAll()
        {
            return DataTable;
        }

        /// <summary>
        /// Удаляет сущность из репозитория
        /// </summary>
        /// <param name="entity">Сущность для удаления</param>
        public virtual void Delete(T entity)
        {
            DataTable.DeleteOnSubmit(entity);
        }

        /// <summary>
        /// Сохраняет изменения в указанной сущности в репозитории
        /// </summary>
        /// <param name="entity">Сущность для сохранения изменений</param>
        public virtual void Save(T entity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Отправляет все накопившиеся изменения в хранилище внутри репозитория
        /// </summary>
        public virtual void SubmitChanges()
        {
            DataContext.SubmitChanges();
        }
    }
}