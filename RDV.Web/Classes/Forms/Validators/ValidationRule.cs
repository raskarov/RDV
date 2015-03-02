namespace RDV.Web.Classes.Forms.Validators
{
    /// <summary>
    /// Правило валидации
    /// </summary>
    public class ValidationRule
    {
        /// <summary>
        /// Имя правила валидации
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Значение правила валидации
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Инициализирует правило валидации с указанными параметрами
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public ValidationRule(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}