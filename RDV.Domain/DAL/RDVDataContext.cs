namespace RDV.Domain.DAL
{
    /// <summary>
    /// LINQ 2 SQL Контекст доступа к данным
    /// </summary>
    public partial class RDVDataContext
    {
        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public RDVDataContext() : base(System.Configuration.ConfigurationManager.ConnectionStrings["Main"].ConnectionString)
        {

        }
    }
}