namespace RDV.Domain.Entities
{
    /// <summary>
    /// Информация о файле, сохраненном в серверной файловой системе
    /// </summary>
    public partial class StoredFile
    {
        /// <summary>
        /// Возвращает URI этого файла для поиска в системе хранения файлов
        /// </summary>
        /// <returns></returns>
        public string GetURI()
        {
            return string.Format("file://{0}", Id);
        }

        /// <summary>
        /// Возвращает размер файла в формате пригодном для человеческого понимания
        /// </summary>
        /// <returns></returns>
        public string GetSize()
        {
            var prefix = "байт";
            var multipler = 1;
            if (ContentSize > 1024*1024)
            {
                prefix = "мегабайт";
                multipler = 1024*1024;
            }
            if (ContentSize > 1024)
            {
                prefix = "килобайт";
                multipler = 1024;
            }
            return string.Format("{0} {1}", ContentSize/multipler, prefix);
        }
    }
}