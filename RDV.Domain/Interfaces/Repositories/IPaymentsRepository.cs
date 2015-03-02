using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстарктный репозиторий платежей
    /// </summary>
    public interface IPaymentsRepository: IBaseRepository<Payment>
    {
         
    }
}