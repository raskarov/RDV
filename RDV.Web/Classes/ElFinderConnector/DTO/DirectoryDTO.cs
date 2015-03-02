using System.Runtime.Serialization;

namespace RDV.Web.Classes.ElFinderConnector.DTO
{
    [DataContract]
    internal class DirectoryDTO : DTOBase
    {           
        /// <summary>
        ///  Hash of parent directory. Required except roots dirs.
        /// </summary>
        [DataMember(Name = "phash")]
        public string ParentHash { get; set; }
        
        /// <summary>
        /// Is directory contains subfolders
        /// </summary>
        [DataMember(Name = "dirs")]
        public byte ContainsChildDirs { get; set; }
    }
}