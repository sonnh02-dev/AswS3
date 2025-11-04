namespace AwsS3.Server.Dtos.Responses
{
    public sealed record BucketResponse
    {
        public string BucketName { get; init; } = string.Empty;
        public DateTime? CreationDate { get; init; }
    }
}
