using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Mailing.Templates;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Classes.Forms.Fields;
using RDV.Domain.Core;

namespace RDV.Web.Classes.Forms.Validators
{
    /// <summary>
    /// Статический валидатор активного статуса
    /// </summary>
    public static class ActiveStatusValidator
    {
        /// <summary>
        /// Валидирует объект недвижимости, удостоверяясь что в нем заполнены все необходиые для работы системы статусные поля и возвращает коллекцию ошибок
        /// </summary>
        /// <param name="obj">Объект недвижимости</param>
        /// <param name="currentUser">Текущий пользователь</param>
        /// <returns>Коллекция ошибок. Если ошибок нет, то объект прошел валидацию</returns>
        public static IList<string> Validate(EstateObject obj, User currentUser)
        {
            var errorsList = new List<string>();

            // Формируем список всех полей объектов, чтобы проверить правильность заполнения объекта для перевода его в активный статус
            var allFields = FormPageFieldsFactory.AllFields(obj, currentUser);

            // Считываем поля с объекта
            allFields.ReadValuesFromObject(obj);

            // Валидируем
            foreach (var requiredField in allFields.Where(f => !string.IsNullOrEmpty(f.Required)))
            {
                var required = IsFieldRequired(obj, requiredField);

                // Если поле должно быть заполнено то проверяем его на заполненность
                var fieldRenderingContext = new FieldRenderingContext(obj, currentUser);
                if (required && requiredField.CheckVisibility(fieldRenderingContext) &&
                    string.IsNullOrEmpty(requiredField.Value))
                {
                    errorsList.Add(string.Format("Не заполнено обязательное поле {0}", requiredField.Caption));
                }
            }

            // Проверяем клиента у объекта
            if (obj.Client == null)
            {
                errorsList.Add("Не указан клиент");
            }

			// Проверяем чтобы номер квартиры не был нулевым или состояил из одних нулей
	        if (!string.IsNullOrEmpty(obj.Address.Flat) && obj.Address.Flat.All(c => c == '0'))
	        {
		        errorsList.Add("Номер квартиры не может состоять из нулей");
	        }

            // Валидируем цену на объект если у нас операция продажа
            if (obj.Operation == (short) EstateOperations.Selling && obj.ObjectMainProperties.Price.HasValue)
            {
                var priceAsString = obj.ObjectMainProperties.Price.Value.ToString("0");
                if (priceAsString.Length < 5)
                {
                    errorsList.Add(string.Format("Неправильно указана цена объекта для этой операции: {0}. Стоимость указывается в руб. (не в тыс. руб.)",
                                                 priceAsString));
                }
            }

            // Фильтруем местоположение
            if (obj.Address.GeoCountry == null || obj.Address.GeoRegion == null || obj.Address.GeoRegionDistrict == null ||
                obj.Address.GeoCity == null || obj.Address.GeoDistrict == null || (obj.ObjectType != (short)EstateTypes.Land && obj.ObjectType != (short)EstateTypes.House && obj.Address.GeoStreet == null))
            {
                errorsList.Add("Не полностью заполнены реквизиты местоположения объекта");
            }

            // Проверяем указано ли положение на карте
            if (obj.ObjectType != (short)EstateTypes.Land && obj.ObjectType != (short)EstateTypes.House && !obj.Address.Latitude.HasValue || !obj.Address.Logitude.HasValue || obj.Address.Latitude.Value == 0 || obj.Address.Logitude.Value == 0)
            {
                errorsList.Add("Не полностью заполнены координаты местоположения объекта");
            }

			// Проверяем статус пользователя
	        if (obj.User.Status != (short) UserStatuses.Active)
	        {
				errorsList.Add(String.Format("Невозможно изменить статус объекта №{0}, поскольку пользователь находится в статусе {1}. За подробностями обратитесь к администратору",obj.Id,((UserStatuses)obj.User.Status).GetEnumMemberName()));
	        }

            // Проверяем, есть ли объект уже с таким адресом в базе данных
            if (errorsList.Count == 0)
            {
                var objectExists = false;
                var objectsRep = Locator.GetService<IEstateObjectsRepository>();
                EstateObject existedObject;
                switch ((EstateTypes) obj.ObjectType)
                {
                    case EstateTypes.Room:
                        objectExists = false;
                        existedObject = null;
                        break;
                    case EstateTypes.Flat:
                        existedObject = objectsRep.Search(
                            o => o.Id != obj.Id &&
                                 o.Address.CountryId == obj.Address.CountryId &&
                                 o.Address.RegionId == obj.Address.RegionId &&
                                 o.Address.RegionDistrictId == obj.Address.RegionDistrictId &&
                                 o.Address.CityId == obj.Address.CityId &&
                                 //o.Address.CityDistrictId == obj.Address.CityDistrictId &&
                                 //o.Address.DistrictResidentialAreaId == obj.Address.DistrictResidentialAreaId &&
                                 (o.Address.GeoStreet != null && obj.Address.GeoStreet != null && o.Address.GeoStreet.Name == obj.Address.GeoStreet.Name) &&
								 o.Address.House == obj.Address.House &&
                                 o.Address.Flat == obj.Address.Flat &&
                                 //o.ObjectMainProperties.FloorNumber == obj.ObjectMainProperties.FloorNumber &&
                                 o.ObjectAdditionalProperties.RoomsCount == obj.ObjectAdditionalProperties.RoomsCount).
                            FirstOrDefault();
                        objectExists = existedObject != null;
                        break;
                    case EstateTypes.House:
                        existedObject = objectsRep.Search(
                            o => o.Id != obj.Id &&
                                 o.Address.CountryId == obj.Address.CountryId &&
                                 o.Address.RegionId == obj.Address.RegionId &&
                                 o.Address.RegionDistrictId == obj.Address.RegionDistrictId &&
                                 o.Address.CityId == obj.Address.CityId &&
                                 //o.Address.CityDistrictId == obj.Address.CityDistrictId &&
                                 //o.Address.DistrictResidentialAreaId == obj.Address.DistrictResidentialAreaId &&
								 //(o.Address.GeoStreet != null && obj.Address.GeoStreet != null && o.Address.GeoStreet.Name == obj.Address.GeoStreet.Name) &&
								 o.Address.House == obj.Address.House)
                            .FirstOrDefault();
                        objectExists = existedObject != null;
                        break;
                    case EstateTypes.Land:
                        existedObject = objectsRep.Search(
                            o => o.Id != obj.Id &&
                                 o.Address.CountryId == obj.Address.CountryId &&
                                 o.Address.RegionId == obj.Address.RegionId &&
                                 o.Address.RegionDistrictId == obj.Address.RegionDistrictId &&
                                 o.Address.CityId == obj.Address.CityId &&
                                 //o.Address.CityDistrictId == obj.Address.CityDistrictId &&
                                 //o.Address.DistrictResidentialAreaId == obj.Address.DistrictResidentialAreaId &&
								 //(o.Address.GeoStreet != null && obj.Address.GeoStreet != null && o.Address.GeoStreet.Name == obj.Address.GeoStreet.Name) &&
								 o.Address.House == obj.Address.House)
                            .FirstOrDefault();
                        objectExists = existedObject != null;
                        break;
                    case EstateTypes.Office:
                        existedObject = objectsRep.Search(
                            o => o.Id != obj.Id &&
                                 o.Address.CountryId == obj.Address.CountryId &&
                                 o.Address.RegionId == obj.Address.RegionId &&
                                 o.Address.RegionDistrictId == obj.Address.RegionDistrictId &&
                                 o.Address.CityId == obj.Address.CityId &&
                                 //o.Address.CityDistrictId == obj.Address.CityDistrictId &&
                                 //o.Address.DistrictResidentialAreaId == obj.Address.DistrictResidentialAreaId &&
                                 (o.Address.GeoStreet != null && obj.Address.GeoStreet != null && o.Address.GeoStreet.Name == obj.Address.GeoStreet.Name) &&
                                 o.Address.Flat == obj.Address.Flat).FirstOrDefault();
                        objectExists = existedObject != null;
                        break;
                    case EstateTypes.Garage:
                        objectExists = false;
                        existedObject = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                if (objectExists && existedObject != null && (existedObject.Status == (short)EstateStatuses.Active) | existedObject.Status == (short)EstateStatuses.TemporarilyWithdrawn | existedObject.Status == (short)EstateStatuses.Advance)
                {
                    //errorsList.Add(string.Format("В системе уже зарегистрирован объект с идентичным адресом (объект №{0}). Пожалуйста, укажите другой объект.", existedObject.Id));
                    if (obj.Client != null)
                    {
                        if (obj.ObjectAdditionalProperties.AgreementType == 266 || obj.ObjectAdditionalProperties.AgreementType == 354) // Есть договор
                        {
                            if (existedObject.ObjectAdditionalProperties.AgreementType == 266 || existedObject.ObjectAdditionalProperties.AgreementType == 354)
                            {
                                errorsList.Add(String.Format("Договорной объект с такими реквизитами уже зарегистрирован в системе: Код объекта - {0}, агент компании {1}, {2}, {3}", existedObject.Id, existedObject.User.Company.Name, existedObject.User.ToString(), existedObject.User.Phone));
                            }
                            else
                            {
                                if (obj.ObjectAdditionalProperties.AgreementStartDate.HasValue && !String.IsNullOrEmpty(obj.ObjectAdditionalProperties.AgreementNumber))
                                {
                                    // Переносим существующий объект в статус черновика и уведомляем пользователя об этом
                                    existedObject.Status = (short)EstateStatuses.Draft;
                                    existedObject.ObjectHistoryItems.Add(new ObjectHistoryItem()
                                    {
                                        HistoryStatus = (short)EstateStatuses.Draft,
                                        EstateObject = existedObject,
                                        DateCreated = DateTimeZone.Now,
                                    });
                                    objectsRep.SubmitChanges();
                                    Locator.GetService<IMailNotificationManager>().Notify(existedObject.User, "Объект перенесен в Черновики", new ParametrizedFileTemplate(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Mail", "ActiveDraft.htm"), new
                                    {
                                        ObjectId = existedObject.Id,
                                        Address = existedObject.Address.ToShortAddressString(),
                                        OldName = existedObject.User.ToString(),
                                        NewName = obj.User.ToString(),
                                        NewCompanyName = obj.User.Company.Name,
                                        Phone = obj.User.Phone
                                    }).ToString());
                                }
                                else
                                {
                                    // Дата и номер договора не заполнены
                                    errorsList.Add("Необходимо ввести номер и дату заключения договора");
                                }    
                            }
                        }
                        else
                        {
                            errorsList.Add(String.Format("Объект с такими реквизитами уже зарегистрирован в системе: Код объекта - {0}, агент компании {1}, {2}, {3}", existedObject.Id, existedObject.User.Company.Name, existedObject.User.ToString(), existedObject.User.Phone));
                        }
                    }

                }   
            
                /*
                if (objectExists && existedObject != null &&
                    (existedObject.Status == (short) EstateStatuses.Active) |
                    existedObject.Status == (short) EstateStatuses.TemporarilyWithdrawn |
                    existedObject.Status == (short) EstateStatuses.Advance)
                {
                    errorsList.Add(
                        string.Format("Аналогичный объект уже существует (№{1}) и зарегистрирован в статусе {0}",
                                      ((EstateStatuses) existedObject.Status).GetEnumMemberName(), existedObject.Id));
                }*/
            }

            // Отдаем список
            return errorsList;
        }

        /// <summary>
        /// Проверяет, требуется ли поле при текущем состоянии объекта
        /// </summary>
        /// <param name="obj">Объект</param>
        /// <param name="field">Поле</param>
        /// <returns>True если требуется, иначе false</returns>
        public static bool IsFieldRequired(EstateObject obj, ObjectFormField field)
        {
			// Действительно ли поле требуется
	        if ((obj.ObjectType == (short) EstateTypes.House & field.Name == "house-number"))
	        {
		        return false;
	        }
            bool required = field.Required == "always";
            if (required)
            {
                return true;
            }
            if (field.Required == null)
            {
                return false;
            }
            if (field.Required.Contains("selling") && obj.Operation == (short) EstateOperations.Selling)
            {
                required = true;
            }
            if (field.Required.Contains("lising") && obj.Operation == (short) EstateOperations.Lising)
            {
                required = true;
            }
            if (field.Required.Contains("house") && obj.ObjectType == (short) EstateTypes.House)
            {
                required = true;
            }
            if (field.NotRequired.Contains("land") && obj.ObjectType == (short) EstateTypes.Land)
            {
                required = false;
            }
            if (field.NotRequired.Contains("house") && obj.ObjectType == (short) EstateTypes.House)
            {
                required = false;
            }
            if (field.NotRequired.Contains("flat") && obj.ObjectType == (short)EstateTypes.Flat)
            {
                required = false;
            }
	        if (field.CustomRequired != null)
	        {
		        required = field.CustomRequired(obj);
	        }
            return required;
        }
    }
}