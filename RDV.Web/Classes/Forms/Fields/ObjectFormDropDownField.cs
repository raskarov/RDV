using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// ������� �����, �������������� ���������� ������
    /// </summary>
    public class ObjectFormDropDownField: ObjectFormField
    {
        /// <summary>
        /// �������� ����������� ������
        /// </summary>
        public Dictionary<string, string> Items { get; set; }

        /// <summary>
        /// ��������� �� ������ ������� ������ ����� ����� ���������� ����������
        /// </summary>
        public bool InsertBlankItem { get; set; }

        /// <summary>
        /// ����������� �����������
        /// </summary>
        public ObjectFormDropDownField()
        {
            Items = new Dictionary<string, string>();
        }

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
            var builder = new TagBuilder("select");

            // ��������� �������� ��������
            ApplyDefaultAttributes(builder);
            
            // ����������� ����� ��� �����������
            builder.AddCssClass("singleSelect");

            // ������������� �������� �� ���������
            if (DefaultValue == null)
            {
                DefaultValue = "";
            }
            if (string.IsNullOrEmpty(Value) && !String.IsNullOrEmpty(DefaultValue.ToString()))
            {
                Value = DefaultValue.ToString();
            }

            // �������� ��������
            var items = new List<string>();
            foreach (var item in Items)
            {
                var itemBuilder = new TagBuilder("option");
                itemBuilder.Attributes.Add("value",item.Key);
                if (item.Key == Value)
                {
                    itemBuilder.Attributes.Add("selected","selected");
                }
                itemBuilder.SetInnerText(item.Value);
                items.Add(itemBuilder.ToString());
            }

            // ��������� ������ ������ ���� ��� ����������� ������� ������
            if (InsertBlankItem)
            {
                var blankBuilder = new TagBuilder("option");
                blankBuilder.Attributes.Add("value", "");
                blankBuilder.SetInnerText(String.Empty);
                items.Insert(0, blankBuilder.ToString());
            }

            // ��������� �������� �� ������ �������� ������
            builder.InnerHtml = string.Join("\n", items);

            // ������ ���������
            return builder.ToString(TagRenderMode.Normal);
        }

        /// <summary>
        /// ���������� ������������� �������� ��� ������
        /// </summary>
        /// <returns>�������� ��� ������</returns>
        public override string ValueAsString()
        {
            if (Value != null && Items.ContainsKey(Value))
            {
                return Items[Value];
            } else
            {
                return string.Empty;
            }
        }
    }
}