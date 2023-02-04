using AngleSharp;
using System;
using System.Globalization;
using EasyUnitech.NetApi.Interfaces;
using EasyUnitech.NetApi.Models;
using EasyUnitech.NetApi.Constants;
using Microsoft.Extensions.Logging;

namespace EasyUnitech.NetApi.Services;

public class UserService: IUserService
{
    public const int FirstNameIndex = 1;
    public const int LastNameIndex = 0;
    public const int PatronymicIndex = 2;
    public const int BirthdayIndex = 3;

    private readonly IHttpService _httpService;
    private readonly ILogger<UserService> _logger;


    public UserService(IHttpService httpService, ILogger<UserService> logger)
    {
        _httpService = httpService;
        _logger = logger;
    }


    public async Task<User> GetUserAsync()
    {
        var content = await _httpService.GetAsync($"{HttpConstants.Host}/user");

        var configuration = Configuration.Default;
        var context = BrowsingContext.New(configuration);
        var document = await context.OpenAsync(response => response.Content(content));

        var firstName = "";
        var lastName = "";
        var patronymic = "";
        var birthday = DateTime.Now;
        var photo = "";

        var info = document.QuerySelectorAll(".info");
        var binds = new Dictionary<int, Action<string>>();
        binds[FirstNameIndex] = x => firstName = x;
        binds[LastNameIndex] = x => lastName = x;
        binds[PatronymicIndex] = x => patronymic = x;
        binds[BirthdayIndex] = x => ParseBirthday(x, out birthday);

        foreach (var infoIndex in binds.Keys)
        {
            if (infoIndex >= info.Length)
            {
                _logger.LogWarning("Unable to parse some data");
                continue;
            }
            var blockContent = info[infoIndex].InnerHtml;
            var value = blockContent.Split("</strong>")
                .ElementAt(1)
                .Substring(1);
            binds[infoIndex].Invoke(value);
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
}

