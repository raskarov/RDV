using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RDV.Web.Classes.ElFinderConnector.Response
{
    [DataContract]
    internal class ListResponse
    {
        [DataMember(Name="list")]
        public List<string> List { get; private set; }

        public ListResponse()
        {
            List = new List<string>();
        }     
    }
}