using System.Globalization;

namespace RDV.Web.Classes.Forms.Validators
{
    /// <summary>
    /// Правило валидации, требующее чтобы занчение поле было ограничено какой то длиной
    /// </summary>
    public class MaxLengthValidationRule: ValidationRule
    {
        /// <summary>
        /// Инициаилизирует валидатор максимальной длины с указанным ограничением
        /// </summary>
        /// <param name="length"></param>
        public MaxLengthValidationRule(int length) : base("maxlength", length.ToString(CultureInfo.InvariantCulture))
        {
        }
    }
}