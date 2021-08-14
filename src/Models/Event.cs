using System;
using System.Text.Json.Serialization;

namespace FPL.Reminder.src.Models
{
    public class Event
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("is_next")]
        public bool IsNext { get; set; }

        [JsonPropertyName("is_current")]
        public bool IsCurrent { get; set; }

        [JsonPropertyName("deadline_time")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime DeadlineTime { get; set; }
    }
}
