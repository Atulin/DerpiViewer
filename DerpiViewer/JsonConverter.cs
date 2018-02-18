// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using DerpiViewer;
//
//    var data = Derpi.FromJson(jsonString);

namespace DerpiViewer
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public partial class Derpi
    {
        [JsonProperty("search")]
        public List<Search> Search { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("interactions")]
        public List<object> Interactions { get; set; }
    }

    public partial class Search
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created_at")]
        public System.DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public System.DateTime UpdatedAt { get; set; }

        [JsonProperty("duplicate_reports")]
        public List<DuplicateReport> DuplicateReports { get; set; }

        [JsonProperty("first_seen_at")]
        public System.DateTime? FirstSeenAt { get; set; }

        [JsonProperty("uploader_id")]
        public string UploaderId { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }

        [JsonProperty("comment_count")]
        public long? CommentCount { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("file_name")]
        public string FileName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("uploader")]
        public string Uploader { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("upvotes")]
        public long Upvotes { get; set; }

        [JsonProperty("downvotes")]
        public long Downvotes { get; set; }

        [JsonProperty("faves")]
        public long Faves { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("tag_ids")]
        public List<string> TagIds { get; set; }

        [JsonProperty("aspect_ratio")]
        public double AspectRatio { get; set; }

        [JsonProperty("original_format")]
        public string OriginalFormat { get; set; }

        [JsonProperty("mime_type")]
        public string MimeType { get; set; }

        [JsonProperty("sha512_hash")]
        public string Sha512Hash { get; set; }

        [JsonProperty("orig_sha512_hash")]
        public string OrigSha512Hash { get; set; }

        [JsonProperty("source_url")]
        public string SourceUrl { get; set; }

        [JsonProperty("representations")]
        public Representations Representations { get; set; }

        [JsonProperty("is_rendered")]
        public bool IsRendered { get; set; }

        [JsonProperty("is_optimized")]
        public bool IsOptimized { get; set; }
    }

    public partial class DuplicateReport
    {
        [JsonProperty("id")]
        public long? Id { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("image_id")]
        public long? ImageId { get; set; }

        [JsonProperty("duplicate_of_image_id")]
        public long? DuplicateOfImageId { get; set; }

        [JsonProperty("user_id")]
        public object UserId { get; set; }

        [JsonProperty("modifier_id")]
        public long? ModifierId { get; set; }

        [JsonProperty("created_at")]
        public System.DateTime? CreatedAt { get; set; }
    }

    public partial class Representations
    {
        [JsonProperty("thumb_tiny")]
        public string ThumbTiny { get; set; }

        [JsonProperty("thumb_small")]
        public string ThumbSmall { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("small")]
        public string Small { get; set; }

        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }

        [JsonProperty("tall")]
        public string Tall { get; set; }

        [JsonProperty("full")]
        public string Full { get; set; }

        [JsonProperty("webm")]
        public string Webm { get; set; }

        [JsonProperty("mp4")]
        public string Mp4 { get; set; }
    }

    public partial class Derpi
    {
        public static Derpi FromJson(string json) => JsonConvert.DeserializeObject<Derpi>(json, DerpiViewer.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Derpi self) => JsonConvert.SerializeObject(self, DerpiViewer.Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}
