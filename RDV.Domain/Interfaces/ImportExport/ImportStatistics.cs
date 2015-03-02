namespace RDV.Domain.Interfaces.ImportExport
{
    /// <summary>
    /// ������ ���������� ���������� ��������������
    /// </summary>
    public class ImportStatistics
    {
        /// <summary>
        /// ���������� ��������������� ���������
        /// </summary>
        public long ImportedCount { get; set; }

        /// <summary>
        /// ���������� �� ��������������� ���������
        /// </summary>
        public long UnImportedCount { get; set; }

        /// <summary>
        /// ��������� � ���������� �������
        /// </summary>
        public string ResultMessage { get; set; }
    }
}