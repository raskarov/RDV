namespace RDV.Web.Models.Administration.Roles
{
    /// <summary>
    /// ������ �������� ����� ����
    /// </summary>
    public class NewRoleModel
    {
        /// <summary>
        /// ������������� ������������
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// ��� ����� ����
        /// </summary>
        public string Name { get; set; }
    }
}