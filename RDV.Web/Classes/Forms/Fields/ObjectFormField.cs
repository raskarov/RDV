using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Web.Classes.Forms.Validators;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// Абстрактное поле у формы объекта
    /// </summary>
    public abstract class ObjectFormField
    {
        /// <summary>
        /// Имя поля объекта, в которое происходит сериализация
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Отображаемый лейбл у объекта
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Идентификатор поля в DOM, если пуст - идентификатор генерируется автоматически
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Плейсхолдер для поля. Рендериться используя фичи HTML5
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        /// Кастомные CSS классы, применяемые к полю ввода
        /// </summary>
        public string CustomClasses { get; set; }

        /// <summary>
        /// Кастомные стили, применяемые к полю ввода
        /// </summary>
        public string CustomStyles { get; set; }

        /// <summary>
        /// Всплывающая подсказка по данному полю, которая появляется на форме, когда курсор становится на данное поле
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// Поле только для чтения
        /// </summary>
        public bool Readonly { get; set; }

        /// <summary>
        /// Строка, описывающая условия, когда данное поле обязательно к заполнению
        /// </summary>
        public string Required { get; set; }

        /// <summary>
        /// Требуемые операции и статусы для того, чтобы данное поле было отрендерено
        /// </summary>
        public IEnumerable<ObjectContextRequirement> RequiredContext { get; set; }

        /// <summary>
        /// Требуемые статусы, при которых данное поле отображается
        /// </summary>
        public IEnumerable<EstateStatuses> RequiredStatuses { get; set; }

        /// <summary>
        /// Значение поля в строковом формате
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Делегат, вызываемый когда система пытается считать значение поля у объекта
        /// </summary>
        public virtual Func<EstateObject, object> GetValueFromObject { get; set; }

        /// <summary>
        /// Делегат, вызываемый, когда система устанавливает поле объекту
        /// </summary>
        public virtual Action<EstateObject, string> SetValueToObject { get; set; }

        /// <summary>
        /// Кастомное условие для определения, требуется ли отрендерить это поле или нет
        /// </summary>
        public virtual Func<FieldRenderingContext, bool> CustomCondition { get; set; }

        /// <summary>
        /// Валидатор данного поля
        /// </summary>
        public FieldValidator Validator { get; set; }

        /// <summary>
        /// Позиция поля в форме. Используется для сортировки
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Значение по умолчанию
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Идентификаторы, которые требуются чтобы поле НЕ БЫЛО ОБЯЗАТЕЛЬНЫМ
        /// </summary>
        public string NotRequired { get; set; }

        /// <summary>
        /// Является ли поле только числовым
        /// </summary>
        public bool IsNumeric
        {
            get
            {
                if (this.Validator == null)
                {
                    return false;
                }
                var digitsRules = new[] { "floatNumbers", "digits", "min", "max" };
                return this.Validator.Rules.Any(r => digitsRules.Contains(r.Name));
            }
        }

        /// <summary>
        /// Категория реквизитов
        /// </summary>
        public string Category { get; set; }

		/// <summary>
		/// Сложный делегат, проверяющий необходимость реквезита для перевода в активный статус
		/// </summary>
		public Func<EstateObject,bool>  CustomRequired { get; set; }

        /// <remarks>
        /// Рендеринг окружающей разметки происходит во ViewEngine
        /// </remarks>
        /// <summary>
        /// Выполняет непосредственный рендеринг разметки HTML поля т.е. объект INPUTA напрямую.
        /// </summary>
        /// <param name="context"> </param>
        /// <returns></returns>
        public abstract string RenderFieldEditor(FieldRenderingContext context);

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        protected ObjectFormField()
        {
            Validator = new FieldValidator();
            RequiredContext = new List<ObjectContextRequirement>();
            NotRequired = "";
        }

        /// <summary>
        /// Применяет основные аттрибуты к построителю тегов
        /// </summary>
        /// <param name="builder">Построитель элементов</param>
        protected virtual void ApplyDefaultAttributes(TagBuilder builder)
        {
            // Рендерим основные свойства
            builder.Attributes.Add("name", Name);
            builder.Attributes.Add("id", string.IsNullOrEmpty(Id) ? string.Format("{0}-field", Name.ToLower()) : Id);
            if (!string.IsNullOrEmpty(Placeholder))
            {
                builder.Attributes.Add("placeholder", Placeholder);
            }
            builder.AddCssClass("round observable");
            if (!string.IsNullOrEmpty(CustomClasses))
            {
                builder.AddCssClass(CustomClasses);
            }
            if (!string.IsNullOrEmpty(CustomStyles))
            {
                builder.Attributes.Add("style", CustomStyles);
            }
            if (!String.IsNullOrEmpty(Tooltip))
            {
                builder.Attributes.Add("title",Tooltip);
            }
            if (Readonly)
            {
                builder.Attributes.Add("readonly","readonly");
                builder.AddCssClass("readonly");
            }

            // Отключаем автокомплит
            builder.Attributes.Add("autocomplete","off");
        }

        /// <summary>
        /// Выполяет рендеринг поля в HTML разметку
        /// </summary>
        /// <param name="context"> </param>
        /// <returns>HTML разметка редактора для этого поля</returns>
        public virtual string RenderField(FieldRenderingContext context)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("<div class=\"field-wrapper\" id=\"{0}-field-wrapper\">\n" +
                                 "<div class=\"editor-label {3} \">{1}</div>\n" +
                                 "<div class=\"editor-field\">\n{2}\n</div>\n" +
                                 "</div>\n", Name, Caption, RenderFieldEditor(context),ActiveStatusValidator.IsFieldRequired(context.EstateObject,this) ? "required-label" : "");
            return builder.ToString();
        }

        /// <summary>
        /// Проверяет нужно ли рендерить этот поле исходя из переданных статусов и состояний
        /// </summary>
        /// <returns>true если поле должно рендериться, иначе false</returns>
        public virtual bool CheckVisibility(FieldRenderingContext context)
        {
            var render = true;
            // Проверка рендеринга по типу объекта и операции
            if (RequiredContext != null && RequiredContext.Any())
            {
                var hasPermission =
                    RequiredContext.Any(
                        r =>
                        r.RequiredEstateType == context.EstateType && r.RequiredOperation == context.EstateOperation);
                if (!hasPermission)
                {
                    render = false;
                }
            }
            // Проверка рендеринга по статусу объекта
            if (RequiredStatuses != null && RequiredStatuses.Any())
            {
                var hasPermission = RequiredStatuses.Contains(context.EstateStatus);
                if (!hasPermission)
                {
                    render = false;
                }
            }
            // Проверка кастомного условия
            if (CustomCondition != null)
            {
                var hasPermission = CustomCondition(context);
                if (!hasPermission)
                {
                    render = false;
                }
            }
            return render;
        }

        /// <summary>
        /// Считывает значение данного поля из коллекции форм, присланных клиентом
        /// </summary>
        /// <param name="collection">Коллекция форм</param>
        public virtual void ReadValueFromFormCollection(FormCollection collection)
        {
            var submittedValue = collection[this.Name];
            this.Value = !(string.IsNullOrEmpty(submittedValue)) ? submittedValue : null;
        }

        /// <summary>
        /// Возвращает представление значения как строки
        /// </summary>
        /// <returns>Значение как строка</returns>
        public virtual string ValueAsString()
        {
            return Value ?? String.Empty;
        }

        /// <summary>
        /// Возвращает разметку для фильтра в поиске
        /// </summary>
        /// <param name="filterValue">Выбранный фильтр для поля</param>
        /// <returns>Разметка</returns>
        public string GetFilterMarkup(string filterValue = "")
        {
            var digitsRules = new[] { "floatNumbers", "digits", "min", "max" };
            var fieldInfo = this;
            if (fieldInfo is ObjectFormTextField)
            {
                if (fieldInfo.Validator != null && fieldInfo.Validator.Rules.Any(r => digitsRules.Contains(r.Name)))
                {
                    return String.Format("<select name=\"filter_{0}\">" +
                                     "<option value='='{1}>=</option>" +
                                     "<option value='!='{2}>!=</option>" +
                                     "<option value='>'{3}>></option>" +
                                     "<option value='<'{4}><</option>" +
                                     "<option value='range'{5}>[..]</option>" +
                                     "</select>", fieldInfo.Name, 
                                     (filterValue == "=" ? " selected='selected'":""),
                                     (filterValue == "!=" ? " selected='selected'":""),
                                     (filterValue == ">" ? " selected='selected'":""),
                                     (filterValue == "<" ? " selected='selected'":""),
                                     (filterValue == "range" ? " selected='selected'":"")
                                     );
                }
                else
                {
                    return String.Format("<select name=\"filter_{0}\">" +
                                     "<option value='=' {1}>=</option>" +
                                     "<option value='%'{2}>%</option>" +
                                     "</select>", fieldInfo.Name,
                                     (filterValue == "=" ? " selected='selected'":""),
                                     (filterValue == "%" ? " selected='selected'":"")
                                     );
                }
            }
            return String.Empty;
        }
    }
}