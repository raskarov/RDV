using System;
using RDV.Domain.Entities;

namespace RDV.Web.Models.Administration.Dictionaries
{
    /// <summary>
    /// ������ �������� �����������
    /// </summary>
    public class DictionaryValueModel
    {
        /// <summary>
        /// ������������� ��������
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// ������������� �����������
        /// </summary>
        public long DictionaryId { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// �������� �������
        /// </summary>
        public string ShortValue { get; set; }

        /// <summary>
        /// ���� ��������
        /// </summary>
        public DateTime? DateCreated { get; set; }

        /// <summary>
        /// ���� ��������������
        /// </summary>
        public DateTime? DateModified { get; set; }

        /// <summary>
        /// ��� �������
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// ��� ��������������
        /// </summary>
        public long ModifiedBy { get; set; }

        /// <summary>
        /// ����������� �����������
        /// </summary>
        public DictionaryValueModel()
        {
        }

        /// <summary>
        /// ����������� �� ������ �������� ������
        /// </summary>
        /// <param name="dictionaryValue">������ �������� �����������</param>
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