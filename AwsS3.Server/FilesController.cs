using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AwsS3.Server
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;
        private readonly S3Settings _s3Settings;

        public FilesController(IAmazonS3 s3Client, IOptions<S3Settings> s3Settings)
        {
            _s3Client = s3Client;
            _s3Settings = s3Settings.Value;
        }

        // POST api/files
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file.Length == 0)
                return BadRequest("No file uploaded");

            using var stream = file.OpenReadStream();
            var key = Guid.NewGuid().ToString();

            var putRequest = new PutObjectRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = $"files/{key}",
                InputStream = stream,
                ContentType = file.ContentType
            };
            putRequest.Metadata["file-name"] = file.FileName;

            await _s3Client.PutObjectAsync(putRequest);

            return Ok(new { key, fileName = file.FileName });
        }

        // GET api/files/{key}
        [HttpGet("{key}")]
        public async Task<IActionResult> GetFile(string key)
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = $"files/{key}"
            };

            var response = await _s3Client.GetObjectAsync(getRequest);

            // AWS Metadata keys are case-insensitive, but stored in lowercase internally.
            string fileName = response.Metadata["file-name"];

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = key;
            }
            return File(response.ResponseStream, response.Headers.ContentType, fileName);
        }

        // DELETE api/files/{key}
        [HttpDelete("{key}")]
        public async Task<IActionResult> DeleteFile(string key)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = $"files/{key}"
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);
            return Ok($"File {key} deleted successfully");
        }

        // GET api/files/{key}/presigned
        [HttpGet("{key}/presigned")]
        public IActionResult GetPresignedDownloadUrl(string key)
        {
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _s3Settings.BucketName,
                    Key = $"files/{key}",
                    Verb = HttpVerb.GET,
                    Expires = DateTime.UtcNow.AddMinutes(15)
                };

                string url = _s3Client.GetPreSignedURL(request);
                return Ok(new { key, url });
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"S3 error generating pre-signed URL: {ex.Message}");
            }
        }

        // POST api/files/presigned
        [HttpPost("presigned")]
        public IActionResult CreatePresignedUploadUrl([FromForm] string fileName, [FromForm] string contentType)
        {
            try
            {
                var key = Guid.NewGuid().ToString();
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _s3Settings.BucketName,
                    Key = $"files/{key}",
                    Verb = HttpVerb.PUT,
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    ContentType = contentType
                };
                request.Metadata["file-name"] = fileName;

                string url = _s3Client.GetPreSignedURL(request);
                return Ok(new { key, url });
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"S3 error generating pre-signed upload URL: {ex.Message}");
            }
        }

        // POST api/files/start-multipart
        [HttpPost("start-multipart")]
        public async Task<IActionResult> StartMultipartUpload([FromForm] string fileName, [FromForm] string contentType)
        {
            try
            {
                var key = Guid.NewGuid().ToString();
                var request = new InitiateMultipartUploadRequest
                {
                    BucketName = _s3Settings.BucketName,
                    Key = $"files/{key}",
                    ContentType = contentType
                };
                request.Metadata["file-name"] = fileName;

                var response = await _s3Client.InitiateMultipartUploadAsync(request);
                return Ok(new { key, uploadId = response.UploadId });
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"S3 error starting multipart upload: {ex.Message}");
            }
        }

        // POST api/files/{key}/presigned-part
        [HttpPost("{key}/presigned-part")]
        public IActionResult GetPresignedPartUrl(
            string key,
            [FromForm] string uploadId,
            [FromForm] int partNumber)
        {
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _s3Settings.BucketName,
                    Key = $"files/{key}",
                    Verb = HttpVerb.PUT,
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    UploadId = uploadId,
                    PartNumber = partNumber
                };

                string url = _s3Client.GetPreSignedURL(request);
                return Ok(new { key, url });
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"S3 error generating pre-signed URL for part: {ex.Message}");
            }
        }

        // POST api/files/{key}/complete-multipart
        [HttpPost("{key}/complete-multipart")]
        public async Task<IActionResult> CompleteMultipartUpload(string key, [FromBody] CompleteMultipartUpload complete)
        {
            try
            {
                var request = new CompleteMultipartUploadRequest
                {
                    BucketName = _s3Settings.BucketName,
                    Key = $"files/{key}",
                    UploadId = complete.UploadId,
                    PartETags = complete.Parts.Select(p => new PartETag(p.PartNumber, p.ETag)).ToList()
                };

                var response = await _s3Client.CompleteMultipartUploadAsync(request);
                return Ok(new { key, location = response.Location });
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"S3 error completing multipart upload: {ex.Message}");
            }
        }
    }
}
