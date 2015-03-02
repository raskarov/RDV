using System;
using System.Text;

namespace RDV.Domain.Entities
{
    /// <summary>
    /// Представляет адрес объекта
    /// </summary>
    public partial class Address
    {
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (GeoCountry != null)
                sb.Append(GeoCountry.Name);
            if (GeoRegion != null)
                sb.AppendFormat(", {0}", GeoRegion.Name);
            if (GeoRegionDistrict != null)
                sb.AppendFormat(", {0}", GeoRegionDistrict.Name);
            if (GeoCity != null)
                sb.AppendFormat(", {0}", GeoCity.Name);
            if (GeoDistrict != null)
                sb.AppendFormat(", {0}", GeoDistrict.Name);
            if (GeoResidentialArea != null)
                sb.AppendFormat(", {0}", GeoResidentialArea.Name);
            if (GeoStreet != null)
                sb.AppendFormat(", {0}", GeoStreet.Name);
            if (sb.Length == 0)
            {
                sb.AppendFormat("Адрес не указан");
                if (Latitude.HasValue && Logitude.HasValue)
                {
                    sb.AppendFormat("(широта = {0:0.00000}; долгота = {1:0.00000}",Latitude.Value,Logitude.Value);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Возвращает адрес в укороченном формате с номером дома, если он присутствует
        /// </summary>
        /// <returns></returns>
        public string ToShortAddressString()
        {
            var sb = new StringBuilder();
            if (GeoCity != null)
                sb.AppendFormat("{0}", GeoCity.Name);
            if (GeoDistrict != null)
                sb.AppendFormat(", {0}", GeoDistrict.Name);
            if (GeoStreet != null)
                sb.AppendFormat(", {0}", GeoStreet.Name);
            if (!String.IsNullOrEmpty(this.House))
            {
                sb.AppendFormat(", {0}", House);
            }
            if (sb.Length == 0)
            {
                sb.AppendFormat("Адрес не указан");
                if (Latitude.HasValue && Logitude.HasValue)
                {
                    sb.AppendFormat("(широта = {0:0.00000}; долгота = {1:0.00000}", Latitude.Value, Logitude.Value);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Возвращает дополнительную информацию по адресу (район и жил массив)
        /// </summary>
        /// <returns></returns>
        public string GetAdditionalAddressInfo()
        {
            var sb = new StringBuilder();
            if (GeoDistrict != null)
                sb.AppendFormat("{0}", GeoDistrict.Name);
            if (GeoResidentialArea != null)
                sb.AppendFormat(": {0}", GeoResidentialArea.Name);
            return sb.ToString();
        }
    }
}