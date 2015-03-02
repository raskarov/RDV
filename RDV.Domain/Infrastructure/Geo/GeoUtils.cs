using System;
using System.Collections.Generic;
using System.Linq;
using RDV.Domain.IoC;

namespace RDV.Domain.Infrastructure.Geo
{
    /// <summary>
    /// Класс содержащий географические утилиты
    /// </summary>
    public static class GeoUtils
    {
        /// <summary>
        /// Парсит строку с координатами границы объектов и возвращает ее в виде массива
        /// </summary>
        /// <param name="bounds">Массив объектов</param>
        /// <returns>Массив координат</returns>
        public static object[] ParseBoundsCoordinates(string bounds)
        {
            // Список координат
            var coordinatesList = (from part in bounds.Split(new[] {"|"}, StringSplitOptions.RemoveEmptyEntries)
                                   select part.Split(',')
                                   into subParts let lon = Convert.ToDouble(subParts[0].Replace('.', ','))
                                   let lat = Convert.ToDouble(subParts[1].Replace('.', ',')) select new[] {lon, lat}).
                Cast<object>().ToList();
            return new[] {coordinatesList};
        }
    }
}