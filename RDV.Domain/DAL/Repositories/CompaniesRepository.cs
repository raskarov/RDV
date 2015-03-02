using System.Collections.Generic;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория компаний
    /// </summary>
    public class CompaniesRepository: BaseRepository<Company>, ICompaniesRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext">Контекст доступа к данным</param>
        public CompaniesRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override Company Load(long id)
        {
            return Find(s => s.Id == id);
        }

        /// <summary>
        /// Возвращает список активных компаний
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Company> GetActiveCompanies()
        {
            return Search(c => !c.Inactive).OrderBy(c => c.Name);
        }

        /// <summary>
        /// Проверяет, существует ли в системе компания с указанным идентификатором
        /// </summary>
        /// <param name="companyId">Идентификатор компании</param>
        /// <returns>true если существует</returns>
        public bool ExistsCompanyWithId(long companyId)
        {
            return Load(companyId) != null;
        }

        /// <summary>
        /// Возвращает всех активных сотрудников всех компаний-членов партнерства
        /// </summary>
        /// <returns></returns>
        public IEnumerable<User> GetActiveAgents()
        {
            return from comp in GetActiveCompanies()
                   from user in comp.Users
                   where (user.Status == (short) UserStatuses.Active || user.Status == (short) UserStatuses.Blocked) && !user.Company.Inactive
                   select user;
        }
    }
}