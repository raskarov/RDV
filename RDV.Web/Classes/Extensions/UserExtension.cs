using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Models.Account.ObjectsList;

namespace RDV.Web.Classes.Extensions
{
    /// <summary>
    /// Статический класс с расширениями для объекта пользователя
    /// </summary>
    public static class UserExtension
    {
        /// <summary>
        /// Проверяет, имеет ли пользователь право на редактирование указанного объекта
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="objectModel">Объект</param>
        /// <returns>true если имеет, иначе false</returns>
        public static bool CanEditObject(this User user, EstateObjectModel objectModel)
        {
            if (objectModel.Status == EstateStatuses.Deal)
            {
                return false;
            }
            if (user.HasObjectRelatedPermission(Permission.EditAllObjects, (short) objectModel.Operation, (short) objectModel.Type))
            {
                return true;
            } else if (objectModel.EstateAgent.CompanyId == user.CompanyId && user.HasObjectRelatedPermission(Permission.EditCompanyObjects, (short) objectModel.Operation, (short) objectModel.Type))
            {
                return true;
            } else if (objectModel.EstateAgent.Id == user.Id && user.HasObjectRelatedPermission(Permission.EditOwnObjects, (short) objectModel.Operation, (short) objectModel.Type))
            {
                return true;
            } else
            {
                return false;
            }
        }

        /// <summary>
        /// Проверяет, имеет ли пользователь право на изменение статуса указанного объекта
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="objectModel">Объект</param>
        /// <returns>true если имеет, иначе false</returns>
        public static bool CanChangeStatus(this User user, EstateObjectModel objectModel)
        {
            if (objectModel.Status == EstateStatuses.Deal)
            {
                return false;
            }
            if (user.HasObjectRelatedPermission(Permission.ChangeAllObjectsStatus, (short) objectModel.Operation, (short) objectModel.Type))
            {
                return true;
            } else if (objectModel.EstateAgent.CompanyId == user.CompanyId && user.HasObjectRelatedPermission(Permission.ChangeCompanyObjectsStatus, (short) objectModel.Operation, (short) objectModel.Type))
            {
                return true;
            } else if (objectModel.EstateAgent.Id == user.Id && user.HasObjectRelatedPermission(Permission.ChangeCompanyObjectsStatus, (short) objectModel.Operation, (short) objectModel.Type))
            {
                return true;
            } else
            {
                return false;
            }
        }

        /// <summary>
        /// Проверяет, имеет ли пользователь право на изменение статуса указанного объекта
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <param name="objId">Идентификатор объекта</param>
        /// <returns>true если имеет, иначе false</returns>
        public static bool CanChangeStatus(this User user, long objId)
        {
            var objectModel = Locator.GetService<IEstateObjectsRepository>().Load(objId);
            if (objectModel.Status == (short)EstateStatuses.Deal)
            {
                return false;
            }
            if (user.HasObjectRelatedPermission(Permission.ChangeAllObjectsStatus, (short) objectModel.Operation, (short) objectModel.ObjectType))
            {
                return true;
            } else if (objectModel.User.CompanyId == user.CompanyId && user.HasObjectRelatedPermission(Permission.ChangeCompanyObjectsStatus, (short) objectModel.Operation, (short) objectModel.ObjectType))
            {
                return true;
            } else if (objectModel.User.Id == user.Id && user.HasObjectRelatedPermission(Permission.ChangeOwnObjectsStatus, (short) objectModel.Operation, (short) objectModel.ObjectType))
            {
                return true;
            } else
            {
                return false;
            }
        }

        /// <summary>
        /// Проверяет, может ли пользователь редактировать объет с указанным идентификатором
        /// </summary>
        /// <param name="user"></param>
        /// <param name="objId"></param>
        /// <returns></returns>
        public static bool CanEditObject(this User user, long objId)
        {
            var objectModel = Locator.GetService<IEstateObjectsRepository>().Load(objId);
            if (objectModel.Status == (short)EstateStatuses.Deal)
            {
                return false;
            }
            if (user.HasObjectRelatedPermission(Permission.EditAllObjects, (short)objectModel.Operation, (short)objectModel.ObjectType))
            {
                return true;
            }
            else if (objectModel.User.CompanyId == user.CompanyId && user.HasObjectRelatedPermission(Permission.EditCompanyObjects, (short)objectModel.Operation, (short)objectModel.ObjectType))
            {
                return true;
            }
            else if (objectModel.User.Id == user.Id && user.HasObjectRelatedPermission(Permission.EditOwnObjects, (short)objectModel.Operation, (short)objectModel.ObjectType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Проверяет, может ли пользователь удалить указанный объект
        /// </summary>
        /// <param name="user"></param>
        /// <param name="objId"></param>
        /// <returns></returns>
        public static bool CanDeleteObject(this User user, long objId)
        {
            var objectModel = Locator.GetService<IEstateObjectsRepository>().Load(objId);
            return objectModel.Status == (short) EstateStatuses.Draft;
        }
    }
}