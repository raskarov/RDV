using System.Web.Mvc;
using RDV.Web.Classes.Forms.Validators;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// ������������� ���� �� �������� �����
    /// </summary>
    public class ObjectFormTextareaField: ObjectFormField
    {
        /// <summary>
        /// ������������ ����� ����
        /// </summary>
        public int? MaxLenght { get; set; }

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
            // �������� ��������� ����������� ���������
            var builder = new TagBuilder("textarea");

            ApplyDefaultAttributes(builder);

            // �������� ����������� ����
            if (MaxLenght.HasValue)
            {
                builder.Attributes.Add("maxlenght", MaxLenght.Value.ToString());
            }

            // ���������, ����������� �� ����
            if (ActiveStatusValidator.IsFieldRequired(context.EstateObject, this) && this.CheckVisibility(context))
            {
                builder.AddCssClass("field-required");
            }

            // �������� �������� ��������
            builder.SetInnerText(Value);

            // ������ ������������� �������
            return builder.ToString(TagRenderMode.Normal);
        }
    }
}