using EasyUnitech.NetApi.Extensions;
using EasyUnitech.NetApi.Interfaces;
using Newtonsoft.Json;

namespace EasyUnitech.NetApi.Services;

public class AuthorizedHttpClient : IAuthorizedHttpClient
{
    private readonly IKeysAccessor _keysService;

    public AuthorizedHttpClient(IKeysAccessor keysService)
    {
        _keysService = keysService;
    }

    public async Task<string> GetAsync(string uri)
    {
        return await SendAsync(uri, HttpMethod.Get, null);
    }

    public async Task<string> PostAsync(string uri, Dictionary<string, string> formData)
    {
        var content = new FormUrlEncodedContent(formData);
        return await SendAsync(uri, HttpMethod.Post, content);
    }

    private async Task<string> SendAsync(string uri, HttpMethod method, HttpContent? content)
    {
        var keys = _keysService.Get();
        if (keys == null)
        {
            throw new NullReferenceException("There is no keys");
        }
        using var client = new HttpClient();
        var request = new HttpRequestMessage(method, uri);
        request.SetKeys(keys);
        request.Content = content;
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new NullReferenceException(
                "Unable to load content. Maybe authorization failed");
        }
        return await response.Content.ReadAsStringAsync();
    }
}
