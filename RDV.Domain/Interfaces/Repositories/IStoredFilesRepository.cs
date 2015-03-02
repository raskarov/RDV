using System.Web;
using RDV.Domain.Entities;

namespace RDV.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Абстрактный репозиторий хранимых файлов
    /// </summary>
    public interface IStoredFilesRepository: IBaseRepository<StoredFile>
    {
        /// <summary>
        /// Возвращает полный путь к указанному файлу, относительно корневой директории сервера
        /// </summary>
        /// <param name="storedFileUrl"></param>
        /// <returns></returns>
        string ResolveFileUrl(string storedFileUrl);

        /// <remarks>Данный метод вызывает SubmitChanges так что нужно использовать его с осторожностью</remarks>
        /// <summary>
        /// Сохраняет указанный файд в сервером хранилище файлов
        /// </summary>
        /// <param name="postedFile">Отправленный на сервер файл</param>
        /// <param name="folder">Опционально - подпапка в которой сохранить</param>
        /// <param name="addWatermark">Добавить ли водяной знак на фотографию</param>
        /// <param name="optimizeFormat">оптимизировать ли формат при сохранении</param>
        /// <returns>Объект сохраненного файла</returns>
        StoredFile SavePostedFile(HttpPostedFileBase postedFile, string folder = "", bool addWatermark = false, bool optimizeFormat = false);

        /// <summary>
        /// получает объект и данные по файлу
        /// </summary>
        /// <param name="fileUrl">URI файла в системе хранения контента</param>
        /// <returns>Объект файла на сервере</returns>
        StoredFile GetFile(string fileUrl);

        /// <summary>
        /// Удаляет файл с указанным URI из системы хранения файлов
        /// </summary>
        /// <param name="fileUrl"></param>
        void DeleteFile(string fileUrl);

        void CopyBlobStorage();
    }
}