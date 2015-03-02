using System.Text.RegularExpressions;

namespace RDV.Web.Classes
{
    /// <summary>
    /// Статический класс со вспомогательными функциями для YouTube 
    /// </summary>
    public static class YouTubeHelper
    {
        /// <summary>
        /// Вытаскивает идентификатор YouTube видео из его 
        /// </summary>
        /// <param name="mediaUrl"></param>
        /// <returns></returns>
        public static string GetVideoId(string mediaUrl)
        {
            var regex = new Regex(@"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)");
            Match youtubeMatch = regex.Match(mediaUrl);
            string id = string.Empty;

            if (youtubeMatch.Success)
                id = youtubeMatch.Groups[1].Value;

            return id;
        }
    }
}