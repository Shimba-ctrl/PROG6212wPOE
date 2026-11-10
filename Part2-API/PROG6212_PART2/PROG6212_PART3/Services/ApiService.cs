
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace PROG6212_PART3.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("RaceDayApi");
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HttpResponseMessage> SendAsync(
            HttpMethod method,
            string endpoint,
            object? data = null)
        {
            var request = new HttpRequestMessage(method, endpoint);

            // Forward the API session cookie when available
            var context = _httpContextAccessor.HttpContext;

            var apiCookie = context?.Session.GetString("ApiSessionCookie");

            if (!string.IsNullOrEmpty(apiCookie))
            {
                request.Headers.Add("Cookie", apiCookie);
            }

            // Attach request data when provided
            if (data != null)
            {
                request.Content = JsonContent.Create(data);
            }

            var response = await _httpClient.SendAsync(request);

            // Save the API session cookie after a response
            if (response.Headers.TryGetValues(
                "Set-Cookie", out var cookies))
            {
                var sessionCookie = cookies.FirstOrDefault(
                    cookie => cookie.StartsWith(
                        ".AspNetCore.Session=",
                        StringComparison.OrdinalIgnoreCase));

                if (sessionCookie != null && context != null)
                {
                    var cookieValue = sessionCookie.Split(';')[0];

                    context.Session.SetString(
                        "ApiSessionCookie", cookieValue);
                }
            }

            return response;
        }
    }
}

