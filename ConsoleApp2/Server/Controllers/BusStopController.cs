using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ConsoleApp1.Model;
using System.Diagnostics;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BusStopController : ControllerBase
    {
        private static string apiKeyYandex = "a144e13e-59e5-4c9e-a40a-717782010126";

        private const string apiUrlFormat = "https://search-maps.yandex.ru/v1/?text={0}, остановка, Минск&lang=ru_RU&apikey={1}";

        private static List<string> validApiKeys = new List<string>
        {
            "a144e13e-59e5-4c9e-a40a-717782010126",
            "1234-5678"
        };

        private static List<BusStop> busStops = new List<BusStop>();

        [HttpGet("GetBusStops")]
        [SwaggerOperation(Summary = "Get bus stops", Description = "Retrieve bus stops based on user query.")]
        public async Task<IActionResult> GetBusStops(string userQuery, [FromQuery] string apikey)
        {
            if (!IsValidApiKey(apikey) || string.IsNullOrEmpty(apikey))
            {
                return StatusCode(401, "Invalid API key");
            }
            string apiUrl = string.Format(apiUrlFormat, userQuery, apiKeyYandex);


            using (HttpClient client = new HttpClient())
            {
                apiUrl += $"&apikey={apikey}";
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var busStops = ProcessSuccessResponse(responseBody, userQuery);

                    var jsonOutput = JsonConvert.SerializeObject(busStops, Formatting.Indented);

                    return new ContentResult
                    {
                        Content = jsonOutput,
                        ContentType = "application/json; charset=utf-8",
                        StatusCode = 200
                    };
                }
                else
                {
                    return StatusCode(500, "Error executing the request");
                }
            }
        }
        [HttpGet("GetBusStopById/{id}")]
        [SwaggerOperation(Summary = "Get bus stop by ID", Description = "Retrieve a bus stop based on its identifier.")]
        public IActionResult GetBusStopById(int id, [FromQuery] string apikey)
        {
            if (!IsValidApiKey(apikey) || string.IsNullOrEmpty(apikey))
            {
                return StatusCode(401, "Invalid API key");
            }

            var busStop = busStops.FirstOrDefault(bs => bs.Count == id);

            if (busStop != null)
            {
                var jsonOutput = JsonConvert.SerializeObject(busStop, Formatting.Indented);

                return new ContentResult
                {
                    Content = jsonOutput,
                    ContentType = "application/json; charset=utf-8",
                    StatusCode = 200
                };
            }
            else
            {
                return StatusCode(404, "Bus stop not found");
            }
        }
        


        private static List<BusStop> ProcessSuccessResponse(string responseBody, string userQuery)
        {
            var jsonResponse = DeserializeJsonResponse(responseBody);


            if (jsonResponse != null && jsonResponse.Features != null && jsonResponse.Features.Count > 0)
            {
                Console.WriteLine("Список найденных остановок:");
                busStops = ProcessFeatures(jsonResponse.Features);
                SaveAndDisplayResults(userQuery, busStops);
            }
            else
            {
                Console.WriteLine("Остановки не найдены.");
            }

            return busStops;
        }
        private static YandexApiResponse DeserializeJsonResponse(string responseBody)
        {
            try
            {
                return JsonConvert.DeserializeObject<YandexApiResponse>(responseBody);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Ошибка при десериализации JSON: {ex.Message}");
                return null;
            }
        }

        private static List<BusStop> ProcessFeatures(List<Feature> features)
        {
            int count = 1;
            List<BusStop> busStops = new List<BusStop>();

            foreach (var feature in features)
            {
                var name = feature.Properties.Name;
                var latitude = feature.Geometry.Coordinates[1];
                var longitude = feature.Geometry.Coordinates[0];
                var uri = feature.Properties.CompanyMetaData.Url;
                var companyName = feature.Properties.CompanyMetaData.Name;
                var address = feature.Properties.CompanyMetaData.Address;
                var companyId = feature.Properties.CompanyMetaData.Id;
                var CategoriesClasses = feature.Properties.CompanyMetaData.Categories.Select(c => c.Class).ToList();
                var CategoriesName = feature.Properties.CompanyMetaData.Categories.Select(n => n.Name).ToList();
                var boundedBy = feature.Properties.BoundedBy;
                var text = feature.Properties.CompanyMetaData.Hours?.Text ?? "Информация о часах работы отсутствует";
                var availabilities = feature.Properties.CompanyMetaData.Hours?.Availabilities ?? new List<Availabilities>();

                Console.WriteLine($"{name} \n\tШирота: {latitude}, " +
                                     $"\n\tДолгота: {longitude}, " +
                                     $"\n\tОфф. сайт: {uri}, " +
                                     $"\n\tНазвание: {companyName}, " +
                                     $"\n\tАдрес: {address}, " +
                                     $"\n\tID: {companyId}, " +
                                     $"\n\tКласс: {string.Join(", ", CategoriesClasses)}, " +
                                     $"\n\tНазвание: {string.Join(", ", CategoriesClasses)}" +
                                     $"\n\tКоординаты: {string.Join(", ", boundedBy[0])}, " +
                                     $"{string.Join(", ", boundedBy[1])}" +
                                     $"\n\tЧасы работы: {text}");

                busStops.Add(new BusStop
                {
                    Count = count++,
                    Name = name,
                    Latitude = latitude,
                    Longitude = longitude,
                    CompanyMetaData = new CompanyMetaData
                    {
                        Id = companyId,
                        Url = uri,
                        Name = companyName,
                        Address = address,
                        Categories = CategoriesClasses.Select((c, index) => new Categories
                        {
                            Class = c,
                            Name = CategoriesName[index]
                        }).ToList(),
                        Hours = new Hours
                        {
                            Text = text,
                            Availabilities = availabilities.Select(a => new Availabilities
                            {
                                Intervals = a.Intervals,
                                Everyday = a.Everyday
                            }).ToList()
                        }
                    },
                    BoundedBy = boundedBy
                });

            }

            return busStops;
        }

        private static void SaveAndDisplayResults(string userQuery, List<BusStop> busStops)
        {
            try
            {
                BusStopsData busStopsData = new BusStopsData
                {
                    Query = userQuery,
                    BusStops = busStops
                };

                string jsonOutput = JsonConvert.SerializeObject(busStopsData, Formatting.Indented);
                //System.IO.File.WriteAllText("StationsWithCoordinates.json", jsonOutput);
                Console.WriteLine($"Кол-во данных:{busStops.Count}\n");
                Console.WriteLine("Результаты сохранены в StationsWithCoordinates.json");
                //Process.Start("explorer.exe", "StationsWithCoordinates.json");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");

            }
        }
        private bool IsValidApiKey(string apiKey)
        {
            return validApiKeys.Contains(apiKey);
        }
    }
}
