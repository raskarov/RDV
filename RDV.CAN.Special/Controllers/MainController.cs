using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using RDV.CAN.Special.Models;

namespace RDV.CAN.Special.Controllers
{
    /// <summary>
    /// Основной контроллер сайта
    /// </summary>
    public class MainController : Controller
    {
        /// <summary>
        /// Отображает главную страницу сайта со спец предложением
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

		/// <summary>
		/// Отображает английскую версию сайта
		/// </summary>
		/// <returns></returns>
	    public ActionResult En()
		{
			return View();
		}

		/// <summary>
		/// Отображдает китайскую версию сайта
		/// </summary>
		/// <returns></returns>
	    public ActionResult Ch()
		{
			return View();
		}

        /// <summary>
        /// Обрабатывает заказ звонка
        /// </summary>
        /// <returns>Вид с информацией об успешной отправке</returns>
        [HttpPost]
        public ActionResult Callback(string name, string phone)
        {
            // Корневой объект отправки
            var rootObject = new JObject(new JProperty("apikey", new JValue(System.Configuration.ConfigurationManager.AppSettings["SMSAPIKey"])));
            string result = "";

            // Массив, содержащий отправляемые сообщения
            var sendArray = new JArray();

            var sendObject = new JObject(
                    new JProperty("id", new JValue(Guid.NewGuid())),
                    new JProperty("from", new JValue("chance27")),
                    new JProperty("to", new JValue(ConfigurationManager.AppSettings["CallbackNumber"])),
                    new JProperty("text", new JValue(String.Format("Заказ звонка с сайта цанспецпредложения.рф на номер {0} от {1}", phone, name))));

            sendArray.Add(sendObject);
            rootObject.Add(new JProperty("send", sendArray));

            // Отправляем
            HttpWebResponse response = null;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(String.Format("http://smspilot.ru/api2.php?json={0}", HttpUtility.UrlEncode(rootObject.ToString())));
                response = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (Exception e)
            {
                result = "Не удалось отправить сообщение: " + e.Message;
            }

            // Обрабатываем результат
            bool sendSuccess = false;
            var responseStream = response.GetResponseStream();
            if (responseStream != null)
            {
                // Анализируем ответ
                var json = new StreamReader(responseStream).ReadToEnd();
                if (json.Contains("\"send\""))
                {
                    result = "Заказ звонка был успешно отправлен";
                }
            }
            ViewBag.result = result;
            return View("ActionResult");
        }

        /// <summary>
        /// Обрабатывает написание сообщения
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Feedback(FeedbackModel model)
        {
            // Получаем email куда отправлять
            var targetEmail = System.Configuration.ConfigurationManager.AppSettings["FeedbackEmail"];
            string result = "";

            // Подгатавливаем клиент и сообщение к отправке
            var client = new SmtpClient()
            {
                EnableSsl = true,
                Host = "smtp.gmail.com",
                Port = 587,
                Credentials = new NetworkCredential("tracker@trust-media.ru", "NetTracker"),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            var message = new MailMessage(new MailAddress(model.Email, model.Name), new MailAddress(targetEmail))
            {
                IsBodyHtml = true,
                Subject = "Сообщение с сайта цанспецпредложения.рф",
                Body = String.Format("{0} на сайте <a href='http://цанспецпредложения.рф'>цанспецпредложения.рф</a> {1} отправил сообщение:<br/>{2}", DateTime.Now, String.Format("{0} ({1} - {2})", model.Name, model.Phone,model.Email), Server.HtmlEncode(model.Content))
            };

            try
            {
                client.Send(message);
                result = "Сообщение было успешно отправлено";
            }
            catch (Exception e)
            {
                result = String.Format("Не удалось отправить сообщение: {0}", e.Message);
            }

            ViewBag.result = result;
            return View("ActionResult");
        }

    }
}
