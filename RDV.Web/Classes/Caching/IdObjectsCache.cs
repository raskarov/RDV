using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Hosting;
using RDV.Domain.Infrastructure.Misc;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.Interfaces.Repositories.Geo;
using RDV.Domain.IoC;

namespace RDV.Web.Classes.Caching
{
    /// <summary>
    /// Глобальный класс, используемый в интерфейсе для быстрого запоминания пары ид пользователя -> логин
    /// </summary>
    public static class IdObjectsCache
    {
        /// <summary>
        /// Кеширующий словарь логинов
        /// </summary>
        private static Dictionary<long, string> LoginsCache { get; set; }

        /// <summary>
        /// Кеширующий словарь имен пользователей
        /// </summary>
        private static Dictionary<long, string> UserNamesCache { get; set; }

        /// <summary>
        /// Кеширующий словарь значений справочников
        /// </summary>
        private static Dictionary<long, string> DictionaryValuesCache { get; set; }

        /// <summary>
        /// Кеширующий словарь скоращений значений справочников
        /// </summary>
        private static Dictionary<long, string> ShortDictionaryValuesCache { get; set; }

        /// <summary>
        /// Кеширующий словарь имен клиентов
        /// </summary>
        private static Dictionary<long, string> ClientNamesCache { get; set; }

        /// <summary>
        /// Кеширующий словарь имен улиц
        /// </summary>
        private static Dictionary<long, string> StreetNamesCache { get; set; }

        /// <summary>
        /// Статический конструктор
        /// </summary>
        static IdObjectsCache()
        {
            LoginsCache = new Dictionary<long, string>();
            UserNamesCache = new Dictionary<long, string>();
            DictionaryValuesCache = new Dictionary<long, string>();
            ShortDictionaryValuesCache = new Dictionary<long, string>();
            ClientNamesCache = new Dictionary<long, string>();
            StreetNamesCache = new Dictionary<long, string>();
        }

        /// <summary>
        /// Возвращает из кеша логин пользователя на основании его идентифитатора
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static string GetUserLogin(long userId)
        {
            if (userId == -1)
            {
                return "Не установлен";
            }
            
            // Ищем значение в кеше
            string login;
            if (!LoginsCache.TryGetValue(userId, out login))
            {
                // Проверяем если ли у нас HTTP Context для текущего потокола
                if (HttpContext.Current == null)
                {
                    HttpContext.Current = new HttpContext(new SimpleWorkerRequest("temp",string.Empty,null));
                }

                // Ищем в базе данных
                var user = Locator.GetService<IUsersRepository>().Load(userId);
                if (user == null)
                {
                    return "Не установлен";
                }

                login = user.Login;

                // Сохраняем в кеше используя синхронизацию
                lock (LoginsCache)
                {
                    LoginsCache[userId ] = login;
                }
            }
            return login;
        }

        /// <summary>
        /// Возвращает имя пользователя по его идентификатору с использованием кеша
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns></returns>
        public static string GetUserName(long userId)
        {
            if (userId == -1)
            {
                return "Система";
            }

            // Ищем значение в кеше
            string username;
            if (!UserNamesCache.TryGetValue(userId, out username))
            {
                // Проверяем если ли у нас HTTP Context для текущего потокола
                if (HttpContext.Current == null)
                {
                    HttpContext.Current = new HttpContext(new SimpleWorkerRequest("temp", string.Empty, null));
                }

                // Ищем в базе данных
                var user = Locator.GetService<IUsersRepository>().Load(userId);

                username = user != null ? user.ToString() : "";

                // Сохраняем в кеше используя синхронизацию
                lock (UserNamesCache)
                {
                    UserNamesCache[userId] = username;
                }
            }
            return username;
        }

        /// <summary>
        /// Возвращает значение справочника с указанным идентификатором, если не найдено то возвращается пустая строка
        /// </summary>
        /// <param name="dictionaryValueId"></param>
        /// <returns></returns>
        public static string GetDictionaryValue(long? dictionaryValueId)
        {
            if (!dictionaryValueId.HasValue || dictionaryValueId == -1)
            {
                return String.Empty;
            }

            // Ищем значение в кеше
            string value;
            if (!DictionaryValuesCache.TryGetValue(dictionaryValueId.Value, out value))
            {
                // Проверяем если ли у нас HTTP Context для текущего потокола
                if (HttpContext.Current == null)
                {
                    HttpContext.Current = new HttpContext(new SimpleWorkerRequest("temp", string.Empty, null));
                }

                // Ищем в базе данных
                var val = Locator.GetService<IDictionaryValuesRepository>().Load(dictionaryValueId.Value);
                if (val == null)
                {
                    return String.Empty;
                }

                value = val.Value;

                // Сохраняем в кеше используя синхронизацию
                lock (DictionaryValuesCache)
                {
                    DictionaryValuesCache[dictionaryValueId.Value] = value;
                }
            }
            return value;
        }

	    /// <summary>
	    /// Возвращает значение справочников в виде строки
	    /// </summary>
	    /// <param name="valueIds"></param>
	    /// <returns></returns>
	    public static string GetDictionaryValue(string valueIds)
	    {
		    if (!String.IsNullOrEmpty(valueIds))
		    {
			    var array =
				    valueIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).
					    Select(i => GetDictionaryValue(i)).ToList();
			    return string.Join(", ", array);    
		    }
		    else
		    {
			    return String.Empty;
		    }
	    }

	    /// <summary>
	    /// Возвращает значение справочников в виде строки
	    /// </summary>
	    /// <param name="valueIds"></param>
	    /// <returns></returns>
	    public static string GetShortDictionaryValue(string valueIds)
	    {
		    if (!String.IsNullOrEmpty(valueIds))
		    {
			    var array =
				    valueIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(i => Convert.ToInt64(i)).
					    Select(i => GetShortDictionaryValue(i)).ToList();
			    return string.Join(", ", array);    
		    }
		    else
		    {
			    return String.Empty;
		    }
	    }

	    /// <summary>
        /// Возвращает сокращенное значение справочника с указанным идентификатором, если не найдено то возвращается пустая строка
        /// </summary>
        /// <param name="dictionaryValueId"></param>
        /// <returns></returns>
        public static string GetShortDictionaryValue(long? dictionaryValueId)
        {
            if (!dictionaryValueId.HasValue || dictionaryValueId == -1)
            {
                return String.Empty;
            }

            // Ищем значение в кеше
            string value;
            if (!ShortDictionaryValuesCache.TryGetValue(dictionaryValueId.Value, out value))
            {
                // Проверяем если ли у нас HTTP Context для текущего потокола
                if (HttpContext.Current == null)
                {
                    HttpContext.Current = new HttpContext(new SimpleWorkerRequest("temp", string.Empty, null));
                }

                // Ищем в базе данных
                var val = Locator.GetService<IDictionaryValuesRepository>().Load(dictionaryValueId.Value);
                if (val == null)
                {
                    return String.Empty;
                }

                value = val.ShortValue ?? val.Value;

                // Сохраняем в кеше используя синхронизацию
				lock (ShortDictionaryValuesCache)
                {
					ShortDictionaryValuesCache[dictionaryValueId.Value] = value;
                }
            }
            return value;
        }

        /// <summary>
        /// Возвращает имя клиента по его идентификатору из кеша либо из базы
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns></returns>
        public static string GetClientName(long? clientId)
        {
            if (!clientId.HasValue || clientId == -1)
            {
                return String.Empty;
            }

            // Ищем значение в кеше
            string value;
            if (!ClientNamesCache.TryGetValue(clientId.Value, out value))
            {
                // Проверяем если ли у нас HTTP Context для текущего потокола
                if (HttpContext.Current == null)
                {
                    HttpContext.Current = new HttpContext(new SimpleWorkerRequest("temp", string.Empty, null));
                }

                // Ищем в базе данных
                var val = Locator.GetService<IClientsRepository>().Load(clientId.Value);
                if (val == null)
                {
                    return String.Empty;
                }

                value = String.Format("{0} ({1})",val.ToString(),val.Phone.FormatPhoneNumber());

                // Сохраняем в кеше используя синхронизацию
                lock (ClientNamesCache)
                {
                    ClientNamesCache[clientId.Value] = value;
                }
            }
            return value;
        }

	    /// <summary>
        /// Возвращает наименование улицы по ее идентификатору беря значение из кеша
        /// </summary>
        /// <param name="streetId">Идентификатор улицы</param>
        /// <returns></returns>
        public static string GetStreetName(long? streetId)
        {
            if (!streetId.HasValue || streetId == -1)
            {
                return String.Empty;
            }

            // Ищем значение в кеше
            string value;
            if (!StreetNamesCache.TryGetValue(streetId.Value, out value))
            {
                // Проверяем если ли у нас HTTP Context для текущего потокола
                if (HttpContext.Current == null)
                {
                    HttpContext.Current = new HttpContext(new SimpleWorkerRequest("temp", string.Empty, null));
                }

                // Ищем в базе данных
                var val = Locator.GetService<IGeoStreetsRepository>().Load(streetId.Value);
                if (val == null)
                {
                    return String.Empty;
                }

                value = val.Name;

                // Сохраняем в кеше используя синхронизацию
                lock (StreetNamesCache)
                {
                    StreetNamesCache[streetId.Value] = value;
                }
            }
            return value;
        }

        /// <summary>
        /// Получает имена улиц 
        /// </summary>
        /// <param name="geo"></param>
        /// <returns></returns>
        public static string GetCityNames(string geo)
        {
            using (var scope = Locator.BeginNestedHttpRequestScope())
            {
                var mgr = Locator.GetService<IGeoManager>();
                return String.Join(", ",
                                   (geo ?? "").Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries).Select(
                                       c => Convert.ToInt64(c)).Select(s => mgr.CitiesRepository.Load(s)).Where(
                                           s => s != null).Select(c => c.Name));
            }

        }
    }
}