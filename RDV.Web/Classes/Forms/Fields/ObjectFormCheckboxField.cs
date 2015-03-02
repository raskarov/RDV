using System.Web.Mvc;
using RDV.Web.Classes.Extensions;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// ���� ���� ������� �� �����
    /// </summary>
    public class ObjectFormCheckboxField: ObjectFormField
    {
        /// <remarks>
        /// ��������� ���������� �������� ���������� �� ViewEngine
        /// </remarks>
        /// <summary>
        /// ��������� ���������������� ��������� �������� HTML ���� �.�. ������ INPUTA ��������.
        /// </summary>
        /// <param name="context"> </param>
        /// <returns></returns>
        public override string RenderFieldEditor(FieldRenderingContext context)
        {
            var builder = new TagBuilder("input");

            builder.Attributes.Add("type", "checkbox");

            ApplyDefaultAttributes(builder);

            // �������� �������� ��������
            builder.Attributes.Add("value", "true");

            // ���������, ��������� �� ������� ����������
            if (!string.IsNullOrEmpty(Value) && Value.ToLower() == "true")
            {
                builder.Attributes.Add("checked","checked");
            }

            // ������ ������� ������, ��� ����������� ������� �����
            var hiddenBuilder = new TagBuilder("input");
            hiddenBuilder.Attributes.Add("type","hidden");
            hiddenBuilder.Attributes.Add("name",Name);
            hiddenBuilder.Attributes.Add("value","false");
            builder.InnerHtml = hiddenBuilder.ToString(TagRenderMode.SelfClosing);

            // ������ ������������� �������
            return builder.ToString(TagRenderMode.SelfClosing);
        }

        /// <summary>
        /// ���������� ������������� �������� ��� ������
        /// </summary>
        /// <returns>�������� ��� ������</returns>
        public override string ValueAsString()
        {
            bool? val = Value.ConvertToNullableBool();
            if (val.HasValue)
            {
                return val.Value ? "��" : "���";
            } else
            {
                return string.Empty;
            }
        }
    }
}