using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Helpers;
using RDV.Domain.Entities;
using RDV.Domain.Interfaces.Cache;
using RDV.Domain.Interfaces.Repositories;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Blob;
using RDV.Domain.Core;

namespace RDV.Domain.DAL.Repositories
{
    /// <summary>
    /// Репозиторий для хранения файлов на сервере
    /// </summary>
    public class StoredFilesRepository: BaseRepository<StoredFile>,IStoredFilesRepository
    {
        /// <summary>
        /// Глобальный кеш строк для хранения преобразованных URI
        /// </summary>
        private IStringCache StringCache { get; set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="dataContext">Контекст данных</param>
        /// <param name="stringCache">Хранилище строкового кеша</param>
        public StoredFilesRepository(RDVDataContext dataContext, IStringCache stringCache) : base(dataContext)
        {
            StringCache = stringCache;
        }

        /// <summary>
        /// Загружает указанную сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns>Сущность с указанным идентификатором или Null</returns>
        public override StoredFile Load(long id)
        {
            return Find(s => s.Id == id);
        }

        /// <summary>
        /// Возвращает полный путь к указанному файлу, относительно корневой директории сервера
        /// </summary>
        /// <param name="storedFileUrl"></param>
        /// <returns></returns>
        public string ResolveFileUrl(string storedFileUrl)
        {
            if (String.IsNullOrEmpty(storedFileUrl))
            {
                return null;
            }
            
            // Пытаемся извлечь объект из кеша
            string alreadyParsed;
            bool hasInCache = false;
            if (StringCache.TryGetFromCache(storedFileUrl, out alreadyParsed))
            {
                hasInCache = true;
                return alreadyParsed;
            }
            else
            {
                hasInCache = false;
            }

            //var serverImageFolderPath = ConfigurationManager.AppSettings["FilesDBFolder"];
            var serverImageFolderPath = ConfigurationManager.AppSettings["BlobFilesDBFolder"];

            // Загружаем используя более ранний метод
            var file = GetFile(storedFileUrl);

            // вычисляем путь
            //var path = String.Format("/{0}/{1}", serverImageFolderPath, file.ServerFilename.Replace('\\', '/'));
            var path = String.Format("{0}/{1}", serverImageFolderPath, file.ServerFilename.ToLower().Replace('\\', '/'));

            // Кладем строку в кеш
            if (!hasInCache)
            {
                StringCache.Set(storedFileUrl, path);
            }

            // Отдаем картинку
            return path;
        }

        /// <remarks>Данный метод вызывает SubmitChanges так что нужно использовать его с осторожностью</remarks>
        /// <summary>
        /// Сохраняет указанный файд в сервером хранилище файлов
        /// </summary>
        /// <param name="postedFile">Отправленный на сервер файл</param>
        /// <param name="folder">Опционально - подпапка в которой сохранить</param>
        /// <param name="addWatermark">Добавить ли водяной знак на фотографию</param>
        /// <returns>Объект сохраненного файла</returns>
        public StoredFile SavePostedFile(HttpPostedFileBase postedFile, string folder = "", bool addWatermark = false, bool optimizeFormat = false)
        {
            var folderPath = folder;
            folder = folder.ToLower();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = null;

            // ФИзически сохраняем файл на сервере
            //var filesPath = ConfigurationManager.AppSettings["FilesStoragePath"];
            var ext = Path.GetExtension(postedFile.FileName);
            if (String.IsNullOrEmpty(ext))
            {
                if (postedFile.ContentType.Contains("png"))
                {
                    ext = ".jpg";
                }
                else {
                    ext = ".jpg";
                }
            }
            var serverFilename = string.Format("{0}{1}", Path.GetRandomFileName(),ext);
            serverFilename = Path.ChangeExtension(serverFilename, ".jpg");
            if (!String.IsNullOrEmpty(folder))
            {
                //serverFilename = String.Format("{0}\\{1}", folder, serverFilename);
                container = blobClient.GetContainerReference(folder);
                container.CreateIfNotExists();
            }

            //var fullPath = Path.Combine(filesPath, serverFilename);
            //var directoryPath = Path.GetDirectoryName(fullPath);
            //if (!Directory.Exists(directoryPath))
            //{
            //    Directory.CreateDirectory(directoryPath);
            //}

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(serverFilename);

            if (addWatermark)
            {
                var image = new WebImage(postedFile.InputStream);
                image = image.AddImageWatermark(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                     "Content\\images\\common\\watermark.png"));
                var bytes = image.GetBytes();

                blockBlob.UploadFromByteArray(bytes, 0, bytes.Length);

                //image.Save(fullPath,"jpeg");
            }
            else
            {
                if (postedFile.ContentType.Contains("image") && optimizeFormat)
                {
                    // Сжимаем загруженную фотку
                    var image = new WebImage(postedFile.InputStream);
                    image = image.Resize(1024, 1024, true, false);

                    var bytes = image.GetBytes();

                    blockBlob.UploadFromByteArray(bytes, 0, bytes.Length);

                    //image.Save(fullPath);
                }
                else
                {
                    var stream = postedFile.InputStream;

                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        var bytes = memoryStream.ToArray();

                        blockBlob.UploadFromByteArray(bytes, 0, bytes.Length);
                    }

                    //postedFile.SaveAs(fullPath);        
                }
                
            }
            
            //  Сохраняем информацию о файле в БД
            var storedFile = new StoredFile()
            {
                ContentSize = postedFile.InputStream.Length,
                DateCreated = DateTimeZone.Now,
                MimeType = postedFile.ContentType,
                OriginalFilename = postedFile.FileName,
                ServerFilename = String.Format("{0}\\{1}", folderPath, serverFilename)
            };

            Add(storedFile);

            // Добавляем в БД и строим URL
            SubmitChanges();

            // Отдаем результат
            return storedFile;
        }

        public void CopyBlobStorage()
        {
            CloudStorageAccount sourceStorageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudStorageAccount targetStorageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString2"));

            CloudBlobClient sourceBlobClient = sourceStorageAccount.CreateCloudBlobClient();
            CloudBlobClient targetBlobClient = targetStorageAccount.CreateCloudBlobClient();

            // Получаем все контейнеры источника
            var sourceContainers = sourceBlobClient.ListContainers();

            // Создаем такие же контейнеры в цели
            foreach (var item in sourceContainers)
            {
                var container = targetBlobClient.GetContainerReference(item.Name);
                container.CreateIfNotExists();

                // Получаем все BLOB контейнера источника
                var sourceContainerBlobs = item.ListBlobs();

                // Копируем BLOB
                foreach (var item2 in sourceContainerBlobs)
                {
                    CloudBlockBlob sourceBlockBlob = (CloudBlockBlob)item2;
                    CloudBlockBlob targetBlockBlob = container.GetBlockBlobReference(sourceBlockBlob.Name);

                    if (!targetBlockBlob.Exists())
                    {
                        targetBlockBlob.StartCopyFromBlob(sourceBlockBlob);
                    }
                }
            }
        }

        /// <summary>
        /// получает объект и данные по файлу
        /// </summary>
        /// <param name="fileUrl">URI файла в системе хранения контента</param>
        /// <returns>Объект файла на сервере</returns>
        public StoredFile GetFile(string fileUrl)
        {
            // Парсим строку и вырываем из нее идентификатор
            if (!fileUrl.ToLower().StartsWith("file"))
            {
                return null;
            }
            var parts = fileUrl.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            var id = Convert.ToInt64(parts[1]);

            // Отдаем найденный если нашли
            return Load(id);
        }

        /// <summary>
        /// Удаляет файл с указанным URI из системы хранения файлов
        /// </summary>
        /// <param name="fileUrl"></param>
        public void DeleteFile(string fileUrl)
        {
            // TODO: реализовать удаление
        }
    }
}