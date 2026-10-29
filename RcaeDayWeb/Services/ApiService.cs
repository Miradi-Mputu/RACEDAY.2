using System.Text;
using System.Text.Json;

namespace RaceDayWeb.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;

            //  Make sure this matches your API's port!
            _httpClient.BaseAddress = new Uri("https://localhost:7234/");
        }

        // This grabs the session cookie saved in the MVC and sends it to the API
        private void AttachSessionCookie()
        {
            var sessionId = _httpContextAccessor.HttpContext?.Session.GetString("ApiSessionId");
            if (!string.IsNullOrEmpty(sessionId))
            {
                _httpClient.DefaultRequestHeaders.Remove("Cookie");
                _httpClient.DefaultRequestHeaders.Add("Cookie", $".AspNetCore.Session={sessionId}");
            }
        }

        // --- LOGIN ---
        public async Task<(bool IsSuccess, string Role)> LoginAsync(object loginDto)
        {
            var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                // Save the API session cookie into the MVC session
                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    var sessionCookie = cookies.FirstOrDefault(c => c.StartsWith(".AspNetCore.Session="));
                    if (sessionCookie != null)
                    {
                        var sessionId = sessionCookie.Split(';')[0].Replace(".AspNetCore.Session=", "");
                        _httpContextAccessor.HttpContext?.Session.SetString("ApiSessionId", sessionId);
                    }
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(json);
                var role = result.GetProperty("role").GetString() ?? "";

                return (true, role);
            }
            return (false, "");
        }

        // --- GENERIC GET ---
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            AttachSessionCookie();
            var response = await _httpClient.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return default;
        }

        // --- GENERIC POST ---
        public async Task<bool> PostAsync(string endpoint, object data)
        {
            AttachSessionCookie();
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            return response.IsSuccessStatusCode;
        }

        // --- GENERIC PUT ---
        public async Task<bool> PutAsync(string endpoint, object data)
        {
            AttachSessionCookie();
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            return response.IsSuccessStatusCode;
        }

        // --- GENERIC DELETE ---
        public async Task<bool> DeleteAsync(string endpoint)
        {
            AttachSessionCookie();
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
    }
}