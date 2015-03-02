namespace RDV.Domain.Enums
{
    /// <summary>
    /// Перечисление стандартных пермишеннов, которые есть в системе
    /// </summary>
    public enum StandartPermissions: long
    {
        /// <summary>
        /// Просмотр объектов
        /// </summary>
        [EnumDescription("Просмотр объектов")]
        ViewObjects = 1,

        /// <summary>
        /// Добавление объектов
        /// </summary>
        [EnumDescription("Добавление объектов")]
        AddObjects = 2,

        /// <summary>
        /// Редактирование своих объектов
        /// </summary>
        [EnumDescription("Редактирование своих объектов")]
        EditOwnObjects = 3,

        /// <summary>
        /// Редактирование объектов компании
        /// </summary>
        [EnumDescription("Редактирование объектов компании")]
        EditCompanyObjects = 4,

        /// <summary>
        /// Редактирование всех объектов
        /// </summary>
        [EnumDescription("Редактирование всех объектов")]
        EditAllObjects = 5,

        /// <summary>
        /// Удаление своих объектов
        /// </summary>
        [EnumDescription("Удаление своих объектов")]
        DeleteOwnObjects = 6,

        /// <summary>
        /// Удаление объектов компании
        /// </summary>
        [EnumDescription("Удаление объектов компании")]
        DeleteCompanyObjects = 7,

        /// <summary>
        /// Удаление всех объектов
        /// </summary>
        [EnumDescription("Удаление всех объектов")]
        DeleteAllObjects = 8,

        /// <summary>
        /// Изменение статуса своих объектов
        /// </summary>
        [EnumDescription("Изменение статуса своих объектов")]
        ChangeOwnObjectsStatus = 9,

        /// <summary>
        /// Изменение статуса объектов
        /// </summary>
        [EnumDescription("Изменение статуса объектов компании")]
        ChangeCompanyObjectsStatus = 10,

        /// <summary>
        /// Изменение статуса всех объектов
        /// </summary>
        [EnumDescription("Изменение статуса всех объектов")]
        ChangeAllObjectsStatus = 11,

        /// <summary>
        /// Редактирование информации о компании
        /// </summary>
        [EnumDescription("Редактирование информации о компании")]
        EditOwnCompanyInfo = 12,

        /// <summary>
        /// Просмотр клиентов компании
        /// </summary>
        [EnumDescription("Просмотр клиентов компании")]
        ViewCompanyClients = 13,

        /// <summary>
        /// Редактирование списка агентов компании
        /// </summary>
        [EnumDescription("Редактирование списка агентов компании")]
        EditCompanyAgents = 14,

        /// <summary>
        /// Изменение агента для объектов
        /// </summary>
        [EnumDescription("Изменение агента для объектов")]
        ChangeObjectAgent = 15,

        /// <summary>
        /// Просмотр дашбоарда
        /// </summary>
        [EnumDescription("Просмотр дашбоарда")]
        ViewDashboard = 16,

        /// <summary>
        /// Управление контентом системы
        /// </summary>
        [EnumDescription("Управление контентом системы")]
        ManageContent = 17,

        /// <summary>
        /// Управление компаниями
        /// </summary>
        [EnumDescription("Управление компаниями")]
        ManageCompanies = 18,

        /// <summary>
        /// Управление пользователями
        /// </summary>
        [EnumDescription("Управление пользователями")]
        ManageUsers = 19,

        /// <summary>
        /// Управление ролями
        /// </summary>
        [EnumDescription("Управление ролями")]
        ManageRoles = 20,

        /// <summary>
        /// Управление справочниками
        /// </summary>
        [EnumDescription("Управление справочниками")]
        ManageDictionaries = 21,

        /// <summary>
        /// Управление списком системных событий
        /// </summary>
        [EnumDescription("Управление списком системных событий")]
        ManageSystemEvents = 22,

        /// <summary>
        /// Управление статистикой
        /// </summary>
        [EnumDescription("Управление статистикой")]
        ManageStatistics = 23,

        /// <summary>
        /// Доступ к системным инструментам
        /// </summary>
        [EnumDescription("Доступ к системным инструментам")]
        ManageTools = 24,

        /// <summary>
        /// Управление настройками
        /// </summary>
        [EnumDescription("Управление настройками")]
        ManageSettings = 25
    }
}