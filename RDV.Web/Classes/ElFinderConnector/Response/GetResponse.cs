using System.Runtime.Serialization;

namespace RDV.Web.Classes.ElFinderConnector.Response
{
    [DataContract]
    internal class GetResponse
    {
        [DataMember(Name="content")]
        public string Content { get; set; }
    }
}