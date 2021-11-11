namespace Selector {
    public class SpotifyAppCredentials {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public SpotifyAppCredentials() { }

        public SpotifyAppCredentials(string clientId, string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }
    }
}