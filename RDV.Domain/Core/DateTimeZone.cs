using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace RDV.Domain.Core
{
    public class DateTimeZone
    {
        public static DateTime Now
        {
            get
            {
                String zoneId = ConfigurationManager.AppSettings["TimeZone"].ToString();

                DateTime timeUtc = DateTime.UtcNow;
                TimeZoneInfo needZone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
                DateTime needTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, needZone);

                return needTime;
            }
        }
    }
}
