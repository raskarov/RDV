using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RDV.Domain.Entities;
using RDV.Web.Classes.Forms.Fields;

namespace RDV.Web.Classes.Forms
{
    /// <summary>
    /// Список полей формы объектов на данной странице
    /// </summary>
    public class FieldsList: List<ObjectFormField>
    {
        /// <summary>
        /// Считывает данные полей из указанного объекта используя делегаты, заданные в каждом поле
        /// </summary>
        /// <param name="estateObject">Объект недвижимости, данные из которого надо считать</param>
        public void ReadValuesFromObject(EstateObject estateObject)
        {
            // Перебираем все поля у которых есть делегат на считывание данных из объекта
            foreach (var field in this.Where(f=> f.GetValueFromObject != null))
            {
                var value = field.GetValueFromObject(estateObject);
                field.Value = value != null ? value.ToString() : null;
            }
        }

        /// <summary>
        /// Записывает данные полей в указанный объект используя делегаты, заданные в каждом поле
        /// </summary>
        /// <param name="estateObject"></param>
        public void WriteValuesToObject(EstateObject estateObject)
        {
            // Перебираем все поля у которых есть делегат на считывание данных из объекта
            foreach (var field in this.Where(f => f.SetValueToObject != null))
            {
                field.SetValueToObject(estateObject, field.Value);
            }
        }

        /// <summary>
        /// Считывает данные полей из коллекции значений форм, пришедших с клиента
        /// </summary>
        /// <param name="collection">Коллекция</param>
        public void ReadValuesFromFormCollection(FormCollection collection)
        {
            // Перебираем все поля и пытаемся считать в них то что нам прислали
            foreach (var field in this)
            {
                field.ReadValueFromFormCollection(collection);
            }
        }

        /// <summary>
        /// Присваивает категорию всем полям в списке
        /// </summary>
        /// <param name="categoryName">Имя категории</param>
        /// <returns></returns>
        public FieldsList AssignCategory(string categoryName)
        {
            foreach (var field in this)
            {
                field.Category = categoryName;
            }
            return this;
        }
    }
}