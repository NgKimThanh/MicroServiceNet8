namespace MicroServiceNet8.DTO.Auth
{
    public class RefreshToken
    {
        public int UserID { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefeshToken { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Expires { get; set; }
    }
}
