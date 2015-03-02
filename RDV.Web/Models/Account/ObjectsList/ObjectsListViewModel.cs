using System.Collections.Generic;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Web.Classes;
using RDV.Web.Classes.Enums;

namespace RDV.Web.Models.Account.ObjectsList
{
    /// <summary>
    /// Модель страницы списка объектов
    /// </summary>
    public class ObjectsListViewModel
    {
        /// <summary>
        /// Список уже загруженных объектов, отсортированных по дате изменения
        /// </summary>
        public IList<EstateObjectModel> LoadedObjects { get; set; }

        /// <summary>
        /// Общее количество всех объектов в данном разделе
        /// </summary>
        public long TotalObjectsCount { get; set; }

        /// <summary>
        /// Раздел личного кабинета, где рендериться список объектов
        /// </summary>
        public ObjectsListLocation ListLocation { get; set; }

        /// <summary>
        /// Список агентов компании для выбора нового агента
        /// </summary>
        public IList<User> CompanyAgents { get; set; }

        /// <summary>
        /// Дополнительный фильтр по статусу
        /// </summary>
        public EstateStatuses? CustomStatusFilter { get; set; }
    }
}