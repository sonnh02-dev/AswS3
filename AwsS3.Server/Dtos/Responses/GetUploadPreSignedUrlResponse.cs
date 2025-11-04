namespace AwsS3.Server.Dtos.Responses
{
    public sealed record GetUploadPreSignedUrlResponse(
        string Key,
        string UploadUrl
    );
}
