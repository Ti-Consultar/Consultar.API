using System.Net.Http;
using System.Text.Json;
using _2___Application._2_Dto_s.Cep;
using _2___Application.Base;
using _4_InfraData._2_AppSettings;
using System.Linq;

public class CepService : BaseService
{
    private readonly HttpClient _httpClient;

    public CepService(IAppSettings appSettings) : base(appSettings)
    {
        _httpClient = new HttpClient();
    }

    public async Task<ResultValue> BuscarCep(string cep)
    {
        try
        {
            var cepFormatado = FormatarCep(cep);

            if (!EhCepValido(cepFormatado))
                return ErrorResponse("CEP inválido");

            var resposta = await ObterRespostaViaCep(cepFormatado);

            if (!resposta.IsSuccessStatusCode)
                return ErrorResponse("Erro ao buscar endereço no ViaCEP");

            var dto = await ConverterParaDto(resposta);

            return dto is null
                ? ErrorResponse("CEP não encontrado")
                : SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            return ErrorResponse($"Erro ao buscar CEP: {ex.Message}");
        }
    }

    // --- Métodos privados ---

    private string FormatarCep(string cep)
    {
        return new string(cep.Where(char.IsDigit).ToArray());
    }

    private bool EhCepValido(string cep)
    {
        return cep.Length == 8;
    }

    private async Task<HttpResponseMessage> ObterRespostaViaCep(string cep)
    {
        var url = $"https://viacep.com.br/ws/{cep}/json/";
        return await _httpClient.GetAsync(url);
    }

    private async Task<CepResponseDto?> ConverterParaDto(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(json);

        if (jsonDoc.RootElement.TryGetProperty("erro", out var erro) && erro.GetBoolean())
            return null;

        return new CepResponseDto
        {
            Cep = ObterValor(jsonDoc, "cep"),
            Logradouro = ObterValor(jsonDoc, "logradouro"),
            Complemento = ObterValor(jsonDoc, "complemento"),
            Bairro = ObterValor(jsonDoc, "bairro"),
            Localidade = ObterValor(jsonDoc, "localidade"),
            Uf = ObterValor(jsonDoc, "uf")
        };
    }

    private string? ObterValor(JsonDocument doc, string propriedade)
    {
        return doc.RootElement.TryGetProperty(propriedade, out var prop) ? prop.GetString() : null;
    }
}
