using System;
using RDV.Domain.Entities;

namespace RDV.Web.Models.Administration.Dictionaries
{
    /// <summary>
    /// Модель значения справочника
    /// </summary>
    public class DictionaryValueModel
    {
        /// <summary>
        /// Идентификатор значения
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Идентификатор справочника
        /// </summary>
        public long DictionaryId { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Значение краткое
        /// </summary>
        public string ShortValue { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime? DateCreated { get; set; }

        /// <summary>
        /// Дата редактирования
        /// </summary>
        public DateTime? DateModified { get; set; }

        /// <summary>
        /// Кем создано
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// Кем модифицировано
        /// </summary>
        public long ModifiedBy { get; set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public DictionaryValueModel()
        {
        }

        /// <summary>
        /// Конструктор на основе доменной модели
        /// </summary>
        /// <param name="dictionaryValue">Объект значения справочника</param>
        public DictionaryValueModel(DictionaryValue dictionaryValue)
        {
            Id = dictionaryValue.Id;
            DictionaryId = dictionaryValue.DictionaryId;
            Value = dictionaryValue.Value;
            ShortValue = dictionaryValue.ShortValue;
            DateCreated = dictionaryValue.DateCreated;
            DateModified = dictionaryValue.DateModified;
            CreatedBy = dictionaryValue.CreatedBy;
            ModifiedBy = dictionaryValue.ModifiedBy;
        }
    }
}