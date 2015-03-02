using System.Linq;
using RDV.Domain.Enums;

namespace RDV.Domain.Entities
{
    /// <summary>
    /// Компания в которой состоят пользователи
    /// </summary>
    public partial class Company
    {
        /// <summary>
        /// Возвращает количество пользователей в компании
        /// </summary>
        /// <returns></returns>
        public int GetUsersCount()
        {
            return Users.Count;
        }

        /// <summary>
        /// Возвращает количество объектов в компании
        /// </summary>
        /// <returns></returns>
        public int GetObjectsCount()
        {
            return Users.Sum(user => user.GetObjectsCount());
        }

        /// <summary>
        /// Возвращает баланс кошелька компании
        /// </summary>
        /// <returns></returns>
        public decimal GetAccountBalance()
        {
            if (Payments.Count == 0)
            {
                return 0;
            }
            var income = Payments.Where(p => p.Payed && p.Direction == (short)PaymentDirection.Income).Sum(p => p.Amount);
            var outcome = Payments.Where(p => p.Direction == (short)PaymentDirection.Outcome).Sum(p => p.Amount);
            return income - outcome;
        }
    }
}