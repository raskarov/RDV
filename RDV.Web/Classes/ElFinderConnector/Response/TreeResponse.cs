using System.Collections.Generic;
using System.Runtime.Serialization;
using RDV.Web.Classes.ElFinderConnector.DTO;

namespace RDV.Web.Classes.ElFinderConnector.Response
{
    [DataContract]
    internal class TreeResponse
    {
        [DataMember(Name="tree")]
        public List<DTOBase> Tree { get; private set; }

        public TreeResponse()
        {
            Tree = new List<DTOBase>();
        }     
    }
}