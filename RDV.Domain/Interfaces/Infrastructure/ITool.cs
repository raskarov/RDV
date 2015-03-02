using System;
using RDV.Domain.Enums;

namespace RDV.Domain.Interfaces.Infrastructure
{
    /// <summary>
    /// ���������� ��������� �������, ���������� � ������� ������
    /// </summary>
    public interface ITool
    {
        /// <summary>
        /// ������������� �������
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// ������������ �������
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// �������� ����������
        /// </summary>
        void StartExecuting();

        /// <summary>
        /// ��������� ���������� (�� ������ ��������)
        /// </summary>
        void Break();

        /// <summary>
        /// ������� ������ �������
        /// </summary>
        ToolState State { get; }

        /// <summary>
        /// �������� ���������� ����������
        /// </summary>
        ToolLaunchInterval Interval { get; set; }

        /// <summary>
        /// ���� � ����� ���������� �������
        /// </summary>
        DateTime? LastLaunch { get; set; }

        /// <summary>
        /// ���� � ����� ���������� ��������� ���������� ���������� ��������
        /// </summary>
        DateTime? LastCompleted { get; set; }
    }
}