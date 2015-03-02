using RDV.Domain.Interfaces.Repositories.Content;
using RDV.Domain.IoC;

namespace RDV.Web.Classes.Caching
{
    /// <summary>
    /// Класс для быстрого извлечения статических страниц
    /// </summary>
    public static class StaticPageExtractor
    {
        /// <summary>
        /// Быстро извлекает содержимое статической страницы по ее роуту. Если не найдено, то возвращается пустое содержимое
        /// </summary>
        /// <param name="route">Роут страницы</param>
        /// <returns></returns>
        public static string Extract(string route)
        {
            var rep = Locator.GetService<IStaticPagesRepository>();
            var page = rep.Find(p => p.Route == route);
            if (page == null)
            {
                return string.Empty;
            }
            page.Views = page.Views + 1;
            rep.SubmitChanges();
            return page.Content;
        }
    }
}