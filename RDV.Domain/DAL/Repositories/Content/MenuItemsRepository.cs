using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories.Content;

namespace RDV.Domain.DAL.Repositories.Content
{
    /// <summary>
    /// СУБД реализация репозитория элементов меню
    /// </summary>
    public class MenuItemsRepository: BaseRepository<MenuItem>, IMenuItemsRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext">Контекст доступа к данным</param>
        public MenuItemsRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override MenuItem Load(long id)
        {
            return Find(i => i.Id == id);
        }
    }
}