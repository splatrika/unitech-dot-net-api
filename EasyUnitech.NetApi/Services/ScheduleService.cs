using System.Globalization;
using AngleSharp.Dom;
using EasyUnitech.NetApi.Constants;
using EasyUnitech.NetApi.Extensions;
using EasyUnitech.NetApi.Interfaces;
using EasyUnitech.NetApi.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace EasyUnitech.NetApi.Services;

public class ScheduleService : IScheduleService
{
    public const string SchedulePageRoute = "/schedule";

    private readonly IHttpService _httpService;
    private const string QueryDateFormat = "{0:dd.MM.yyyy}";

    public ScheduleService(
        IHttpService httpService)
    {
        _httpService = httpService;
    }

    public async Task<IReadOnlyCollection<ScheduleEvent>> GetDayAsync(DateTime day)
    {
        var dateString = String.Format(QueryDateFormat, day);
        var content = await _httpService
            .GetAsync($"{HttpConstants.Host}{SchedulePageRoute}?d={dateString}");
        var document = await content.ParseHtmlAsync();
        var column = FindColumn(document, day);

        return await ParseEventsAsync(document, column, day);
    }

    public async Task<IReadOnlyCollection<ScheduleEvent>> GetWeekAsync(DateTime monday)
    {
        const int FirstDateColumn = 1;
        const int DaysInWeek = 7;
        var dateString = String.Format(QueryDateFormat, monday);
        var content = await _httpService.GetAsync($"{HttpConstants.Host}/schedule?d={dateString}");
        var document = await content.ParseHtmlAsync();

        var events = new List<ScheduleEvent>();
        for (int i = FirstDateColumn; i < FirstDateColumn + DaysInWeek; i++)
        {
            events.AddRange(await ParseEventsAsync(document, i, monday.AddDays(i - 1)));
        }
        return events;
    }

    private int FindColumn(IDocument document, DateTime date)
    {
        const string DateFormatInHtml = "{0:dd.MM}";
        var dateString = String.Format(DateFormatInHtml, date);

        var dateCells = document
            .QuerySelector("thead")?
            .QuerySelectorAll("th")
            .Select((cell, index) => (Value: cell.QuerySelector("span"), Index: index))
            ?? throw new FormatException("Unable to find table header with dates");

        foreach (var cell in dateCells)
        {
            if (cell.Value == null) continue;
            if (cell.Value.InnerHtml == dateString)
            {
                return cell.Index;
            }
        }
        throw new FormatException($"There is no column for date {dateString}");
    }

    private async Task<IReadOnlyCollection<ScheduleEvent>> ParseEventsAsync(
        IDocument document,
        int column,
        DateTime date)
    {
        var eventCells = document
            .QuerySelector("tbody")?
            .QuerySelectorAll("tr")
            .Select(r => r
                .QuerySelectorAll("td")
                .ElementAtOrDefault(column))
            ?? throw new FormatException("Unable to find table body with events");

        var events = new List<ScheduleEvent>();
        foreach (var cell in eventCells)
        {
            if (cell == null) throw new FormatException("Event cell can't be null");
            var parsedEvent = await ParseEventAsync(cell, date);
            if (parsedEvent != null) events.Add(parsedEvent);
        }

        return events;
    }

    private async Task<ScheduleEvent?> ParseEventAsync(IElement eventCell, DateTime date)
    {
        var content = eventCell
            .QuerySelector(".time_table_item_validated")?
            .Attributes
            .FirstOrDefault(a => a.Name == "data-content")?
            .TextContent;
        if (content == null) return null;
        var eventDocument = await content.ParseHtmlAsync();

        var dateTime = ParseDateTime(eventCell, date);
        var room = eventDocument
            .QuerySelector("a")?
            .InnerHtml;

        return new(
            science: ParseScience(content, eventDocument),
            start: dateTime.Start,
            end: dateTime.End,
            room: room,
            unitechIndividualNote: ParseUnitechIndividualNotes(eventDocument));
    }

    private (DateTime Start, DateTime End) ParseDateTime(IElement eventCell, DateTime date)
    {
        const string TimeFormat = "HH:mm";
        var times = eventCell
            .QuerySelector("span")?
            .InnerHtml
            .Split(" - ");
        if (times == null || times.Length != 2)
        {
            throw new FormatException("Unable to find event times");
        }
        for (var i = 0; i < 2; i++)
        {
            // change 8:00 to 08:00
            if (times[i].Length == 4) // todo refacor
            {
                times[i] = "0" + times[i];
            }
        }
        if (!DateTime.TryParseExact(times.First(), TimeFormat,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime start))
        {
            throw new FormatException("Unable to find start time");
        }
        if (!DateTime.TryParseExact(times.Last(), TimeFormat,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime end))
        {
            throw new FormatException("Unable to find end time");
        }

        return (
            date.Date.Add(start.TimeOfDay),
            date.Date.Add(end.TimeOfDay));
    }

    private Science ParseScience(string eventCellContent, IDocument eventDocument)
    {
        var showMoreButton = eventDocument.QuerySelector("button");
        var scienceId = showMoreButton?
            .Attributes["data-subject"]?
            .TextContent ?? "0";
        var name = eventCellContent
            .Split("<br")
            .First()
            .Split("<hr/>")
            .Last();
        return new(
            unitechId: int.TryParse(scienceId, out int id) ? id : 0,
            name: name);
    }

    private string? ParseUnitechIndividualNotes(IDocument eventDocument)
    {
        var unitechIndividualNotes = eventDocument
            .QuerySelector(".individual_schedule_note")?
            .QuerySelectorAll("div")
            .Select(x => x.InnerHtml);
        if (unitechIndividualNotes != null && !unitechIndividualNotes.Any())
        {
            unitechIndividualNotes = null;
        }
        return unitechIndividualNotes != null
            ? string.Join(" ", unitechIndividualNotes)
            : null;
    }
}

