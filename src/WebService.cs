using FPL.Reminder.src.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FPL.Reminder.src
{
    public class WebService : IWebService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        public WebService(IConfiguration config)
        {
            _config = config;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", Consts.UserAgent);
        }
        public async Task<List<Event>> GetEvents()
        {
            var response = await _httpClient.GetAsync(Consts.BootstrapStaticUrl);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var bootstrapStatic = JsonSerializer.Deserialize<BootstrapStatic>(responseAsString);

            return bootstrapStatic.Events;
        }

        public async Task<bool> SendReminder(int hoursRemaining, int gameweek)
        {
            var msg = $"{hoursRemaining} hours until gameweek {gameweek} deadline";
            var webhookMsg = new WebhookMessage { Content = msg };
            var webhookMsgAsString = JsonSerializer.Serialize(webhookMsg);
            var webhookHttpString = new StringContent(webhookMsgAsString, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_config.GetValue<string>("WebhookUrl"), webhookHttpString);

            return response.IsSuccessStatusCode;
        }
    }
}
