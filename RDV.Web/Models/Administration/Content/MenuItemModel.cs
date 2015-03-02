namespace RDV.Web.Models.Administration.Content
{
    /// <summary>
    /// Модель данных для элемента меню
    /// </summary>
    public class MenuItemModel
    {
        /// <summary>
        /// Идентификатор элемента
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Заголовок элемента
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Ссылка на элемент
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// Позиция элемента для сортировки
        /// </summary>
        public int Position { get; set; }
    }
}