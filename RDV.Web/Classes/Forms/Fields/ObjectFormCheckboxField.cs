using System.Web.Mvc;
using RDV.Web.Classes.Extensions;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// Поле типа чекбокс на форме
    /// </summary>
    public class ObjectFormCheckboxField: ObjectFormField
    {
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
            var builder = new TagBuilder("input");

            builder.Attributes.Add("type", "checkbox");

            ApplyDefaultAttributes(builder);

            // Рендерим значение элемента
            builder.Attributes.Add("value", "true");

            // Проверяем, поставить ли элемент помеченным
            if (!string.IsNullOrEmpty(Value) && Value.ToLower() == "true")
            {
                builder.Attributes.Add("checked","checked");
            }

            // Строим скрытый хидден, для корректного сабмита формы
            var hiddenBuilder = new TagBuilder("input");
            hiddenBuilder.Attributes.Add("type","hidden");
            hiddenBuilder.Attributes.Add("name",Name);
            hiddenBuilder.Attributes.Add("value","false");
            builder.InnerHtml = hiddenBuilder.ToString(TagRenderMode.SelfClosing);

            // Отдаем сформироанный элемент
            return builder.ToString(TagRenderMode.SelfClosing);
        }

        /// <summary>
        /// Возвращает представление значения как строки
        /// </summary>
        /// <returns>Значение как строка</returns>
        public override string ValueAsString()
        {
            bool? val = Value.ConvertToNullableBool();
            if (val.HasValue)
            {
                return val.Value ? "да" : "нет";
            } else
            {
                return string.Empty;
            }
        }
    }
}