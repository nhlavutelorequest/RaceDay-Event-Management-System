using System.Net.Http.Json;

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

        public static async Task<string> ReadMessageAsync(HttpResponseMessage res, string fallback)
        {
            try
            {
                var json = await res.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                if (json.TryGetProperty("message", out var m)) return m.GetString() ?? fallback;
            }
            catch { }
            return fallback;
        }
    }
}