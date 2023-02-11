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
    private readonly Mock<IHttpService> _httpServiceMock = new();
    private readonly DateTime _monday = new DateTime(2023, 11, 3);
	private readonly string _dateString = "03.11.2023";
	// TODO: Fix formatting
	private readonly string _schedulePage = $@"
<head></head>
<body>
    <table class=""schedule_day_time_table"">
		<thead>
			<tr>
				<th class="""">
					<span onclick=""getScheduleTime()"" data-toggle=""body-tooltip"" title="""" class=""glyphicon glyphicon-time"" data-original-title=""Время занятий""></span>
				</th>
				<th class=""date_simple"" data-date=""03.11.2023"">	
					Пн
					<span class=""short_date"">03.11</span>
				</th>
				<th class=""date_simple"" data-date=""04.11.2023"">	
					Вт
					<span class=""short_date"">04.11</span>
				</th>
				<th class=""date_simple"" data-date=""05.11.2023"">
					Ср
					<span class=""short_date"">05.11</span>
				</th>
				<th class=""date_simple"" data-date=""06.11.2023"">		
					Чт
					<span class=""short_date"">06.11</span>
				</th>
				<th class=""date_simple"" data-date=""07.11.2023"">	
					Пт
					<span class=""short_date"">07.11</span>
				</th>
				<th class=""date_simple"" data-date=""08.11.2023"">
					Сб
					<span class=""short_date"">08.11</span>
				</th>
				<th class=""date_simple"" data-date=""09.11.2023"">
					Вс
					<span class=""short_date"">09.11</span>
				</th>
			</tr>
		</thead>
		<tbody>
			<tr>
				<td class="""">1</td>
				<td data-stt-time=""1"" data-stt-day=""1"" id=""stt_1_1"" class=""""><div class=""time_table_item_wrp""><div class=""time_table_item_validated time_table_item_validated-3 item_noted"" data-toggle=""popover"" data-html=""true"" data-placement=""top"" data-content=""<span class=&quot;individual_schedule_note&quot;><div>Внимание!</div><div>Занятие группы Г0-00 переносится в аудиторию 0</div></span> <hr/>Иванов Иван Иванович. Высшая Математика <br/><a target=&quot;_blank&quot; href=&quot;/&quot;>ауд. 1</a><div class=&quot;schedule_materials_open_btn_wrp&quot;><button data-subject=&quot;12&quot; class=&quot;schedule_materials_open_btn&quot; onclick=&quot;scheduleGetMaterials(this)&quot; style=&quot;display: none;&quot;>Материалы занятия</button></div>"" data-original-title="""" title=""""><span>9:00 - 10:30</span></div></div></td>
				<td data-stt-time=""1"" data-stt-day=""2"" id=""stt_1_2"" class=""""><div class=""time_table_item_wrp""><div class=""time_table_item_validated time_table_item_validated-3"" data-toggle=""popover"" data-html=""true"" data-placement=""bottom"" data-content=""Иванов Иван Иванович. Теория алгоритмов <br></div><div class=&quot;schedule_materials_open_btn_wrp&quot;><button data-subject=&quot;100&quot; class=&quot;schedule_materials_open_btn&quot; onclick=&quot;scheduleGetMaterials(this)&quot; style=&quot;display: none;&quot;>Материалы занятия</button></div>"" data-original-title="""" title=""""><span>9:00 - 10:30</span></div></div></td>
				<td data-stt-time=""1"" data-stt-day=""3"" id=""stt_1_3"" class=""""></td>
				<td data-stt-time=""1"" data-stt-day=""4"" id=""stt_1_4"" class=""""></td>
				<td data-stt-time=""1"" data-stt-day=""5"" id=""stt_1_5"" class=""""></td>
				<td data-stt-time=""1"" data-stt-day=""6"" id=""stt_1_6"" class=""""></td>
				<td data-stt-time=""1"" data-stt-day=""7"" id=""stt_1_7""></td>
			</tr>
			<tr>
				<td class="""">2</td>
				<td data-stt-time=""2"" data-stt-day=""1"" id=""stt_2_1"" class=""""></td>
				<td data-stt-time=""2"" data-stt-day=""2"" id=""stt_2_2"" class=""""><div class=""time_table_item_wrp""><div class=""time_table_item_validated time_table_item_validated-3"" data-toggle=""popover"" data-html=""true"" data-placement=""bottom"" data-content=""Иванов Иван Иванович. Теория алгоритмов <br></div><div class=&quot;schedule_materials_open_btn_wrp&quot;><button data-subject=&quot;100&quot; class=&quot;schedule_materials_open_btn&quot; onclick=&quot;scheduleGetMaterials(this)&quot; style=&quot;display: none;&quot;>Материалы занятия</button></div>"" data-original-title="""" title=""""><span>10:40 - 12:10</span></div></div></td>
				<td data-stt-time=""2"" data-stt-day=""3"" id=""stt_2_3"" class=""""></td>
				<td data-stt-time=""2"" data-stt-day=""4"" id=""stt_2_4"" class=""""><div class=""time_table_item_wrp""><div class=""time_table_item_validated time_table_item_validated-3 item_noted"" data-toggle=""popover"" data-html=""true"" data-placement=""top"" data-content=""Иванов Иван Иванович. Высшая Математика <br/><a target=&quot;_blank&quot; href=&quot;/&quot;>ауд. 3</a><div class=&quot;schedule_materials_open_btn_wrp&quot;><button data-subject=&quot;12&quot; class=&quot;schedule_materials_open_btn&quot; onclick=&quot;scheduleGetMaterials(this)&quot; style=&quot;display: none;&quot;>Материалы занятия</button></div>"" data-original-title="""" title=""""><span>9:00 - 10:30</span></div></div></td>
				<td data-stt-time=""2"" data-stt-day=""5"" id=""stt_2_5"" class=""""></td>
				<td data-stt-time=""2"" data-stt-day=""6"" id=""stt_2_6"" class=""""></td>
				<td data-stt-time=""2"" data-stt-day=""7"" id=""stt_2_7""></td>
			</tr>
		</tbody>
	</table>
</body>";

	public ScheduleServiceTests()
	{
        _httpServiceMock.Setup(x => x.GetAsync(
                $"{HttpConstants.Host}{ScheduleService.SchedulePageRoute}?d={_dateString}"))
            .Returns(Task.FromResult(_schedulePage));
		_service = new(_httpServiceMock.Object);
    }


	[Fact]
	public async void GetDay_ServerReturnsSchedulePage_ParsedSuccefuly()
	{
		var exceptedEvents = new List<ScheduleEvent>()
        {
            new(
				_math,
				new DateTime(2023, 11, 3, 9, 0, 0),
				new DateTime(2023, 11, 3, 10, 30, 0),
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
				_math,
				new DateTime(2023, 11, 3, 9, 0, 0),
				new DateTime(2023, 11, 3, 10, 30, 0),
				"ауд. 1",
				"Внимание! Занятие группы Г0-00 переносится в аудиторию 0"),
            new(
				_algorithm,
				new DateTime(2023, 11, 4, 9, 0, 0),
				new DateTime(2023, 11, 4, 10, 30, 0),
				null,
				null),
            new(
				_algorithm,
				new DateTime(2023, 11, 4, 10, 40, 0),
				new DateTime(2023, 11, 4, 12, 10, 0),
				null,
				null),
            new(
				_math,
				new DateTime(2023, 11, 6, 9, 0, 0),
				new DateTime(2023, 11, 6, 10, 30, 0),
				"ауд. 3",
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

