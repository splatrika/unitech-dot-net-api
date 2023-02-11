using System;
using EasyUnitech.NetApi.Services;
using RichardSzalay.MockHttp;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using EasyUnitech.NetApi.Models;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using EasyUnitech.NetApi.Constants;

namespace EasyUnitech.NetApi.Tests;

public class LoginServiceTests
{
	private MockHttpMessageHandler _httpMock;
	private HttpClient _httpClient;
	private LoginService _service;

	public LoginServiceTests()
	{
		_httpMock = new();
		_httpClient = new(_httpMock);
		_service = new(new Mock<ILogger<LoginService>>().Object, _httpClient);
    }

	[Fact]
	public async void ValidLogin()
	{
		var login = "uwu";
		var password = "owo";
        var responceKeys = new Keys("eee", "rrr");

		var response = new HttpResponseMessage();
        SetAuthDefaults(response, responceKeys);
        response.Content = new StringContent("{ \"success\": true }",
			Encoding.UTF8, "application/json");

		_httpMock.When(HttpMethod.Post, HttpConstants.Host + "auth")
			.WithFormData("login", login)
			.WithFormData("pass", password)
			.Respond(_ => response);

		Keys? keys = null;
		var ok = await _service.TryLoginAsync(login, password, x => keys = x);

		Assert.True(ok);
		Assert.NotNull(keys);
		Assert.Equal(responceKeys.CSRF, keys.CSRF);
		Assert.Equal(responceKeys.SessCommon, keys.SessCommon);
    }

	[Fact]
	public async void InvalidLogin()
	{
		var responceKeys = new Keys("aaa", "ooo");

        var response = new HttpResponseMessage();
		SetAuthDefaults(response, responceKeys);
        response.Content = new StringContent(
			"{ \"success\": false, \"error\": \"custom\" }",
            Encoding.UTF8, "application/json");

        _httpMock.When(HttpMethod.Post, HttpConstants.Host + "auth")
			.Respond(_ => response);

		Keys? keys = null;
		var ok = await _service.TryLoginAsync("x", "y", x => keys = x);

		Assert.False(ok);
        Assert.NotNull(keys);
        Assert.Equal(responceKeys.CSRF, keys.CSRF);
        Assert.Equal(responceKeys.SessCommon, keys.SessCommon);
    }

	[Fact]
	public async void ValidValidate()
	{
        var keys = new Keys("aaa", "ooo");
		_httpMock.When(HttpMethod.Get, HttpConstants.Host + LoginService.GuardedPage)
			.WithHeaders("Cookie", $"ft_csrf={keys.CSRF};ft_sess_common={keys.SessCommon}")
			.Respond(HttpStatusCode.OK);
		var ok = await _service.ValidateAsync(keys);
		Assert.True(ok);
    }

	[Fact]
	public async void InvalidValidate()
	{
		_httpMock.When(HttpMethod.Get, HttpConstants.Host + LoginService.GuardedPage)
			.Respond(HttpStatusCode.NotFound);
        var ok = await _service.ValidateAsync(new("aa", "uu"));
		Assert.False(ok);
    }

	private void SetAuthDefaults(HttpResponseMessage response, Keys keys)
	{
        response.StatusCode = HttpStatusCode.OK;
        response.Headers.Add("Set-Cookie", $"ft_csrf={keys.CSRF}");
        response.Headers.Add("Set-Cookie", $"ft_sess_common={keys.SessCommon}");
    }
}

