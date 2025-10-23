namespace AwsS3.Server.Dtos.Responses
{
    public sealed record UploadFileResponse
    {
        public string Key { get; init; }
        public string FileName { get; init; }
        public string Url { get; init; }
    }
}
