using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace GymTube.API.Services
{
    // Klase koje predstavljaju JSON odgovor od Mux API-ja
    public class MuxUploadResponseData
    {
        public string Id { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class MuxApiCreateUploadResponse
    {
        public MuxUploadResponseData Data { get; set; } = new();
    }

    public class AssetStatusData
    {
        public required string Status { get; set; }
        public string? PlaybackId { get; set; }
        public double Duration { get; set; }
    }

    public class MuxService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MuxService> _logger;

        public MuxService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<MuxService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        // OVA verzija je jedina ispravna!
        public async Task<string> GetSignedUploadUrlAsync(Guid videoId)
        {
            try
            {
                var tokenId = _configuration["Mux:TokenId"];
                var tokenSecret = _configuration["Mux:TokenSecret"];

                var credentials = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes($"{tokenId}:{tokenSecret}")
                );

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                // passthrough MORA biti unutar new_asset_settings!
                var requestBody = new
                {
                    cors_origin = "*",
                    new_asset_settings = new
                    {
                        playback_policy = new[] { "public" },
                        passthrough = videoId.ToString()
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.mux.com/video/v1/uploads", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var uploadData = JsonSerializer.Deserialize<JsonElement>(responseContent);

                    if (uploadData.TryGetProperty("data", out var data) &&
                        data.TryGetProperty("url", out var url))
                    {
                        return url.GetString() ?? throw new Exception("Upload URL is null");
                    }
                    throw new Exception("Response from Mux does not contain expected 'data.url' field");
                }

                throw new Exception($"Failed to get signed upload URL from Mux. Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting signed upload URL from Mux");
                throw;
            }
        }

        // Ostatak filea (druge metode) ostaje nepromijenjen
        public async Task<AssetStatusData> GetAssetStatusAsync(string assetId)
        {
            try
            {
                var tokenId = _configuration["Mux:TokenId"];
                var tokenSecret = _configuration["Mux:TokenSecret"];

                var credentials = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes($"{tokenId}:{tokenSecret}")
                );

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                var response = await client.GetAsync($"https://api.mux.com/video/v1/assets/{assetId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var assetData = JsonSerializer.Deserialize<JsonElement>(content);

                    if (assetData.TryGetProperty("data", out var data))
                    {
                        var status = data.TryGetProperty("status", out var statusProp) ? statusProp.GetString() ?? "unknown" : "unknown";
                        var playbackId = "";
                        var duration = 0.0;

                        // Get playback ID
                        if (data.TryGetProperty("playback_ids", out var playbackIds) && playbackIds.GetArrayLength() > 0)
                        {
                            var firstPlaybackId = playbackIds[0];
                            if (firstPlaybackId.TryGetProperty("id", out var id))
                            {
                                playbackId = id.GetString() ?? string.Empty;
                            }
                        }

                        // Get duration
                        if (data.TryGetProperty("duration", out var durationProp))
                        {
                            duration = durationProp.GetDouble();
                        }

                        return new AssetStatusData
                        {
                            Status = status,
                            PlaybackId = playbackId,
                            Duration = duration
                        };
                    }
                    throw new Exception("Response from Mux does not contain expected 'data' field");
                }

                throw new Exception($"Failed to get asset status from Mux. Status: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting asset status for {AssetId}", assetId);
                throw;
            }
        }

        public async Task DeleteAssetAsync(string assetId)
        {
            try
            {
                var tokenId = _configuration["Mux:TokenId"];
                var tokenSecret = _configuration["Mux:TokenSecret"];
                var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{tokenId}:{tokenSecret}"));
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                var response = await client.DeleteAsync($"https://api.mux.com/video/v1/assets/{assetId}");
                // Ignoriraj 404 (asset već ne postoji)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Mux asset {AssetId}", assetId);
            }
        }
    }
}