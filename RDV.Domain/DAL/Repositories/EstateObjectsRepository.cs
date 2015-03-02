using System.Collections.Generic;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория объектов
    /// </summary>
    public class EstateObjectsRepository: BaseRepository<EstateObject>, IEstateObjectsRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext">Контекст доступа к данным</param>
        public EstateObjectsRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override EstateObject Load(long id)
        {
            return Find(o => o.Id == id);
        }

        /// <summary>
        /// Возвращает список активных объектов т.е. объектов, не находящихся в архиве
        /// </summary>
        /// <returns>Коллекция объектов</returns>
        public IEnumerable<EstateObject> GetActiveObjects()
        {
            return FindAll();
        }

        /// <summary>
        /// Возвращает временный - зарегистрированный в системе объект
        /// </summary>
        /// <returns>Временный объект</returns>
        public EstateObject GetTempObject()
        {
            return new EstateObject()
                       {
                           Address = new Address(),
                           ObjectMainProperties = new ObjectMainProperty(),
                           ObjectChangementProperties = new ObjectChangementProperty(),
                           ObjectAdditionalProperties = new ObjectAdditionalProperty(),
                           ObjectCommunications = new ObjectCommunication(),
                           ObjectRatingProperties = new ObjectRatingProperty(),
                           User = new User()
                           {
                               CompanyId = -1
                           }
                       };
        }

        /// <summary>
        /// Удаляет сущность из репозитория
        /// </summary>
        /// <param name="entity">Сущность для удаления</param>
        public override void Delete(EstateObject entity)
        {
            // Удаляем все ссылки на объект
            foreach (var searchRequestsObject in entity.SearchRequestObjects.ToList())
            {
                searchRequestsObject.SearchRequest.SearchRequestObjects.Remove(searchRequestsObject);
                entity.SearchRequestObjects.Remove(searchRequestsObject);
            }
            base.Delete(entity);
        }
    }
}