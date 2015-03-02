using System.Collections.Generic;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Repositories;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// СУБД реализация репозитория пользователей
    /// </summary>
    public class UsersRepository: BaseRepository<User>, IUsersRepository
    {
        /// <summary>
        /// СУБД реализация репозитория пользователей
        /// </summary>
        /// <param name="dataContext"></param>
        public UsersRepository(RDVDataContext dataContext) : base(dataContext)
        {
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override User Load(long id)
        {
            return Find(u => u.Id == id);
        }

        /// <summary>
        /// Возвращает пользователя с указанной комбинацией логина и пароля
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="passwordHash">Хеш пароля</param>
        /// <returns>Объект пользователя</returns>
        public User GetUserByLoginAndPasswordHash(string login, string passwordHash)
        {
            return Find(u => u.Login.ToLower() == login.ToLower() && u.PasswordHash == passwordHash);
        }

        /// <summary>
        /// Проверяет, существует ли в системе пользователь с указанным логином
        /// </summary>
        /// <param name="login">Логин пользователь</param>
        /// <returns>true если существует</returns>
        public bool ExistsUserWithLogin(string login)
        {
            return Find(s => s.Login.ToLower() == login.ToLower()) != null;
        }

        /// <summary>
        /// Добавляет указанную учебную программу к указанному пользователю
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="trainingProgram">Учебная программа</param>
        public void AddTrainingProgram(User user, TrainingProgram trainingProgram)
        {
            trainingProgram.UserId = user.Id;
            DataContext.TrainingPrograms.InsertOnSubmit(trainingProgram);
        }

        /// <summary>
        /// Удаляет указанные учебые программы у пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="programs">Учебные программы</param>
        public void DeleteTrainingPrograms(User user, IEnumerable<TrainingProgram> programs)
        {
            foreach (var trainingProgram in programs)
            {
                user.TrainingPrograms.Remove(trainingProgram);
            }
            DataContext.TrainingPrograms.DeleteAllOnSubmit(programs);
        }

        /// <summary>
        /// Возвращает список активных незаблокированных пользователей отсортированных по алфавиту
        /// </summary>
        /// <returns></returns>
        public IEnumerable<User> GetActiveUsers()
        {
            return Search(u => !u.Blocked && u.Status == (short)UserStatuses.Active);
        }

        /// <summary>
        /// Возвращает временного пользователя, фактически не зарегистрированного в системе и обладающего всеми привилегиями гостя т.е. фактически без привелегий
        /// </summary>
        /// <returns></returns>
        public User GetGuestUser()
        {
            return new User()
                       {
                           CompanyId = -1
                       };
        }
    }
}