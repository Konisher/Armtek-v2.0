using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TextCopy;

namespace ConsoleApp3
{
class Program
    {
        static async Task Main(string[] args)
        {
            string baseUrl = "http://localhost:5088/";

            Console.WriteLine("Welcome to the Console App for interacting with the Web API!");

            while (true)
            {
                Console.WriteLine("\nChoose an option:");
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Register");
                Console.WriteLine("3. Get UserInfo");
                Console.WriteLine("4. Exit");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await Login(baseUrl);
                        break;
                    case "2":
                        await Register(baseUrl);
                        break;
                    case "3":
                        await GetUserInfo(baseUrl);
                        break;
                    case "4":
                        Console.WriteLine("Exiting the application...");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        static async Task Login(string baseUrl)
        {
            Console.WriteLine("\nEnter your email:");
            string email = Console.ReadLine();

            Console.WriteLine("Enter your password:");
            string password = Console.ReadLine();

            using (var httpClient = new HttpClient())
            {
                var data = new { 
                    Email = email, 
                    Password = password 
                };
                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(baseUrl + "token?username=" + email + "&password=" + password, null);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("\nLogin successful!");
                    Console.WriteLine("Token: " + result);
                    var tokenObject = JObject.Parse(result);

                    // Extract the 'access_token' value
                    string accessToken = tokenObject["access_token"].ToString();

                    // Copy the token to the clipboard
                    var clipboard = new Clipboard();
                    clipboard.SetText(accessToken);
                }
                else
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("\nLogin failed. Error message: " + result);
                }

            }
        }



        static async Task Register(string baseUrl)
        {
            Console.WriteLine("\nEnter your email:");
            string email = Console.ReadLine();

            Console.WriteLine("Enter your password:");
            string password = Console.ReadLine();

            using (var httpClient = new HttpClient())
            {
                var data = new { 
                    Email = email, 
                    Password = password 
                };
                var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(baseUrl + "api/Account/Register", content);
                var result = await response.Content.ReadAsStringAsync();

                Console.WriteLine("\n" + result);
            }
        }

        static async Task GetUserInfo(string baseUrl)
        {
            int bufSize = 1024;
            Stream inStream = Console.OpenStandardInput(bufSize);
            Console.SetIn(new StreamReader(inStream, Console.InputEncoding, false, bufSize));


            Console.WriteLine("\nRetrieving user information...");

            Console.WriteLine("Enter your JWT token:");
            string token = Console.ReadLine();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                Console.WriteLine("Authorization Header: " + httpClient.DefaultRequestHeaders.Authorization);
                var response = await httpClient.GetAsync(baseUrl + "api/Account/UserInfo");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("\nUser Info: " + content);
                }
                else
                {
                    Console.WriteLine("\nFailed to retrieve user info. Error message: " + response.ReasonPhrase);
                }
            }
        }

    }
}
