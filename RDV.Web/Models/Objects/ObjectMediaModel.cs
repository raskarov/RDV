using System;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Domain.Interfaces.Repositories;
using RDV.Domain.IoC;
using RDV.Web.Classes.Caching;

namespace RDV.Web.Models.Objects
{
    /// <summary>
    /// ������ ����� �������� ������������ � �������
    /// </summary>
    public class ObjectMediaModel
    {
        /// <summary>
        /// ������������� ����� �������
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// ��� ����� �������
        /// </summary>
        public ObjectMediaTypes MediaType { get; set; }

        /// <summary>
        /// ��������� ����� �������
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// ������ �� ������ ��� ����� ������� �� ���������� �������� �������
        /// </summary>
        public string PreviewUrl { get; set; }

        /// <summary>
        /// �������������� ������ �� ������ ����� �������
        /// </summary>
        public string PreviewFullUrl { get; set; }

        /// <summary>
        /// ������ �� ���� ����� �������
        /// </summary>
        public StoredFile PreviewFile { get; set; }

        /// <summary>
        /// ������ �� ��� ������ ����� � �������� �������
        /// </summary>
        public string MediaUrl { get; set; }

        /// <summary>
        /// �������������� ������ �� ����� ������
        /// </summary>
        public string MediaFullUrl { get; set; }

        /// <summary>
        /// ����� ����
        /// </summary>
        public StoredFile MediaFile { get; set; }

        /// <summary>
        /// ���������� ���������� ����� ��������
        /// </summary>
        public int Views { get; set; }

        /// <summary>
        /// ���� �������� ����� ��������
        /// </summary>
        public DateTime? DateUploaded { get; set; }

        /// <summary>
        /// ��� �������� ����� �������
        /// </summary>
        public string UploadedBy { get; set; }

        /// <summary>
        /// ����������� �����������
        /// </summary>
        public ObjectMediaModel()
        {
        }

        /// <summary>
        /// ����������� �� ������ ��������� �������
        /// </summary>
        public ObjectMediaModel(ObjectMedia media)
        {
            // �������� �����������
            var rep = Locator.GetService<IStoredFilesRepository>();

            Id = media.Id;
            Title = media.Title;
            MediaType = (ObjectMediaTypes) media.MediaType;
            PreviewUrl = media.PreviewUrl;
            if (!String.IsNullOrEmpty(PreviewUrl))
            {
                PreviewFile = rep.GetFile(PreviewUrl);
                PreviewFullUrl = rep.ResolveFileUrl(PreviewUrl);
            }
            MediaUrl = media.MediaUrl;
            if (!String.IsNullOrEmpty(MediaUrl))
            {
                MediaFile = rep.GetFile(MediaUrl);
                MediaFullUrl = rep.ResolveFileUrl(MediaUrl);
            }
            Views = media.Views;
            DateUploaded = media.DateCreated;
            UploadedBy = IdObjectsCache.GetUserLogin(media.CreatedBy);
            Position = media.Position;
            IsMain = media.IsMain;
        }

        /// <summary>
        /// ���������� �������� �������
        /// </summary>
        public bool IsMain { get; set; }

        /// <summary>
        /// ������ ���������� ���������� ��� ����������
        /// </summary>
        public int Position { get; set; }
    }
}