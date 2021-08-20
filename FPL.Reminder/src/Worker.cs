using System;
using System.Linq;
using System.Threading.Tasks;
using FPL.Reminder.src;
using FPL.Reminder.src.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FPL.Reminder
{
    public class Worker
    {
        private readonly IWebService _webService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IConfiguration _config;

        public Worker(
            IWebService webService, 
            IDateTimeProvider dateTimeProvider,
            IConfiguration config)
        {
            _webService = webService;
            _dateTimeProvider = dateTimeProvider;
            _config = config;
        }

        [FunctionName("FPLReminder")]
        public async Task Run([TimerTrigger("0 */15 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("Checking for upcoming deadline...");
            log.LogInformation($"Mention role: {_config.GetValue<string>("MentionRole")}");
            log.LogInformation($"Webhook URL: {_config.GetValue<string>("WebhookUrl")}");
            log.LogInformation($"Current time to nearest 15: {ToNearest15Mins(_dateTimeProvider.UtcNow)}");
            log.LogInformation($"Current time + 24hr to nearest 15: {ToNearest15Mins(_dateTimeProvider.UtcNow.AddHours(24))}");
            log.LogInformation($"Current time + 12hr to nearest 15: {ToNearest15Mins(_dateTimeProvider.UtcNow.AddHours(12))}");
            log.LogInformation($"Current time + 2hr to nearest 15: {ToNearest15Mins(_dateTimeProvider.UtcNow.AddHours(2))}");

            var events = await _webService.GetEvents();

            var currentDatetime = _dateTimeProvider.UtcNow;
            if (currentDatetime.Hour == 22 && currentDatetime.Minute == 0)
            {
                await _webService.SendTestReminder();
            }
            
            await DoWork(events.Single(gw => gw.IsNext));
        }

        public async Task DoWork(Event e)
        {
            var oneDayAhead = ToNearest15Mins(_dateTimeProvider.UtcNow.AddHours(Consts.OneDay));
            var halfDayAhead = ToNearest15Mins(_dateTimeProvider.UtcNow.AddHours(Consts.HalfDay));
            var twoHoursAhead = ToNearest15Mins(_dateTimeProvider.UtcNow.AddHours(Consts.TwoHrs));

            if (oneDayAhead == e.DeadlineTime)
            {
                // one day message
                await _webService.SendReminder(Consts.OneDay, e.Id);
            }
            if (halfDayAhead == e.DeadlineTime)
            {
                // 12 hour message
                await _webService.SendReminder(Consts.HalfDay, e.Id);
            }
            if (twoHoursAhead == e.DeadlineTime)
            {
                // 2 hour message
                await _webService.SendReminder(Consts.TwoHrs, e.Id);
            }
        }

        public DateTime ToNearest15Mins(DateTime datetime)
        {
            return datetime.AddMinutes(-(datetime.Minute % 15)).AddSeconds(-datetime.Second);
        }
    }
}