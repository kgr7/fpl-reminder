using FPL.Reminder.src.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FPL.Reminder.src
{
    public interface IWebService
    {
        Task<List<Event>> GetEvents();
        Task<bool> SendReminder(int hoursRemaining, int gameweek);
        Task<bool> SendTestReminder();
    }
}
