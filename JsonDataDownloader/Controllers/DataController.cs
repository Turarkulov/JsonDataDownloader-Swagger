using System.Text.Json;

using JsonDataDownloader.Models;

using Microsoft.AspNetCore.Mvc;

using Npgsql;

namespace JsonDataDownloader.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DownloadController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public DownloadController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;

            var apiUrl = _configuration.GetValue<string>("ConnectionStrings:ApiUrl");
            _httpClient.BaseAddress = new Uri(apiUrl);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync()
        {
            var properties = _configuration.GetSection("ConnectionStrings:Properties").Get<List<string>>();

            try
            {
                var response = await _httpClient.GetAsync("");
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();

                var jsonDocument = JsonDocument.Parse(jsonString);
                var propertyValues = new List<string>();
                foreach (var property in properties)
                {
                    if (jsonDocument.RootElement.TryGetProperty(property, out var value))
                    {
                        propertyValues.Add(value.ToString());
                    }
                    else
                    {
                        propertyValues.Add(null);
                    }
                }

                var connectionString = _configuration.GetValue<string>("PgDbContextConnection");
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var commandText = "INSERT INTO JsonData (Name) VALUES (@Name)";
                    using (var command = new NpgsqlCommand(commandText, connection))
                    {
                        command.Parameters.AddWithValue("Name", propertyValues.ElementAtOrDefault(0) ?? (object)DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok("Data downloaded and saved to the database.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
