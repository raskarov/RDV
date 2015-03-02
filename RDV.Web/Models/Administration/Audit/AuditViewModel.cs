using System;
using System.Collections.Generic;
using RDV.Domain.Entities;
using RDV.Domain.Enums;

namespace RDV.Web.Models.Administration.Audit
{
    /// <summary>
    /// Модель, используемая в окне аудита событий
    /// </summary>
    public class AuditViewModel
    {
        /// <summary>
        /// События аудита, отображаемые в окне
        /// </summary>
        public IList<AuditEvent> Events { get; set; }

        /// <summary>
        /// Начало периода за который отображаются события
        /// </summary>
        public DateTime? FilterPeriodStartDate { get; set; }

        /// <summary>
        /// Конец периода, за который отображаются события
        /// </summary>
        public DateTime? FilterPeriodEndDate { get; set; }

        /// <summary>
        /// Идентификаторы фильтруемых типов событий
        /// </summary>
        public IList<AuditEventTypes> FilterEvents { get; set; }

        /// <summary>
        /// Идентификаторы фильтруемых компаний, если null то отображаются всех компаний
        /// </summary>
        public IList<long> FilterCompaniesIds { get; set; }

        /// <summary>
        /// Идентификатор фильтруемых пользователей, если null то отображаются события всех пользователей
        /// </summary>
        public IList<long> FilterUsersIds { get; set; }

        /// <summary>
        /// Все пользователи системы
        /// </summary>
        public List<User> AllUsers { get; set; }

        /// <summary>
        /// Все компании системы
        /// </summary>
        public List<Company> AllCompanies { get; set; }
    }
}