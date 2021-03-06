﻿using System.Runtime.Serialization;

namespace RDV.Web.Classes.ElFinderConnector.DTO
{
    [DataContract]
    internal class FileDTO : DTOBase
    {          
        /// <summary>
        ///  Hash of parent directory. Required except roots dirs.
        /// </summary>
        [DataMember(Name = "phash")]
        public string ParentHash { get; set; }
    }
}