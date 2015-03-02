using System.Collections.Generic;
using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий объектов недвижимости
    /// </summary>
    public interface IEstateObjectsRepository: IBaseRepository<EstateObject>
    {
        /// <summary>
        /// Возвращает список активных объектов т.е. объектов, не находящихся в архиве
        /// </summary>
        /// <returns>Коллекция объектов</returns>
        IEnumerable<EstateObject> GetActiveObjects();

        /// <summary>
        /// Возвращает временный - зарегистрированный в системе объект
        /// </summary>
        /// <returns>Временный объект</returns>
        EstateObject GetTempObject();
    }
}