namespace AwsS3.Server.Dtos.Responses
{
    public sealed record S3ObjectResponse
    {
        public string Key { get; init; } = default!;
        public string FileName { get; init; } = default!;
        public long? Size { get; init; }
        public DateTime? LastModified { get; init; }
    }
}
