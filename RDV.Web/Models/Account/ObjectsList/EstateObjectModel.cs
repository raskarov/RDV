using System;
using System.Text;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Infrastructure.Misc;
using RDV.Web.Classes.Caching;
using RDV.Web.Classes.Extensions;

namespace RDV.Web.Models.Account.ObjectsList
{
    /// <summary>
    /// ������ ������� ������������ ��� ������ ��������
    /// </summary>
    public class EstateObjectModel
    {
        /// <summary>
        /// ������������� �������
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// �������� ���������� � ��������
        /// </summary>
        public EstateOperations Operation { get; set; }

        /// <summary>
        /// ������ �������
        /// </summary>
        public EstateStatuses Status { get; set; }

        /// <summary>
        /// ��� ������� ������������
        /// </summary>
        public EstateTypes Type { get; set; }

        /// <summary>
        /// �����, ���������� �������� � ��������
        /// </summary>
        public User EstateAgent { get; set; }

        /// <summary>
        /// ����c ��������������� �������
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// ���������� �� �������
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// ���������� �� �������
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// ���� ��������� �����������, ��������� � ��������
        /// </summary>
        public DateTime? DateModified { get; set; }

        /// <summary>
        /// ���� �� ���������� �� ������� �������
        /// </summary>
        public bool HasPhotos { get; set; }

        /// <summary>
        /// ����������� �����������
        /// </summary>
        public EstateObjectModel()
        {
            
        }

        /// <summary>
        /// ����������� �� ������ ��������� �������
        /// </summary>
        /// <param name="object"></param>
        public EstateObjectModel(EstateObject @object)
        {
            Id = @object.Id;
            Operation = (EstateOperations) @object.Operation;
            Status = (EstateStatuses) @object.Status;
            Type = (EstateTypes) @object.ObjectType;
            EstateAgent = @object.User;
            if (@object.Address != null)
            {
                Address = @object.Address.ToShortAddressString();
            } else if (EstateAgent.Company != null)
            {
                if (EstateAgent.Company.GeoCity != null)
                {
                    Address = EstateAgent.Company.GeoCity.Name;    
                } else
                {
                    Address = null;
                }
            }
            Notes = @object.ObjectMainProperties.Notes;
            // ������������ ������
            Notes = (Notes ?? "").Replace(",", ", ");
            if (Notes.Length > 100)
            {
                Notes = Notes.Substring(0, 100) + "...";
            }
            Price = String.Format("{0} {1}",@object.ObjectMainProperties.Price.FormatPrice(), IdObjectsCache.GetDictionaryValue(@object.ObjectMainProperties.Currency));
            DateModified = @object.DateModified.HasValue ? @object.DateModified : @object.DateCreated;
            HasPhotos = @object.GetObjectsMedia(true).Count > 0;
        }
    }
}