namespace RDV.Web.Classes.Forms.Validators
{
    /// <summary>
    /// ������� ���������
    /// </summary>
    public class ValidationRule
    {
        /// <summary>
        /// ��� ������� ���������
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// �������� ������� ���������
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// �������������� ������� ��������� � ���������� �����������
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public ValidationRule(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}