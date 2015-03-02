// 
// 
// Solution: RDV
// Project: RDV.Domain
// File: ICounterAgentsRepository.cs
// 
// Created by: ykors_000 at 31.01.2014 12:29
// 
// Property of SoftGears
// 
// ========

using System.Collections;
using System.Collections.Generic;
using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий контрагентов
    /// </summary>
    public interface ICounterAgentsRepository: IBaseRepository<ObjectHistoryItem>
    {
        /// <summary>
        /// Выполняет поиск по контрагентам различных объектов
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        IList<string> SearchCounteragents(string term);
    }
}