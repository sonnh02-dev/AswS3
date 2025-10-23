using Amazon.S3;
using Amazon.S3.Model;
using AwsS3.Server.Dtos;
using AwsS3.Server.Dtos.Requests;
using AwsS3.Server.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AwsS3.Server.Controllers
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


        [HttpPost]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload([FromForm] UploadFilesRequest request)
        {
            if (request.Files == null || request.Files.Count == 0)
                return BadRequest("No files uploaded");

            var results = new List<UploadFileResponse>();

            foreach (var file in request.Files)
            {
                if (file.Length == 0) continue;

                var prefixNormalized = request.Prefix.Trim().TrimStart('/').TrimEnd('/');
                var key = $"{prefixNormalized}/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var bucketName = request.BucketName ?? _s3Settings.BucketName;

                using var stream = file.OpenReadStream();
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = file.ContentType
                };

                putRequest.Metadata["file-name"] = file.FileName;

                await _s3Client.PutObjectAsync(putRequest);

                var fileUrl = $"https://{bucketName}.s3.amazonaws.com/{key}"; // hoặc CloudFront URL

                results.Add(new UploadFileResponse
                {
                    Key = key,
                    FileName = file.FileName,
                    Url = fileUrl
                });
            }

            return Ok(results);
        }



        [HttpGet("{*key}")]
        public async Task<IActionResult> Download(string key)
        {

            key = Uri.UnescapeDataString(key); //giải mã %2F -> /

            var getRequest = new GetObjectRequest
            {
                BucketName = _s3Settings.BucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(getRequest);

            string originalFileName = response.Metadata["file-name"];

            return File(response.ResponseStream, response.Headers.ContentType, originalFileName, enableRangeProcessing: true);//support resume download

        }


        [HttpGet("list")]
        public async Task<IActionResult> GetFiles([FromQuery] string folder)
        {
            var listRequest = new ListObjectsV2Request
            {
                BucketName = _s3Settings.BucketName,
                Prefix = $"{folder}/", // lấy tất cả object bắt đầu với prefix này
            };

            var files = new List<object>();

            ListObjectsV2Response listResponse;
            do
            {
                listResponse = await _s3Client.ListObjectsV2Async(listRequest);

                foreach (var s3Object in listResponse.S3Objects)
                {
                    files.Add(new
                    {
                        s3Object.Key,
                        Url = $"https://{_s3Settings.BucketName}.s3.amazonaws.com/{s3Object.Key}",
                        s3Object.Size,
                        s3Object.LastModified
                    });
                }

                listRequest.ContinuationToken = listResponse.NextContinuationToken;
            }
            while ((bool)listResponse.IsTruncated); // tiếp tục nếu còn nhiều trang

            return Ok(files);
        }

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
       // Hiển thị file (ảnh, PDF, video, v.v.)
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
