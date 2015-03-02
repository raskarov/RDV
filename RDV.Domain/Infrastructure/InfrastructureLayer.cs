using Autofac;
using Autofac.Integration.Mvc;
using RDV.Domain.Infrastructure.Geo;
using RDV.Domain.Infrastructure.Mailing;
using RDV.Domain.Infrastructure.Misc;
using RDV.Domain.Infrastructure.Tools;
using RDV.Domain.Interfaces.Cache;
using RDV.Domain.Interfaces.Infrastructure;

namespace RDV.Domain.Infrastructure
{
    /// <summary>
    /// Модуль регистрации зависимостей инфраструктурного слоя
    /// </summary>
    public class InfrastructureLayer: Module
    {
        /// <summary>
        /// Регистрирует зависимости в строителе
        /// </summary>
        /// <param name="builder">Строитель</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<UniSenderMailNotificationManager>().As<IMailNotificationManager>().SingleInstance();
            builder.RegisterType<DictionaryStringCache>().As<IStringCache>().SingleInstance();
            builder.RegisterType<GeoManager>().As<IGeoManager>().InstancePerHttpRequest();
            builder.RegisterType<AuditManager>().As<IAuditManager>().InstancePerHttpRequest();
            builder.RegisterType<ToolsManager>().As<IToolsManager>().SingleInstance();
            builder.RegisterType<SMSNotificationManager>().As<ISMSNotificationManager>().SingleInstance();
        }
    }
}