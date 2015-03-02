namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// Компонент для отображения дат на форме объекта
    /// </summary>
    public class ObjectFormDateField: ObjectFormTextField
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public ObjectFormDateField()
        {
            CustomClasses = "datepicker";
        }
    }
}