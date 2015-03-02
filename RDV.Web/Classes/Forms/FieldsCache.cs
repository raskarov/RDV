using System;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;

namespace RDV.Web.Classes.Forms
{
    /// <summary>
    /// Статический класс, содержащий инстанс всех полей формы объектов
    /// </summary>
    public static class FieldsCache
    {
        /// <summary>
        /// Все поля формы объектов которые могут использоваться для поиска
        /// </summary>
        public static Lazy<FieldsList> AllSearchFields = new Lazy<FieldsList>(() =>
                                                                             {
                                                                                 // Список всех полей
                                                                                 var allFields =
                                                                                     FormPageFieldsFactory.AllFields(Locator.GetService<IEstateObjectsRepository>().GetTempObject(),
                                                                                                                     Locator.GetService<IUsersRepository>().GetGuestUser());
                                                                                 allFields.RemoveAll(f => f.Name == "client");
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("currency"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("title"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("description"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("conditions"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("period"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("real-price"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("date-"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("price-changement"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("delay"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("company-name"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("residents"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("doors-rating"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("wc-rooms"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("advertising-text"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("agent-id"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("object-id"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("removal-reason"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("status-changed-by"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("agent-phone-"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("agent"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("agreement-agency-payment"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("agency-payment-condition"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("object-type"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("object-operation"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("special-offer-text"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("owner-price"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("price-zone"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("exclusive"));
                                                                                 allFields.RemoveAll(
                                                                                     f =>
                                                                                     f.Name.ToLower()
                                                                                      .Contains("agreement-number"));
                                                                                 return allFields;
                                                                             });
    }
}