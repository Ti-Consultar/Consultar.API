using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using _2___Application.Base;
using _4_InfraData._2_AppSettings;

public class ChatGptService : BaseService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ChatGptService(IAppSettings appSettings) : base(appSettings)
    {
        _httpClient = new HttpClient();
        _apiKey = appSettings.OpenAIApiKey; // ⚡ Sua chave no appsettings.json
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<ResultValue> Perguntar(string pergunta)
    {
        try
        {
            var requestBody = new
            {
                model = "gpt-4o-mini", // ou gpt-4.1/gpt-3.5-turbo
                messages = new[]
                {
                    new { role = "system", content = "Você é um assistente especializado em contabilidade e gestão empresarial." },
                    new { role = "user", content = pergunta }
                },
                max_tokens = 500
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
                return ErrorResponse($"Erro da API OpenAI: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            var resposta = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return SuccessResponse(resposta);
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Erro ao consultar ChatGPT: {ex.Message}");
        }
    }
}
