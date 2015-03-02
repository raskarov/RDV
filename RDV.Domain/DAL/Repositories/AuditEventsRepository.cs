using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория событий аудита
    /// </summary>
    public class AuditEventsRepository: BaseRepository<AuditEvent>, IAuditEventsRepository
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext">Контекст доступа к данным</param>
        public AuditEventsRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Добавляет указанную сущность в репозиторий
        /// </summary>
        /// <param name="entity"></param>
        public override void Add(AuditEvent entity)
        {
            // Формируем команду для вставки
            var command =
                new SqlCommand(
                    "INSERT INTO dbo.AuditEvents VALUES (@userId,@eventDate,@eventType,@message,@ip,@browserInfo,@addInfo);",DataContext.Connection as SqlConnection);
            command.Parameters.AddWithValue("@userId", entity.UserId);
            command.Parameters.AddWithValue("@eventDate", entity.EventDate);
            command.Parameters.AddWithValue("@eventType", (short)entity.EventType);
            command.Parameters.AddWithValue("@message", entity.Message);
            command.Parameters.AddWithValue("@ip", entity.IP);
            command.Parameters.AddWithValue("@browserInfo", entity.BrowserInfo);
            command.Parameters.AddWithValue("@addInfo", entity.AdditionalInformation);

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
            }

            // TODO: решить проблему и переделать на LINQ

            // Выполняем команду
            command.ExecuteNonQuery();

            command.Connection.Close();
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override AuditEvent Load(long id)
        {
            return Find(i => i.Id == id);
        }

        /// <summary>
        /// Возвращает список событий произошедших в указанной компании, отсортированных по дате
        /// </summary>
        /// <param name="company">Компания</param>
        /// <returns>Коллекция событий произошедших в указанной компании</returns>
        public IEnumerable<AuditEvent> GetEventsForCompany(Company company)
        {
            return Search(e => e.User.CompanyId == company.Id).OrderByDescending(e => e.EventDate);
        }

        /// <summary>
        /// Возвращает коллекцию событий аудита для указанного пользователя, отсортированных по дате
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns>Коллекция событий, произошедших у указанного пользователя</returns>
        public IEnumerable<AuditEvent> GetEventsForUser(User user)
        {
            return Search(e => e.UserId == user.Id).OrderByDescending(e => e.EventDate);
        }
    }
}