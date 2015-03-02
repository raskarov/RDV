using System.Collections.Generic;

namespace RDV.Domain.Entities
{
    /// <summary>
    /// Справочник
    /// </summary>
    public partial class Dictionary
    {
        /// <summary>
        /// Возвращает количество записей в справочнике
        /// </summary>
        /// <returns></returns>
        public int GetValuesCount()
        {
            return DictionaryValues.Count;
        }

        /// <summary>
        /// Возвращает коллекцию значений данного справочника
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DictionaryValue> GetValues()
        {
            return this.DictionaryValues;
        }
    }
}