using System.ComponentModel.DataAnnotations;

namespace AwsS3.Server.Dtos {

public class CompleteMultipartUpload
{
    [Required]
    public string Key { get; set; }

    [Required]
    public string UploadId { get; set; }

    [Required]
    public List<PartETagInfo> Parts { get; set; }
}
}