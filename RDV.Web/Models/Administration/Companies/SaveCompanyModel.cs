namespace RDV.Web.Models.Administration.Companies
{
    /// <summary>
    /// модель сохранения изменений в компании
    /// </summary>
    public class SaveCompanyModel
    {

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Наименование компании
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание компании
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Идентификатор директора
        /// </summary>
        public long DirectorId { get; set; }

        /// <summary>
        /// Идентификатор города в котором распологается компания
        /// </summary>
        public long CityId { get; set; }

        /// <summary>
        /// Идентификатор типа компании
        /// </summary>
        public long CompanyType { get; set; }

        /// <summary>
        /// Емейл
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Контактный телефон
        /// </summary>
        public string Phone1 { get; set; }

        /// <summary>
        /// Дополнительный телефон
        /// </summary>
        public string Phone2 { get; set; }

        /// <summary>
        /// Дополнительный телефон
        /// </summary>
        public string Phone3 { get; set; }

        /// <summary>
        /// Адрес
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Информация о филиалах
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// Контактное лицо
        /// </summary>
        public string ContactPerson { get; set; }

        /// <summary>
        /// Компания неактивна
        /// </summary>
        public bool Inactive { get; set; }

        /// <summary>
        /// Компания - поставщик услуг
        /// </summary>
        public bool IsServiceProvider { get; set; }

        /// <summary>
        /// Компания - плательщик НДС
        /// </summary>
        public bool NDSPayer { get; set; }

        /// <summary>
        /// URL куда пойти после сохранения
        /// </summary>
        public string Redirect { get; set; }

        /// <summary>
        /// Краткое наименование компании
        /// </summary>
        public string ShortName { get; set; }
    }
}