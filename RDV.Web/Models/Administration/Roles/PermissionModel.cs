using System.Linq;
using RDV.Domain.Entities;

namespace RDV.Web.Models.Administration.Roles
{
    /// <summary>
    /// ������ �����, ������� ����� ���� � ����
    /// </summary>
    public class PermissionModel
    {
        /// <summary>
        /// ������������� ���������� � ����
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// ������������� �����
        /// </summary>
        public long PermissionId { get; set; }

        /// <summary>
        /// ��������� ��� �����
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// ������������ ��� �����
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// ������ ����������
        /// </summary>
        public string PermissionGroup { get; set; }

        /// <summary>
        /// �������� ��������
        /// </summary>
        public bool OperationContext { get; set; }

        /// <summary>
        /// ����� ������ ����
        /// </summary>
        public string Options { get; set; }

        /// <summary>
        /// ����������� �����������
        /// </summary>
        public PermissionModel()
        {
        }

        /// <summary>
        /// ����������� �� ������ �������� ����� �� ����
        /// </summary>
        /// <param name="model">�������</param>
        public PermissionModel(RolePermission model)
        {
            Id = model.Id;
            PermissionId = model.PermissionId;
            SystemName = model.Permission.SystemName;
            DisplayName = model.Permission.DisplayName;
            PermissionGroup = model.Permission.PermissionGroup;
            OperationContext = model.Permission.OperationContext;
            Options = string.Join(",",
                                  model.RolePermissionOptions.Select(
                                      po => string.Format("{0}_{01}", po.ObjectType, po.ObjectOperation)));
        }

        /// <summary>
        /// ����������� �� ������ ����
        /// </summary>
        /// <param name="model">�����</param>
        public PermissionModel(Permission model)
        {
            Id = model.Id;
            SystemName = model.SystemName;
            DisplayName = model.DisplayName;
            PermissionGroup = model.PermissionGroup;
            OperationContext = model.OperationContext;
        }
    }
}