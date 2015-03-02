using System.Globalization;

namespace RDV.Web.Classes.Forms.Validators
{
    /// <summary>
    /// Правило валидации, требующее чтобы значение поле было не менее определенной длины
    /// </summary>
    public class MinLengthValidationRule : ValidationRule
    {
        /// <summary>
        /// Инициаилизирует валидатор минимальной длины с указанным ограничением
        /// </summary>
        /// <param name="length">Длина значения</param>
        public MinLengthValidationRule(int length): base("minlength", length.ToString(CultureInfo.InvariantCulture))
        {

        }
    }
}