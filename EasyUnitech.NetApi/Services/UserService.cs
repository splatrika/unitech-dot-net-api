using AngleSharp;
using System;
using System.Globalization;
using EasyUnitech.NetApi.Interfaces;
using EasyUnitech.NetApi.Models;
using EasyUnitech.NetApi.Constants;
using Microsoft.Extensions.Logging;
using AngleSharp.Dom;
using System.Reflection.Metadata;
using EasyUnitech.NetApi.Extensions;

namespace EasyUnitech.NetApi.Services;

public class UserService: IUserService
{
    public const int FirstNameIndex = 1;
    public const int LastNameIndex = 0;
    public const int PatronymicIndex = 2;
    public const int BirthdayIndex = 3;
    public const int FirstYearIndex = 9;
    public const string UserPageRoute = "/user";

    private readonly IHttpService _httpService;
    private readonly ILogger<UserService> _logger;


    public UserService(IHttpService httpService, ILogger<UserService> logger)
    {
        _httpService = httpService;
        _logger = logger;
    }


    public async Task<User> GetUserAsync()
    {
        var content = await _httpService
            .GetAsync($"{HttpConstants.Host}{UserPageRoute}");
        var document = await content.ParseHtmlAsync();

        var firstName = "";
        var lastName = "";
        var patronymic = "";
        var birthday = DateTime.Now;
        var photo = "";

        var info = document.QuerySelectorAll(".info");
        var mappers = new Dictionary<int, Action<string>>();
        mappers[FirstNameIndex] = x => firstName = x;
        mappers[LastNameIndex] = x => lastName = x;
        mappers[PatronymicIndex] = x => patronymic = x;
        mappers[BirthdayIndex] = x => ParseBirthday(x, out birthday);

        foreach (var infoIndex in mappers.Keys)
        {
            if (infoIndex >= info.Length)
            {
                _logger.LogWarning("Unable to parse some data");
                continue;
            }
            var value = GetValueFromInfo(info[infoIndex]);
            mappers[infoIndex].Invoke(value);
        }

        var photoContainer = document.QuerySelector(".users_avatar_wrap");
        if (photoContainer != null)
        {
            var styles = photoContainer.Attributes
                .FirstOrDefault(x => x.Name == "style");
            if (styles != null)
            {
                photo = styles.Value
                    .Replace("background: url(", "")
                    .Replace(") top left no-repeat;", "");
            }
        }
        if (string.IsNullOrEmpty(photo))
        {
            _logger.LogWarning("Unable to parse photo");
        }

        return new(firstName, lastName, patronymic, birthday, photo);
    }


    public async Task<bool> TryGetFirstYearAsync(Action<int> callback)
    {
        var content = await _httpService
            .GetAsync($"{HttpConstants.Host}{UserPageRoute}");
        var document = await content.ParseHtmlAsync();

        var info = document.QuerySelectorAll(".info");
        if (info.Length <= FirstYearIndex)
        {
            return false;
        }
        var firstYearString = GetValueFromInfo(info[FirstYearIndex]);
        var firstYear = 0;
        if (int.TryParse(firstYearString, out firstYear))
        {
            callback.Invoke(firstYear);
            return true;
        }
        return false;
    }

    public async Task<bool> IsStudentAsync()
    {
        var content = await _httpService
            .GetAsync($"{HttpConstants.Host}{UserPageRoute}");
        var document = await content.ParseHtmlAsync();

        var neighborsOpenLink = document.QuerySelector("#neighborsopenlink");
        if (neighborsOpenLink == null)
        {
            _logger.LogWarning(
                "Unable to determine type of user: neighborsopenlink not found");
            return false;
        }
        if (neighborsOpenLink.ParentElement == null)
        {
            _logger.LogWarning(
                "Unable to determine type of user: neighborsopenlink's parent not found");
            return false;
        }
        return neighborsOpenLink.ParentElement.InnerHtml.Contains("Одногруппники");
    }


    private void ParseBirthday(string value, out DateTime output)
    {
        value = value.Replace("\t", "")
            .Replace("\n", "");
        if (!DateTime.TryParseExact(value, "dd.MM.yyyy",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out output))
        {
            _logger.LogWarning("Unable to parse birthday data");
        }
    }

    private string GetValueFromInfo(IElement infoElement)
    {
        return infoElement.InnerHtml
            .Split("</strong>")
            .ElementAt(1)
            .Substring(1);
    }
}

