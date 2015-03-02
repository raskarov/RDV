using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RDV.Web.Classes.Extensions;
using RDV.Web.Classes.Forms;
using RDV.Web.Classes.Search.Interfaces;

namespace RDV.Web.Models.Search
{
    /// <summary>
    /// Модель, используемая для формы поиска
    /// </summary>
    public class ComplexSearchFormModel
    {
        public List<Int64> DistrictIds { get; set; }

        public List<Int64> CompanyIds { get; set; }
    }
}