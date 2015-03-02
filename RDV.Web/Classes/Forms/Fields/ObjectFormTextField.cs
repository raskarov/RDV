using System.Web.Mvc;
using RDV.Domain.Enums;
using RDV.Web.Classes.Forms.Validators;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// Текстовое поле у формы объекта
    /// </summary>
    public class ObjectFormTextField: ObjectFormField
    {
        /// <summary>
        /// Максимальная длина строки
        /// </summary>
        public int? MaxLenght { get; set; }

        /// <remarks>
        /// Рендеринг окружающей разметки происходит во ViewEngine
        /// </remarks>
        /// <summary>
        /// Выполняет непосредственный рендеринг разметки HTML поля т.е. объект INPUTA напрямую.
        /// </summary>
        /// <param name="context"> </param>
        /// <returns></returns>
        public override string RenderFieldEditor(FieldRenderingContext context)
        {
            // Рендерим используя построитель элементов
            var builder = new TagBuilder("input");
            
            ApplyDefaultAttributes(builder);

            // Рендерим специфичные теги
            if (MaxLenght.HasValue)
            {
                builder.Attributes.Add("maxlenght", MaxLenght.Value.ToString());
            }

            // Проверяем, обязательно ли поле
            if (ActiveStatusValidator.IsFieldRequired(context.EstateObject,this))
            {
                builder.AddCssClass("field-required");
            }

            // Рендерим значение элемента
            builder.Attributes.Add("value",Value);

            // Отдаем сформироанный элемент
            return builder.ToString(TagRenderMode.SelfClosing);
        }
    }
}