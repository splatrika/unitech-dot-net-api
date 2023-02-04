using EasyUnitech.NetApi.Extensions;
using EasyUnitech.NetApi.Interfaces;

namespace EasyUnitech.NetApi.Services;

public class HttpService : IHttpService
{
    private readonly IKeysAccessor _keysService;


    public HttpService(IKeysAccessor keysService)
    {
        _keysService = keysService;
    }


    public async Task<string> GetAsync(string uri)
    {
        var keys = _keysService.Get();
        if (keys == null)
        {
            throw new NullReferenceException("There is no keys");
        }
        using var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.SetKeys(keys);
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new NullReferenceException(
                "Unable to load content. Maybe authorization failed");
        }
        return await response.Content.ReadAsStringAsync();
    }
}
