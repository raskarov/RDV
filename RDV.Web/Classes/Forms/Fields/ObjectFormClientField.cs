using System.Web.Mvc;
using RDV.Web.Classes.Caching;
using RDV.Web.Classes.Extensions;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// Поле выбора клиента на форме объекта
    /// </summary>
    public class ObjectFormClientField: ObjectFormTextField
    {
        /// <summary>
        /// Применяет основные аттрибуты к построителю тегов
        /// </summary>
        /// <param name="builder">Построитель элементов</param>
        protected override void ApplyDefaultAttributes(TagBuilder builder)
        {
            base.ApplyDefaultAttributes(builder);
            builder.AddCssClass("client-field");
        }

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
            // Строка основного редактора
            // Рендерим используя построитель элементов
            var builder = new TagBuilder("input");

            ApplyDefaultAttributes(builder);

            // Рендерим значение элемента
            builder.Attributes.Add("value", IdObjectsCache.GetClientName(Value.ConvertToNullableLong()));
            builder.Attributes.Remove("name");

            // Отдаем сформироанный элемент
            var textFieldString = builder.ToString(TagRenderMode.SelfClosing);
            
            // Строим хидден, который будет держать в себе значение
            var hiddenBuilder = new TagBuilder("input");
            hiddenBuilder.Attributes.Add("id","client-id-field");
            hiddenBuilder.Attributes.Add("type","hidden");
            hiddenBuilder.Attributes.Add("name",Name);
            hiddenBuilder.Attributes.Add("value",Value);
            var hiddenFieldString = hiddenBuilder.ToString(TagRenderMode.SelfClosing);

            // Отдаем результат
            return string.Concat(hiddenFieldString, "\n", textFieldString);
        }

        /// <summary>
        /// Возвращает представление значения как строки
        /// </summary>
        /// <returns>Значение как строка</returns>
        public override string ValueAsString()
        {
            return IdObjectsCache.GetClientName(Value.ConvertToNullableLong());
        }
    }
}