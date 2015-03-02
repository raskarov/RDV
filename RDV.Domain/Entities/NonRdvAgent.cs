using System;

namespace RDV.Domain.Entities
{
    /// <summary>
    /// Агент - не член РДВ
    /// </summary>
    public partial class NonRdvAgent
    {
        public override string ToString()
        {
            return String.Format("{0} {1} {2}", LastName, FirstName, SurName);
        }
    }
}