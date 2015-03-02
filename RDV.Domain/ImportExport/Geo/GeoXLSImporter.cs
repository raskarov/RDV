using System;
using System.IO;
using System.Linq;
using FlexCel.XlsAdapter;
using NLog;
using RDV.Domain.Entities;
using RDV.Domain.Interfaces.ImportExport;
using RDV.Domain.Interfaces.ImportExport.Geo;
using RDV.Domain.Interfaces.Infrastructure;
using RDV.Domain.Core;

namespace RDV.Domain.ImportExport.Geo
{
    /// <summary>
    /// Реализация импортера гео объектов из XLS файлов
    /// </summary>
    public class GeoXLSImporter: GeoImporterBase, IGeoXLSImporter
    {
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        /// <param name="geoManager">менеджер гео данных</param>
        public GeoXLSImporter(IGeoManager geoManager) : base(geoManager)
        {
        }

        /// <summary>
        /// Количество рядов, которое необходимо пропустить от начала таблицы для начала импорта
        /// </summary>
        public int SkipRow { get; set; }

        /// <summary>
        /// Логгер, связанный с текущим классом
        /// </summary>
        private Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Импортирует данные из указанного файла
        /// </summary>
        /// <param name="filename">Имя файла для импорта</param>
        /// <returns>Статистика импорта</returns>
        public override ImportStatistics ImportFile(string filename)
        {
            var xls = new XlsFile(false);
            xls.Open(filename);
            return DoImport(xls);
        }

        /// <summary>
        /// Импортирует данные из указанного потока
        /// </summary>
        /// <param name="stream">Поток с данными</param>
        /// <returns>Статистика импорта</returns>
        public override ImportStatistics ImportStream(Stream stream)
        {
            var xls = new XlsFile(false);
            xls.Open(stream);
            return DoImport(xls);
        }

        /// <summary>
        /// Выполняет импорт из XLS файла и возвращает статистику по процессу
        /// </summary>
        /// <param name="xls">XLS файл</param>
        /// <returns>Статистика по импорту</returns>
        private ImportStatistics DoImport(XlsFile xls)
        {
            var result = new ImportStatistics();

            // Переходим к первой вкладке
            xls.ActiveSheet = 1;

            // Начинаем перебирать данные в ячейках
            var firstRow = 1 + SkipRow;
            for (var currentRowIndex = firstRow; currentRowIndex <= xls.RowCount; currentRowIndex++ )
            {
                // Считываем наименование страны
                var countryName = (string)xls.GetCellValue(currentRowIndex, 1);

                // Проверяем что успешно считали наименование
                if (String.IsNullOrEmpty(countryName))
                {
                    continue;
                }

                // Ищем страну
                var country = GeoManager.CountriesRepository.GetCountryByName(countryName);
                if (country == null)
                {
                    // Создаем страну если ее не найдено
                    country = new GeoCountry()
                                  {
                                      Name = countryName,
                                      DateCreated = DateTimeZone.Now
                                  };
                    GeoManager.CountriesRepository.Add(country);
                    result.ImportedCount = result.ImportedCount + 1;
                    GeoManager.CountriesRepository.SubmitChanges();
                }

                //Считываем наименование региона
                var regionName = (string)xls.GetCellValue(currentRowIndex, 2);

                // Проверяем что успешно считали наименование
                if (String.IsNullOrEmpty(regionName))
                {
                    continue;
                }

                // Ищем регион
                var region = country.GeoRegions.FirstOrDefault(r => r.Name.ToLower() == regionName.ToLower());
                if (region == null)
                {
                    region = new GeoRegion()
                                 {
                                     Name = regionName,
                                     GeoCountry = country,
                                     DateCreated = DateTimeZone.Now
                                 };
                    GeoManager.RegionsRepository.Add(region);
                    country.GeoRegions.Add(region);
                    result.ImportedCount = result.ImportedCount + 1;
                    GeoManager.RegionsRepository.SubmitChanges();
                }

                // Считываем наименование района в регионе
                var regionDistrictName = (string)xls.GetCellValue(currentRowIndex, 3);

                // Проверяем что успешно считали наименование
                if (String.IsNullOrEmpty(regionDistrictName))
                {
                    continue;
                }

                // Ищем район региона
                var regionDistrict =
                    region.GeoRegionDistricts.FirstOrDefault(rd => rd.Name.ToLower() == regionDistrictName.ToLower());
                if (regionDistrict == null)
                {
                    regionDistrict = new GeoRegionDistrict()
                                         {
                                             Name = regionDistrictName,
                                             GeoRegion = region,
                                             DateCreated = DateTimeZone.Now
                                         };
                    GeoManager.RegionsDistrictsRepository.Add(regionDistrict);
                    region.GeoRegionDistricts.Add(regionDistrict);
                    result.ImportedCount = result.ImportedCount + 1;
                    GeoManager.RegionsDistrictsRepository.SubmitChanges();
                }

                // Считываем наименование города
                var cityName = (string)xls.GetCellValue(currentRowIndex, 4);

                // Проверяем что успешно считали наименование
                if (String.IsNullOrEmpty(cityName))
                {
                    continue;
                }
                
                // Ищем город
                var city = regionDistrict.GeoCities.FirstOrDefault(c => c.Name.ToLower() == cityName.ToLower());
                if (city == null)
                {
                    city = new GeoCity()
                               {
                                   Name = cityName,
                                   GeoRegionDistrict = regionDistrict,
                                   DateCreated = DateTimeZone.Now
                               };
                    GeoManager.CitiesRepository.Add(city);
                    regionDistrict.GeoCities.Add(city);
                    result.ImportedCount = result.ImportedCount + 1;
                    GeoManager.CitiesRepository.SubmitChanges();
                }

                // Считываем название района городаа
                var districtName = (string)xls.GetCellValue(currentRowIndex, 5);

                // Проверяем что успешно считали наименование
                if (String.IsNullOrEmpty(districtName))
                {
                    continue;
                }

                // Ищем район
                var district = city.GeoDistricts.FirstOrDefault(d => d.Name.ToLower() == districtName.ToLower());
                if (district == null)
                {
                    district = new GeoDistrict()
                                   {
                                       Name = districtName,
                                       GeoCity = city,
                                       DateCreated = DateTimeZone.Now
                                   };
                    GeoManager.DistrictsRepository.Add(district);
                    city.GeoDistricts.Add(district);
                    result.ImportedCount = result.ImportedCount + 1;
                    GeoManager.DistrictsRepository.SubmitChanges();
                }

                // Считываем название жилого массива
                var residentialAreaName = (string)xls.GetCellValue(currentRowIndex, 6);

                // Проверяем что успешно считали наименование
                if (String.IsNullOrEmpty(residentialAreaName))
                {
                    continue;
                }

                // Ищем жилой массив
                var residentialArea =
                    district.GeoResidentialAreas.FirstOrDefault(a => a.Name.ToLower() == residentialAreaName.ToLower());
                if (residentialArea == null)
                {
                    residentialArea = new GeoResidentialArea()
                                          {
                                              Name = residentialAreaName,
                                              GeoDistrict = district,
                                              DateCreated = DateTimeZone.Now
                                          };
                    GeoManager.ResidentialAreasRepository.Add(residentialArea);
                    district.GeoResidentialAreas.Add(residentialArea);
                    result.ImportedCount = result.ImportedCount + 1;
                    GeoManager.ResidentialAreasRepository.SubmitChanges();
                }

                // Считываем наименование улицы
                var streetName = (string)xls.GetCellValue(currentRowIndex, 7);

                // Проверяем что успешно считали наименование
                if (String.IsNullOrEmpty(streetName))
                {
                    continue;
                }
                
                // Ищем улицу
                var street = residentialArea.GeoStreets.FirstOrDefault(s => s.Name.ToLower() == streetName.ToLower());
                if (street == null)
                {
                    street = new GeoStreet()
                                 {
                                     Name = streetName,
                                     GeoResidentialArea = residentialArea,
                                     DateCreated = DateTimeZone.Now
                                 };
                    GeoManager.StreetsRepository.Add(street);
                    residentialArea.GeoStreets.Add(street);
                    result.ImportedCount = result.ImportedCount + 1;
                    GeoManager.StreetsRepository.SubmitChanges();
                }

                // Считываем наименование объекта
                var objectName = (string)xls.GetCellValue(currentRowIndex, 8);

                // Проверяем что успешно считали наименование
                if (String.IsNullOrEmpty(objectName))
                {
                    continue;
                }

                // Ищем этот объект на улице
                var geoObject = street.GeoObjects.FirstOrDefault(o => o.Name.ToLower() == objectName.ToLower());
                if (geoObject == null)
                {
                    geoObject = new GeoObject()
                                    {
                                        GeoStreet = street,
                                        Name = objectName,
                                        DateCreated = DateTimeZone.Now
                                    };
                    GeoManager.ObjectsRepository.Add(geoObject);
                    street.GeoObjects.Add(geoObject);
                    result.ImportedCount = result.ImportedCount + 1;
                    GeoManager.ObjectsRepository.SubmitChanges();
                }
            }

            return result;
        }
    }
}