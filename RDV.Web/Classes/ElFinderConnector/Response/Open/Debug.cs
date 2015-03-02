using System.Runtime.Serialization;

namespace RDV.Web.Classes.ElFinderConnector.Response.Open
{
    [DataContract]
    internal class Debug
    {
        [DataMember(Name = "connector")]
        public string Connector { get { return ".net"; } }
    }
}
