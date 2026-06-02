using GhaithAI.API.Configurations;
using GhaithAI.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Runtime;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GhaithAI.API.Services.Class
{
    public class LangflowService : ILangflowService
    {
        private readonly HttpClient _httpClient;
        private readonly LangflowSettings _langflowSettings;

        public LangflowService(HttpClient httpClient, IOptions<LangflowSettings> langflowSettings)
        {
            _httpClient = httpClient;
            _langflowSettings = langflowSettings.Value;
        }

        public async Task<string> SendMessageAsync(string userMessage , string sessionId)
        {
            var request = new
            {
                input_value = userMessage,
                session_id = sessionId,
                output_type = "chat",
                input_type = "chat"
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _langflowSettings.ApiKey);

            var response =await _httpClient.PostAsJsonAsync($"{_langflowSettings.BaseUrl}/api/v1/run/{_langflowSettings.FlowId}?stream=false",request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Langflow Error: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<LangflowResponse>();

            return result!.Outputs[0].Outputs[0].Results.Message.Text;
        }
    }


    public class LangflowResponse
    { 
        [JsonPropertyName("outputs")]
        public List<OutputItem> Outputs { get; set; } = new();
    }

    public class OutputItem
    {
        [JsonPropertyName("outputs")]
        public List<OutputData> Outputs { get; set; } = new();
    }

    public class OutputData
    {
        [JsonPropertyName("results")]
        public ResultData Results { get; set; } = new();
    }

    public class ResultData
    {
        [JsonPropertyName("message")]
        public MessageData Message { get; set; } = new();
    }

    public class MessageData
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

}
