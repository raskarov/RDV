using RDV.Domain.Enums;

namespace RDV.Web.Classes.Notification.UI
{
    /// <summary>
    /// ��� �������� �����������
    /// </summary>
    public enum NotificationItemType
    {
        /// <summary>
        /// ��������� �� ������
        /// </summary>
        [EnumDescription("success-notification")]
        Success,

        /// <summary>
        /// ��������� �� ������
        /// </summary>
        [EnumDescription("error-notification")]
        Error,

        /// <summary>
        /// ��������������
        /// </summary>
        [EnumDescription("warning-notification")]
        Warning
    }
}