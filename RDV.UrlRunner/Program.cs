using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RDV.UrlRunner
{
	class Program
	{
		static void Main(string[] args)
		{
            //var initialUrl = ConfigurationManager.AppSettings["InitialUrl"];
            //var checkUrl = ConfigurationManager.AppSettings["CheckUrl"];
            //var delay = Convert.ToInt32(ConfigurationManager.AppSettings["Delay"]);

            var initialUrl = @"http://rdv-prod.azurewebsites.net";
            var checkUrl = @"http://rdv-prod.azurewebsites.net/Objects/CheckObjects";
            var delay = 4000;

			var client = new WebClient();
			try
			{
				var result = client.DownloadString(initialUrl);
				Console.WriteLine(result);
				Thread.Sleep(delay);
				result = client.DownloadString(checkUrl);
				Console.WriteLine(result);
				Thread.Sleep(5000);
			}
			catch (Exception e)
			{
				Console.Write(e.Message);
			}
		}
	}
}
