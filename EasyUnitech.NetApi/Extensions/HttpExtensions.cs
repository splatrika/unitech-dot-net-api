using System;
using System.Net;
using EasyUnitech.NetApi.Models;
namespace EasyUnitech.NetApi.Extensions;

public static class HttpExtensions
{
    public static IEnumerable<Cookie> GetCookies(this HttpResponseMessage response)
    {
        var container = new List<Cookie>();
        var cookieHeader = response.Headers.First(x => x.Key == "Set-Cookie");
        foreach (var header in cookieHeader.Value)
        {
            var keyValue = header.Split(";")
                .ElementAt(0)
                .Split("=");
            var cookie = new Cookie(keyValue[0], keyValue[1]);
            container.Add(cookie);
        }
        return container;
    }


    public static void SetKeys(this HttpRequestMessage request, Keys keys)
    {
        request.Headers.Add("Cookie",
            $"ft_csrf={keys.CSRF};ft_sess_common={keys.SessCommon}");
    }
}

