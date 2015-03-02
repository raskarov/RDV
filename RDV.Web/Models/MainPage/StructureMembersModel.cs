using System.Collections.Generic;
using RDV.Domain.Entities;

namespace RDV.Web.Models.MainPage
{
    /// <summary>
    /// Модель для рендеринга списка пользователей в разделе структуры РДВ
    /// </summary>
    public class StructureMembersModel
    {
        /// <summary>
        /// Текст сверху списка пользователей
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Текст снизу списка пользователей
        /// </summary>
        public string Footer { get; set; }

        /// <summary>
        /// Сам список пользователей для рендеринга
        /// </summary>
        public IEnumerable<User> Members { get; set; }

        /// <summary>
        /// Заголовок страницы
        /// </summary>
        public string Title { get; set; }
    }
}