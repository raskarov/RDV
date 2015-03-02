using System;
using System.Globalization;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Web.Classes.Caching;
using RDV.Web.Classes.Extensions;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// Наследник стандартного дропдауна отображающим выбор значений из справочников
    /// </summary>
    public class ObjectFormDictionaryField: ObjectFormDropDownField
    {
        /// <summary>
        /// Инициаилизирует поле для выборки из указанного справочника
        /// </summary>
        /// <param name="dictionary">Справочник</param>
        public ObjectFormDictionaryField(Dictionary dictionary): base()
        {
            Items = dictionary.DictionaryValues.ToDictionary(dv => dv.Id.ToString(CultureInfo.InvariantCulture), dv => dv.Value);
            InsertBlankItem = true;
        }

        /// <summary>
        /// Возвращает представление значения как строки
        /// </summary>
        /// <returns>Значение как строка</returns>
        public override string ValueAsString()
        {
            long? id;
            try
            {
                id = Value.ConvertToNullableLong();
            }
            catch (Exception)
            {
                return Value;
            }
            return IdObjectsCache.GetDictionaryValue(id);
        }
    }
}