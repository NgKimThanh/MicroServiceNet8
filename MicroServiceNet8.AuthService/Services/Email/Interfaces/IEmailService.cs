namespace MicroServiceNet8.Auth.Services.Email.Interfaces
{
    public interface IEmailService
    {
        Task Send(string toEmail, string subject, string body);
    }
}
