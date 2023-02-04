using System.Text;
using EasyUnitech.NetApi.Extensions;
using EasyUnitech.NetApi.Interfaces;
using EasyUnitech.NetApi.Models;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using EasyUnitech.NetApi.Constants;

namespace EasyUnitech.NetApi.Services;

public class LoginService : ILoginService
{
    public const string GuardedPage = "rp_instruction";

    private readonly ILogger<LoginService> _logger;
    private readonly HttpClient _httpClient;


    public LoginService(ILogger<LoginService> logger,
        HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task LogoutAsync(Keys keys)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            HttpConstants.Host + "auth?action=logout");
        request.SetKeys(keys);
        await _httpClient.SendAsync(request);
    }

    public async Task<bool> TryLoginAsync(string login, string password,
        Action<Keys> callback)
    {
        var request = new HttpRequestMessage(HttpMethod.Post,
            HttpConstants.Host + "auth");
        request.Content = new StringContent(
            $"login={login}&pass={password}&auth=1&ajax=1",
            Encoding.UTF8,
            "application/x-www-form-urlencoded");

        var httpResponse = await _httpClient.SendAsync(request);
        var loginResponse = JsonConvert.DeserializeObject<LoginResponce>(
            await httpResponse.Content.ReadAsStringAsync());
        if (loginResponse == null)
        {
            _logger.LogError("Invalid json login response");
            return false;
        }
        var cookies = httpResponse.GetCookies();
        var csrf = cookies.FirstOrDefault(x => x.Name == "ft_csrf")?
            .Value;
        var sessCommon = cookies.FirstOrDefault(x => x.Name == "ft_sess_common")?
            .Value;
        if (csrf == null || sessCommon == null)
        {
            _logger.LogError("Cookies were not found");
            return false;
        }
        callback.Invoke(new(csrf, sessCommon));
        return loginResponse.Success;
    }

    public async Task<bool> ValidateAsync(Keys keys)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            HttpConstants.Host + GuardedPage);
        request.SetKeys(keys);
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    private class LoginResponce
    {
        public bool Success;
    }
}
