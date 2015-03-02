// ============================================================
// 
// 	RDV
// 	RDV.Domain 
// 	EstateObject.cs
// 
// 	Created by: ykorshev 
// 	 at 12.11.2012 22:43
// 
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Misc;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;

namespace RDV.Domain.Entities
{
    /// <summary>
    /// Объект недвижимости
    /// </summary>
    public partial class EstateObject
    {
        /// <summary>
        /// Возвращает коллекцию медиа объектов у текущего объекта недвижимости
        /// </summary>
        /// <param name="onlyPhotos">Только фотографии</param>
        /// <returns></returns>
        public IList<ObjectMedia> GetObjectsMedia(bool onlyPhotos)
        {
            IEnumerable<ObjectMedia> result = this.ObjectMedias;
            if (onlyPhotos)
            {
                result = result.Where(m => m.MediaType == (short) ObjectMediaTypes.Photo);
            }
            return result.OrderByDescending(d => d.IsMain).ThenBy(d => d.Position).ToList();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("№{0} - {1}", Id, Address.ToShortAddressString());
        }

        /// <summary>
        /// Возвращает дату активации объекта
        /// </summary>
        /// <returns></returns>
        public DateTime? GetRegistrationDate()
        {
            return (ObjectChangementProperties.DateRegisted ??
                      ObjectHistoryItems.Where(a => a.HistoryStatus == (short) EstateStatuses.Active)
                                        .OrderByDescending(d => d.DateCreated)
                                        .Select(d => d.DateCreated)
                                        .FirstOrDefault()) ?? DateCreated;
        }

        /// <summary>
        /// Возвращает дату и время изменения цены на объект
        /// </summary>
        /// <returns></returns>
        public DateTime? GetPriceChangementDate()
        {
            return ObjectChangementProperties.PriceChanged ?? GetRegistrationDate();
        }

        /// <summary>
        /// Возвращает контрагента, который участвовал при сделке
        /// </summary>
        /// <returns></returns>
        public string GetDealCounterAgent()
        {
            var lastHistoryItem =
                ObjectHistoryItems.Where(i => i.HistoryStatus == (short) EstateStatuses.Deal)
                                  .OrderByDescending(d => d.DateCreated)
                                  .FirstOrDefault();
            if (lastHistoryItem == null)
            {
                // Мы стали жертвой бага
                lastHistoryItem = ObjectHistoryItems.Where(i => i.HistoryStatus == (short)EstateStatuses.Advance)
                              .OrderByDescending(d => d.DateCreated)
                              .FirstOrDefault();
                if (lastHistoryItem == null)
                {
                    return String.Empty;    
                }
            }

            // Контрагент - клиент рдв?
            if (lastHistoryItem.ClientId > 0)
            {
                var client = Locator.GetService<IClientsRepository>().Load(lastHistoryItem.ClientId);
                return client != null ? client.ToString() : "Неизвестное физическое лицо";
            }
            // контрагент - компания рдв?
            if (lastHistoryItem.CompanyId > 0)
            {
                var company = Locator.GetService<ICompaniesRepository>().Load(lastHistoryItem.CompanyId);
                var result = new StringBuilder();
                if (company != null)
                {
                    result.Append(company.Name);
                }
                else
                {
                    result.Append("Незивестная компания из РДВ");
                }
                return result.ToString();
            } 
            // Контрагент другая компания?
            return lastHistoryItem.CustomerName;
        }

        /// <summary>
        /// Возвращает все элементы истории объекта
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ObjectHistoryItem> GetHistoryItems()
        {
            return ObjectHistoryItems;
        }

        /// <summary>
        /// Возвращает тип выбранного контрагента
        /// </summary>
        /// <returns></returns>
        public string GetDealCounterAgentType()
        {
            var lastHistoryItem =
                ObjectHistoryItems.Where(i => i.HistoryStatus == (short)EstateStatuses.Deal)
                                  .OrderByDescending(d => d.DateCreated)
                                  .FirstOrDefault();
            if (lastHistoryItem == null)
            {
                // Мы стали жертвой бага
                lastHistoryItem = ObjectHistoryItems.Where(i => i.HistoryStatus == (short)EstateStatuses.Advance)
                              .OrderByDescending(d => d.DateCreated)
                              .FirstOrDefault();
                if (lastHistoryItem == null)
                {
                    return String.Empty;
                }
            }

            // Контрагент - клиент рдв?
            if (lastHistoryItem.ClientId > 0)
            {
                var client = Locator.GetService<IClientsRepository>().Load(lastHistoryItem.ClientId);
                return "Физическое лицо";
            }
            // контрагент - компания рдв?
            if (lastHistoryItem.CompanyId > 0)
            {
                var company = Locator.GetService<ICompaniesRepository>().Load(lastHistoryItem.CompanyId);
                var resultSb = new StringBuilder();
                resultSb.AppendFormat("Компания член РДВ", company.Name);
                /*
                if (lastHistoryItem.RDVAgentId > 0)
                {
                    
                    var rdvAgent = Locator.GetService<IUsersRepository>().Load(lastHistoryItem.NonRDVAgentId);
                    if (rdvAgent != null)
                    {
                        resultSb.AppendFormat(", агент {0}", rdvAgent.ToString());
                    }
                }*/
                return resultSb.ToString();
            }
            else
            {
                // Указано ли имя компании
                if (!String.IsNullOrEmpty(lastHistoryItem.CustomerName) && lastHistoryItem.NonRdvAgent != null)
                {
                    return String.Format("Компания не член РДВ", lastHistoryItem.CustomerName,
                        lastHistoryItem.NonRdvAgent.ToString(), lastHistoryItem.NonRdvAgent.Phone.FormatPhoneNumber());
                }
            }
            // Контрагент другая компания?
            return "Риэлторская компания";
        }

        /// <summary>
        /// Возвращает абсолютный размер комиссии бонуса мультилистинга для сортировки объектов
        /// </summary>
        /// <returns></returns>
        public double AbsoluteBonusSize()
        {
            if (ObjectAdditionalProperties.AgreementType == 354)
            {
                switch (ObjectMainProperties.MultilistingBonusType)
                {
                    case 355:
                        // Вычисляем размер бонуса по размеру процента
                        if (ObjectMainProperties.Price.HasValue && ObjectMainProperties.MultilistingBonus.HasValue)
                        {
                            return ObjectMainProperties.MultilistingBonus.Value/100*ObjectMainProperties.Price.Value;
                        }
                        else
                        {
                            return 0.0;
                        }
                        break;
                    case 356:
                        return ObjectMainProperties.MultilistingBonus.HasValue ? ObjectMainProperties.MultilistingBonus.Value : 0.0;
                        break;
                    default:
                        return 0;
                        break;
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Возвращает все подходящие запросы под указанный объект
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EstateObjectMatchedSearchRequest> GetAllMatchedRequests()
        {
            return EstateObjectMatchedSearchRequests;
        }
    }
}