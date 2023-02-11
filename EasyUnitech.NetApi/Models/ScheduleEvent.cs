namespace EasyUnitech.NetApi.Models;

public class ScheduleEvent
{
    public Science Science { get; }
	public DateTime Start { get; }
	public DateTime End { get; }
    public string? Room { get; }
    public string? UnitechIndividualNote { get; }

    public ScheduleEvent(
        Science science,
        DateTime start,
        DateTime end,
        string? room,
        string? unitechIndividualNote)
    {
        Science = science;
        Start = start;
        End = end;
        Room = room;
        UnitechIndividualNote = unitechIndividualNote;
    }
}

