using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория агентов не членов РДВ
    /// </summary>
    public class NonRdvAgentsRepository: BaseRepository<NonRdvAgent>, INonRdvAgentsRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext"></param>
        public NonRdvAgentsRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Получает агента по его идентификатору
        /// </summary>
        /// <param name="id">Идентификатор агента</param>
        /// <returns></returns>
        public override NonRdvAgent Load(long id)
        {
            return Find(a => a.Id == id);
        }
    }
}