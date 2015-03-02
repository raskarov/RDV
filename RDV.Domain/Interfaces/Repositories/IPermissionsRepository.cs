using System.Collections.Generic;
using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий полномочий
    /// </summary>
    public interface IPermissionsRepository: IBaseRepository<Permission>
    {
        /// <summary>
        /// Возвращает список всех пермишеннов системы
        /// </summary>
        /// <returns></returns>
        IEnumerable<Permission> GetAllPermissions();
    }
}