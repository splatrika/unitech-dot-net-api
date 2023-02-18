using System;
using EasyUnitech.NetApi.Models;

namespace EasyUnitech.NetApi.Interfaces;

public interface IScheduleService
{
	Task<IReadOnlyCollection<ScheduleEvent>> GetDayAsync(DateTime day);
	Task<IReadOnlyCollection<ScheduleEvent>> GetWeekAsync(DateTime day);
}

