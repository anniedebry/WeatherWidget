using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using dotenv.net;

namespace WeatherApp
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherService()
        {
            _httpClient = new HttpClient();
            DotEnv.Load();
            _apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY");

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("API key not found in environment variables.");
            }
        }

        public async Task<bool> IsValidCityAsync(string cityName)
        {
            //checking weather API for city validation
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={_apiKey}";
            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<HourlyWeather>> GetHourlyForecastAsync(string cityName)
        {
            string url = $"https://api.openweathermap.org/data/2.5/forecast?q={cityName}&appid={_apiKey}&units=imperial";
            Debug.WriteLine($"Requesting forecast from: {url}");

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Forecast request failed with status: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var listElement = root.GetProperty("list");

            var forecastList = new List<HourlyWeather>();

            foreach (var forecastItem in listElement.EnumerateArray())
            {
                //time stamps 
                long dt = forecastItem.GetProperty("dt").GetInt64();
                DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(dt).LocalDateTime;

                //temperature 
                double temp = forecastItem.GetProperty("main").GetProperty("temp").GetDouble();

                //default weather icons from the API
                var weatherArray = forecastItem.GetProperty("weather");
                string icon = weatherArray[0].GetProperty("icon").GetString();

                forecastList.Add(new HourlyWeather
                {
                    Temperature = ((int)Math.Round(temp)).ToString(),
                    Time = dateTime.ToString("htt").ToLower(),  //formatting for hours
                    IconSource = $"https://openweathermap.org/img/wn/{icon}@2x.png"
                });
            }

            return forecastList;
        }
    }
}
