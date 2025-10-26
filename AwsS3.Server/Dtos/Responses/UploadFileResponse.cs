namespace AwsS3.Server.Dtos.Responses
{
    public sealed record UploadFileResponse
    {
        public string Key { get; init; }=default!;
        public string FileName { get; init; } = default!;
        public string PreSignedUrl { get; init; } = default!;
    }
}
