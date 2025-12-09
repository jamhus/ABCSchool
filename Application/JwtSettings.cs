namespace Application
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public int TokenExpiryTimeInMinutes { get; set; }
        public int RefreshTokenExpiryInDays { get; set; } 
    }
}
