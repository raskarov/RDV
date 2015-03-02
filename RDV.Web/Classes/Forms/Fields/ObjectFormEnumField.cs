using System;
using System.Globalization;
using RDV.Domain.Enums;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// Наследник стандартного дропдауна для отображения перечислений
    /// </summary>
    /// <typeparam name="TEnum">Перечисление, которое необходимо отобразить</typeparam>
    public class ObjectFormEnumField<TEnum>: ObjectFormDropDownField 
    {
        /// <summary>
        /// Инициализирует поле, заполняя его данными из перечисления
        /// </summary>
        public ObjectFormEnumField(): base()
        {
            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            {
                var key = Convert.ToInt16(value).ToString(CultureInfo.InvariantCulture);
                var displayValue = value.GetEnumMemberName();
                Items[key] = displayValue;
            }
        }

        /// <summary>
        /// Возвращает представление значения как строки
        /// </summary>
        /// <returns>Значение как строка</returns>
        public override string ValueAsString()
        {
            object valueAsShort = Convert.ToInt16(Value);
            TEnum valueAsEnum = (TEnum) valueAsShort;
            return valueAsEnum.GetEnumMemberName();
        }
    }
}