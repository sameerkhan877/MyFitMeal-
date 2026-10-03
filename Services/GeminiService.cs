using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration; // IConfiguration ke liye zaroori hai
using MyFitMeal.DTOs;

namespace MyFitMeal.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        // IConfiguration ko constructor mein inject kiya gaya hai
        public GeminiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            // appsettings.json se key fetch ho rahi hai
            _apiKey = config["Gemini:ApiKey"] ?? string.Empty;
        }

        public async Task<List<DailyDietDto>?> GetDietPlan(string prompt)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    Console.WriteLine("Error: Gemini API Key is missing in configuration.");
                    return null;
                }

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key={_apiKey}";

                var requestBody = new
                {
                    contents = new[] { new { parts = new[] { new { text = prompt } } } },
                    generationConfig = new
                    {
                        response_mime_type = "application/json",
                        temperature = 0.7
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonString);

                    var textResponse = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    if (string.IsNullOrEmpty(textResponse)) return null;

                    return JsonSerializer.Deserialize<List<DailyDietDto>>(textResponse,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    var errorDetail = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Gemini API Error: {response.StatusCode} - {errorDetail}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gemini Service Crash: {ex.Message}");
                return null;
            }
            return null;
        }
    }
}