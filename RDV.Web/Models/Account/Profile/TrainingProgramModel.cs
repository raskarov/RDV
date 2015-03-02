using System;

namespace RDV.Web.Models.Account.Profile
{
    /// <summary>
    /// Модель учебной программы
    /// </summary>
    public class TrainingProgramModel
    {
        /// <summary>
        /// Идентификатор для редактирования
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Дата проведения
        /// </summary>
        public DateTime TrainingDate { get; set; }

        /// <summary>
        /// Наименование программы
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// Организатор
        /// </summary>
        public string Organizer { get; set; }

        /// <summary>
        /// Место проведения
        /// </summary>
        public string TrainingPlace { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public long UserId { get; set; }
    }
}