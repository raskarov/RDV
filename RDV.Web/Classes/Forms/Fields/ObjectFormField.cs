using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using RDV.Domain.Entities;
using RDV.Domain.Enums;
using RDV.Web.Classes.Forms.Validators;

namespace RDV.Web.Classes.Forms.Fields
{
    /// <summary>
    /// ����������� ���� � ����� �������
    /// </summary>
    public abstract class ObjectFormField
    {
        /// <summary>
        /// ��� ���� �������, � ������� ���������� ������������
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ������������ ����� � �������
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// ������������� ���� � DOM, ���� ���� - ������������� ������������ �������������
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ����������� ��� ����. ����������� ��������� ���� HTML5
        /// </summary>
        public string Placeholder { get; set; }

        /// <summary>
        /// ��������� CSS ������, ����������� � ���� �����
        /// </summary>
        public string CustomClasses { get; set; }

        /// <summary>
        /// ��������� �����, ����������� � ���� �����
        /// </summary>
        public string CustomStyles { get; set; }

        /// <summary>
        /// ����������� ��������� �� ������� ����, ������� ���������� �� �����, ����� ������ ���������� �� ������ ����
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// ���� ������ ��� ������
        /// </summary>
        public bool Readonly { get; set; }

        /// <summary>
        /// ������, ����������� �������, ����� ������ ���� ����������� � ����������
        /// </summary>
        public string Required { get; set; }

        /// <summary>
        /// ��������� �������� � ������� ��� ����, ����� ������ ���� ���� �����������
        /// </summary>
        public IEnumerable<ObjectContextRequirement> RequiredContext { get; set; }

        /// <summary>
        /// ��������� �������, ��� ������� ������ ���� ������������
        /// </summary>
        public IEnumerable<EstateStatuses> RequiredStatuses { get; set; }

        /// <summary>
        /// �������� ���� � ��������� �������
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// �������, ���������� ����� ������� �������� ������� �������� ���� � �������
        /// </summary>
        public virtual Func<EstateObject, object> GetValueFromObject { get; set; }

        /// <summary>
        /// �������, ����������, ����� ������� ������������� ���� �������
        /// </summary>
        public virtual Action<EstateObject, string> SetValueToObject { get; set; }

        /// <summary>
        /// ��������� ������� ��� �����������, ��������� �� ����������� ��� ���� ��� ���
        /// </summary>
        public virtual Func<FieldRenderingContext, bool> CustomCondition { get; set; }

        /// <summary>
        /// ��������� ������� ����
        /// </summary>
        public FieldValidator Validator { get; set; }

        /// <summary>
        /// ������� ���� � �����. ������������ ��� ����������
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// �������� �� ���������
        /// </summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// ��������������, ������� ��������� ����� ���� �� ���� ������������
        /// </summary>
        public string NotRequired { get; set; }

        /// <summary>
        /// �������� �� ���� ������ ��������
        /// </summary>
        public bool IsNumeric
        {
            get
            {
                if (this.Validator == null)
                {
                    return false;
                }
                var digitsRules = new[] { "floatNumbers", "digits", "min", "max" };
                return this.Validator.Rules.Any(r => digitsRules.Contains(r.Name));
            }
        }

        /// <summary>
        /// ��������� ����������
        /// </summary>
        public string Category { get; set; }

		/// <summary>
		/// ������� �������, ����������� ������������� ��������� ��� �������� � �������� ������
		/// </summary>
		public Func<EstateObject,bool>  CustomRequired { get; set; }

        /// <remarks>
        /// ��������� ���������� �������� ���������� �� ViewEngine
        /// </remarks>
        /// <summary>
        /// ��������� ���������������� ��������� �������� HTML ���� �.�. ������ INPUTA ��������.
        /// </summary>
        /// <param name="context"> </param>
        /// <returns></returns>
        public abstract string RenderFieldEditor(FieldRenderingContext context);

        /// <summary>
        /// ����������� �����������
        /// </summary>
        protected ObjectFormField()
        {
            Validator = new FieldValidator();
            RequiredContext = new List<ObjectContextRequirement>();
            NotRequired = "";
        }

        /// <summary>
        /// ��������� �������� ��������� � ����������� �����
        /// </summary>
        /// <param name="builder">����������� ���������</param>
        protected virtual void ApplyDefaultAttributes(TagBuilder builder)
        {
            // �������� �������� ��������
            builder.Attributes.Add("name", Name);
            builder.Attributes.Add("id", string.IsNullOrEmpty(Id) ? string.Format("{0}-field", Name.ToLower()) : Id);
            if (!string.IsNullOrEmpty(Placeholder))
            {
                builder.Attributes.Add("placeholder", Placeholder);
            }
            builder.AddCssClass("round observable");
            if (!string.IsNullOrEmpty(CustomClasses))
            {
                builder.AddCssClass(CustomClasses);
            }
            if (!string.IsNullOrEmpty(CustomStyles))
            {
                builder.Attributes.Add("style", CustomStyles);
            }
            if (!String.IsNullOrEmpty(Tooltip))
            {
                builder.Attributes.Add("title",Tooltip);
            }
            if (Readonly)
            {
                builder.Attributes.Add("readonly","readonly");
                builder.AddCssClass("readonly");
            }

            // ��������� �����������
            builder.Attributes.Add("autocomplete","off");
        }

        /// <summary>
        /// �������� ��������� ���� � HTML ��������
        /// </summary>
        /// <param name="context"> </param>
        /// <returns>HTML �������� ��������� ��� ����� ����</returns>
        public virtual string RenderField(FieldRenderingContext context)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("<div class=\"field-wrapper\" id=\"{0}-field-wrapper\">\n" +
                                 "<div class=\"editor-label {3} \">{1}</div>\n" +
                                 "<div class=\"editor-field\">\n{2}\n</div>\n" +
                                 "</div>\n", Name, Caption, RenderFieldEditor(context),ActiveStatusValidator.IsFieldRequired(context.EstateObject,this) ? "required-label" : "");
            return builder.ToString();
        }

        /// <summary>
        /// ��������� ����� �� ��������� ���� ���� ������ �� ���������� �������� � ���������
        /// </summary>
        /// <returns>true ���� ���� ������ �����������, ����� false</returns>
        public virtual bool CheckVisibility(FieldRenderingContext context)
        {
            var render = true;
            // �������� ���������� �� ���� ������� � ��������
            if (RequiredContext != null && RequiredContext.Any())
            {
                var hasPermission =
                    RequiredContext.Any(
                        r =>
                        r.RequiredEstateType == context.EstateType && r.RequiredOperation == context.EstateOperation);
                if (!hasPermission)
                {
                    render = false;
                }
            }
            // �������� ���������� �� ������� �������
            if (RequiredStatuses != null && RequiredStatuses.Any())
            {
                var hasPermission = RequiredStatuses.Contains(context.EstateStatus);
                if (!hasPermission)
                {
                    render = false;
                }
            }
            // �������� ���������� �������
            if (CustomCondition != null)
            {
                var hasPermission = CustomCondition(context);
                if (!hasPermission)
                {
                    render = false;
                }
            }
            return render;
        }

        /// <summary>
        /// ��������� �������� ������� ���� �� ��������� ����, ���������� ��������
        /// </summary>
        /// <param name="collection">��������� ����</param>
        public virtual void ReadValueFromFormCollection(FormCollection collection)
        {
            var submittedValue = collection[this.Name];
            this.Value = !(string.IsNullOrEmpty(submittedValue)) ? submittedValue : null;
        }

        /// <summary>
        /// ���������� ������������� �������� ��� ������
        /// </summary>
        /// <returns>�������� ��� ������</returns>
        public virtual string ValueAsString()
        {
            return Value ?? String.Empty;
        }

        /// <summary>
        /// ���������� �������� ��� ������� � ������
        /// </summary>
        /// <param name="filterValue">��������� ������ ��� ����</param>
        /// <returns>��������</returns>
        public string GetFilterMarkup(string filterValue = "")
        {
            var digitsRules = new[] { "floatNumbers", "digits", "min", "max" };
            var fieldInfo = this;
            if (fieldInfo is ObjectFormTextField)
            {
                if (fieldInfo.Validator != null && fieldInfo.Validator.Rules.Any(r => digitsRules.Contains(r.Name)))
                {
                    return String.Format("<select name=\"filter_{0}\">" +
                                     "<option value='='{1}>=</option>" +
                                     "<option value='!='{2}>!=</option>" +
                                     "<option value='>'{3}>></option>" +
                                     "<option value='<'{4}><</option>" +
                                     "<option value='range'{5}>[..]</option>" +
                                     "</select>", fieldInfo.Name, 
                                     (filterValue == "=" ? " selected='selected'":""),
                                     (filterValue == "!=" ? " selected='selected'":""),
                                     (filterValue == ">" ? " selected='selected'":""),
                                     (filterValue == "<" ? " selected='selected'":""),
                                     (filterValue == "range" ? " selected='selected'":"")
                                     );
                }
                else
                {
                    return String.Format("<select name=\"filter_{0}\">" +
                                     "<option value='=' {1}>=</option>" +
                                     "<option value='%'{2}>%</option>" +
                                     "</select>", fieldInfo.Name,
                                     (filterValue == "=" ? " selected='selected'":""),
                                     (filterValue == "%" ? " selected='selected'":"")
                                     );
                }
            }
            return String.Empty;
        }
    }
}