using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AudicaDownloader
{

    public partial class AudicaSongList
    {
        [JsonProperty("pagesize")]
        public long Pagesize { get; set; }

        [JsonProperty("song_count")]
        public long SongCount { get; set; }

        [JsonProperty("page")]
        public long Page { get; set; }

        [JsonProperty("songs")]
        public List<Song> Songs { get; set; }

        [JsonProperty("total_pages")]
        public long TotalPages { get; set; }
    }

    public partial class Song
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("high_tempo")]
        public double HighTempo { get; set; }

        [JsonProperty("video_url")]
        public string VideoUrl { get; set; }

        [JsonProperty("midi_for_cues")]
        public bool MidiForCues { get; set; }

        [JsonProperty("song_id")]
        public string SongId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("advanced")]
        public bool Advanced { get; set; }

        [JsonProperty("beginner")]
        public bool Beginner { get; set; }

        [JsonProperty("uploader")]
        public double Uploader { get; set; }

        [JsonProperty("low_tempo")]
        public double LowTempo { get; set; }

        [JsonProperty("upload_time")]
        public DateTimeOffset UploadTime { get; set; }

        [JsonProperty("download_url")]
        public Uri DownloadUrl { get; set; }

        [JsonProperty("drum_kit")]
        public DrumKit DrumKit { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("expert")]
        public bool Expert { get; set; }

        [JsonProperty("standard")]
        public bool Standard { get; set; }

        [JsonProperty("song_length")]
        public double SongLength { get; set; }

        [JsonProperty("leaderboard_id")]
        public string LeaderboardId { get; set; }
    }

    public enum DrumKit { Destruct };

    public partial class AudicaSongList
    {
        public static AudicaSongList FromJson(string json) => JsonConvert.DeserializeObject<AudicaSongList>(json, AudicaDownloader.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this AudicaSongList self) => JsonConvert.SerializeObject(self, AudicaDownloader.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                DrumKitConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class DrumKitConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(DrumKit) || t == typeof(DrumKit?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "destruct")
            {
                return DrumKit.Destruct;
            }
            throw new Exception("Cannot unmarshal type DrumKit");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (DrumKit)untypedValue;
            if (value == DrumKit.Destruct)
            {
                serializer.Serialize(writer, "destruct");
                return;
            }
            throw new Exception("Cannot marshal type DrumKit");
        }

        public static readonly DrumKitConverter Singleton = new DrumKitConverter();
    }
}
