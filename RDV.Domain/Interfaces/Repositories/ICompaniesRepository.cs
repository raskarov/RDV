using System.Collections.Generic;
using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий компаний
    /// </summary>
    public interface ICompaniesRepository: IBaseRepository<Company>
    {
        /// <summary>
        /// Возвращает список активных компаний
        /// </summary>
        /// <returns></returns>
        IEnumerable<Company> GetActiveCompanies();

        /// <summary>
        /// Проверяет, существует ли в системе компания с указанным идентификатором
        /// </summary>
        /// <param name="companyId">Идентификатор компании</param>
        /// <returns>true если существует</returns>
        bool ExistsCompanyWithId(long companyId);

        /// <summary>
        /// Возвращает всех активных сотрудников всех компаний-членов партнерства
        /// </summary>
        /// <returns></returns>
        IEnumerable<User> GetActiveAgents();
    }
}