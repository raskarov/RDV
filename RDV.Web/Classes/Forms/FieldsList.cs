using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RDV.Domain.Entities;
using RDV.Web.Classes.Forms.Fields;

namespace RDV.Web.Classes.Forms
{
    /// <summary>
    /// ������ ����� ����� �������� �� ������ ��������
    /// </summary>
    public class FieldsList: List<ObjectFormField>
    {
        /// <summary>
        /// ��������� ������ ����� �� ���������� ������� ��������� ��������, �������� � ������ ����
        /// </summary>
        /// <param name="estateObject">������ ������������, ������ �� �������� ���� �������</param>
        public void ReadValuesFromObject(EstateObject estateObject)
        {
            // ���������� ��� ���� � ������� ���� ������� �� ���������� ������ �� �������
            foreach (var field in this.Where(f=> f.GetValueFromObject != null))
            {
                var value = field.GetValueFromObject(estateObject);
                field.Value = value != null ? value.ToString() : null;
            }
        }

        /// <summary>
        /// ���������� ������ ����� � ��������� ������ ��������� ��������, �������� � ������ ����
        /// </summary>
        /// <param name="estateObject"></param>
        public void WriteValuesToObject(EstateObject estateObject)
        {
            // ���������� ��� ���� � ������� ���� ������� �� ���������� ������ �� �������
            foreach (var field in this.Where(f => f.SetValueToObject != null))
            {
                field.SetValueToObject(estateObject, field.Value);
            }
        }

        /// <summary>
        /// ��������� ������ ����� �� ��������� �������� ����, ��������� � �������
        /// </summary>
        /// <param name="collection">���������</param>
        public void ReadValuesFromFormCollection(FormCollection collection)
        {
            // ���������� ��� ���� � �������� ������� � ��� �� ��� ��� ��������
            foreach (var field in this)
            {
                field.ReadValueFromFormCollection(collection);
            }
        }

        /// <summary>
        /// ����������� ��������� ���� ����� � ������
        /// </summary>
        /// <param name="categoryName">��� ���������</param>
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