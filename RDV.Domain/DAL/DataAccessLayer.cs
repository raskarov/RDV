using Autofac;
using Autofac.Integration.Mvc;
using RDV.Domain.DAL.Repositories;
using RDV.Domain.DAL.Repositories.Content;
using RDV.Domain.DAL.Repositories.Geo;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL
{
    /// <summary>
    /// Модуль регистрации зависимостей слоя доступа к данным
    /// </summary>
    public class DataAccessLayer: Module
    {
        /// <summary>
        /// Инициализирует зависимости слоя доступа к данным
        /// </summary>
        /// <param name="builder">строитель контейнера</param>
        protected override void Load(ContainerBuilder builder)
        {
            // Регистрация стандартных типов
            builder.RegisterType<RDVDataContext>().AsSelf().InstancePerHttpRequest();
            builder.RegisterType<PermissionsRepository>().As<IPermissionsRepository>();
            builder.RegisterType<RolesRepository>().As<IRolesRepository>();
            builder.RegisterType<CompaniesRepository>().As<ICompaniesRepository>();
            builder.RegisterType<UsersRepository>().As<IUsersRepository>();
            builder.RegisterType<DictionariesRepository>().As<IDictionariesRepository>();
            builder.RegisterType<DictionaryValuesRepository>().As<IDictionaryValuesRepository>();
            builder.RegisterType<ClientsRepository>().As<IClientsRepository>();
            builder.RegisterType<StoredFilesRepository>().As<IStoredFilesRepository>();
            builder.RegisterType<MailNotificationMessagesRepository>().As<IMailNotificationMessagesRepository>();
            builder.RegisterType<AuditEventsRepository>().As<IAuditEventsRepository>();
            builder.RegisterType<EstateObjectsRepository>().As<IEstateObjectsRepository>();
            builder.RegisterType<SMSNotificationMessagesRepository>().As<ISMSNotificationMessagesRepository>();
            builder.RegisterType<ObjectNotificationsRepository>().As<IObjectNotificationsRepository>();
            builder.RegisterType<SettignsRepository>().As<ISettingsRepository>();
            builder.RegisterType<PaymentsRepository>().As<IPaymentsRepository>();
            builder.RegisterType<SearchRequestsRepository>().As<ISearchRequestsRepository>();
            builder.RegisterType<SearchRequestObjectsRepository>().As<ISearchRequestObjectsRepository>();
            builder.RegisterType<ServiceTypesRepository>().As<IServiceTypesRepository>();
            builder.RegisterType<SystemStatsRepository>().As<ISystemStatsRepository>();
            builder.RegisterType<NonRdvAgentsRepository>().As<INonRdvAgentsRepository>();
            builder.RegisterType<CounterAgentsRepository>().As<ICounterAgentsRepository>();

            // Регистрация гео модуля
            builder.RegisterModule(new DALGeoModule());

            // Регистрация контентного модуля
            builder.RegisterModule(new DALContentModule());
        }
    }
}