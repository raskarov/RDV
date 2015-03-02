using System.Runtime.Serialization;
using RDV.Web.Classes.ElFinderConnector.DTO;
using RDV.Web.Classes.ElFinderConnector.Facade;

namespace RDV.Web.Classes.ElFinderConnector.Response.Open
{
    [DataContract]
    internal class OpenResponse : OpenResponseBase
    {
        public OpenResponse(DTOBase currentWorkingDirectory, FullPath fullPath)
            : base(currentWorkingDirectory)
        {
            Options = new Options(fullPath);
            _files.Add(currentWorkingDirectory);
        }
    }
}