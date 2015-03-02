namespace RDV.Domain.Entities
{
    /// <summary>
    /// Разрешение; которое позволяет роли выполнять различные действия
    /// </summary>
    public partial class Permission
    {
        /// <summary>
        /// Просмотр объектов
        /// </summary>
        public const long ViewObjects = 1;

        /// <summary>
        /// Добавление объектов
        /// </summary>
        public const long AddObjects = 2;

        /// <summary>
        /// Редактирование своих объектов
        /// </summary>
        public const long EditOwnObjects = 3;

        /// <summary>
        /// Редактирование объектов компании
        /// </summary>
        public const long EditCompanyObjects = 4;

        /// <summary>
        /// Редактирование всех объектов
        /// </summary>
        public const long EditAllObjects = 5;

        /// <summary>
        /// Удаление своих объектов
        /// </summary>
        public const long DeleteOwnObjects = 6;

        /// <summary>
        /// Удаление объектов компании
        /// </summary>
        public const long DeleteCompanyObjects = 7;

        /// <summary>
        /// Удаление всех объектов
        /// </summary>
        public const long DeleteAllObjects = 8;

        /// <summary>
        /// Изменение статуса своих объектов
        /// </summary>
        public const long ChangeOwnObjectsStatus = 9;

        /// <summary>
        /// Изменение статуса объектов
        /// </summary>
        public const long ChangeCompanyObjectsStatus = 10;

        /// <summary>
        /// Изменение статуса всех объектов
        /// </summary>
        public const long ChangeAllObjectsStatus = 11;

        /// <summary>
        /// Редактирование информации о компании
        /// </summary>
        public const long EditOwnCompanyInfo = 12;

        /// <summary>
        /// Просмотр клиентов компании
        /// </summary>
        public const long ViewCompanyClients = 13;

        /// <summary>
        /// Редактирование списка агентов компании
        /// </summary>
        public const long EditCompanyAgents = 14;

        /// <summary>
        /// Изменение агента для объектов
        /// </summary>
        public const long ChangeObjectAgent = 15;

        /// <summary>
        /// Просмотр дашбоарда
        /// </summary>
        public const long ViewDashboard = 16;

        /// <summary>
        /// Управление новостями
        /// </summary>
        public const long ManageContent = 17;

        /// <summary>
        /// Управление компаниями
        /// </summary>
        public const long ManageCompanies = 18;

        /// <summary>
        /// Управление пользователями
        /// </summary>
        public const long ManageUsers = 19;

        /// <summary>
        /// Управление ролями
        /// </summary>
        public const long ManageRoles = 20;

        /// <summary>
        /// Управление справочниками
        /// </summary>
        public const long ManageDictionaries = 21;

        /// <summary>
        /// Управление списком системных событий
        /// </summary>
        public const long ManageSystemEvents = 22;

        /// <summary>
        /// Управление статистикой
        /// </summary>
        public const long ManageStatistics = 23;

        /// <summary>
        /// Доступ к системным инструментам
        /// </summary>
        public const long ManageTools = 24;

        /// <summary>
        /// Управление настройками
        /// </summary>
        public const long ManageSettings = 25;

        /// <summary>
        /// Просмотр списков объектов в статусе аванс
        /// </summary>
        public const long ViewAdvanceObjectsList = 26;

        /// <summary>
        /// Просмотр списка объектов в статусе сделка
        /// </summary>
        public const long ViewDealObjectsList = 27;

        /// <summary>
        /// Просмотр списка объектов, в статусе снято с продажи
        /// </summary>
        public const long ViewWithdrawObjectsList = 28;

        /// <summary>
        /// Просмотр списка объектов, временно снятых с продажи
        /// </summary>
        public const long ViewTemporaryWithdrawObjectsList = 29;

        /// <summary>
        /// Просмотр списка объектов в архиве
        /// </summary>
        public const long ViewArchiveObjectsList = 30;

        /// <summary>
        /// Просмотр списка объектов в архиве
        /// </summary>
        public const long EditUserAppointment = 31;

        /// <summary>
        /// Доступ к архиву партнерства
        /// </summary>
        public const long PartnershipArchive = 33;

        /// <summary>
        /// Редактирование данных о сертификации пользователя
        /// </summary>
        public const long EditUserCertification = 34;

        /// <summary>
        /// Управление услугами
        /// </summary>
        public const long ManageServices = 35; 

        /// <summary>
        /// Просмотр информации о подписках
        /// </summary>
        public const long MyServices = 36; 

        /// <summary>
        /// Доступ к кошельку компании
        /// </summary>
        public const long AccessCompanyPayments = 37;

        /// <summary>
        /// Управление спонсорством
        /// </summary>
        public const long ControlCompanySponsoring = 38;

        /// <summary>
        /// Управление платежами системы
        /// </summary>
        public const long ManagePayments = 39;
    }
}