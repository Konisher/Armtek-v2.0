using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

class Program
{
    public static string GenerateApiKey(int length = 32)
    {
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] chars = new char[length];

        using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
        {
            byte[] data = new byte[length];
            crypto.GetBytes(data);

            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[data[i] % validChars.Length];
            }
        }

        return new string(chars);
    }
    static void Main(string[] args)
    {
        string apiKey = GenerateApiKey();
        Console.WriteLine($"Generated API Key: {apiKey}");
        // Sample user location
        double userLatitude = 52.4121;
        double userLongitude = 41.3770;
        double radiusInKm = 1.0; // Specify the radius in kilometers

        // Load bus stops from JSON file
        var publicStations = LoadBusStopsFromJson("public_stations.json");

        // Specify the bus stop you want to check against
        string selectedBusStopTitle = "Кузьминский, поворот";

        // Find the selected bus stop
        var selectedBusStop = publicStations.Find(station => station.Title.Equals(selectedBusStopTitle, StringComparison.OrdinalIgnoreCase));

        if (selectedBusStop != null)
        {
            double distance = CalculateDistance(userLatitude, userLongitude, selectedBusStop.Latitude ?? 0.0, selectedBusStop.Longitude ?? 0.0);

            if (distance <= radiusInKm)
            {
                Console.WriteLine($"User is near the selected bus stop '{selectedBusStopTitle}', Distance: {distance} km");
            }
            else
            {
                Console.WriteLine($"User is NOT near the selected bus stop '{selectedBusStopTitle}', Distance: {distance} km");
            }
        }
        else
        {
            Console.WriteLine($"Selected bus stop '{selectedBusStopTitle}' not found.");
        }

        Console.ReadKey();
    }

    static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula
        var R = 6371; // Radius of Earth in kilometers
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = R * c; // Distance in kilometers

        return distance;
    }

    static double ToRadians(double angle)
    {
        return angle * (Math.PI / 180);
    }

    static List<Station> LoadBusStopsFromJson(string filePath)
    {
        // Load bus stops from JSON file
        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<List<Station>>(json);
    }
}

public class StationsResponse
{
    [JsonProperty("countries")]
    public List<Country> Countries { get; set; }
}

public class Country
{
    [JsonProperty("regions")]
    public List<Region> Regions { get; set; }
}

public class Region
{
    [JsonProperty("settlements")]
    public List<Settlement> Settlements { get; set; }
}

public class Settlement
{
    [JsonProperty("stations")]
    public List<Station> Stations { get; set; }
}

public class Station
{
    [JsonProperty("direction")]
    public string Direction { get; set; }

    [JsonProperty("codes")]
    public Dictionary<string, string> Codes { get; set; }

    [JsonProperty("station_type")]
    public string StationType { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("longitude")]
    public double? Longitude { get; set; }

    [JsonProperty("transport_type")]
    public string TransportType { get; set; }

    [JsonProperty("latitude")]
    public double? Latitude { get; set; }
}
