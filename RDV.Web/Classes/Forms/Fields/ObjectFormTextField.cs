using System.Web.Mvc;
using RDV.Domain.Enums;
using RDV.Web.Classes.Forms.Validators;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// ��������� ���� � ����� �������
    /// </summary>
    public class ObjectFormTextField: ObjectFormField
    {
        /// <summary>
        /// ������������ ����� ������
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
            var builder = new TagBuilder("input");
            
            ApplyDefaultAttributes(builder);

            // �������� ����������� ����
            if (MaxLenght.HasValue)
            {
                builder.Attributes.Add("maxlenght", MaxLenght.Value.ToString());
            }

            // ���������, ����������� �� ����
            if (ActiveStatusValidator.IsFieldRequired(context.EstateObject,this))
            {
                builder.AddCssClass("field-required");
            }

            // �������� �������� ��������
            builder.Attributes.Add("value",Value);

            // ������ ������������� �������
            return builder.ToString(TagRenderMode.SelfClosing);
        }
    }
}