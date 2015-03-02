using System.Collections.Generic;
using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий пользователей
    /// </summary>
    public interface IUsersRepository: IBaseRepository<User>
    {
        /// <summary>
        /// Возвращает пользователя с указанной комбинацией логина и пароля
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="passwordHash">Хеш пароля</param>
        /// <returns>Объект пользователя</returns>
        User GetUserByLoginAndPasswordHash(string login, string passwordHash);

        /// <summary>
        /// Проверяет, существует ли в системе пользователь с указанным логином
        /// </summary>
        /// <param name="login">Логин пользователь</param>
        /// <returns>true если существует</returns>
        bool ExistsUserWithLogin(string login);

        /// <summary>
        /// Добавляет указанную учебную программу к указанному пользователю
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="trainingProgram">Учебная программа</param>
        void AddTrainingProgram(User user, TrainingProgram trainingProgram);

        /// <summary>
        /// Удаляет указанные учебые программы у пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="programs">Учебные программы</param>
        void DeleteTrainingPrograms(User user, IEnumerable<TrainingProgram> programs);

        /// <summary>
        /// Возвращает список активных незаблокированных пользователей отсортированных по алфавиту
        /// </summary>
        /// <returns></returns>
        IEnumerable<User> GetActiveUsers();

        /// <summary>
        /// Возвращает временного пользователя, фактически не зарегистрированного в системе и обладающего всеми привилегиями гостя т.е. фактически без привелегий
        /// </summary>
        /// <returns></returns>
        User GetGuestUser();
    }
}