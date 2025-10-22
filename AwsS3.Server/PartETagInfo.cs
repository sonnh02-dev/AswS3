using System.ComponentModel.DataAnnotations;
namespace AwsS3.Server
{

    public class PartETagInfo
    {
        [Required]
        public int PartNumber { get; set; }

        [Required]
        public string ETag { get; set; }
    }
}