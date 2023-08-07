namespace aspnetauthentication.Options
{
    public class JwtBearerConfig
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SigningKey { get; set; }
        public string ExpiryDays { get; set; }
    }
}
