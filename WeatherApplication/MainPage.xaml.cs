using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;

namespace WeatherApplication
{
    public partial class MainPage : ContentPage
    {
        private const string ApiKey = "beb5ad58dd1c49f042037a254a91c1d4"; 
        private const string BaseUrl = "https://api.openweathermap.org/data/2.5/weather";

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnGetWeatherClicked(object sender, EventArgs e)
        {
            string city = CityEntry.Text?.Trim(); // Get city name from Entry field
            if (string.IsNullOrEmpty(city))
            {
                await DisplayAlert("Error", "Please enter a city name.", "OK");
                return;
            }

            string url = $"{BaseUrl}?q={city}&appid={ApiKey}&units=metric";

            try
            {
                string weatherInfo = await FetchWeatherDataAsync(url);
                DisplayWeather(weatherInfo);
            }
            catch (HttpRequestException httpEx)
            {
                await DisplayAlert("Error", $"Unable to connect to the server: {httpEx.Message}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Unable to get weather updates: {ex.Message}", "OK");
            }
        }

        private async Task<string> FetchWeatherDataAsync(string url)
        {
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                // Handle specific HTTP error codes
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("Invalid API key. Please check your API key.");
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new Exception("City not found. Please check the city name.");
                else
                    throw new Exception($"HTTP Error: {response.StatusCode}");
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            return jsonResponse;
        }

        private void DisplayWeather(string jsonResponse)
        {
            var weatherData = JsonConvert.DeserializeObject<WeatherResponse>(jsonResponse);

            string weatherDetails =
                $"City: {weatherData.Name}\n" +
                $"Temperature: {weatherData.Main.Temp}°C\n" +
                $"Feels Like: {weatherData.Main.Feels_Like}°C\n" +
                $"Humidity: {weatherData.Main.Humidity}%\n" +
                $"Condition: {weatherData.Weather[0].Description}\n" +
                $"Wind Speed: {weatherData.Wind.Speed} m/s";

            WeatherLabel.Text = weatherDetails; // Display weather information
        }
    }

    // JSON Model Classes
    public class WeatherResponse
    {
        public string Name { get; set; }
        public Main Main { get; set; }
        public Weather[] Weather { get; set; }
        public Wind Wind { get; set; }
    }

    public class Main
    {
        public double Temp { get; set; }
        public double Feels_Like { get; set; }
        public int Humidity { get; set; }
    }

    public class Weather
    {
        public string Description { get; set; }
    }

    public class Wind
    {
        public double Speed { get; set; }
    }
}
