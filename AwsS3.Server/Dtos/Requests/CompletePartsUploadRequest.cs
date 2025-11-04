using System.ComponentModel.DataAnnotations;

namespace AwsS3.Server.Dtos.Requests
{
    public sealed record CompletePartsUploadRequest(
        [Required] string UploadId,
        [Required] List<PartETagInfo> Parts,
        string? Key = null
    );

    public sealed record PartETagInfo(
        [Required] int PartNumber,
        [Required] string ETag
    );
}
