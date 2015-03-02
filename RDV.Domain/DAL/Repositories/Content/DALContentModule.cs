using Autofac;
using RDV.Domain.Interfaces.Repositories.Content;

namespace RDV.Domain.DAL.Repositories.Content
{
    /// <summary>
    /// Модуль регистрации контентных зависимостей в слое доступа к данным
    /// </summary>
    public class DALContentModule: Module
    {
        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        /// <param name="builder">The builder through which components can be
        ///             registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ArticlesRepository>().As<IArticlesRepository>();
            builder.RegisterType<MenuItemsRepository>().As<IMenuItemsRepository>();
            builder.RegisterType<StaticPagesRepository>().As<IStaticPagesRepository>();
            builder.RegisterType<BooksRepository>().As<IBooksRepository>();
            builder.RegisterType<PartnersRepository>().As<IPartnersRepository>();
        }
    }
}