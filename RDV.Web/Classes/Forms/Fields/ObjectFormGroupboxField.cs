using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using RDV.Domain.Entities;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// Поле, представляющее собой элемент группы внутриполя
    /// </summary>
    public class ObjectFormGroupboxField: ObjectFormField, IEnumerable<ObjectFormField>
    {
        /// <summary>
        /// Делегат, вызываемый когда система пытается считать значение поля у объекта
        /// </summary>
        public override sealed Func<EstateObject, object> GetValueFromObject { get; set; }

        /// <summary>
        /// Делегат, вызываемый, когда система устанавливает поле объекту
        /// </summary>
        public override sealed Action<EstateObject, string> SetValueToObject { get; set; }

        /// <summary>
        /// Список полей, находяшихся внутри гроупбокса
        /// </summary>
        public IList<ObjectFormField> Fields { get; private set; }

        /// <summary>
        /// Стандартный конструктор, подменяющий свойства чтения и записи из и в объекты
        /// </summary>
        public ObjectFormGroupboxField()
        {
            Fields = new List<ObjectFormField>();
            GetValueFromObject = (estateObject) =>
                {
                    foreach (var field in Fields)
                    {
                        field.GetValueFromObject(estateObject);
                    }
                    return null;
                };
            SetValueToObject = (estateObject, value) =>
                {
                    foreach (var field in Fields)
                    {
                        field.SetValueToObject(estateObject, field.Value);
                    }
                };
        }

        /// <summary>
        /// Считывает значение данного поля из коллекции форм, присланных клиентом
        /// </summary>
        /// <param name="collection">Коллекция форм</param>
        public override void ReadValueFromFormCollection(FormCollection collection)
        {
            // Считываем значения всех вложенных полей
            foreach(var field in this)
            {
                field.ReadValueFromFormCollection(collection);
            }
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
            // Строим элемент fieldset
            var builder = new TagBuilder("fieldset");
            ApplyDefaultAttributes(builder);

            // Строим элемент legend
            var legendBuilder = new TagBuilder("legend");
            legendBuilder.SetInnerText(Caption);

            // Строим все содержиморе целиком
            var innerHtml = new StringBuilder();
            innerHtml.AppendLine(legendBuilder.ToString(TagRenderMode.Normal));
            foreach (var field in this)
            {
                innerHtml.AppendLine(field.RenderField(context));
            }
            
            // Собираем все целиком и отдаем
            builder.InnerHtml = innerHtml.ToString();
            return builder.ToString();
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
        public IEnumerator<ObjectFormField> GetEnumerator()
        {
            return Fields.GetEnumerator();
        }
    }
}