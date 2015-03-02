using System.Collections.Generic;
using System.Linq;
using RDV.Domain.Entities;

namespace RDV.Web.Models.Administration.Dictionaries
{
    /// <summary>
    /// Модель редактирования справочника
    /// </summary>
    public class EditDictionaryModel
    {
        /// <summary>
        /// Идентификатор справочника
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Системное имя справочника
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Отображаемое имя справочника
        /// </summary>
        public string DisplayName { get; set; }

        public IList<DictionaryValueModel> DictionaryValues { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ({1})", DisplayName, SystemName);
        }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public EditDictionaryModel()
        {
            DictionaryValues = new List<DictionaryValueModel>();
        }

        /// <summary>
        /// Конструктор на основе доменной модели
        /// </summary>
        public EditDictionaryModel(Dictionary source)
        {
            Id = source.Id;
            SystemName = source.SystemName;
            DisplayName = source.DisplayName;
            DictionaryValues = source.DictionaryValues.Select(v => new DictionaryValueModel(v)).ToList();
        }
    }
}