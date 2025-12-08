namespace Application
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public int TokenExpiryInMinutes { get; set; }
        public int RefreshTokenExpiryInDays { get; set; } 
    }
}
