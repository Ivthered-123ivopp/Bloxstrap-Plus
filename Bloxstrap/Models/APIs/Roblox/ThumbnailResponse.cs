﻿namespace Bloxstrap.Models.APIs.Roblox
{
    /// <summary>
    /// Roblox.Web.Responses.Thumbnails.ThumbnailResponse
    /// </summary>
    public class ThumbnailResponse
    {
        [JsonPropertyName("targetId")]
        public long TargetId { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; } = null!;

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = null!;
    }
}
