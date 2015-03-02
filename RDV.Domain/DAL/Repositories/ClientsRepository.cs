using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория клиентов
    /// </summary>
    public class ClientsRepository: BaseRepository<Client>,IClientsRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext"></param>
        public ClientsRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override Client Load(long id)
        {
            return Find(c => c.Id == id);
        }
    }
}