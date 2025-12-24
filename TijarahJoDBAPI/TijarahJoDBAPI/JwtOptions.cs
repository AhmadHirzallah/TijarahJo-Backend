namespace TijarahJoDBAPI
{
    public class JwtOptions
    {
        // Same as in: appsettings.Development.json
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int Lifetime { get; set; }
        public string SigningKey { get; set; }
    }
}

