using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий агентов не членов РДВ
    /// </summary>
    public interface INonRdvAgentsRepository: IBaseRepository<NonRdvAgent>
    {
         
    }
}