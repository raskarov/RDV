using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RDV.Domain.Entities;
using RDV.Web.Classes.Caching;
using RDV.Web.Classes.Extensions;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// Поле с возможностью множественного выбора значений из справочников
    /// </summary>
    public class ObjectFormMultiDictionaryField: ObjectFormDictionaryField
    {
        /// <summary>
        /// Инициализирует поле поддерживающее множественный выбор
        /// </summary>
        /// <param name="dictionary"></param>
        public ObjectFormMultiDictionaryField(Dictionary dictionary) : base(dictionary)
        {
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
            // Массив установленных значений
            var selectedValues = !String.IsNullOrEmpty(Value) ? Value.Split(',') : Enumerable.Empty<string>();

            // Рендерим компонент
            var builder = new TagBuilder("select");

            // Применяем основные свойства
            ApplyDefaultAttributes(builder);

            // Применяем класс мультивыборности
            builder.AddCssClass("multiselect");
            builder.Attributes.Add("multiple","multiple");

            // Рендерим элементы
            var items = new List<string>();
            foreach (var item in Items)
            {
                var itemBuilder = new TagBuilder("option");
                itemBuilder.Attributes.Add("value", item.Key);
                if (selectedValues.Contains(item.Key))
                {
                    itemBuilder.Attributes.Add("selected", "selected");
                }
                itemBuilder.SetInnerText(item.Value);
                items.Add(itemBuilder.ToString());
            }

            // Вставляем элементы во внутрь элемента селект
            builder.InnerHtml = string.Join("\n", items);

            // Отдаем результат
            return builder.ToString(TagRenderMode.Normal);
        }

        /// <summary>
        /// Возвращает представление значения как строки
        /// </summary>
        /// <returns>Значение как строка</returns>
        public override string ValueAsString()
        {
            if (Value == null)
            {
                return String.Empty;
            }
            var ids = Value.Split(',').Select(v => v.ConvertToNullableLong());
            var vals = ids.Select(IdObjectsCache.GetDictionaryValue);
            return String.Join(", ", vals);
        }
    }
}