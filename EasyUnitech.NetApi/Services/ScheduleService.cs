using System.Globalization;
using System.Text.Json.Serialization;
using AngleSharp.Dom;
using EasyUnitech.NetApi.Constants;
using EasyUnitech.NetApi.Extensions;
using EasyUnitech.NetApi.Interfaces;
using EasyUnitech.NetApi.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EasyUnitech.NetApi.Services;

public class ScheduleService : IScheduleService
{
    public const string SchedulePageRoute = "/schedule";
    private const string QueryDateFormat = "{0:dd.MM.yyyy}";
    private readonly IAuthorizedHttpClient _httpClient;

    public ScheduleService(IAuthorizedHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyCollection<ScheduleEvent>> GetDayAsync(DateTime day)
    {
        var unitechEvents = await GetUnitechEventsWeekAsync(day);
        return await unitechEvents
            .Where(x => x.DayOfWeek == GetDayOfWeek(day))
            .ToAsyncEnumerable()
            .SelectAwait(async x => await ParseScheduleEvent(x, day))
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<ScheduleEvent>> GetWeekAsync(DateTime day)
    {
        var unitechEvents = await GetUnitechEventsWeekAsync(day);
        return await unitechEvents
            .ToAsyncEnumerable()
            .SelectAwait(async x => await ParseScheduleEvent(x, day))
            .ToListAsync();
    }

    private async Task<ScheduleEvent> ParseScheduleEvent(UnitechEvent unitechEvent, DateTime day)
    {
        var monday = day.AddDays((GetDayOfWeek(day) - 1) * -1).Date;

        var eventDay = monday.AddDays(unitechEvent.DayOfWeek - 1);
        var time = ParseTime(unitechEvent.Time, eventDay);
        var htmlDocument = await unitechEvent.Html.ParseHtmlAsync();
        var individualNoteDocument = unitechEvent.IndividualNote is null
            ? null
            : await unitechEvent.IndividualNote.ParseHtmlAsync();

        return new(
            science: ParseScience(unitechEvent.Html, htmlDocument),
            start: time.Start,
            end: time.End,
            room: ParseRoom(htmlDocument),
            unitechIndividualNote: ParseUnitechIndividualNote(individualNoteDocument));
    }

    private async Task<IReadOnlyCollection<UnitechEvent>> GetUnitechEventsWeekAsync(DateTime day)
    {
        var dateString = string.Format(QueryDateFormat, day);
        var content = await _httpClient.PostAsync(
            uri: $"{HttpConstants.Host}{SchedulePageRoute}?d={dateString}",
            formData: new() { ["load"] = "1" });
        var unitechEvents = JsonConvert.DeserializeObject<IReadOnlyCollection<UnitechEvent>>(content);
        if (unitechEvents == null)
        {
            throw new FormatException("Unable to load events");
        }
        return unitechEvents
            .Where(x => !IsHoliday(x))
            .ToList();
    }

    private bool IsHoliday(UnitechEvent unitechEvent)
    {
        return unitechEvent.Time == null;
    }

    private (DateTime Start, DateTime End) ParseTime(string timeRangeString, DateTime eventDay)
    {
        const string TimeFormat = "HH:mm";
        var timeStrings = timeRangeString.Split(" - ");
        if (timeStrings.Length != 2)
        {
            throw new FormatException($"Unable to parse time: { timeRangeString }");
        }
        for (var i = 0; i < 2; i++)
        {
            // change 8:00 to 08:00
            if (timeStrings[i].Length == 4)
            {
                timeStrings[i] = "0" + timeStrings[i];
            }
        }
        if (!DateTime.TryParseExact(timeStrings.First(), TimeFormat,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime start))
        {
            throw new FormatException("Unable to find start time");
        }
        if (!DateTime.TryParseExact(timeStrings.Last(), TimeFormat,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime end))
        {
            throw new FormatException("Unable to find end time");
        }
        return (
            eventDay.Date.Add(start.TimeOfDay),
            eventDay.Date.Add(end.TimeOfDay));
    }

    private Science ParseScience(string htmlString, IDocument html)
    {
        var showMoreButton = html.QuerySelector("button");
        var scienceId = showMoreButton?
            .Attributes["data-subject"]?
            .TextContent ?? "0";
        var name = htmlString
            .Split("<br")
            .First();
        return new(
            unitechId: int.TryParse(scienceId, out int id) ? id : 0,
            name: name);
    }

    private string? ParseRoom(IDocument html)
    {
        return html
            .QuerySelector("a")?
            .InnerHtml;
    }

    private string? ParseUnitechIndividualNote(IDocument? individualNote)
    {
        if (individualNote == null) return null;
        var unitechIndividualNotes = individualNote
            .QuerySelectorAll("div")
            .Select(x => x.InnerHtml);
        return unitechIndividualNotes != null && unitechIndividualNotes.Any()
            ? string.Join(" ", unitechIndividualNotes)
            : null;
    }

    private int GetDayOfWeek(DateTime day)
    {
        var dayOfWeek = day.DayOfWeek;
        if (dayOfWeek == DayOfWeek.Sunday) return 7;
        return (int)dayOfWeek;
    }

    private class UnitechEvent
    {
        [JsonProperty("daynum")]
        public required int DayOfWeek { get; init; }

        [JsonProperty("time")]
        public required string Time { get; init; }

        [JsonProperty("lparam")]
        public required string Html { get; init; }

        [JsonProperty("note")]
        public required string IndividualNote { get; init; }
    }
}

