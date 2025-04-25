namespace MicroServiceNet8.Auth.Services.User.Interfaces
{
    public interface ICurrentUser
    {
        int UserID { get; }
        string? Email { get; }
        string? Name { get; }
    }
}
