namespace AwsS3.Server.Dtos.Requests
{
    public sealed record GetDownloadPreSignedUrlRequest(
        string Key,
        int ExpiresInMinutes = 15
    );
}
