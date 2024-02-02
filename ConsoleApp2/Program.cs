using ConsoleApp1.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

class Program
{
    private static string apiKey = "a144e13e-59e5-4c9e-a40a-717782010126";
    private const string apiUrlFormat = "https://search-maps.yandex.ru/v1/?text={0}, остановка, Минск&lang=ru_RU&apikey={1}";

    private enum HttpStatusCode
    {
        Success = 200,
        NotFound = 404,
        BadRequest = 400
    }

    static async Task Main()
    {
        string userQuery = GetUserInput();
        string apiUrl = string.Format(apiUrlFormat, userQuery, apiKey);

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (IsSuccessStatusCode(response))
            {
                ProcessSuccessResponse(await response.Content.ReadAsStringAsync(), userQuery);
            }
            else
            {
                ProcessErrorResponse(response);
            }
        }
        Console.ReadKey();
    }

    private static string GetUserInput()
    {
        Console.Write("Введите название остановки транспорта: ");
        Console.ForegroundColor = ConsoleColor.Red;
        return Console.ReadLine();
    }

    private static bool IsSuccessStatusCode(HttpResponseMessage response)
    {
        return response.StatusCode == (System.Net.HttpStatusCode)HttpStatusCode.Success;
    }

    private static void ProcessSuccessResponse(string responseBody, string userQuery)
    {
        var jsonResponse = DeserializeJsonResponse(responseBody);

        if (jsonResponse != null && jsonResponse.Features != null && jsonResponse.Features.Count > 0)
        {
            Console.WriteLine("Список найденных остановок:");
            List<BusStop> busStops = ProcessFeatures(jsonResponse.Features);

            SaveAndDisplayResults(userQuery, busStops);
        }
        else
        {
            Console.WriteLine("Остановки не найдены.");
        }
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
        BusStopsData busStopsData = new BusStopsData
        {
            Query = userQuery,
            BusStops = busStops
        };

        string jsonOutput = JsonConvert.SerializeObject(busStopsData, Formatting.Indented);
        File.WriteAllText("StationsWithCoordinates.json", jsonOutput);
        Console.WriteLine($"Кол-во данных:{busStops.Count}\n");
        Console.WriteLine("Результаты сохранены в StationsWithCoordinates.json");
        Process.Start("StationsWithCoordinates.json");
    }

    private static void ProcessErrorResponse(HttpResponseMessage response)
    {
        Console.WriteLine($"Ошибка при выполнении запроса: {HttpStatusCode.NotFound} {response.StatusCode}");
    }
}