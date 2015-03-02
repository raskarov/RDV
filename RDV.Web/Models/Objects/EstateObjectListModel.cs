using System;
using System.Globalization;
using System.Linq;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Misc;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Classes.Caching;
using RDV.Web.Classes.Extensions;

namespace RDV.Web.Models.Objects
{
    /// <summary>
    /// ������ �������
    /// </summary>
    public class EstateObjectListModel
    {
        /// <summary>
        /// ��������� �������
        /// </summary>
        public string TitleLine { get; set; }

        /// <summary>
        /// ����� ���������� ������
        /// </summary>
        public string SizeLine { get; set; }

        /// <summary>
        /// ����� ������
        /// </summary>
        public string DistrictLine { get; set; }

        /// <summary>
        /// ����� �����
        /// </summary>
        public string StreetLine { get; set; }

        /// <summary>
        /// ����� ����
        /// </summary>
        public string PriceLine { get; set; }

        /// <summary>
        /// ������ �� �����������
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// ������������� �������
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// ����������� �� ������ ��������� �������
        /// </summary>
        /// <param name="obj"></param>
        public EstateObjectListModel(EstateObject obj)
        {
            Id = obj.Id;
            // ��������� ������ ��������� � ������
            var operation = "";
            var objectType = "";
            switch ((EstateOperations)obj.Operation)
            {
                case EstateOperations.Selling:
                    operation = "������";
                    break;
                case EstateOperations.Buying:
                    operation = "�����";
                    break;
                case EstateOperations.Lising:
                    operation = "����";
                    break;
                case EstateOperations.Rent:
                    operation = "�����";
                    break;
                case EstateOperations.Exchange:
                    operation = "�������";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch ((EstateTypes)obj.ObjectType)
            {
                case EstateTypes.Room:
                    objectType = "�������";
                    SizeLine = String.Format("�������: {0}",
                                             obj.ObjectMainProperties.TotalArea.HasValue
                                                 ? obj.ObjectMainProperties.TotalArea.Value.ToString(CultureInfo.InvariantCulture)
                                                 : "�� �������");
                    break;
                case EstateTypes.Flat:
                    objectType = "��������";
                    SizeLine = String.Format("���������� ������: {0}",
                                             obj.ObjectAdditionalProperties.RoomsCount.HasValue
                                                 ? obj.ObjectAdditionalProperties.RoomsCount.Value.ToString(CultureInfo.InvariantCulture)
                                                 : "-");
                    break;
                case EstateTypes.House:
                    objectType = "�������";
                    SizeLine = String.Format("���������� ������: {0}",
                                             obj.ObjectAdditionalProperties.RoomsCount.HasValue
                                                 ? obj.ObjectAdditionalProperties.RoomsCount.Value.ToString(CultureInfo.InvariantCulture)
                                                 : "-");
                    break;
                case EstateTypes.Land:
                    objectType = "������. �������";
                    SizeLine = String.Format("�������: {0}",
                                             obj.ObjectMainProperties.TotalArea.HasValue
                                                 ? obj.ObjectMainProperties.TotalArea.Value.ToString(CultureInfo.InvariantCulture)
                                                 : "�� �������");
                    break;
                case EstateTypes.Office:
                    objectType = "����";
                    SizeLine = String.Format("�������: {0}",
                                             obj.ObjectMainProperties.TotalArea.HasValue
                                                 ? obj.ObjectMainProperties.TotalArea.Value.ToString(CultureInfo.InvariantCulture)
                                                 : "�� �������");
                    break;
                case EstateTypes.Garage:
                    objectType = "�����";
                    SizeLine = String.Format("�������: {0}",
                                             obj.ObjectMainProperties.TotalArea.HasValue
                                                 ? obj.ObjectMainProperties.TotalArea.Value.ToString(CultureInfo.InvariantCulture)
                                                 : "�� �������");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            TitleLine = String.Format("{0} {1}", operation, objectType);
            if (!String.IsNullOrEmpty(obj.ObjectMainProperties.Title))
            {
                TitleLine = obj.ObjectMainProperties.Title;
            }

            // ��������� �����
            string districtName;
            if (obj.Address != null && obj.Address.GeoDistrict != null)
            {
                districtName = obj.Address.GeoDistrict.Name;
            }
            else
            {
                districtName = "�� ������";
            }
            DistrictLine = String.Format("�����: {0}", districtName);

            // ��������� �����
            string streetName;
            if (obj.Address != null && obj.Address.GeoStreet != null)
            {
                streetName = obj.Address.GeoStreet.Name;
            }
            else
            {
                streetName = "�� �������";
            }
            StreetLine = String.Format("�����: {0}", streetName);

            // ��������� ����
            PriceLine = String.Format("{0} {1}", obj.ObjectMainProperties.Price.FormatPrice(),
                                      IdObjectsCache.GetDictionaryValue(obj.ObjectMainProperties.Currency));

            // �������� �����������
            var firstPhoto = obj.ObjectMedias.FirstOrDefault(m => m.MediaType == (short) ObjectMediaTypes.Photo);
            if (firstPhoto != null)
            {
                var imagesRep = Locator.GetService<IStoredFilesRepository>();
                ImageUrl = imagesRep.ResolveFileUrl(firstPhoto.MediaUrl);
            }
        }
    }
}