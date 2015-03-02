using System.Runtime.Serialization;

namespace RDV.Web.Classes.ElFinderConnector.DTO
{
    [DataContract]
    internal class RootDTO : DTOBase
    {
        [DataMember(Name = "volumeId")]
        public string VolumeId { get; set; }

        [DataMember(Name = "dirs")]
        public byte Dirs { get; set; }
    }
}