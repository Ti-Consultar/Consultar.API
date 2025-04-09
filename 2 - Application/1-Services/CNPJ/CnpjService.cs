using System.Net.Http;
using System.Text.Json;
using _2___Application._2_Dto_s.CNPJ;
using _2___Application.Base;
using _4_InfraData._2_AppSettings;
using System.Linq;

public class CnpjService : BaseService
{
    private readonly HttpClient _httpClient;

    public CnpjService(IAppSettings appSettings) : base(appSettings)
    {
        _httpClient = new HttpClient();
    }

    public async Task<ResultValue> BuscarCnpj(string cnpj)
    {
        try
        {
            var cnpjFormatado = FormatarCnpj(cnpj);

            if (!EhCnpjValido(cnpjFormatado))
                return ErrorResponse("CNPJ inválido");

            var resposta = await ObterRespostaReceitaWs(cnpjFormatado);

            if (!resposta.IsSuccessStatusCode)
                return ErrorResponse("Erro ao buscar CNPJ na ReceitaWS");

            var dto = await ConverterParaDto(resposta);

            return dto is null
                ? ErrorResponse("CNPJ não encontrado ou inválido")
                : SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Erro ao buscar CNPJ: {ex.Message}");
        }
    }

    // --- Métodos privados ---

    private string FormatarCnpj(string cnpj)
    {
        return new string(cnpj.Where(char.IsDigit).ToArray());
    }

    private bool EhCnpjValido(string cnpj)
    {
        return cnpj.Length == 14;
    }

    private async Task<HttpResponseMessage> ObterRespostaReceitaWs(string cnpj)
    {
        var url = $"https://www.receitaws.com.br/v1/cnpj/{cnpj}";
        return await _httpClient.GetAsync(url);
    }

    private async Task<CnpjResponseDto?> ConverterParaDto(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(json);

        if (jsonDoc.RootElement.TryGetProperty("status", out var status) &&
            status.GetString()?.ToUpper() == "ERROR")
        {
            return null;
        }

        return new CnpjResponseDto
        {
            Cnpj = ObterValor(jsonDoc, "cnpj"),
            Nome = ObterValor(jsonDoc, "nome"),
            Fantasia = ObterValor(jsonDoc, "fantasia"),
        };
    }

    private string? ObterValor(JsonDocument doc, string propriedade)
    {
        return doc.RootElement.TryGetProperty(propriedade, out var prop) ? prop.GetString() : null;
    }
}
