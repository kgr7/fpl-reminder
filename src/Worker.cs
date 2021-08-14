using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FPL.Reminder.src;
using FPL.Reminder.src.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FPL.Reminder
{
    public class Worker
    {
        private readonly IWebService _webService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public Worker(IWebService webService, IDateTimeProvider dateTimeProvider)
        {
            _webService = webService;
            _dateTimeProvider = dateTimeProvider;
        }

        [FunctionName("FPLReminder")]
        public async Task Run([TimerTrigger("0 */15 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("Checking for upcoming deadline...");
            await DoWork(await _webService.GetEvents());
        }

        public async Task DoWork(List<Event> events)
        {
            foreach (var gameweek in events)
            {
                var oneDayAhead = ToNearest15Mins(_dateTimeProvider.Now.AddHours(Consts.OneDay));
                var halfDayAhead = ToNearest15Mins(_dateTimeProvider.Now.AddHours(Consts.HalfDay));
                var twoHoursAhead = ToNearest15Mins(_dateTimeProvider.Now.AddHours(Consts.TwoHrs));

                if (oneDayAhead == gameweek.DeadlineTime)
                {
                    // one day message
                    await _webService.SendReminder(Consts.OneDay, gameweek.Id);
                }
                if (halfDayAhead == gameweek.DeadlineTime)
                {
                    // 12 hour message
                    await _webService.SendReminder(Consts.HalfDay, gameweek.Id);
                }
                if (twoHoursAhead == gameweek.DeadlineTime)
                {
                    // 2 hour message
                    await _webService.SendReminder(Consts.TwoHrs, gameweek.Id);
                }
            }
        }

        public DateTime ToNearest15Mins(DateTime datetime)
        {
            return datetime.AddMinutes(-(datetime.Minute % 15)).AddSeconds(-datetime.Second);
        }
    }
}
