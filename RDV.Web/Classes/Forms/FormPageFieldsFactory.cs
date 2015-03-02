using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Classes.Caching;
using RDV.Web.Classes.Extensions;
using RDV.Web.Classes.Forms.Fields;
using RDV.Web.Classes.Forms.Validators;
using System.Web;

// ReSharper disable ConvertToLambdaExpression
// ReSharper disable RedundantEmptyObjectCreationArgumentList
// ReSharper disable RedundantLambdaSignatureParentheses
namespace RDV.Web.Classes.Forms
{
	/// <summary>
	/// Фабрика по производству наборов полей для разных страниц формы заполнения объекта
	/// </summary>
	public static class FormPageFieldsFactory
	{
		/// <summary>
		/// Возвращает набор полей для главной страницы формы редактирования объектов
		/// </summary>
		/// <returns>Список полей</returns>
		public static FieldsList MainPageList(EstateObject currentEstateObject)
		{
			// Подгаталиваем список компаний в тот момент когда получаем значение
			var contractorCompanies =
				Locator.GetService<ICompaniesRepository>().Search(
					c => c.Id != currentEstateObject.User.CompanyId).OrderBy(c => c.Name).ToDictionary
					(c => c.Id.ToString(), c => c.Name);
			contractorCompanies["-1"] = "Сделка не с компанией";

            var rieltorsCompanies =
                Locator.GetService<ICompaniesRepository>().Search(c => c.CompanyType == 374).OrderBy(c => c.Name).ToDictionary
                    (c => c.Id.ToString(), c => c.Name);
            contractorCompanies["-1"] = "Сделка не с компанией";

			var dictRep = Locator.GetService<IDictionariesRepository>();
            
			// Создаем список
			return new FieldsList()
                 {
                     new ObjectFormTextField()
                         {
                             Caption = "Email риэлтора",
                             Name = "agent-email",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Email риэлтора, ведущего данный объект",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.User.Email;
                                 },
                             Position = 1,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Дата изменения",
                             Name = "date-modified",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Дата последней правки данного объекта",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.DateModified.FormatDateTime();
                                 },
                             Position = 2,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Дата изменения цены",
                             Name = "date-price-modified",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Дата последнего изменения цены на данный объект",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.ObjectChangementProperties.PriceChanged.FormatDateTime();
                                 },
                             Position = 3,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Дата перемещения в раздел",
                             Name = "date-moved",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Дата и время, когда объект был перемещен в данный раздел",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.ObjectChangementProperties.DateMoved.FormatDateTime();
                                 },
                             Position = 4,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Дата регистрации (дата добавления на сайт)",
                             Name = "date-registred",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Дата и время, когда данный объект был зарегистрирован в системе",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.DateCreated.FormatDateTime();
                                 }
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Дата сделки",
                             Name = "date-deal",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Дата и время, когда была заключена сделка по данному объекту",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.ObjectChangementProperties.DealDate.FormatDateTime();
                                 },
                             Position = 5,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Изменение цены",
                             Name = "price-changement",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Величина, на которую изменилась цена в последний раз",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.ObjectChangementProperties.PriceChanging.FormatString();
                                 },
                             Position = 6,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Наименование компании",
                             Name = "company-name",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Наименование компании, в которой работает агент, ведущей данный объект",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.User.Company.Name;
                                 },
                             CustomCondition = (context) =>
                                 {
                                     return context.EstateObject.Status == (short) EstateStatuses.Deal;
                                 },
                             Position = 7,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Код объекта",
                             Name = "object-id",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Уникальный идентификатор объекта в системе",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.Id;
                                 },
                             Position = 8,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Код риэлтора",
                             Name = "agent-id",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Уникальный идентификатор риэлтора, ведущего объект в системе",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.UserId;
                                 },
                             Position = 9,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Номер ICQ риэлтора",
                             Name = "agent-icq",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Номер ICQ для связи с риэлтором",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.User.ICQ;
                                 },
                             Position = 10,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Отложено до даты",
                             Name = "delay-to-date",
                             MaxLenght = 255,
                             Readonly = true,
                             CustomClasses = "datepicker",
                             Tooltip = "Дата, до которой объект временно снимается с продажи. В указанную дату риэлтор получит уведомление на email.",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.ObjectChangementProperties.DelayToDate.FormatDate();
                                 },
                             SetValueToObject = (estateObject,value) =>
                                 {
                                     estateObject.ObjectChangementProperties.DealDate =
                                         value.ConvertToNullableDateTime();
                                 },
                             Position = 11,
                            
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Перенесено пользователем",
                             Name = "status-changed-by",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Пользователь, который установил текущий статус у объекта.",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return IdObjectsCache.GetUserName(estateObject.ObjectChangementProperties.StatusChangedBy);
                                 },
                             Position = 12,
                         },
                     new ObjectFormDictionaryField(dictRep.GetDictionaryByName("removal_reasons"))
                         {
                             Caption = "Причина снятия",
                             Name = "removal-reason",
                             Readonly = true,
                             Tooltip = "Причина снятия данного объекта с продажи.",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return IdObjectsCache.GetDictionaryValue(estateObject.ObjectMainProperties.RemovalReason);
                                 },
                             Position = 13,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Риэлтор",
                             Name = "agent",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Имя агента, ведущего деятельность по объекту",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.User.ToString();
                                 },
                             Position = 14,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Телефон риэлтора 1",
                             Name = "agent-phone-1",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Основной контактный телефон, по которому можно связаться с риэлтором, ведущим данный объект",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.User.Phone;
                                 },
                             Position = 15,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Телефон риэлтора 2",
                             Name = "agent-phone-2",
                             MaxLenght = 255,
                             Readonly = true,
                             Tooltip = "Вспомогательный телефон, по которому можно связаться с риэлтором, ведущим данный объект",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.User.Phone2;
                                 },
                             CustomCondition = (context) =>
                                 {
                                     return !String.IsNullOrEmpty(context.EstateObject.User.Phone2);
                                 },
                             Position = 16,
                         },
                    new ObjectFormMultiDropDownField()
                        {
                            Caption = "Агентство",
                            Name = "rieltor-company",
                            Readonly = false,
                            Tooltip = "Агентство",
                            GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.User.CompanyId;
                                 },
                            SetValueToObject = (estateObject,value) =>
                                 {
                                     estateObject.User.CompanyId = value != "-1" && value != ""
                                                                ? Convert.ToInt64(value)
                                                                : -1;
                                 },
                            Items = rieltorsCompanies,
                            Position = 18,
                        },
                 };
		}

		/// <summary>
		/// Возвращает список полей для пустой страницы
		/// </summary>
		/// <returns></returns>
		public static FieldsList EmptyPageList()
		{
			return new FieldsList();
		}

		/// <summary>
		/// Формируем список полей для страницы технических реквизитов объектов
		/// </summary>
		/// <param name="currentEstateObject"></param>
		/// <returns></returns>
		public static FieldsList TechPageList(EstateObject currentEstateObject)
		{
			// Репозитории
			var dictRep = Locator.GetService<IDictionariesRepository>();
			return new FieldsList()
                {
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("security"))
                        {
                            Caption = "Безопасность",
                            Name = "security",
                            Tooltip = "Безопасность, имеющаяся у объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Security;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Security = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                },
                             Position = 240,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Высота потолков",
                            Name = "ceiling-height",
                            Tooltip = "Высота потолков в объекте",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.CelingHeight.FormatString();
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.CelingHeight = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                },
                             Position = 360,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Высота чердака",
                            Name = "attic-height",
                            Tooltip = "Высота чердака у объекта",
                            Required = "selling/lising",
                            NotRequired = "house",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.AtticHeight.FormatString();
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.AtticHeight = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                },
                             Position = 162,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Жилая / полезная площадь",
                            Name = "living-area",
                            Tooltip = "Жилая площадь - площадь жилых помещений в жилых объектах. Полезная - полезная площадь в нежилых объектах",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.ActualUsableFloorArea.FormatString();
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.ActualUsableFloorArea = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                },
							CustomRequired = (obj) =>
							{
								 return obj.ObjectMainProperties.NewBuilding != true;
							},
                            Position = 120,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Заглубление первого этажа",
                            Name = "first-floor-downset",
                            Tooltip = "Высота заглубления пола первого этажа, относительно уровня земли.",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.FirstFloorDownSet.FormatString();
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.FirstFloorDownSet = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent)
                                },
                             Position = 25,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Расшифровка метража",
                            Name = "metrage-description",
                            Tooltip = "Детальная информация по метражу у объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.MetricDescription;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.MetricDescription = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),  
                                },
                             Position = 350,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("unit_usage"))
                        {
                            Caption = "Использование объекта",
                            Name = "unit-usage",
                            Tooltip = "Текущее использование объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.ObjectUsage;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.ObjectUsage = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent)
                                },
                             Position = 173,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Использование под нежилое",
                            Name = "non-living-usage",
                            Tooltip = "Возможность использования объекта под нежилое",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.NonResidenceUsage;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.NonResidenceUsage = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                },
                             Position = 250,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("building_class"))
                        {
                            Caption = "Класс здания",
                            Name = "building-class",
                            Tooltip = "Класс здания согласно классификатору коммерческой надежности",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.BuildingClass;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.BuildingClass = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                },
                             Position = 171,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Количество окон",
                            Name = "windows-count",
                            Tooltip = "Количество окон, имеющихся у объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.WindowsCount;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.WindowsCount = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 300,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Количество семей",
                            Name = "famalies-count",
                            Tooltip = "Количество семей, проживающих в объекте",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.FamiliesCount;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.FamiliesCount = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),  
                                },
                             Position = 157,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Количество телефонных линий",
                            Name = "phone-lines-count",
                            Tooltip = "Количество телефонных линий, которые проведены в данный объект",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.PhoneLinesCount;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.PhoneLinesCount = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                },
                             Position = 176,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Количество уровней",
                            Name = "levels-count",
                            Tooltip = "Количество уровней, которые имеет объект",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.LevelsCount;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.LevelsCount = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                },
                             Position = 100,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Количество фасадных окон",
                            Name = "facade-windows-count",
                            Tooltip = "Количество окон, которые расположены на фасадной части объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.FacadeWindowsCount;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.FacadeWindowsCount = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 310,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("floor_material"))
                        {
                            Caption = "Материал перекрытий",
                            Name = "floor-material",
                            Tooltip = "Материал перекрытий объекта",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.FloorMaterial;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.FloorMaterial = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 30,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("construction_material"))
                        {
                            Caption = "Материал постройки",
                            Name = "building-material",
                            Tooltip = "Материалы, использованные при постройке объекта",
                            Required = "house",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.BuildingMaterial;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.BuildingMaterial = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 152,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("unit_function"))
                        {
                            Caption = "Назначение объекта",
                            Name = "object-function",
                            Tooltip = "Назначение, по которому объект может использоваться",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.ObjectAssignment;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.ObjectAssignment = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent) 
                                },
                             Position = 172
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Новострой",
                            Name = "new-builded-object",
                            Tooltip = "Объект, с недавним сроком постройки",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.NewBuilding;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.NewBuilding = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),   
                                },
                             Position = 200,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Общая площадь",
                            Name = "total-area",
                            Tooltip = "Общая площадь объекта недвижимости",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.TotalArea;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.TotalArea = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
							 
							 CustomRequired = (obj) =>
							 {
								 return obj.ObjectMainProperties.NewBuilding != true;
							 },
                             Position = 110,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("development_period"))
                        {
                            Caption = "Период застройки",
                            Name = "development-period",
                            Tooltip = "Период, в течении которой был построен данный объект недвижимости",
                            Required = "selling/lising",
                            NotRequired = "flat",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.BuildingPeriod;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.BuildingPeriod = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent)       
                                },
                             Position = 151,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Площадь зала",
                            Name = "bigroom-area",
                            Tooltip = "Общая площадь самой большой комнаты - зала",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.BigRoomFloorArea;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.BigRoomFloorArea = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),   
                                },
                             Position = 340,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Площадь застройки",
                            Name = "building-floor",
                            Tooltip = "Общая площадь застройки данного объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.BuildingFloor;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.BuildingFloor = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),    
                                },
                             Position = 161,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Площадь кухни",
                            Name = "kitchen-floor",
                            Tooltip = "Площадь кухни в квадратных метрах",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.KitchenFloorArea;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.KitchenFloorArea = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 330,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Площадь участка, кв.м.",
                            Name = "land-floor",
                            Tooltip = "Площадь участка согласно документам в квадратных метрах",
                            //Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.LandArea;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.LandArea = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 158,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Фактическая площадь участка",
                            Name = "land-factical-floor",
                            Tooltip = "Фактическая площадь участка фактическая, в квадратных метрах",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.LandFloorFactical;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.LandFloorFactical = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 163,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Полное описание",
                            Name = "full-description",
                            Tooltip = "Полный вариант описание объекта",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.FullDescription;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.FullDescription = value;
                                },
                            Validator = new FieldValidator()
                                {
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                },
                             Position = 150,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("entrance"))
                        {
                            Caption = "Расположение входа",
                            Name = "entrance",
                            Tooltip = "Расположение входа в данный объект",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.EntranceToObject;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.EntranceToObject = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),   
                                },
                             Position = 175,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("land_relief"))
                        {
                            Caption = "Рельеф участка",
                            Name = "land-relief",
                            Tooltip = "Реальеф на данном участке",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Relief;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Relief = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 159,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Строительная готовность",
                            Name = "ready-percent",
                            Tooltip = "Процент, показывающий насколько строительство объекта завершено",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.BuildingReadyPercent;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.BuildingReadyPercent = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 190,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("house_type"))
                        {
                            Caption = "Тип дома",
                            Name = "house-type",
                            Tooltip = "Тип дома, выбираемый из классификатора",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.HouseType;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.HouseType = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent)   
                                },
                             Position = 171,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("flat_type"))
                        {
                            Caption = "Тип квартиры",
                            Name = "flat-type",
                            Tooltip = "Тип квартиры, выбираемый из классификатора",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.FlatType;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.FlatType = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent)
                                },
                             Position = 10,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("type_of_structure"))
                        {
                            Caption = "Тип строения",
                            Name = "building-type",
                            Tooltip = "Тип строения, выбираемый из классификатора",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.BuildingType;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.BuildingType = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 153
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Жилой фонд",
                            Name = "living-found",
                            Tooltip = "Принадлежность объекта к жилому фонду",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.HousingStock;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.HousingStock = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent) 
                                },
                             Position = 174,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("foundation"))
                        {
                            Caption = "Фундамент",
                            Name = "foundation",
                            Tooltip = "Материал фундамента у объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Foundation;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Foundation = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),  
                                },
                             Position = 154,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Электрическая мощность",
                            Name = "electric-power",
                            Tooltip = "Выделенная для объекта электрическая мощность",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.ElectricPower;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.ElectricPower = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 155,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Этаж",
                            Name = "number-floor",
                            Tooltip = "Этаж, на котором расположен данный объект",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.FloorNumber;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.FloorNumber = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),    
                                },
                             Position = 70,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Этажность здания",
                            Name = "total-floors",
                            Tooltip = "Количество этажей в здании объекта или этажность самого объекта",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.TotalFloors;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.TotalFloors = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),    
                                },
                             Position = 80,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("window_view"))
                        {
                            Caption = "Вид из окон",
                            Name = "window-view",
                            Tooltip = "Вид доступный из окон объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.ViewFromWindows;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.ViewFromWindows = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                },
                             Position = 420,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Год постройки",
                            Name = "building-year",
                            Tooltip = "Год сдачи дома в эксплуатацию",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.BuildingYear;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.BuildingYear = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                },
                             Position = 20,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("constructor"))
                        {
                            Caption = "Застройщик",
                            Name = "constructor",
                            Tooltip = "Застройщик объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.Builder;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.Builder = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                },
                             Position = 180,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Количество балконов",
                            Name = "balcony-count",
                            Tooltip = "Количество балконов, которые имеет данный объект",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.BalconiesCount;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.BalconiesCount = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                },
                             Position = 270,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Количество комнат",
                            Name = "rooms-count",
                            Tooltip = "Количество комнат у объекта",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.RoomsCount;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.RoomsCount = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                },
                             Position = 90,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Количество комнат в квартире",
                            Name = "rooms-flat-count",
                            Tooltip = "Количество продаваемых комнат в квартире",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.FlatRoomsCount;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.FlatRoomsCount = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                },
                             Position = 241,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Количество лоджий",
                            Name = "loggias-count",
                            Tooltip = "Количество лоджий у объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.LoggiasCount;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.LoggiasCount = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                },
                             Position = 280,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Количество спален",
                            Name = "bedrooms-count",
                            Tooltip = "Количество спален у объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.BedroomsCount;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.BedroomsCount = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 260,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Количество эркеров",
                            Name = "erkers-count",
                            Tooltip = "Количество эркеров у объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.ErkersCount;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.ErkersCount = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),  
                                },
                             Position = 290,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("roof"))
                        {
                            Caption = "Кровля",
                            Name = "roof",
                            Tooltip = "Тип кровли",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.Roof;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.Roof = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 220,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("plan_type"))
                        {
                            Caption = "Планировка комнат",
                            Name = "plan-type",
                            Tooltip = "Планировка комнат",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.RoomPlanning;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.RoomPlanning = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
							 CustomRequired = (obj) =>
							 {
								 return obj.ObjectMainProperties.NewBuilding != true;
							 },
                             Position = 130,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("basement"))
                        {
                            Caption = "Подвал",
                            Name = "basement",
                            Tooltip = "Подвал объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.Basement;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.Basement = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 230,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("flat_arrangement"))
                        {
                            Caption = "Расположение квартиры",
                            Name = "flat-arrangement",
                            Tooltip = "Расположение квартиры относительно части дома",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.FlatLocation;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.FlatLocation = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),   
                                },
                            Position = 370,
                            DefaultValue = 127
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("window_arrangement"))
                        {
                            Caption = "Расположение окон",
                            Name = "window-arrangement",
                            Tooltip = "Расположение окон объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.WindowsLocation;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.WindowsLocation = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 410,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("windows"))
                        {
                            Caption = "Окна",
                            Name = "windows",
                            Tooltip = "Материалы окон",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.WindowsLocation;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.WindowsLocation = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 430,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Перепланировка",
                            Name = "replanning",
                            Tooltip = "В объекте была проведена перепланировка",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.Redesign;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.Redesign = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),  
                                },
                             Position = 380,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Законность перепланировки",
                            Name = "replanning-legality",
                            Tooltip = "Указывает законность перепланировки",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.RedesignLegality;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.RedesignLegality = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying)
                                },
                             Position = 56,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("land_form"))
                        {
                            Caption = "Форма участка",
                            Name = "land-form",
                            Tooltip = "Указывается форма участка (правильная - прямоугольная)",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.PlotForm;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.PlotForm = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 164,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("water"))
                        {
                            Caption = "Водоснабжение",
                            Name = "water",
                            Tooltip = "Указывается существующее холодное и горячее водоснабжение объекта",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectCommunications.Water;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectCommunications.Water = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                },
                             Position = 50,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("gas"))
                        {
                            Caption = "Газ",
                            Name = "gas",
                            Tooltip = "Газоснабженеие объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectCommunications.Gas;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectCommunications.Gas = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                },
                             CustomCondition = (context) =>
                                                   {
                                                       return false; // Не рендерим
                                                   }
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("sewer"))
                        {
                            Caption = "Канализация",
                            Name = "sewer",
                            Tooltip = "Канализация, присутствующая у объекта",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectCommunications.Sewer;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectCommunications.Sewer = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                },
                             Position = 60,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("heating"))
                        {
                            Caption = "Отопление",
                            Name = "heating",
                            Tooltip = "Указываются существующие системы отопления объекта",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectCommunications.Heating;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectCommunications.Heating = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                   new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 40,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("phones"))
                        {
                            Caption = "Телефон",
                            Name = "phones",
                            Tooltip = "Виды телефонной связи, присутствующие у объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectCommunications.Phone;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectCommunications.Phone = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 560,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("tubes"))
                        {
                            Caption = "Трубы",
                            Name = "tubes",
                            Tooltip = "Тип труб отопления и канализации",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectCommunications.Tubes;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectCommunications.Tubes = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 550,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("electricity"))
                        {
                            Caption = "Электричество",
                            Name = "electricity",
                            Tooltip = "Сведения об электроснабжении объекта",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectCommunications.Electricy;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectCommunications.Electricy = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 21,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("balcony"))
                        {
                            Caption = "Балкон / Лоджия",
                            Name = "balcony",
                            Tooltip = "Остекление и решетки на балконах и лоджиях",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.Balcony;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.Balcony = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                },
                             Position = 320,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("entrance_door"))
                        {
                            Caption = "Входная дверь",
                            Name = "entrance-door",
                            Tooltip = "Входная дверь объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.EntranceDoor;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.EntranceDoor = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent)
                                },
                             Position = 450,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("kitchen"))
                        {
                            Caption = "Кухня",
                            Name = "kitchen",
                            Tooltip = "Тип кухни объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.Kitchen;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.Kitchen = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 500,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Описание кухни",
                            Name = "kitchen-description",
                            Tooltip = "Описание кухни, например пол - кафель, потолок - натяжной",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.KitchenDescription;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.KitchenDescription = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(4000)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 510,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("stairs"))
                        {
                            Caption = "Лестница",
                            Name = "ladder",
                            Tooltip = "Материал и вид лестницы объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.Ladder;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.Ladder = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 156,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("unit_state"))
                        {
                            Caption = "Общее состояние",
                            Name = "common-state",
                            Tooltip = "Общее состояние объекта",
                            Required = "always",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.CommonState;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.CommonState = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 140,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Окна / состояние",
                            Name = "windows-description",
                            Tooltip = "Развернутое описание окон объекта. Например \"материал - массив кедра\" или \"профиль - KBE\"",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.WindowsDescription;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.WindowsDescription = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(4000)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 440,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Подсобные помещения",
                            Name = "maintenance-rooms",
                            Tooltip = "Описание подсобных помещений объекта  (постирочные, кладовки, ниши и т.д.)",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.UtilityRooms;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.UtilityRooms = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(4000)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 400,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("floor"))
                        {
                            Caption = "Пол",
                            Name = "floor-rating",
                            Tooltip = "Вид пола в помещении (комнатах) объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.Floor;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.Floor = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 470,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("ceiling"))
                        {
                            Caption = "Потолок",
                            Name = "ceiling-rating",
                            Tooltip = "Вид потолка в помещения (комнатах) объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.Ceiling;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.Ceiling = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 480,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("santeh"))
                        {
                            Caption = "Сантехника",
                            Name = "sanfurniture-rating",
                            Tooltip = "Описание сантехники объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectCommunications.SanFurniture;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectCommunications.SanFurniture = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 540,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("sanfurniture"))
                        {
                            Caption = "Санузел",
                            Name = "wc-rating",
                            Tooltip = "Описание санузла объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.WC;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.WC = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 520,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Санузел / описание",
                            Name = "wc-rooms",
                            Tooltip = "Развернутое описание санузла (материал стен, пола, потолка и т.д.)",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.WCDescription;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.WCDescription = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(4000)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 530,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Оценка состояния объекта",
                            Name = "object-rating",
                            Tooltip = "Оценка состояния объекта по пятибальной шкале",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.Rating;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.Rating = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 390,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("walls"))
                        {
                            Caption = "Стены",
                            Name = "walls-rating",
                            Tooltip = "Вид стен в помещениях (комнатах) объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.Walls;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.Walls = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 490,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("carpentry"))
                        {
                            Caption = "Столярка / двери",
                            Name = "doors-rating",
                            Tooltip = "Описание состояния межкомнатных дверей объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.Carpentry;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.Carpentry = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),    
                                },
                             Position = 460,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("vestibule"))
                        {
                            Caption = "Тамбур",
                            Name = "tambur-rating",
                            Tooltip = "Вид тамбура объекта (тамбур - общее пространство для нескольких квартир на лестничной площадке)",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.Vestibule;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.Vestibule = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent)    
                                },
                             Position = 570,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("in_building_placement"))
                        {
                            Caption = "Расположение в объекте",
                            Name = "in-building-rating",
                            Tooltip = "В какой именно части здания находится объект",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.Placement;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.Placement = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent)    
                                },
                             Position = 580,
                        },
                };
		}

		/// <summary>
		/// Формируем список полей для страницы юридических реквизитов объекта
		/// </summary>
		/// <param name="currentEstateObject">Объект недвижимости, для которого генерируется форма</param>
		/// <returns>Список полей</returns>
		public static FieldsList LegalPageList(EstateObject currentEstateObject)
		{
			// Репозитории
			var dictRep = Locator.GetService<IDictionariesRepository>();
			return new FieldsList()
                {
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("property_types"))
                        {
                            Caption = "Вид собственности на участок",
                            Name = "land-property-type",
                            Tooltip = "Вид собственности на ЗУ в составе объекта",
                            //Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.PropertyType;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.PropertyType = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                },
                             Position = 20,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("property_types"))
                        {
                            Caption = "Вид собственности",
                            Name = "property-type",
                            Tooltip = "Вид собственности на объект",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.PropertyType;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.PropertyType = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                },
                             Position = 30,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Возможность прописки",
                            Name = "registration-possibility",
                            Tooltip = "Возможность прописки на объекте",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.RegistrationPosibility;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.RegistrationPosibility = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying)
                                },
                             Position = 40,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("owner_part"))
                        {
                            Caption = "Доля собственника",
                            Name = "owner-part",
                            Tooltip = "Доля собственника в праве на объект",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.OwnerPart;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.OwnerPart = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                },
                             Position = 50,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Возможность ипотеки",
                            Name = "mortage-possibility",
                            Tooltip = "Флаг, указывающий что возможна покупка данного объекта недвижимости по ипотечному кредиту",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.MortgagePossibility;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.MortgagePossibility = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                },
                             Position = 60,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("banks"))
                        {
                            Caption = "Ипотека банка",
                            Name = "mortage-bank",
                            Tooltip = "Банк, кредитующий (по ипотеке) покупателя",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.MortgageBank;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.MortgageBank = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                },
                            CustomCondition = (context) =>
                                {
                                    return context.EstateObject.Status == (short) EstateStatuses.Deal;
                                },
                             Position = 70,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Количество зарегистрированных",
                            Name = "registred-count",
                            Tooltip = "Количество зарегистрированных по адресу объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.PrescriptionsCount;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.PrescriptionsCount = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                },
                             Position = 80,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Количество собственников",
                            Name = "owners-count",
                            Tooltip = "Количество собственников объекта",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.OwnersCount;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.OwnersCount = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                },
                             Position = 10,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("land_assigment"))
                        {
                            Caption = "Назначение земли",
                            Name = "land-assigment",
                            Tooltip = "Назначение ЗУ (по свидетельству)",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.LandAssignment;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.LandAssignment = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                },
                             Position = 90,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("burdens"))
                        {
                            Caption = "Наличие отягощений",
                            Name = "burdens-avaliability",
                            Tooltip = "Юридические отягощения на объект",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.Burdens;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.Burdens = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                },
                             Position = 100,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Правоустанавливающие документы",
                            Name = "legal-documents",
                            Tooltip = "Документы, на основании которых возникло право собственности (договор дарения, договор купли-продажи, завещание, приватизация и т.д.)",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Documents;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Documents = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(4000)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                },
                             Position = 110,
                        },
                     new ObjectFormTextField()
                         {
                             Caption = "Срок аренды",
                             Name = "rent-date",
                             MaxLenght = 255,
                             Readonly = false,
                             CustomClasses = "datepicker",
                             Tooltip = "Дата окончания аренды данного объекта недвижимости",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.ObjectAdditionalProperties.RentDate.FormatDate();
                                 },
                             SetValueToObject = (estateObject,value) =>
                                 {
                                     estateObject.ObjectAdditionalProperties.RentDate = value.ConvertToNullableDateTime();
                                 },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                },
                             Position = 120,
                         },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Законность пристроек",
                            Name = "additional-buildings-legality",
                            Tooltip = "Флаг, указывающий что пристройки к данному объекту недвижимости возведены законно",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.ExtensionsLegality;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.ExtensionsLegality = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                },
                             Position = 130,
                        },

                };
		}

		/// <summary>
		/// Возвращает список полей для формы инфраструктуры
		/// </summary>
		/// <param name="currentEstateObject">Текущий объект недвижимости</param>
		/// <returns></returns>
		public static FieldsList InfrastructurePageList(EstateObject currentEstateObject)
		{
			// Репозитории
			var dictRep = Locator.GetService<IDictionariesRepository>();
			return new FieldsList()
                {
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("court"))
                        {
                            Caption = "Двор",
                            Name = "court",
                            Tooltip = "Сведения о дворе, распологающимся у объекта или рядом с ним",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.Court;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.Court = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                },
                             Position = 2,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("environment"))
                        {
                            Caption = "Окружение",
                            Name = "environment",
                            Tooltip = "Сведения об окружение объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.Environment;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.Environment = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                },
                             Position = 1,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Парковка машин",
                            Name = "has-parking",
                            Tooltip = "Наличие автопарковки",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.HasParking;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.HasParking = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                },
                             Position = 6,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("loading"))
                        {
                            Caption = "Погрузка",
                            Name = "loading",
                            Tooltip = "Возможные варианты погрузки и разгрузки у объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.Loading;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.Loading = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                },
                             Position = 7,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("approach"))
                        {
                            Caption = "Подъезд к объекту",
                            Name = "way-to-object",
                            Tooltip = "Сведения об подъезде к данному объекту",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.EntryLocation;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.EntryLocation = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                },
                             Position = 4,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Подъезд спецтехники",
                            Name = "special-auto-entrance",
                            Tooltip = "Возможность подъезда крупногабаритной спецтехники к объекту (кран, трал и т.д.)",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.AbilityForMachineryEntrance;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.AbilityForMachineryEntrance = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                },
                             Position = 5,
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("fence"))
                        {
                            Caption = "Ограждение",
                            Name = "fence",
                            Tooltip = "Ограждение объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.Fence;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.Fence = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                },
                             Position = 3,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("add_structures"))
                        {
                            Caption = "Дополнительные сооружения",
                            Name = "add-structires",
                            Tooltip = "Дополнительные сооружения, входящие в стоимость объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.AdditionalBuildings;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.AdditionalBuildings = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                },
                             Position = 9,
                        },
                    
                   new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("add_commerce_structures"))
                        {
                            Caption = "Дополнительные коммерческие сооружения",
                            Name = "add_commerce_structures",
                            Tooltip = "Дополнительные коммерческие сооружения, входящие в состав объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.AddCommercialBuildings;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.AddCommercialBuildings = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                },
                             Position = 8,
                        },
                };
		}

		/// <summary>
		/// Возвращает список полей для раздела эксплуатационных свойств объект
		/// </summary>
		/// <param name="currentEstateObject">Текущий объект недвижимости</param>
		/// <returns></returns>
		public static FieldsList ExpluatationPageList(EstateObject currentEstateObject)
		{
			// Репозитории
			var dictRep = Locator.GetService<IDictionariesRepository>();
			return new FieldsList()
                {
                    
                    new ObjectFormTextField()
                        {
                            Caption = "Аренда в день",
                            Name = "rent-per-day",
                            Tooltip = "Стоимость аренды данного объекта недвижимости за один день в рублях",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.RentPerDay.FormatString();
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.RentPerDay = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                },
                             Position = 1,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Аренда в месяц",
                            Name = "rent-per-month",
                            Tooltip = "Стоимость аренды данного объекта недвижимости за один месяц в рублях",
                            Required = "lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.RentPerMonth.FormatString();
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.RentPerMonth = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                },
                             Position = 2,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Коммунальные платежи в счет аренды",
                            Name = "rent-with-services",
                            Tooltip = "Указывает, включает ли стоимость аренды оплату коммунальных платежей по данному объекту",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.RentWithServices;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.RentWithServices = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                },
                             Position = 4,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Платежи сверх аренды",
                            Name = "rent-overpayments",
                            Tooltip = "Описание требуемых платежей сверх аренды",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.RentOverpayment;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.RentOverpayment = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(4000)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                },
                             Position = 5,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("furniture"))
                        {
                            Caption = "Мебель",
                            Name = "furniture",
                            Tooltip = "Мебель, которая присутствует на объекте недвижимости",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.Furniture;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.Furniture = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                },
                             Position = 8,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Освобождение",
                            Name = "release-conditions",
                            Tooltip = "Описание условий освобождения объекта недвижимости",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.ReleaseInfo;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.ReleaseInfo = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(4000)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling)
                                },
                             Position = 13,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Требуется предоплата",
                            Name = "prepay-required",
                            Tooltip = "Указывает, требуется ли вносить предоплату при совершении операций с данным объектом",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Prepayment;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Prepayment = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                },
                             Position = 3,
                        },
                    new ObjectFormMultiDictionaryField(dictRep.GetDictionaryByName("residents"))
                        {
                            Caption = "Проживающие",
                            Name = "residents",
                            Tooltip = "Описание проживающих в данный момент в объекте, либо предпочтения хозяина по проживающим",
                            Required = "lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.LivingPeoples;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.LivingPeoples = value;
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                },
                             Position = 6,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Описание проживающих",
                            Name = "livers-description",
                            Tooltip = "Подробное описание проживающих",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.LivingPeolplesDescription;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.LivingPeolplesDescription = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(4000)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                },
                             Position = 7,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Установлен счетчик газа",
                            Name = "has-gas-meter",
                            Tooltip = "Указывает, установлен ли на объекте счетчик газа",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectCommunications.HasGasMeter;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectCommunications.HasGasMeter = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent)
                                },
                             Position = 9,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Установлен счетчик холодной воды",
                            Name = "has-cold-water-meter",
                            Tooltip = "Указывает, установлен ли на объекте счетчик холодной воды",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectCommunications.HasColdWaterMeter;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectCommunications.HasColdWaterMeter = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent)
                                },
                             Position = 10,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Установлен счетчик горячей воды",
                            Name = "has-heat-water-meter",
                            Tooltip = "Указывает, установлен ли на объекте счетчик горячей воды",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectCommunications.HasHotWaterMeter;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectCommunications.HasHotWaterMeter = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent)
                                },
                             Position = 11,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Установлен электрический счетчик",
                            Name = "has-electricity-meter",
                            Tooltip = "Указывает, установлен ли на объекте счетчик электричества",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectCommunications.HasElectricyMeter;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectCommunications.HasElectricyMeter = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent)
                                },
                             Position = 12,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Проведен Интернет",
                            Name = "has-internet",
                            Tooltip = "Указывает, проведен ли на данный объект Интернет",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectCommunications.HasInternet;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectCommunications.HasInternet = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                },
                             Position = 13,
                        }
                };
		}

		/// <summary>
		/// Возвращает список полей для страницы сервисной информации
		/// </summary>
		/// <param name="currentEstateObject">Текущий отображаемый объект недвижимости</param>
		/// <param name="currentUser">Текущий авторизованный пользователь</param>
		/// <returns></returns>
		public static FieldsList ServicePageList(EstateObject currentEstateObject, User currentUser)
		{
			// Репозитории
			var dictRep = Locator.GetService<IDictionariesRepository>();
			var clientsRep = Locator.GetService<IClientsRepository>();

            var prevContactPersons =
                Locator.GetService<IUsersRepository>().GetActiveUsers().Where(
                    c => c.CompanyId == currentEstateObject.User.CompanyId).OrderBy(c => c.LastName).ToDictionary
                    (c => c.Id.ToString(), c => new 
                    {
                        Name = c.LastName + " " + c.FirstName,
                        Phone = c.Company != null ? c.Company.Phone1 : String.Empty,
                        Phone2 = c.Company != null ? c.Company.Phone2 : String.Empty,
                        Phone3 = c.Company != null ? c.Company.Phone3 : String.Empty,
                        Phone4 = c.Phone,
                        Phone5 = c.Phone2
                    });

            var prevContactPersonsObj =
                Locator.GetService<IUsersRepository>().GetActiveUsers()
                    .Where(c => c.CompanyId == currentEstateObject.User.CompanyId)
                    .Select(c => new
                    {
                        Name = c.LastName + " " + c.FirstName,
                        AgentId = c.Id,
                        CompanyId = c.CompanyId,
                        Phone = c.Company != null ? c.Company.Phone1 : String.Empty,
                        Phone2 = c.Company != null ? c.Company.Phone2 : String.Empty,
                        Phone3 = c.Company != null ? c.Company.Phone3 : String.Empty,
                        Phone4 = c.Phone,
                        Phone5 = c.Phone2
                    })
                    .ToList()
                    .OrderBy(c => c.CompanyId)
                    .ThenBy(c => c.Name)
                    .ToList();

            Dictionary<String, String> contactPersons = new Dictionary<String, String>();
            foreach (var item in prevContactPersons)
            {
                String val = item.Value.Name + (!String.IsNullOrEmpty(item.Value.Phone) ? "|es|" : String.Empty);
                val += item.Value.Phone + (!String.IsNullOrEmpty(item.Value.Phone2) ? "|es|" : String.Empty);
                val += item.Value.Phone2 + (!String.IsNullOrEmpty(item.Value.Phone3) ? "|es|" : String.Empty);
                val += item.Value.Phone3;

                contactPersons.Add(item.Key, val);
            }

            Dictionary<String, String> contactPhones = new Dictionary<String, String>();
            for (var i = 0; i < prevContactPersonsObj.Count; i++)
            {
                String key = String.Empty;
                String val = String.Empty;
                String newKey = String.Empty;

                if (i != 0 && prevContactPersonsObj[i - 1].Phone != prevContactPersonsObj[i].Phone)
                {
                    newKey = "company_general_" + prevContactPersonsObj[i - 1].CompanyId.ToString() + "_" + prevContactPersonsObj[i - 1].Phone;
                    if (!String.IsNullOrEmpty(prevContactPersonsObj[i - 1].Phone) && !contactPhones.Keys.Contains(newKey))
                    {
                        key = newKey;
                        val = "Основной: " + prevContactPersonsObj[i - 1].Phone;
                        contactPhones.Add(key, val);
                    }

                    newKey = "company_additional1_" + prevContactPersonsObj[i - 1].CompanyId.ToString() + "_" + prevContactPersonsObj[i - 1].Phone2;
                    if (!String.IsNullOrEmpty(prevContactPersonsObj[i - 1].Phone2) && !contactPhones.Keys.Contains(newKey))
                    {
                        key = newKey;
                        val = "Дополнительный №1: " + prevContactPersonsObj[i - 1].Phone2;
                        contactPhones.Add(key, val);
                    }

                    newKey = "company_additional2_" + prevContactPersonsObj[i - 1].CompanyId.ToString() + "_" + prevContactPersonsObj[i - 1].Phone3;
                    if (!String.IsNullOrEmpty(prevContactPersonsObj[i - 1].Phone3) && !contactPhones.Keys.Contains(newKey))
                    {
                        key = newKey;
                        val = "Дополнительный №2: " + prevContactPersonsObj[i - 1].Phone3;
                        contactPhones.Add(key, val);
                    }
                }

                newKey = "agent_phone1_" + prevContactPersonsObj[i].AgentId.ToString() + "_" + prevContactPersonsObj[i].Phone4;
                if (!String.IsNullOrEmpty(prevContactPersonsObj[i].Phone4) && !contactPhones.Keys.Contains(newKey))
                {
                    key = newKey;
                    val = prevContactPersonsObj[i].Name + " " + prevContactPersonsObj[i].Phone4;
                    contactPhones.Add(key, val);
                }

                newKey = "agent_phone2_" + prevContactPersonsObj[i].AgentId.ToString() + "_" + prevContactPersonsObj[i].Phone5;
                if (!String.IsNullOrEmpty(prevContactPersonsObj[i].Phone5) && !contactPhones.Keys.Contains(newKey))
                {
                    key = newKey;
                    val = prevContactPersonsObj[i].Name + " " + prevContactPersonsObj[i].Phone5;
                    contactPhones.Add(key, val);
                }

                if (i != 0 && i == prevContactPersonsObj.Count - 1)
                {
                    newKey = "company_general_" + prevContactPersonsObj[i - 1].CompanyId.ToString() + "_" + prevContactPersonsObj[i - 1].Phone;
                    if (!String.IsNullOrEmpty(prevContactPersonsObj[i - 1].Phone) && !contactPhones.Keys.Contains(newKey))
                    {
                        key = newKey;
                        val = "Основной: " + prevContactPersonsObj[i - 1].Phone;
                        contactPhones.Add(key, val);
                    }

                    newKey = "company_additional1_" + prevContactPersonsObj[i - 1].CompanyId.ToString() + "_" + prevContactPersonsObj[i - 1].Phone2;
                    if (!String.IsNullOrEmpty(prevContactPersonsObj[i - 1].Phone2) && !contactPhones.Keys.Contains(newKey))
                    {
                        key = newKey;
                        val = "Дополнительный №1: " + prevContactPersonsObj[i - 1].Phone2;
                        contactPhones.Add(key, val);
                    }

                    newKey = "company_additional2_" + prevContactPersonsObj[i - 1].CompanyId.ToString() + "_" + prevContactPersonsObj[i - 1].Phone3;
                    if (!String.IsNullOrEmpty(prevContactPersonsObj[i - 1].Phone3) && !contactPhones.Keys.Contains(newKey))
                    {
                        key = newKey;
                        val = "Дополнительный №2: " + prevContactPersonsObj[i - 1].Phone3;
                        contactPhones.Add(key, val);
                    }
                }
            }
            
			return new FieldsList()
                {
                    new ObjectFormClientField()
                        {
                            Caption  = "Клиент",
                            Name = "client",
                            Position = 0,
                            Tooltip = "Клиент, связанный с данным объектом",
                            Placeholder = "Начните печатать имя клиента",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ClientId;
                                },
                            SetValueToObject = (estateObject,value) =>
                                {
                                    estateObject.ClientId = Convert.ToInt64(value);
                                },
                        },
                    new ObjectFormDictionaryField(dictRep.GetDictionaryByName("currency"))
                        {
                            Caption = "Валюта",
                            Name = "currency",
                            Tooltip = "Валюта, в которой ведутся операции с текущим объектом недвижимости",
                            DefaultValue = "251",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Currency;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Currency = value.ConvertToNullableLong();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                },
                             Position = 10,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Возможен торг",
                            Name = "trade-possibility",
                            Tooltip = "Указывает, возможно ли торговаться по данному объекту",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.Auction;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.Auction = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                },
                             Position = 26,
                        },
                     new ObjectFormTextField()
                         {
                             Caption = "Дата осмотра",
                             Name = "date-viewed",
                             MaxLenght = 255,
                             CustomClasses = "datepicker",
                             Tooltip = "Дата, когда объект был осмотрен",
                            Required = "selling/lising",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.ObjectChangementProperties.ViewDate.FormatDate();
                                 },
                             SetValueToObject = (estateObject,value) =>
                                 {
                                     estateObject.ObjectChangementProperties.ViewDate =
                                         value.ConvertToNullableDateTime();
                                 },
                             RequiredContext = new List<ObjectContextRequirement>()
                                 {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                 },
                             Position = 3,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Наименование объекта",
                             Name = "title",
                             MaxLenght = 255,
                             Tooltip = "Заголовок - наименование объекта, например ЗАМОК У МОРЯ",
                            Required = "selling/lising",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return estateObject.ObjectMainProperties.Title;
                                 },
                             SetValueToObject = (estateObject,value) =>
                                 {
                                     estateObject.ObjectMainProperties.Title = value;
                                 },
                             RequiredContext = new List<ObjectContextRequirement>()
                                 {
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),  
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),  
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),  
                                 },
                             Position = 4,
                         },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Краткое описание",
                            Name = "short-description",
                            Tooltip = "Краткий вариант описания объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.ShortDescription;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.ShortDescription = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(4000)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                },
                             Position = 15,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Примечания",
                            Name = "notes",
                            Tooltip = "Примечание по объекту",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Notes;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Notes = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(4000)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    
                                },
                             Position = 16,
                        },
                     new ObjectFormTextField()
                         {
                             Caption = "Тип объекта",
                             Name = "object-type",
                             Readonly = true,
                             Tooltip = "Тип объекта недвижимости, над которым выполняет операция",
                             Required = "selling/lising",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return ((EstateTypes)estateObject.ObjectType).GetEnumMemberName();
                                 },
                             SetValueToObject = (estateObject,value) =>
                                 {
                                     //estateObject.ObjectType = Convert.ToInt16(value);
                                 },
                             Position = 1,
                         },
                     new ObjectFormTextField()
                         {
                             Caption = "Операция",
                             Name = "object-operation",
                             Readonly = true,
                             Tooltip = "Операция, выполняемая над текущим объектом",
                             Required = "selling/lising",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     return ((EstateOperations)estateObject.Operation).GetEnumMemberName();
                                 },
                             SetValueToObject = (estateObject,value) =>
                                 {
                                     //estateObject.Operation = Convert.ToInt16(value);
                                 },
                             Position = 2,
                         },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Рекламный текст 1",
                            Name = "advertising-text-1",
                            Tooltip = "Текст рекламного объявления, предназначенный для автоматической публикации информации по данному объекту в газетах",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Advertising1;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Advertising1 = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(255)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                                  {
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                                  },
                             Position = 17,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Рекламный текст 2",
                            Name = "advertising-text-2",
                            Tooltip = "Текст рекламного объявления, предназначенный для автоматической публикации информации по данному объекту в газетах",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Advertising1;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Advertising1 = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(255)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                                  {
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                                  },
                             Position = 18,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Рекламный текст 3",
                            Name = "advertising-text-3",
                            Tooltip = "Текст рекламного объявления, предназначенный для автоматической публикации информации по данному объекту в газетах",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Advertising1;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Advertising1 = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(255)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                                  {
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                                  },
                             Position = 19,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Рекламный текст 4",
                            Name = "advertising-text-4",
                            Tooltip = "Текст рекламного объявления, предназначенный для автоматической публикации информации по данному объекту в газетах",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Advertising1;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Advertising1 = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(255)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                                  {
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                                  },
                             Position = 20,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Рекламный текст 5",
                            Name = "advertising-text-5",
                            Tooltip = "Текст рекламного объявления, предназначенный для автоматической публикации информации по данному объекту в газетах",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Advertising1;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Advertising1 = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(255)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                                  {
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                                  },
                             Position = 21,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Специальное предложение",
                            Name = "special-offer",
                            Tooltip = "Указывает, предусмотрено ли специальное предложение или акция, связанная с данным объектом",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.SpecialOffer;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.SpecialOffer = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                                  {
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                                  },
                             Position = 14,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Текст специального предложения",
                            Name = "special-offer-text",
                            Tooltip = "Текст специального предложения",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.SpecialOfferDescription;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.SpecialOfferDescription = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(255)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                                  {
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                                  },
                             Position = 14,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Указывать номер агентства",
                            Name = "set-number-agency",
                            Tooltip = "Указывать номер агентства",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.IsSetNumberAgency;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.IsSetNumberAgency = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                                  {
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                                  },
                             Position = 15,
                        },
                     //new ObjectFormDropDownField()
                     //    {
                     //        Caption = "Контактное лицо",
                     //        Name = "contact-person",
                     //        Readonly = false,
                     //        Tooltip = "Контактное лицо",
                     //        GetValueFromObject = (estateObject) =>
                     //            {
                     //                if (estateObject.ObjectMainProperties.ContactPersonId.HasValue)
                     //                {
                     //                    return estateObject.ObjectMainProperties.ContactPersonId.Value;
                     //                } else
                     //                {
                     //                    return "-1";
                     //                }
                     //            },
                     //        SetValueToObject = (estateObject,value) =>
                     //            {
                     //                estateObject.ObjectMainProperties.ContactPersonId = value != "-1" && value != ""
                     //                                                                          ? (Int64?)Convert.ToInt64(value)
                     //                                                                          : null;
                     //            },
                     //        Items = contactPersons,
                     //        Position = 15,
                     //    },
                    new ObjectFormDropDownField()
                         {
                             Caption = "Контактное лицо",
                             Name = "contact-person",
                             Readonly = false,
                             Tooltip = "Контактное лицо",
                             GetValueFromObject = (estateObject) =>
                                 {
                                     String returnKey = String.Empty;
                                     if (estateObject.ObjectMainProperties.ContactPersonId.HasValue)
                                     {
                                         var user = Locator.GetService<IUsersRepository>().Find(x => x.Id == estateObject.ObjectMainProperties.ContactPersonId.Value);

                                         if (estateObject.ObjectMainProperties.ContactPhone.HasValue && estateObject.ObjectMainProperties.ContactPhone.Value == (Int16)TypeContactPhone.AgentPhone1)
                                         {
                                             returnKey = "agent_phone1_" + estateObject.ObjectMainProperties.ContactPersonId.Value.ToString() + "_" + user.Phone;
                                         }
                                         else if (estateObject.ObjectMainProperties.ContactPhone.HasValue && estateObject.ObjectMainProperties.ContactPhone.Value == (Int16)TypeContactPhone.AgentPhone2)
                                         {
                                             returnKey = "agent_phone2_" + estateObject.ObjectMainProperties.ContactPersonId.Value.ToString() + "_" + user.Phone2;
                                         }
                                     }
                                     else if (estateObject.ObjectMainProperties.ContactCompanyId.HasValue)
                                     {
                                         var company = Locator.GetService<ICompaniesRepository>().Find(x => x.Id == estateObject.ObjectMainProperties.ContactCompanyId.Value);

                                         if (estateObject.ObjectMainProperties.ContactPhone.HasValue && estateObject.ObjectMainProperties.ContactPhone.Value == (Int16)TypeContactPhone.CompanyPhone1)
                                         {
                                             returnKey = "company_general_" + estateObject.ObjectMainProperties.ContactCompanyId.Value.ToString() + "_" + company.Phone1;
                                         }
                                         else if (estateObject.ObjectMainProperties.ContactPhone.HasValue && estateObject.ObjectMainProperties.ContactPhone.Value == (Int16)TypeContactPhone.CompanyPhone2)
                                         {
                                             returnKey = "company_additional1_" + estateObject.ObjectMainProperties.ContactCompanyId.Value.ToString() + "_" + company.Phone2;
                                         }
                                         else if (estateObject.ObjectMainProperties.ContactPhone.HasValue && estateObject.ObjectMainProperties.ContactPhone.Value == (Int16)TypeContactPhone.CompanyPhone3)
                                         {
                                             returnKey = "company_additional2_" + estateObject.ObjectMainProperties.ContactCompanyId.Value.ToString() + "_" + company.Phone3;
                                         }
                                     }

                                     return returnKey;
                                 },
                             SetValueToObject = (estateObject, value) =>
                                 {
                                     if (!String.IsNullOrEmpty(value))
                                     {
                                         var values = ((String)value).Split('_');
                                         if (values[0] == "agent" && values[1] == "phone1")
                                         {
                                             estateObject.ObjectMainProperties.ContactPhone = (Int16?)TypeContactPhone.AgentPhone1;
                                             estateObject.ObjectMainProperties.ContactPersonId = Convert.ToInt64(values[2]);
                                             estateObject.ObjectMainProperties.ContactCompanyId = null;
                                         }
                                         else if (values[0] == "agent" && values[1] == "phone2")
                                         {
                                             estateObject.ObjectMainProperties.ContactPhone = (Int16?)TypeContactPhone.AgentPhone2;
                                             estateObject.ObjectMainProperties.ContactPersonId = Convert.ToInt64(values[2]);
                                             estateObject.ObjectMainProperties.ContactCompanyId = null;
                                         }
                                         else if (values[0] == "company" && values[1] == "general")
                                         {
                                             estateObject.ObjectMainProperties.ContactPhone = (Int16?)TypeContactPhone.CompanyPhone1;
                                             estateObject.ObjectMainProperties.ContactCompanyId = Convert.ToInt64(values[2]);
                                             estateObject.ObjectMainProperties.ContactPersonId = null;
                                         }
                                         else if (values[0] == "company" && values[1] == "additional1")
                                         {
                                             estateObject.ObjectMainProperties.ContactPhone = (Int16?)TypeContactPhone.CompanyPhone2;
                                             estateObject.ObjectMainProperties.ContactCompanyId = Convert.ToInt64(values[2]);
                                             estateObject.ObjectMainProperties.ContactPersonId = null;
                                         }
                                         else if (values[0] == "company" && values[1] == "additional2")
                                         {
                                             estateObject.ObjectMainProperties.ContactPhone = (Int16?)TypeContactPhone.CompanyPhone3;
                                             estateObject.ObjectMainProperties.ContactCompanyId = Convert.ToInt64(values[2]);
                                             estateObject.ObjectMainProperties.ContactPersonId = null;
                                         }
                                     }
                                 },
                             Items = contactPhones,
                             Position = 15,
                         },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Срочно",
                            Name = "urgently",
                            Tooltip = "Срочность продажи",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Urgently;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Urgently = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                },
                             Position = 25,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Обмен",
                            Name = "exchange-required",
                            Tooltip = "Указывает, нужно ли клиенту одновременно купить другой объект",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Exchange;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Exchange = value.ConvertToNullableBool();
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                                  {
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                                  },
                             Position = 12,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Условия обмена",
                            Name = "exchange-description",
                            Tooltip = "Развернутая информация по условиям обмена, выдвинутым клиентом",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.ExchangeConditions;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.ExchangeConditions = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(255)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                                  {
                                                      new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                                      new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                                  },
                             Position = 12,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Условия продажи, сдачи",
                            Name = "sell-description",
                            Tooltip = "Развернутая информация по условиям продажи, сдачи данного объекта, выдвинутые клиентом",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.SellConditions;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.SellConditions = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(255)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                },
                             Position = 11,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Цена",
                            Name = "price",
                            Tooltip = "Стоимость, по которой риэлтор предлагает объект на продажу / сдачу",
                            Required = "always",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Price;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Price = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    
                                },
                             Position = 6,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Цена за квадртный метр",
                            Name = "price-per-unit",
                            Tooltip = "Цена за квадратный метр, установленная на данный объект недвижимости",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.PricePerUnit;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.PricePerUnit = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    
                                },
                             Position = 7,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Цена за сотку",
                            Name = "price-per-hundred",
                            Tooltip = "Стоимость одной сотки ЗУ, по которой объект предлагается риэлтором к продаже",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.PricePerHundred;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.PricePerHundred = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                },
                             Position = 8,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Цена хозяина",
                            Name = "owner-price",
                            Tooltip = "Стоимость, по которой собственник предлагает объект на продажу /сдачу",
                            Required = "selling/lising",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.OwnerPrice;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.OwnerPrice = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Garage, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                },
                             Position = 5,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Ценовая зона",
                            Name = "price-zone",
                            Tooltip = "Ценовая зона, согласно кадастрового пасторта ЗУ",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.PricingZone;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.PricingZone = value.ConvertToNullableInt();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(false)
                                },
                            RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                },
                             Position = 9,
                        },
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Эксклюзив",
                            Name = "exclusive",
                            Tooltip = "Эксклюзивные договорные отношения с собственником объекта",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.Exclusive;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.Exclusive = value.ConvertToNullableBool();
                                },
                             Position = 23,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Реальная цена",
                            Name = "real-price",
                            Tooltip = "Цена, реально уплаченная продавцу покупателем за данный объект",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.RealPrice;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectMainProperties.RealPrice = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                            CustomCondition = (context) =>
                                {
                                    return context.EstateObject.Status == (short) EstateStatuses.Deal;
                                },
                             Position = 22,
                        },
                    /*
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Мультилистинг",
                            Name = "multilisting",
                            Tooltip = "Продажа данного объекта по системе партнерских продаж",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectRatingProperties.Multilisting;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectRatingProperties.Multilisting = value.ConvertToNullableBool();
                                },
                            CustomCondition = (context) =>
                                {
                                    return context.EstateOperation == EstateOperations.Selling;
                                },
                             Position = 24,
                        },*/
                    new ObjectFormDictionaryField(Locator.GetService<IDictionariesRepository>().GetDictionaryByName("dogovor"))
                        {
                            Caption = "Тип договора",
                            Name = "agreement-type",
                            Tooltip = "Тип договора с клиентом на данный объект",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.AgreementType;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    if (estateObject.Status != (short) EstateStatuses.Active)
                                    {
                                        estateObject.ObjectAdditionalProperties.AgreementType = value.ConvertToNullableLong();
                                    }
                                    else
                                    {
                                        if (value != "265")
                                        {
                                            estateObject.ObjectAdditionalProperties.AgreementType = value.ConvertToNullableLong();
                                        }
                                    }
                                },
                             Position = 26,
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Номер договора",
                            Name = "agreement-number",
                            Tooltip = "Номер договора, заключенный с клиентом",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.AgreementNumber;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    if (estateObject.Status != (short)EstateStatuses.Active)
                                    estateObject.ObjectAdditionalProperties.AgreementNumber = value;
                                },
                             Position = 27,
                        },
                    new ObjectFormDateField()
                        {
                            Caption = "Дата заключения договора",
                            Name = "agreement-start-date",
                            Tooltip = "Дата заключения договора с клиентом на данный объект",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.AgreementStartDate.FormatDate();
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    if (estateObject.Status != (short)EstateStatuses.Active)
                                    estateObject.ObjectAdditionalProperties.AgreementStartDate = value.ConvertToNullableDateTime();
                                },
                             Position = 28,
                        },
                    new ObjectFormDateField()
                        {
                            Caption = "Дата завершения действия договора",
                            Name = "agreement-end-date",
                            Tooltip = "Дата завершения действия договора с клиентом на данный объект",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.AgreementEndDate.FormatDate();
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    if (estateObject.Status != (short)EstateStatuses.Active)
                                    estateObject.ObjectAdditionalProperties.AgreementEndDate = value.ConvertToNullableDateTime();
                                },
                             Position = 29,
                        },
                    /*
                    new ObjectFormTextField()
                        {
                            Caption = "Комиссия",
                            Name = "agreement-comission",
                            Tooltip = "Информация о комиссии за реализацию данного объекта, указанная в договоре",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.Comission;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.Comission = value;
                                },
                             Position = 30,
                        },*/
                    new ObjectFormCheckboxField()
                        {
                            Caption = "Оплата услуг агентства",
                            Name = "agreement-agency-payment",
                            Tooltip = "Происходит оплата услуг агентства",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.AgencyPayment;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.AgencyPayment = value.ConvertToNullableBool();
                                },
                             Position = 31,
                        },
                    new ObjectFormTextareaField()
                        {
                            Caption = "Условия оплаты услуг агентства",
                            Name = "agency-payment-condition",
                            Tooltip = "Информация об условии оплаты услуг агентства",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectAdditionalProperties.PaymentCondition;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    estateObject.ObjectAdditionalProperties.PaymentCondition = value;
                                },
                            Validator = new FieldValidator()
                                {
                                    new MaxLengthValidationRule(255)
                                },
                             Position = 32
                        },
                    new ObjectFormTextField()
                        {
                            Caption = "Размер бонуса",
                            Name = "multilisting-bonus",
                            Tooltip = "Размер бонуса при проводе сделки мультилистинга",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.MultilistingBonus;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    if (estateObject.Status != (short)EstateStatuses.Active)
                                        estateObject.ObjectMainProperties.MultilistingBonus = value.ConvertToNullableDouble();
                                },
                            Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
                             Position = 33
                        },
                    new ObjectFormDictionaryField(Locator.GetService<IDictionariesRepository>().GetDictionaryByName("bonus_type"))
                        {
                            Caption = "Тип бонуса",
                            Name = "multilisting-bonus-type",
                            Tooltip = "Тип бонуса, который выплачивается при проводе сделки по системе мультилистинг",
                            GetValueFromObject = (estateObject) =>
                                {
                                    return estateObject.ObjectMainProperties.MultilistingBonusType;
                                },
                            SetValueToObject= (estateObject,value) =>
                                {
                                    if (estateObject.Status != (short)EstateStatuses.Active)
                                        estateObject.ObjectMainProperties.MultilistingBonusType = value.ConvertToNullableLong();
                                },
                             Position = 34,
                        },
                };
		}

		/// <summary>
		/// Возвращает набор полей для страниц местоположения
		/// </summary>
		/// <param name="currentEstateObject">Текущий объект недвижимости</param>
		/// <param name="writing">Список генерируется для того чтобы записать значения полей </param>
		/// <returns>Набор полей для отображения списка</returns>
		public static FieldsList LocationPageList(EstateObject currentEstateObject, bool writing = false)
		{
			// Репозитории
			var dictRep = Locator.GetService<IDictionariesRepository>();
			var geoManager = Locator.GetService<IGeoManager>();
			var list = new FieldsList()
                {
                    new ObjectFormDropDownField()
                    {
                        Caption = "Страна",
                        Name = "geo-country",
                        Tooltip = "Страна расположения объекта",
                        Required = "always",
                        GetValueFromObject = (estateObject) =>
                            {
                                return estateObject.Address != null
                                           ? estateObject.Address.CountryId
                                           : geoManager.CountriesRepository.Find(c => c.Name.ToLower() == "россия").Id;
                            },
                        SetValueToObject = (estateObject,value) =>
                            {
                                var id = Convert.ToInt64(value);
                                var obj = geoManager.CountriesRepository.Load(id);
                                if (obj == null && estateObject.Address.GeoCountry != null)
                                {
                                    estateObject.Address.GeoCountry.Addresses.Remove(estateObject.Address);
                                }
                                estateObject.Address.GeoCountry = obj;
                            },
                        Items = geoManager.CountriesRepository.FindAll().OrderBy(c => c.Name).ToDictionary(c => c.Id.ToString(),c => c.Name),
                        Position = 1,
                        InsertBlankItem = true
                    }
                };
			if (currentEstateObject.Address.RegionId > 0 | writing)
			{
				list.Add(new ObjectFormDropDownField()
					{
						Caption = "Регион",
						Name = "geo-region",
						Tooltip = "Регион расположения объекта",
						Required = "always",
						GetValueFromObject = (estateObject) =>
							{
								return estateObject.Address.RegionId;
							},
						SetValueToObject = (estateObject, value) =>
							{
								var val = value.ConvertToGeoId();
								var obj = Locator.GetService<IGeoManager>().RegionsRepository.Load(val);
								if (obj == null && estateObject.Address.GeoRegion != null)
								{
									estateObject.Address.GeoRegion.Addresses.Remove(estateObject.Address);
								}
								estateObject.Address.GeoRegion = obj;
							},
						Items = currentEstateObject.Address.GeoCountry != null ? currentEstateObject.Address.GeoCountry.GeoRegions.OrderBy(c => c.Name).ToDictionary(c => c.Id.ToString(), c => c.Name) : new Dictionary<string, string>(),
						Position = 2,
						InsertBlankItem = true
					});
			}
			if (currentEstateObject.Address.RegionDistrictId > 0 | writing)
			{
				list.Add(new ObjectFormDropDownField()
					{
						Caption = "Район региона",
						Name = "geo-region-district",
						Tooltip = "Район региона, в котором расположен объект",
						Required = "always",
						GetValueFromObject = (estateObject) =>
							{
								return estateObject.Address.RegionDistrictId;
							},
						SetValueToObject = (estateObject, value) =>
							{
								var val = value.ConvertToGeoId();
								var obj = Locator.GetService<IGeoManager>().RegionsDistrictsRepository.Load(val);
								if (obj == null && estateObject.Address.GeoDistrict != null)
								{
									estateObject.Address.GeoRegionDistrict.Addresses.Remove(estateObject.Address);
								}
								estateObject.Address.GeoRegionDistrict = obj;
							},
						Items = currentEstateObject.Address.GeoRegion != null ? currentEstateObject.Address.GeoRegion.GeoRegionDistricts.OrderBy(c => c.Name).ToDictionary(c => c.Id.ToString(), c => c.Name) : new Dictionary<string, string>(),
						Position = 3,
						InsertBlankItem = true

					});
			}
			if (currentEstateObject.Address.CityId > 0 | writing)
			{
				list.Add(new ObjectFormDropDownField()
					{
						Caption = "Город",
						Name = "geo-city",
						Tooltip = "Населенный пункт, в котором расположен объект",
						Required = "always",
						GetValueFromObject = (estateObject) =>
							{
								return estateObject.Address.CityId;
							},
						SetValueToObject = (estateObject, value) =>
							{
								var val = value.ConvertToGeoId();
								var obj = Locator.GetService<IGeoManager>().CitiesRepository.Load(val);
								if (obj == null && estateObject.Address.GeoCity != null)
								{
									estateObject.Address.GeoCity.Addresses.Remove(estateObject.Address);
								}
								estateObject.Address.GeoCity = obj;
							},
						Items = currentEstateObject.Address.GeoRegionDistrict != null ? currentEstateObject.Address.GeoRegionDistrict.GeoCities.OrderBy(c => c.Name).ToDictionary(c => c.Id.ToString(), c => c.Name) : new Dictionary<string, string>(),
						Position = 4,
						InsertBlankItem = true
					});
				list.Add(new ObjectFormDropDownField()
				{
					Caption = "Район города",
					Name = "geo-city-district",
					Tooltip = "Район города, в котором расположен объект",
                    Required = "selling/lising",
                    NotRequired = "land/house",
					GetValueFromObject = (estateObject) =>
					{
						return estateObject.Address.CityDistrictId;
					},
					SetValueToObject = (estateObject, value) =>
					{
						var val = value.ConvertToGeoId();
						var obj = Locator.GetService<IGeoManager>().DistrictsRepository.Load(val);
						if (obj == null && estateObject.Address.GeoDistrict != null)
						{
							estateObject.Address.GeoDistrict.Addresses.Remove(estateObject.Address);
						}
						estateObject.Address.GeoDistrict = obj;
					},
					Items = currentEstateObject.Address.GeoCity != null ? currentEstateObject.Address.GeoCity.GeoDistricts.OrderBy(c => c.Name).ToDictionary(c => c.Id.ToString(), c => c.Name) : new Dictionary<string, string>(),
					Position = 5,
					InsertBlankItem = true
				});
			}
			if (currentEstateObject.Address.CityDistrictId > 0 | writing)
			{

				list.Add(new ObjectFormDropDownField()
				{
					Caption = "Жилой массив города",
					Name = "geo-residential-area",
					Tooltip = "Жилой массив, в котором расположен объект",
					GetValueFromObject = (estateObject) =>
					{
						return estateObject.Address.DistrictResidentialAreaId;
					},
					SetValueToObject = (estateObject, value) =>
					{
						var val = value.ConvertToGeoId();
						var obj = Locator.GetService<IGeoManager>().ResidentialAreasRepository.Load(val);
						if (obj == null && estateObject.Address.GeoResidentialArea != null)
						{
							estateObject.Address.GeoResidentialArea.Addresses.Remove(estateObject.Address);
						}
						estateObject.Address.GeoResidentialArea = obj;
					},
					Items = currentEstateObject.Address.GeoDistrict != null ? currentEstateObject.Address.GeoDistrict.GeoResidentialAreas.OrderBy(c => c.Name).ToDictionary(c => c.Id.ToString(), c => c.Name) : new Dictionary<string, string>(),
					Position = 7,
					InsertBlankItem = true
				});
				list.Add(new ObjectFormStreetField()
				{
					Caption = "Улица",
					Name = "geo-street",
					Tooltip = "Улица, на которой расположен объект",
					Required = "selling/lising",
					NotRequired = "land/house",
					GetValueFromObject = (estateObject) =>
					{
						return estateObject.Address.StreetId;
					},
					SetValueToObject = (estateObject, value) =>
					{
						var val = value.ConvertToGeoId();
						var obj = Locator.GetService<IGeoManager>().StreetsRepository.Load(val);
						if (obj == null && estateObject.Address.GeoStreet != null)
						{
							estateObject.Address.GeoStreet.Addresses.Remove(estateObject.Address);
						}
						estateObject.Address.GeoStreet = obj;
					},
					Position = 6,
				});
				list.Add(new ObjectFormTextField()
				{
					Caption = "Номер дома",
					Name = "house-number",
					Tooltip = "Номер дома объекта недвижимости",
					Required = "selling/lising",
					NotRequired = "land/house",
					GetValueFromObject = (estateObject) =>
					{
						return estateObject.Address.House;
					},
					SetValueToObject = (estateObject, value) =>
					{
						estateObject.Address.House = value;
					},
					Validator = new FieldValidator()
                            {
                                new MaxLengthValidationRule(50),
                                new HouseNumberValidationRule()
                            },
					Position = 8,
					RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                },
				});
				list.Add(new ObjectFormTextField()
				{
					Caption = "Номер корпуса",
					Name = "block-number",
					Tooltip = "Номер корпуса объекта недвижимости",
					GetValueFromObject = (estateObject) =>
					{
						return estateObject.Address.Block;
					},
					SetValueToObject = (estateObject, value) =>
					{
						estateObject.Address.Block = value;
					},
					Validator = new FieldValidator()
                            {
                                new MaxLengthValidationRule(50)
                            },
					Position = 9,
					RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.House, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Office, EstateOperations.Rent),
                                },
				});
				list.Add(new ObjectFormTextField()
				{
					Caption = "Номер квартиры",
					Name = "flat-number",
					Tooltip = "Номер квартиры объекта недвижимости",
					//Required = "selling/lising",
					GetValueFromObject = (estateObject) =>
					{
						return estateObject.Address.Flat;
					},
					SetValueToObject = (estateObject, value) =>
					{
						estateObject.Address.Flat = value;
					},
					Validator = new FieldValidator()
                            {
                                new MaxLengthValidationRule(50)
                            },
					Position = 10,
					RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Room, EstateOperations.Rent),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Flat, EstateOperations.Rent),
                                },
				});
				list.Add(new ObjectFormTextField()
				{
					Caption = "Номер участка",
					Name = "land-number",
					Tooltip = "Номер участка объекта недвижимости",
					GetValueFromObject = (estateObject) =>
					{
						return estateObject.Address.Land;
					},
					SetValueToObject = (estateObject, value) =>
					{
						estateObject.Address.Land = value;
					},
					Validator = new FieldValidator()
                            {
                                new MaxLengthValidationRule(50)
                            },
					Position = 11,
					RequiredContext = new List<ObjectContextRequirement>()
                                {
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Selling),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Buying),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Lising),
                                    new ObjectContextRequirement(EstateTypes.Land, EstateOperations.Rent),
                                },
				});
			}
			if (currentEstateObject.Address.DistrictResidentialAreaId > 0 | writing)
			{

			}
			if (currentEstateObject.Address.StreetId > 0 | writing)
			{

			}

			list.Add(new ObjectFormDictionaryField(dictRep.GetDictionaryByName("landmark"))
						{
							Caption = "Ориентир",
							Name = "landmark",
							Tooltip = "Ориентир, позволяющий добраться до объекта",
							GetValueFromObject = (estateObject) =>
								{
									return estateObject.ObjectMainProperties.Landmark;
								},
							SetValueToObject = (estateObject, value) =>
								{
									estateObject.ObjectMainProperties.Landmark = value.ConvertToNullableLong();
								},
							Position = 12,
						});
			list.Add(new ObjectFormTextField()
						{
							Caption = "Расстояние до города",
							Name = "distance-to-city",
							Tooltip = "Расстояние до города в километрах от данного объекта недвижимости",
							GetValueFromObject = (estateObject) =>
								{
									return estateObject.ObjectMainProperties.DistanceToCity.FormatString();
								},
							SetValueToObject = (estateObject, value) =>
								{
									estateObject.ObjectMainProperties.DistanceToCity = value.ConvertToNullableDouble();
								},
							Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
							Position = 13,
						});
			list.Add(new ObjectFormTextField()
						{
							Caption = "Расстояние до моря",
							Name = "distance-to-sea",
							Tooltip = "Расстояние до моря в километрах от данного объекта недвижимости",
							GetValueFromObject = (estateObject) =>
								{
									return estateObject.ObjectMainProperties.DistanceToSea.FormatString();
								},
							SetValueToObject = (estateObject, value) =>
								{
									estateObject.ObjectMainProperties.DistanceToSea = value.ConvertToNullableDouble();
								},
							Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
							Position = 14,
						});
			list.Add(new ObjectFormDictionaryField(dictRep.GetDictionaryByName("how_to_reach"))
					 {
						 Caption = "Чем добраться",
						 Name = "how-to-reach",
						 Tooltip = "Способ добраться до данного объекта недвижимости",
						 GetValueFromObject = (estateObject) =>
							 {
								 return estateObject.ObjectMainProperties.HowToReach;
							 },
						 SetValueToObject = (estateObject, value) =>
							 {
								 estateObject.ObjectMainProperties.HowToReach = value.ConvertToNullableLong();
							 },
						 Position = 15,
					 });
			list.Add(new ObjectFormTextField()
			{
				Caption = "Широта",
				Name = "latitude",
				Tooltip = "Географическая широта объекта",
				GetValueFromObject = (estateObject) =>
				{
					return estateObject.Address.Latitude;
				},
				SetValueToObject = (estateObject, value) =>
				{
					estateObject.Address.Latitude = value.ConvertToNullableDouble();
				},
				Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
				Position = 16,
			});
			list.Add(new ObjectFormTextField()
			{
				Caption = "Долгота",
				Name = "longitude",
				Tooltip = "Географическая долгота объекта",
				GetValueFromObject = (estateObject) =>
				{
					return estateObject.Address.Logitude;
				},
				SetValueToObject = (estateObject, value) =>
				{
					estateObject.Address.Logitude = value.ConvertToNullableDouble();
				},
				Validator = new FieldValidator()
                                {
                                    new NumbersValidationRule(true)
                                },
				Position = 17,
			});
			// Отдаем список
			return list;
		}

		/// <summary>
		/// Создает список, содержащий абсолютно все поля формы
		/// </summary>
		/// <param name="obj">Объект для которого формируются поля</param>
		/// <param name="currentUser">Пользователь для которого формируется поля</param>
		/// <returns></returns>
		public static FieldsList AllFields(EstateObject obj, User currentUser)
		{
			var allFields = new FieldsList();
			var fields = new FieldsList();
			allFields.AddRange(FormPageFieldsFactory.ServicePageList(obj, currentUser).AssignCategory("Сервисные"));
			allFields.AddRange(FormPageFieldsFactory.TechPageList(obj).AssignCategory("Технические"));
			allFields.AddRange(FormPageFieldsFactory.InfrastructurePageList(obj).AssignCategory("Инфаструктурные"));
			allFields.AddRange(FormPageFieldsFactory.ExpluatationPageList(obj).AssignCategory("Эксплуатационные"));
			allFields.AddRange(FormPageFieldsFactory.LocationPageList(obj, true).AssignCategory("Местоположение"));
			allFields.AddRange(FormPageFieldsFactory.MainPageList(obj).AssignCategory("Системные"));

			return allFields;
		}
	}
}
// ReSharper restore ConvertToLambdaExpression
// ReSharper restore RedundantEmptyObjectCreationArgumentList
// ReSharper restore RedundantLambdaSignatureParentheses