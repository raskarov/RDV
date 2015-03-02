using System.IO;

namespace RDV.Web.Classes.ElFinderConnector.PicturesEditor
{
    public class ImageWithMime
    {
        public string Mime { get; private set; }
        public Stream ImageStream { get; private set; }
        public ImageWithMime(string mime, Stream stream)
        {
            Mime = mime;
            ImageStream = stream;
        }
    }
}
