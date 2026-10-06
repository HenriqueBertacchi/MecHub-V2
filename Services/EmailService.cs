using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MecHub.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public EmailService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task EnviarEmailAsync(string destinatario, string assunto, string mensagem)
    {
        var apiKey = _configuration["Resend:ApiKey"];
        var from = _configuration["Resend:From"];

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(from))
            throw new Exception("Configurações do Resend não encontradas.");

        var payload = new
        {
            from = from,
            to = new[] { destinatario },
            subject = assunto,
            text = mensagem
        };

        var json = JsonSerializer.Serialize(payload);

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Erro Resend: {response.StatusCode} - {responseBody}");
    }
}