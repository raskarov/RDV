using Ninject.Modules;
using RDV.Domain.Infrastructure.Mailing;
using RDV.Domain.Infrastructure.Misc;
using RDV.Domain.Interfaces.Cache;
using RDV.Domain.Interfaces.Infrastructure;

namespace RDV.Domain.Infrastructure
{
    /// <summary>
    /// Модуль регистрации инфраструктурных зависимостей
    /// </summary>
    public class InfrastructureRegistrationModule: NinjectModule
    {
        /// <summary>
        /// Loads the module into the kernel.
        /// </summary>
        public override void Load()
        {
            Kernel.Bind<IMailNotificationManager>().To<MailNotificationManager>().InSingletonScope();
            Kernel.Bind<IStringCache>().To<DictionaryStringCache>().InSingletonScope();
        }
    }
}