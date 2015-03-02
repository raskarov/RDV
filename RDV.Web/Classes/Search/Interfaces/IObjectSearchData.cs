using System.Collections.Generic;
using RDV.Web.Classes.Forms;

namespace RDV.Web.Classes.Search.Interfaces
{
    /// <summary>
    /// ��������� ��������������� ������ ������������ ��� ������ ��������
    /// </summary>
    public interface IObjectSearchData
    {
        /// <summary>
        /// ������������� �������
        /// </summary>
        long ObjID { get; set; }

        /// <summary>
        /// ��� �������, ���� -1 �� �����
        /// </summary>
        short ObjectType { get; set; }

        /// <summary>
        /// ��������, ���������� ��� ��������
        /// </summary>
        short Operation { get; set; }

        /// <summary>
        /// ���� ��
        /// </summary>
        double? PriceFrom { get; set; }

        /// <summary>
        /// ���� ��
        /// </summary>
        double? PriceTo { get; set; }

        /// <summary>
        /// ������� ��
        /// </summary>
        double? SquareFrom { get; set; }

        /// <summary>
        /// ������� ��
        /// </summary>
        double? SquareTo { get; set; }

        /// <summary>
        /// ������������� ������, � ������� ���������� ����� ��������
        /// </summary>
        long CityId { get; set; }

        /// <summary>
        /// ������ � ���������������� �������� �������
        /// </summary>
        string DistrictIds { get; set; }

        /// <summary>
        /// ������ � ���������������� ��������� ����� �������� � ������
        /// </summary>
        string AreaIds { get; set; }

        /// <summary>
        /// ������, � ���������������� ��������� ����
        /// </summary>
        string StreetIds { get; set; }

        /// <summary>
        /// ������ �������������� ��������� ��� ������
        /// </summary>
        FieldsList AdditionalCriterias { get; set; }

        /// <summary>
        /// ������� �����
        /// </summary>
        Dictionary<string, string> FieldsFilters { get; set; }

        /// <summary>
        /// ���������� ������ - 1
        /// </summary>
        bool? CountRoom1 { get; set; }

        /// <summary>
        /// ���������� ������ - 2
        /// </summary>
        bool? CountRoom2 { get; set; }

        /// <summary>
        /// ���������� ������ - 3
        /// </summary>
        bool? CountRoom3 { get; set; }

        /// <summary>
        /// ���������� ������ - 4 � �����
        /// </summary>
        bool? CountRoom4 { get; set; }

        /// <summary>
        /// �������������� ��������
        /// </summary>
        string CompanyIds { get; set; }

        /// <summary>
        /// �������������� �������
        /// </summary>
        string AgentIds { get; set; }
    }
}