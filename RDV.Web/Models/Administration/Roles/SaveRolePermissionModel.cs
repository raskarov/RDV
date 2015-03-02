namespace RDV.Web.Models.Administration.Roles
{
    /// <summary>
    /// ������ ������������ ��� ���������� ���� ��� ������������ ����
    /// </summary>
    public class SaveRolePermissionModel
    {
        /// <summary>
        /// ������������� ����
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// ������������� ������������ ������ � ����
        /// </summary>
        public long RolePermissionId { get; set; }

        /// <summary>
        /// ������������� ������������
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// ������������� ����� � ����
        /// </summary>
        public long PermissionId { get; set; }
    }
}