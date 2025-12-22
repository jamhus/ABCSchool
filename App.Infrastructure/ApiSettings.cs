namespace Portal.Infrastructure
{
    public class ApiSettings
    {
        public string BaseAPIUrl { get; set; }
        public TokenEndpoints TokenEndpoints { get; set; }
    }

    public class TokenEndpoints
    {
        public string Login { get; set; }
        public string GetRefreshToken { get; set; }

    }
}
