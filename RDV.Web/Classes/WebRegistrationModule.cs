using Ninject.Modules;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Web.Classes.Notification.UI;

namespace RDV.Web.Classes
{
    /// <summary>
    /// Модуль регистрации зависимостей Web приложения
    /// </summary>
    public class WebRegistrationModule: NinjectModule
    {
        /// <summary>
        /// Loads the module into the kernel.
        /// </summary>
        public override void Load()
        {
            Kernel.Bind<IUINotificationManager>().To<UINotificationManager>();
        }
    }
}