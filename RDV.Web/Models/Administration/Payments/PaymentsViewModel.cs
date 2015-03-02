using System.Collections.Generic;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Web.Models.Administration.Audit;

namespace RDV.Web.Models.Administration.Payments
{
    /// <summary>
    /// Вьюмодель платежей
    /// </summary>
    public class PaymentsViewModel: AuditViewModel
    {
        /// <summary>
        /// Список отфильтрованных платежей
        /// </summary>
        public new IList<Payment> Events { get; set; }

        /// <summary>
        /// Идентификаторы фильтруемых операций
        /// </summary>
        public new IList<PaymentDirection> FilterEvents { get; set; }
    }
}