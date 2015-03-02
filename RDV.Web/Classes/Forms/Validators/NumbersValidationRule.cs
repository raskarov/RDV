namespace RDV.Web.Classes.Forms.Validators
{
    /// <summary>
    /// Валидатор для поля, принимающий только цифры
    /// </summary>
    public class NumbersValidationRule: ValidationRule
    {
        /// <summary>
        /// Валидатор поля, принимающий только цифры
        /// </summary>
        public NumbersValidationRule(bool allowFloat) : base(allowFloat ? "floatNumbers" :"digits", "true")
        {

        }
    }
}