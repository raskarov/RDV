using System.Collections.Generic;

namespace RDV.Web.Models.Administration.Roles
{
    /// <summary>
    /// ������ ���� � ������� ����� �������� ������������
    /// </summary>
    public class RoleModel
    {
        /// <summary>
        /// ������������� ����
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// ����������� �����������
        /// </summary>
        public RoleModel()
        {

        }

        /// <summary>
        /// ��� ����
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// �����, ������� ���� � ������ ����
        /// </summary>
        public IList<PermissionModel> Permissions { get; set; }
    }
}