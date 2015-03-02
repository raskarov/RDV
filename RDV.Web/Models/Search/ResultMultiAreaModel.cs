using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RDV.Web.Models.Search
{
    public class ResultMultiAreaModel
    {
        public Boolean success { get; set; }

        public Object[] parentCoords { get; set; }

        public List<ResultAreaMultiAreaModel> areas { get; set; }
    }

    public class ResultAreaMultiAreaModel
    {
        public String name { get; set; }

        public Object[] coordinates { get; set; }

        public Int64 id { get; set; }
    }
}