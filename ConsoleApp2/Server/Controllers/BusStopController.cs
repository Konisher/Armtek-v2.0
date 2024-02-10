using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ConsoleApp1.Model;
using System.Security.Cryptography;
using System.Diagnostics;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using TextCopy;


namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BusStopController : ControllerBase
    {
        private static readonly string apiKeyYandexStop = "a144e13e-59e5-4c9e-a40a-717782010126";

        private const string apiUrlFormat = "https://search-maps.yandex.ru/v1/?text={0},остановка,Минск&lang=ru_RU&apikey={1}";
        /*private const string apiUrlFormat = "https://search-maps.yandex.ru/v1/?text={0}, остановка, Минск&lang=ru_RU&apikey={1}";*/
        //https://search-maps.yandex.ru/v1/?text=Метро малиновка, остановка, Минск&lang=ru_RU&apikey=a144e13e-59e5-4c9e-a40a-717782010126

        private readonly ILogger<BusStopController> _logger;

        private static List<string> validApiKeys = new();
        private static List<BusStop> busStops = new();

        public BusStopController(ILogger<BusStopController> logger)
        {
            _logger = logger;
            _logger.LogInformation("Logger initialized successfully.");
        }


        [HttpGet("GetBusStops")]
        [SwaggerOperation(Summary = "Get bus stops", Description = "Retrieve bus stops based on user query.")]
        public async Task<IActionResult> GetBusStops(string userQuery, [FromQuery] string apikey)
        {
            try
            {
                if (!ValidateApiKey(apikey) || string.IsNullOrEmpty(apikey))
                {
                    _logger.LogError("Error 401: An error occurred in GetBusStops. Invalid API key");
                    return StatusCode(401, "Invalid API key");
                }
                string apiUrl = string.Format(apiUrlFormat, userQuery, apiKeyYandexStop);


                using HttpClient client = new();
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    busStops.Clear();
                    var responseBody = await response.Content.ReadAsStringAsync();
                    busStops = ProcessSuccessResponse(responseBody, userQuery, GetBusStops());
                    if (busStops != null)
                    {
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
                        _logger.LogError("Error 500: executing the request");
                        return StatusCode(500, "Error executing the request");
                    }
                }
                else
                {
                    _logger.LogError("Error 500: executing the request");
                    return StatusCode(500, "Error executing the request");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error: {ex}");
                return StatusCode(520, "Unknown Error");
            }
        }
        [HttpGet("GetBusStopById/{id}")]
        [SwaggerOperation(Summary = "Get bus stop by ID", Description = "Retrieve a bus stop based on its identifier.")]
        public IActionResult GetBusStopById(int id, [FromQuery] string? apikey)
        {
            if (string.IsNullOrEmpty(apikey) || !ValidateApiKey(apikey))
            {
                _logger.LogError("Error 401: An error occurred in GetBusStops. Invalid API key");
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
                _logger.LogError("Error 404: Bus stop not found");
                return StatusCode(404, "Bus stop not found");
            }
        }

        [HttpDelete("DeleteBusStop/{id}")]
        [SwaggerOperation(Summary = "Remove a bus stop", Description = "Remove busStop")]
        public IActionResult DeleteBusStop(int id, [FromQuery] string? apikey)
        {
            if (string.IsNullOrEmpty(apikey) || !ValidateApiKey(apikey))
            {
                _logger.LogError("Error 401: An error occurred in GetBusStops. Invalid API key");
                return StatusCode(401, "Invalid API key");
            }

            busStops.RemoveAt(id);

            return Ok("Bus stop removed successfully");
        }

        [HttpPost("AddBusStop")]
        [SwaggerOperation(Summary = "Add a new bus stop", Description = "Add a new bus stop to the list.")]
        public IActionResult AddBusStop([FromBody] BusStop newBusStop, [FromQuery] string? apikey)
        {
            if (string.IsNullOrEmpty(apikey) || !ValidateApiKey(apikey))
            {
                _logger.LogError("Error 401: An error occurred in GetBusStops. Invalid API key");
                return StatusCode(401, "Invalid API key");
            }

            busStops.Add(newBusStop);

            return Ok("Bus stop added successfully");
        }

        [Authorize(Roles = "admin")]
        [HttpGet("GetNewApi")]
        [SwaggerOperation(Summary = "Get API Key", Description = "Generation new API Key")]
        public async Task<IActionResult> GetNewApiAsync()
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-";
            char[] chars = new char[32];

            using (RNGCryptoServiceProvider crypto = new())
            {
                byte[] data = new byte[32];
                await Task.Run(() =>
                {
                    crypto.GetBytes(data);
                });

                for (int i = 0; i < 32; i++)
                {
                    chars[i] = validChars[data[i] % validChars.Length];
                }
            }

            string apiKey = new(chars);

            while (apiKey[0] == '-' || apiKey[^1] == '-' || !apiKey.Contains('-'))
            {
                using (RNGCryptoServiceProvider crypto = new())
                {
                    byte[] data = new byte[32];
                    await Task.Run(() =>
                    {
                        crypto.GetBytes(data);
                    });

                    for (int i = 0; i < 32; i++)
                    {
                        chars[i] = validChars[data[i] % validChars.Length];
                    }
                }

                apiKey = new(chars);
            }

            // Hash the API key using SHA-256
            string hashedApiKey = ComputeSHA256Hash(apiKey);

            Console.WriteLine(hashedApiKey);
            validApiKeys.Add(hashedApiKey);
            ClipboardService.SetText(apiKey);

            var keysObject = new
            {
                OriginalApiKey = apiKey,
                HashedApiKey = hashedApiKey
            };

            var jsonOutput = JsonConvert.SerializeObject(keysObject, Formatting.Indented);

            Console.WriteLine(jsonOutput);
            validApiKeys.Add(hashedApiKey);

            return new ContentResult
            {
                Content = jsonOutput,
                ContentType = "application/json; charset=utf-8",
                StatusCode = 200
            };
        }


        private static string ComputeSHA256Hash(string input)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder builder = new();
            for (int i = 0; i < hashedBytes.Length; i++)
            {
                builder.Append(hashedBytes[i].ToString("x2"));
            }

            return builder.ToString();

        }

        private static List<BusStop>? GetBusStops()
        {
            return busStops;
        }

        private static List<BusStop> ProcessSuccessResponse(string responseBody, string userQuery, List<BusStop>? busStops)
        {
            var jsonResponse = DeserializeJsonResponse(responseBody);


            if (jsonResponse != null && jsonResponse.Features != null && jsonResponse.Features.Count > 0)
            {
                Console.WriteLine("Список найденных остановок:");
                busStops = ProcessFeatures(jsonResponse.Features);
                if (busStops != null)
                {
                    SaveAndDisplayResults(userQuery, busStops);
                }
            }
            else
            {
                Console.WriteLine("Остановки не найдены.");
            }

            return busStops;
        }
        private static YandexApiResponse? DeserializeJsonResponse(string responseBody)
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
            List<BusStop> busStops = new();

            foreach (var feature in features)
            {
                var categories = feature.Properties.CompanyMetaData.Categories;

                if (categories.Any(c => c.Name.Contains("Остановка общественного транспорта")))
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
            }

            return busStops;
        }


        private static void SaveAndDisplayResults(string userQuery, List<BusStop> busStops)
        {
            try
            {
                BusStopsData busStopsData = new()
                {
                    Query = userQuery,
                    BusStops = busStops
                };

                string jsonOutput = JsonConvert.SerializeObject(busStopsData, Formatting.Indented);
                //System.IO.File.WriteAllText("StationsWithCoordinates.json", jsonOutput);
                Console.WriteLine($"Кол-во данных:{busStops.Count}\n");
                //Console.WriteLine("Результаты сохранены в StationsWithCoordinates.json");
                //Process.Start("explorer.exe", "StationsWithCoordinates.json");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");

            }
        }
        private static bool ValidateApiKey(string apiKeyToValidate)
        {
            string hashedApiKey = ComputeSHA256Hash(apiKeyToValidate);

            return validApiKeys.Contains(hashedApiKey);
        }
    }
}
