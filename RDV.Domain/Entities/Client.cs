using System.Text;

namespace RDV.Domain.Entities
{
    /// <summary>
    /// Клиент, работающий с компанией
    /// </summary>
    public partial class Client
    {
        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1}", LastName, FirstName);
            if (!string.IsNullOrEmpty(SurName))
            {
                sb.AppendFormat(" {0}", SurName);
            }
            return sb.ToString();
        }
    }
}