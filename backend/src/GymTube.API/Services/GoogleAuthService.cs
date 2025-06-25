using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace GymTube.API.Services
{
    public class GoogleAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GoogleAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<GoogleUserInfo> GetUserInfoAsync(string authorizationCode)
        {
            var googleAuthSection = _configuration.GetSection("GoogleAuth");
            var clientId = googleAuthSection["ClientId"]!;
            var clientSecret = googleAuthSection["ClientSecret"]!;
            var redirectUri = googleAuthSection["RedirectUri"]!; // Povuci iz konfiguracije

            // Step 1: Exchange authorization code for access token
            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", authorizationCode),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            });

            var tokenResponse = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            
            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get access token: {tokenContent}");
            }

            var tokenData = JsonSerializer.Deserialize<GoogleTokenResponse>(tokenContent);
            if (tokenData?.AccessToken == null)
            {
                throw new Exception("Access token not received from Google");
            }

            // Step 2: Get user info using access token
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData.AccessToken);

            var userInfoResponse = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
            var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();

            if (!userInfoResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get user info: {userInfoContent}");
            }

            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(userInfoContent);
            if (userInfo == null)
            {
                throw new Exception("Failed to deserialize user info");
            }

            return userInfo;
        }
    }

    public class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
        public string? Scope { get; set; }
        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }
    }

    public class GoogleUserInfo
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }
        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }
        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
        [JsonPropertyName("verified_email")]
        public bool VerifiedEmail { get; set; }
    }
} 