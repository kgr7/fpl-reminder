using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FPL.Reminder.src.Models
{
    public class WebhookMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
