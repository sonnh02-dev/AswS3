namespace AwsS3.Server.Dtos.Requests
{
    public sealed record GetUploadPartPreSignedUrlRequest(
     string UploadId,
     int PartNumber,
     string? Key = null
 );

}
