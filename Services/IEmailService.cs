namespace MecHub.Services;

public interface IEmailService
{
    Task EnviarEmailAsync(string destinatario, string assunto, string mensagem);
}