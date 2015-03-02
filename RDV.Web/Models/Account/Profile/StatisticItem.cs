namespace RDV.Web.Models.Account.Profile
{
    /// <summary>
    /// ������� ������������ ��� ����������� ���������� � �������
    /// </summary>
    public class StatisticItem
    {
        /// <summary>
        /// ������������ ��������
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// �������� ��������
        /// </summary>
        public string ItemValue { get; set; }

        /// <summary>
        /// ������ �������������� ���������
        /// </summary>
        public string ItemsGroup { get; set; }

        /// <summary>
        /// �������������� �������������� ������� � ���������� �����������
        /// </summary>
        /// <param name="itemName">������������ ����������</param>
        /// <param name="itemValue">�������� ����������</param>
        public StatisticItem(string itemName, string itemValue)
        {
            ItemName = itemName;
            ItemValue = itemValue;
        }
    }
}