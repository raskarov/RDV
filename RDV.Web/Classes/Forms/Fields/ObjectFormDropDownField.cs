using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// Элемент формы, представляющий выпадающий список
    /// </summary>
    public class ObjectFormDropDownField: ObjectFormField
    {
        /// <summary>
        /// Элементы выпадающего списка
        /// </summary>
        public Dictionary<string, string> Items { get; set; }

        /// <summary>
        /// Вставлять ли пустой элемент выбора перед всеми остальными элементами
        /// </summary>
        public bool InsertBlankItem { get; set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public ObjectFormDropDownField()
        {
            Items = new Dictionary<string, string>();
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
            var builder = new TagBuilder("select");

            // Применяем основные свойства
            ApplyDefaultAttributes(builder);
            
            // Накладываем класс для сингселекта
            builder.AddCssClass("singleSelect");

            // Устанавливаем значение по умолчанию
            if (DefaultValue == null)
            {
                DefaultValue = "";
            }
            if (string.IsNullOrEmpty(Value) && !String.IsNullOrEmpty(DefaultValue.ToString()))
            {
                Value = DefaultValue.ToString();
            }

            // Рендерим элементы
            var items = new List<string>();
            foreach (var item in Items)
            {
                var itemBuilder = new TagBuilder("option");
                itemBuilder.Attributes.Add("value",item.Key);
                if (item.Key == Value)
                {
                    itemBuilder.Attributes.Add("selected","selected");
                }
                itemBuilder.SetInnerText(item.Value);
                items.Add(itemBuilder.ToString());
            }

            // Вставляем пустое первое поле для возможности пустого выбора
            if (InsertBlankItem)
            {
                var blankBuilder = new TagBuilder("option");
                blankBuilder.Attributes.Add("value", "");
                blankBuilder.SetInnerText(String.Empty);
                items.Insert(0, blankBuilder.ToString());
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
            if (Value != null && Items.ContainsKey(Value))
            {
                return Items[Value];
            } else
            {
                return string.Empty;
            }
        }
    }
}