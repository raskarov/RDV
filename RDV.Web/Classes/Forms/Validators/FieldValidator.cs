using System.Collections;
using System.Collections.Generic;

namespace RDV.Web.Classes.Forms.Validators
{
    /// <summary>
    /// Валидатор для поля, задающий то, как оно будет отрендерено в правила jquery.validate
    /// </summary>
    public class FieldValidator: IEnumerable<ValidationRule>
    {
        /// <summary>
        /// Правила валидации
        /// </summary>
        public IList<ValidationRule> Rules { get; private set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public FieldValidator()
        {
            Rules = new List<ValidationRule>();
        }

        /// <summary>
        /// Добавляет новое правило валидации. Используется для поддержки компилятора
        /// </summary>
        /// <param name="rule">Новое правило валидации</param>
        public void Add(ValidationRule rule)
        {
            Rules.Add(rule);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<ValidationRule> GetEnumerator()
        {
            return Rules.GetEnumerator();
        }
    }
}