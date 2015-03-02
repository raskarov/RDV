using RDV.Domain.Entities;
using RDV.Web.Classes.Forms;

namespace RDV.Web.Models.Account.ObjectsList
{
    /// <summary>
    /// Модель содержащая в себе сводные данные по одному объекту
    /// </summary>
    public class ObjectComparationModel
    {
        /// <summary>
        /// Объект недвижимости
        /// </summary>
        public EstateObject EstateObject { get; set; }

        /// <summary>
        /// Сервисные свойства
        /// </summary>
        public FieldsList ServiceFields { get; private set; }

        /// <summary>
        /// Свойства местоположения
        /// </summary>
        public FieldsList LocationFields { get; private set; }

        /// <summary>
        /// Технические реквизиты
        /// </summary>
        public FieldsList TechFields { get; private set; }

        /// <summary>
        /// Инфраструктурные реквизиты
        /// </summary>
        public FieldsList InfrastructureFields { get; private set; }

        /// <summary>
        /// Эксплуатационные свойства
        /// </summary>
        public FieldsList ExpluatationFields { get; private set; }

        /// <summary>
        /// Юридические свойства
        /// </summary>
        public FieldsList LegalFields { get; private set; }

        /// <summary>
        /// Системные свойства
        /// </summary>
        public FieldsList SystemFields { get; private set; }

        /// <summary>
        /// Конструктор на основе объекта
        /// </summary>
        public ObjectComparationModel(EstateObject obj, User currentUser)
        {
            EstateObject = obj;
            // Инициализируем наборы полей
            ServiceFields = FormPageFieldsFactory.ServicePageList(obj, currentUser);
            LocationFields = FormPageFieldsFactory.LocationPageList(obj,true);
            TechFields = FormPageFieldsFactory.TechPageList(obj);
            InfrastructureFields = FormPageFieldsFactory.InfrastructurePageList(obj);
            ExpluatationFields = FormPageFieldsFactory.ExpluatationPageList(obj);
            LegalFields = FormPageFieldsFactory.LegalPageList(obj);
            SystemFields = FormPageFieldsFactory.MainPageList(obj);

            // Считываем данные в поля
            ServiceFields.ReadValuesFromObject(obj);
            LocationFields.ReadValuesFromObject(obj);
            TechFields.ReadValuesFromObject(obj);
            InfrastructureFields.ReadValuesFromObject(obj);
            ExpluatationFields.ReadValuesFromObject(obj);
            LegalFields.ReadValuesFromObject(obj);
            SystemFields.ReadValuesFromObject(obj);
        }
    }
}