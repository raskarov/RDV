using Autofac;
using Autofac.Integration.Mvc;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Web.Classes.Notification.UI;
using RDV.Web.Classes.Search;
using RDV.Web.Classes.Search.Interfaces;
using RDV.Web.Classes.Triggers;

namespace RDV.Web.Classes
{
    /// <summary>
    /// Модуль регистрации зависимостей веб интерфейса
    /// </summary>
    public class WebLayer: Module
    {
        /// <summary>
        /// Регистрирует зависимости в строителе
        /// </summary>
        /// <param name="builder">Строитель</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UINotificationManager>().As<IUINotificationManager>().SingleInstance();
            builder.RegisterType<ObjectSearchManager>().As<IObjectSearchManager>().InstancePerDependency();
            builder.RegisterType<ObjectsTriggerManager>().As<IObjectsTriggerManager>().InstancePerHttpRequest();
            builder.RegisterControllers(this.ThisAssembly);
        }
    }
}