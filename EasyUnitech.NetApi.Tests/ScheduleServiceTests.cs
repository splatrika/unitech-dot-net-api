using EasyUnitech.NetApi.Constants;
using EasyUnitech.NetApi.Interfaces;
using EasyUnitech.NetApi.Models;
using EasyUnitech.NetApi.Services;
using Moq;
using Xunit;

namespace EasyUnitech.NetApi.Tests;

public class ScheduleServiceTests
{

    private readonly Science _math = new(12, "Иванов Иван Иванович. Высшая Математика ");
    private readonly Science _algorithm = new(100, "Иванов Иван Иванович. Теория алгоритмов ");

	private ScheduleService _service;
    private readonly Mock<IAuthorizedHttpClient> _httpServiceMock = new();
    private readonly DateTime _monday = new DateTime(2023, 1, 9);
	private readonly string _dateString = "09.01.2023";
	// TODO: Fix formatting
	private readonly string _scheduleResponse = $@"
[
    {{
        ""daynum"": 1,
        ""timenum"": 0,
        ""weeknum"": 0,
        ""time"": ""9:00 - 10:30"",
        ""lparam"": ""Иванов Иван Иванович. Теория алгоритмов <br/><a target=\""_blank\"" href=\""\"">ауд. 1</a><div class=\""schedule_materials_open_btn_wrp\""><button data-subject=\""100\"" class=\""schedule_materials_open_btn\"" onclick=\""scheduleGetMaterials(this)\"" style=\""display: none;\"">Материалы занятия</button></div>"",
        ""note"": ""<span class=\""individual_schedule_note\""><div>Внимание!</div><div>Занятие группы Г0-00 переносится в аудиторию 0</div></span> ""
    }},
    {{
        ""daynum"": 3,
        ""timenum"": 0,
        ""weeknum"": 0,
        ""time"": ""9:00 - 10:30"",
        ""lparam"": ""Иванов Иван Иванович. Высшая Математика <br/><div class=\""schedule_materials_open_btn_wrp\""><button data-subject=\""12\"" class=\""schedule_materials_open_btn\"" onclick=\""scheduleGetMaterials(this)\"" style=\""display: none;\"">Материалы занятия</button></div>"",
        ""note"": null
    }},
    {{
        ""daynum"": 3,
        ""timenum"": 0,
        ""weeknum"": 0,
        ""time"": ""10:40 - 12:10"",
        ""lparam"": ""Иванов Иван Иванович. Теория алгоритмов <br/><a target=\""_blank\"" href=\""\"">ауд. 12</a><div class=\""schedule_materials_open_btn_wrp\""><button data-subject=\""100\"" class=\""schedule_materials_open_btn\"" onclick=\""scheduleGetMaterials(this)\"" style=\""display: none;\"">Материалы занятия</button></div>"",
        ""note"": null
    }},
]";

	public ScheduleServiceTests()
	{
        _httpServiceMock
            .Setup(
                x => x.PostAsync(
                    $"{HttpConstants.Host}{ScheduleService.SchedulePageRoute}?d={_dateString}",
                    It.Is<Dictionary<string, string>>(x => x["load"] == "1"))
                )
            .Returns(Task.FromResult(_scheduleResponse));
		_service = new(_httpServiceMock.Object);
    }



	[Fact]
	public async void GetDay_ServerReturnsSchedulePage_ParsedSuccefuly()
	{
		var exceptedEvents = new List<ScheduleEvent>()
        {
            new(
                _algorithm,
				new DateTime(2023, 1, 9, 9, 0, 0),
				new DateTime(2023, 1, 9, 10, 30, 0),
				"ауд. 1",
				"Внимание! Занятие группы Г0-00 переносится в аудиторию 0")
        };

		var events = await _service.GetDayAsync(_monday);

		Assert.Equal(exceptedEvents[0].Science.Name, events.ElementAt(0).Science.Name);
		Assert.Equal(exceptedEvents[0].Science.UnitechId, events.ElementAt(0).Science.UnitechId);
		Assert.Equal(exceptedEvents[0].Start, events.ElementAt(0).Start);
		Assert.Equal(exceptedEvents[0].End, events.ElementAt(0).End);
		Assert.Equal(exceptedEvents[0].Room, events.ElementAt(0).Room);
		Assert.Equal(exceptedEvents[0].UnitechIndividualNote, events.ElementAt(0).UnitechIndividualNote);
    }

    [Fact]
    public async void GetWeek_ServerReturnsSchedulePage_ParsedSuccefuly()
    {
        var exceptedEvents = new List<ScheduleEvent>()
        {
            new(
                _algorithm,
				new DateTime(2023, 1, 9, 9, 0, 0),
				new DateTime(2023, 1, 9, 10, 30, 0),
				"ауд. 1",
				"Внимание! Занятие группы Г0-00 переносится в аудиторию 0"),
            new(
                _math,
				new DateTime(2023, 1, 11, 9, 0, 0),
				new DateTime(2023, 1, 11, 10, 30, 0),
				null,
				null),
            new(
				_algorithm,
				new DateTime(2023, 1, 11, 10, 40, 0),
				new DateTime(2023, 1, 11, 12, 10, 0),
                "ауд. 12",
				null)
        };

        var events = await _service.GetWeekAsync(_monday);

        Assert.Equal(exceptedEvents[0].Science.Name, events.ElementAt(0).Science.Name);
        Assert.Equal(exceptedEvents[0].Science.UnitechId, events.ElementAt(0).Science.UnitechId);
        Assert.Equal(exceptedEvents[0].Start, events.ElementAt(0).Start);
        Assert.Equal(exceptedEvents[0].End, events.ElementAt(0).End);
        Assert.Equal(exceptedEvents[0].Room, events.ElementAt(0).Room);
        Assert.Equal(exceptedEvents[0].UnitechIndividualNote, events.ElementAt(0).UnitechIndividualNote);

        Assert.Equal(exceptedEvents[1].Science.Name, events.ElementAt(1).Science.Name);
        Assert.Equal(exceptedEvents[1].Science.UnitechId, events.ElementAt(1).Science.UnitechId);
        Assert.Equal(exceptedEvents[1].Start, events.ElementAt(1).Start);
        Assert.Equal(exceptedEvents[1].End, events.ElementAt(1).End);
        Assert.Equal(exceptedEvents[1].Room, events.ElementAt(1).Room);
        Assert.Equal(exceptedEvents[1].UnitechIndividualNote, events.ElementAt(1).UnitechIndividualNote);

        Assert.Equal(exceptedEvents[2].Science.Name, events.ElementAt(2).Science.Name);
        Assert.Equal(exceptedEvents[2].Science.UnitechId, events.ElementAt(2).Science.UnitechId);
        Assert.Equal(exceptedEvents[2].Start, events.ElementAt(2).Start);
        Assert.Equal(exceptedEvents[2].End, events.ElementAt(2).End);
        Assert.Equal(exceptedEvents[2].Room, events.ElementAt(2).Room);
        Assert.Equal(exceptedEvents[2].UnitechIndividualNote, events.ElementAt(2).UnitechIndividualNote);
    }
}

