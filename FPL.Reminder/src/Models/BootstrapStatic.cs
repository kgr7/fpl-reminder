using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FPL.Reminder.src.Models
{
    public class BootstrapStatic
    {
        [JsonPropertyName("events")]
        public List<Event> Events { get; set; }
    }
}
