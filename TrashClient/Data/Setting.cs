using System;
using System.IO;
using Newtonsoft.Json;

namespace TrashClient.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public sealed record Setting
    {
        private static readonly Lazy<Setting> Lazy = new(JsonConvert.DeserializeObject<Setting>(File.ReadAllText("./Setting.json")));

        public static Setting Value => Lazy.Value;

        [JsonProperty("awsAccessKeyId")]
        public string AwsAccessKeyId { get; init; }
        [JsonProperty("awsSecretAccessKey")]
        public string AwsSecretAccessKey { get; init; }
        [JsonProperty("modelArn")]
        public string ModelArn { get; init; }
        [JsonProperty("projectArn")]
        public string ProjectArn { get; init; }
        [JsonProperty("versionName")]
        public string VersionName { get; init; }
        [JsonProperty("s3BucketName")]
        public string S3BucketName { get; init; }
        [JsonProperty("queueUrl")]
        public string QueueUrl { get; init; }
    }
}