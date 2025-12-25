namespace TijarahJoDBAPI
{
    public class JwtOptions
    {
        // Same as in: appsettings.Development.json
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public int Lifetime { get; set; }
        public required string SigningKey { get; set; }
    }
}

