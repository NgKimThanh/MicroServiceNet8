namespace MicroServiceNet8.Services.Services.Email.Interfaces
{
    public interface IEmailService
    {
        Task Send(string toEmail, string subject, string body);
    }
}
