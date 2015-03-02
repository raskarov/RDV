using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Web;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Misc;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Domain.Core;

namespace RDV.Domain.Infrastructure.Mailing
{
    /// <summary>
    /// Реализация менеджера смс рассылок
    /// </summary>
    public class SMSNotificationManager: ISMSNotificationManager
    {
         /// <summary>
        /// Логгер текущего класса
        /// </summary>
        private Logger Logger { get; set; }

        /// <summary>
        /// Период срабатывания таймера
        /// </summary>
        private const long TimerPeriod = 60000;

        /// <summary>
        /// Флаг, указывающий, находится ли очередь в процессе обработки
        /// </summary>
        private bool ProcessingActive { get; set; }

        /// <summary>
        /// Таймер, выполняющий обработку сообщений по времени
        /// </summary>
        private System.Threading.Timer ProcessingTimer { get; set; }

        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public SMSNotificationManager()
        {
            ProcessingTimer = new Timer(state =>
                                            {
                                                if (!ProcessingActive)
                                                {
                                                    try
                                                    {
                                                        ProcessingActive = true;
                                                        FlushQueue();
                                                        ProcessingActive = false;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        ProcessingActive = false;
                                                        Logger.Error(String.Format("Ошибка в ходе обработки очереди смс сообщений: {0}",e.Message));
                                                    }    
                                                }
                                                
                                            },null,TimerPeriod,TimerPeriod);
            Logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Обрабатывает очередь сообщений
        /// </summary>
        private void FlushQueue()
        {
            using (var httpRequestScope = Locator.BeginNestedHttpRequestScope())
            {
                // Репозиторий
                var repository = Locator.GetService<ISMSNotificationMessagesRepository>();
                var messages = repository.GetEnqueuedMessages().ToList();

                // Выходим если в очереди пусто
                if (messages.Count == 0)
                {
                    return;
                }

                

                // Обрабатываем очередь
                Logger.Info(string.Format("Обрабатываем очередь сообщений, в очереди {0} писем", messages.Count));

                // Корневой объект отправки
                var rootObject = new JObject(new JProperty("apikey", new JValue(System.Configuration.ConfigurationManager.AppSettings["SMSAPIKey"])));

                // Массив, содержащий отправляемые сообщения
                var sendArray = new JArray();

                // Формируем запрос на отправку
                foreach (var msg in messages)
                {
                    var sendObject = new JObject(
                        new JProperty("id", new JValue(msg.Id)),
                        new JProperty("from",new JValue("nprdv.ru")),
                        new JProperty("to",new JValue(StringUtils.NormalizePhoneNumber(msg.Recipient))),
                        new JProperty("text",new JValue(msg.Message)));

                    sendArray.Add(sendObject);
                }
                rootObject.Add(new JProperty("send",sendArray));

                // Отправляем
                HttpWebResponse response = null;
                try
                {
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(String.Format("http://smspilot.ru/api2.php?json={0}", HttpUtility.UrlEncode(rootObject.ToString())));
                    response = (HttpWebResponse) webRequest.GetResponse();
                }
                catch (Exception e)
                {
                    Logger.Error(string.Format("Ошибка в ходе обработки очереди СМС: {0}",e.Message)); 
                    throw;
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
                        sendSuccess = true;
                    }
                }

                // Помечаем письма как отправленные
                if (sendSuccess)
                {
                    foreach (var msg in messages)
                    {
                        msg.Sended = true;
                        msg.DateSended = DateTimeZone.Now;
                    }
                    repository.SubmitChanges();
                }
                Logger.Info(string.Format("Обработка очереди смс сообщений завершена."));    
            }
        }

        /// <summary>
        /// Нотифицирует указанный номер телефона указанным сообщением смс
        /// </summary>
        /// <param name="phoneNumber">Номер телефона</param>
        /// <param name="message">Сообщение</param>
        public void Notify(string phoneNumber, string message)
        {
            using (var httpRequestScope = Locator.BeginNestedHttpRequestScope())
            {
                // Создаем сообщение и помещаем его в очередь
                var repository = Locator.GetService<ISMSNotificationMessagesRepository>();
                var newMessage = new SMSNotificationMessage()
                {
                    Recipient = phoneNumber,
                    Message = message,
                    DateEnqueued = DateTimeZone.Now,
                    Sended = false
                };
                repository.Add(newMessage);
                repository.SubmitChanges();
            }
        }

        /// <summary>
        /// Нотифицирует указанного пользователя указанным сообщением по смс
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="message">сообщение</param>
        public void Notify(User user, string message)
        {
	        if (user.Status == (short) UserStatuses.Active)
	        {
				Notify(user.Phone, message);    
	        }
            
        }

        /// <summary>
        /// Инициализирует менеджер
        /// </summary>
        public void Init()
        {
            // NOTE: ничего не делает
        }
    }
}