using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Web.Services
{
    public class ApiClient
    {
        private const string CookieKey = "ApiCookie";
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _accessor;

        public ApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor)
        {
            _factory = factory;
            _accessor = accessor;
        }

        private ISession Session => _accessor.HttpContext!.Session;

        public Task<HttpResponseMessage> GetAsync(string url) =>
            SendAsync(new HttpRequestMessage(HttpMethod.Get, url));

        public Task<HttpResponseMessage> PostAsync(string url, object? body = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, url);
            if (body != null) req.Content = JsonContent.Create(body);
            return SendAsync(req);
        }

        public Task<HttpResponseMessage> PutAsync(string url, object body) =>
            SendAsync(new HttpRequestMessage(HttpMethod.Put, url) { Content = JsonContent.Create(body) });

        public Task<HttpResponseMessage> DeleteAsync(string url) =>
            SendAsync(new HttpRequestMessage(HttpMethod.Delete, url));

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req)
        {
            var cookie = Session.GetString(CookieKey);
            if (!string.IsNullOrEmpty(cookie))
                req.Headers.TryAddWithoutValidation("Cookie", cookie);

            var res = await _factory.CreateClient("RaceDayApi").SendAsync(req);

            if (res.Headers.TryGetValues("Set-Cookie", out var values))
            {
                var session = values.FirstOrDefault(v => v.StartsWith(".AspNetCore.Session="));
                if (session != null)
                    Session.SetString(CookieKey, session.Split(';')[0]);
            }
            return res;
        }

        // Turns an API error response into a plain sentence a user can understand.
        public static async Task<string> ReadMessageAsync(HttpResponseMessage res, string fallback)
        {
            try
            {
                var json = await res.Content.ReadFromJsonAsync<JsonElement>();

                if (json.TryGetProperty("message", out var m) && !string.IsNullOrWhiteSpace(m.GetString()))
                    return m.GetString()!;

                if (json.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var messages = errors.EnumerateObject()
                        .SelectMany(p => p.Value.EnumerateArray())
                        .Select(e => e.GetString())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct();

                    var joined = string.Join(" ", messages);
                    if (joined.Length > 0) return joined;
                }
            }
            catch { }
            return fallback;
        }
    }
}