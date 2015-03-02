using RDV.Domain.Enums;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// �����, ����������� ���������� � �������� ��������� �������, ����� ���� ���� ����������
    /// </summary>
    public class ObjectContextRequirement
    {
        /// <summary>
        /// ��������� ��� ������� ������������
        /// </summary>
        public EstateTypes RequiredEstateType { get; set; }

        /// <summary>
        /// ��������� �������� ��� �������
        /// </summary>
        public EstateOperations RequiredOperation { get; set; }

        /// <summary>
        /// ����������� �����������
        /// </summary>
        /// <param name="requiredEstateType">��������� ��� �������</param>
        /// <param name="requiredOperation">��������� ��������</param>
        public ObjectContextRequirement(EstateTypes requiredEstateType, EstateOperations requiredOperation)
        {
            RequiredEstateType = requiredEstateType;
            RequiredOperation = requiredOperation;
        }
    }
}