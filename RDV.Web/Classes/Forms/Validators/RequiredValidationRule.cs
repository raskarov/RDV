namespace RDV.Web.Classes.Forms.Validators
{
    /// <summary>
    /// Правило валидации, требующее чтобы поле было обязательно заполнено
    /// </summary>
    public class RequiredValidationRule: ValidationRule
    {
        /// <summary>
        /// Инициаилизирует правило валидации, требующее, чтобы поле было обязательно заполнено
        /// </summary>
        public RequiredValidationRule() : base("required", "true")
        {
        }
    }
}