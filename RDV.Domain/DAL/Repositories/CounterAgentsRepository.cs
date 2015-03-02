// 
// 
// Solution: RDV
// Project: RDV.Domain
// File: CounterAgentsRepository.cs
// 
// Created by: ykors_000 at 31.01.2014 12:31
// 
// Property of SoftGears
// 
// ========

using System.Collections.Generic;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория контрагентов
    /// </summary>
    public class CounterAgentsRepository : BaseRepository<ObjectHistoryItem>, ICounterAgentsRepository
    {
        /// <summary>
        /// Инициализирует новый инстанс абстрактного репозитория для указанного типа
        /// </summary>
        /// <param name="dataContext"></param>
        public CounterAgentsRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }


        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override ObjectHistoryItem Load(long id)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Выполняет поиск по контрагентам различных объектов
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public IList<string> SearchCounteragents(string term)
        {
            return Search(s => s.CustomerName != null && s.CustomerName.ToLower().Contains(term)).Select(s => s.CustomerName).ToList();
        }
    }
}