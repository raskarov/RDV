using System.Collections.Generic;
using System.Runtime.Serialization;
using RDV.Web.Classes.ElFinderConnector.DTO;

namespace RDV.Web.Classes.ElFinderConnector.Response
{
    [DataContract]
    internal class ChangedResponse
    {
        [DataMember(Name="changed")]
        public List<FileDTO> Changed { get; private set; }

        public ChangedResponse()
        {
            Changed = new List<FileDTO>();
        }
    }
}