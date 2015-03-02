using Ninject.Modules;
using Ninject.Web.Common;
using RDV.Domain.DAL.Repositories;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL
{
    /// <summary>
    /// Модуль регистрации зависимостей в слое доступа к данным
    /// </summary>
    public class DALRegistrationModule: NinjectModule
    {
        /// <summary>
        /// Loads the module into the kernel.
        /// </summary>
        public override void Load()
        {
            Kernel.Bind<RDVDataContext>().ToSelf().InRequestScope(); // BUG : Фактически датаконтекст создается каждый раз
            Kernel.Bind<IPermissionsRepository>().To<PermissionsRepository>().InRequestScope();
            Kernel.Bind<IRolesRepository>().To<RolesRepository>().InRequestScope();
            Kernel.Bind<ICompaniesRepository>().To<CompaniesRepository>().InRequestScope();
            Kernel.Bind<IUsersRepository>().To<UsersRepository>().InRequestScope();
            Kernel.Bind<IDictionariesRepository>().To<DictionariesRepository>().InRequestScope();
            Kernel.Bind<IDictionaryValuesRepository>().To<DictionaryValuesRepository>().InRequestScope();
            Kernel.Bind<IClientsRepository>().To<ClientsRepository>().InRequestScope();
            Kernel.Bind<IStoredFilesRepository>().To<StoredFilesRepository>().InRequestScope();
            Kernel.Bind<IMailNotificationMessagesRepository>().To<MailNotificationMessagesRepository>();
            Kernel.Bind<RDVDataContext>().ToSelf().WhenInjectedInto<IMailNotificationMessagesRepository>().
                InTransientScope();
        }
    }
}