using System.Collections.Generic;
using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория пермишеннов
    /// </summary>
    public class PermissionsRepository: BaseRepository<Permission>, IPermissionsRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext"></param>
        public PermissionsRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает пермишен по его идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override Permission Load(long id)
        {
            return Find(p => p.Id == id);
        }

        /// <summary>
        /// Возвращает список всех пермишеннов системы
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Permission> GetAllPermissions()
        {
            return FindAll();
        }
    }
}