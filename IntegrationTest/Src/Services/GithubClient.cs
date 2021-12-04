using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Src.Services
{
    public interface IGithubClient
    {
        Task<GithubUser> GetUserAsync(string username);

    }

    public class GithubUser
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
    }

    public class GithubClient : IGithubClient
    {
        private readonly HttpClient _client;

        public GithubClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<GithubUser> GetUserAsync(string username)
        {
            var response = await _client.GetAsync($"/users/{Uri.EscapeDataString(username)}");
            response.EnsureSuccessStatusCode();
            // return await response.Content.ReadAsAsync<GithubUser>();// Apparanlt the ReadAsAsync Deprectated we can use the following methods"
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GithubUser>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}