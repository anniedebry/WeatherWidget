using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace WeatherApp
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<HourlyWeather> HourlyForecast { get; set; }
        private WeatherService _weatherService;
        public string CityName { get; set; }
        public string CurrentTemperature { get; set; }
        public string HighTemperature { get; set; }
        public string LowTemperature { get; set; }
        public string WeatherBackground { get; set; }
        public bool PixelSelected { get; set; } = false;

        //keep track of cities for the action sheet
        private List<string> savedCities = new List<string> { "Salt Lake City" };

        public MainPage()
        {
            InitializeComponent();
            _weatherService = new WeatherService();
            GenerateWeatherDataAsync("Salt Lake City");
        }

        private async void OnCityButtonClicked(object sender, EventArgs e)
        {
            var options = new List<string>(savedCities);
            options.Add("Add new city...");

            string action = await DisplayActionSheet("Select City", "Cancel", null, options.ToArray());

            if (action == "Add new city...")
            {
                string city = await DisplayPromptAsync("Add City", "Enter a city name:");
                if (!string.IsNullOrWhiteSpace(city))
                {
                    bool isValidCity = await _weatherService.IsValidCityAsync(city);
                    if (isValidCity)
                    {
                        city = ToTitleCase(city);
                        savedCities.Add(city);
                        await DisplayAlert("Success", $"Weather data for '{city}' added.", "OK");
                        await GenerateWeatherDataAsync(city);
                    }
                    else
                    {
                        await DisplayAlert("Error", $"City '{city}' not found.", "OK");
                    }
                }
            }
            else if (!string.IsNullOrEmpty(action) && action != "Cancel")
            {
                await GenerateWeatherDataAsync(action);
            }
        }

        private async void OnStyleClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("Select Style", "Cancel", null, "Default", "Pixel");

            if (action == "Pixel")
            {
                Application.Current.Resources["AppFont"] = "PixelFont";
                PixelSelected = true;
            }
            else if (action == "Default")
            {
                Application.Current.Resources["AppFont"] = "OpenSansRegular";
                PixelSelected = false;
                Application.Current.Resources["AppBackground"] = "clear.png";
            }

            await CheckCurrentWeather();
        }

        private async Task CheckCurrentWeather()
        {
            BindingContext = null;

            var hourlyForecast = await _weatherService.GetHourlyForecastAsync(CityName);

            if (hourlyForecast != null)
            {
                HourlyForecast = new ObservableCollection<HourlyWeather>(hourlyForecast.Take(24));

                if (PixelSelected)
                {
                    WeatherBackground = GetBackgroundImageForWeatherPixel(hourlyForecast[0].IconSource);
                    Application.Current.Resources["AppBackground"] = WeatherBackground;
                }
                else
                {
                    WeatherBackground = "clear.png";
                    Application.Current.Resources["AppBackground"] = WeatherBackground;
                }
                BindingContext = this;
            }
            else
            {
                await DisplayAlert("Error", $"Could not retrieve weather for {CityName}.", "OK");
            }
        }

        private async Task GenerateWeatherDataAsync(string cityName)
        {
            BindingContext = null;

            var hourlyForecast = await _weatherService.GetHourlyForecastAsync(cityName);

            if (hourlyForecast != null)
            {
                HourlyForecast = new ObservableCollection<HourlyWeather>(hourlyForecast.Take(24));

                CityName = cityName.ToUpper();
                CurrentTemperature = hourlyForecast[0].Temperature;
                HighTemperature = hourlyForecast.Max(h => int.Parse(h.Temperature)).ToString();
                LowTemperature = hourlyForecast.Min(h => int.Parse(h.Temperature)).ToString();

                if (PixelSelected)
                {
                    WeatherBackground = GetBackgroundImageForWeatherPixel(hourlyForecast[0].IconSource);
                    Application.Current.Resources["AppBackground"] = WeatherBackground;
                }
                else
                {
                    WeatherBackground = "clear.png";
                    Application.Current.Resources["AppBackground"] = WeatherBackground;
                }
                BindingContext = this;
            }
            else
            {
                await DisplayAlert("Error", $"Could not retrieve weather for {cityName}.", "OK");
            }
        }

        private string GetBackgroundImageForWeatherPixel(string iconSource)
        {
            if (iconSource.Contains("01")) return "sunny.png";
            if (iconSource.Contains("02")) return "cloudy.png";
            if (iconSource.Contains("09") || iconSource.Contains("10")) return "rain.png";
            if (iconSource.Contains("13")) return "snow.png";
            return "clearperfect.png";
        }

        private string ToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }
    }
}
