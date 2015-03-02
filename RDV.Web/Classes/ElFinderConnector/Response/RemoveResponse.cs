using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RDV.Web.Classes.ElFinderConnector.Response
{
    [DataContract]
    internal class RemoveResponse
    {
        private List<string> _removed;

        [DataMember(Name = "removed")]
        public List<string> Removed { get { return _removed; } }

        public RemoveResponse()
        {
            _removed = new List<string>();
        }
    }
}