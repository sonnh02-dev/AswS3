using AwsS3.Server.Dtos;
using AwsS3.Server.Dtos.Requests;
using AwsS3.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace AwsS3.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        
        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] UploadFilesRequest request)
            => Ok(await _fileService.UploadFilesAsync(request));

        [HttpPost("start-multipart")]
        public async Task<IActionResult> StartMultipart([FromForm] string fileName, [FromForm] string contentType)
        {
            var result = await _fileService.StartMultipartUploadAsync(fileName, contentType);
            return Ok(result);
        }


        [HttpPost("{key}/presigned-part")]
        public IActionResult GetPresignedPartUrl(string key, [FromForm] string uploadId, [FromForm] int partNumber)
            => Ok(new { key, url = _fileService.GetPreSignedPartUrl(key, uploadId, partNumber) });

        [HttpPost("{key}/complete-multipart")]
        public async Task<IActionResult> CompleteMultipart(string key, [FromBody] CompleteMultipartUpload complete)
            => Ok(new { key, location = await _fileService.CompleteMultipartUploadAsync(key, complete) });

        [HttpGet("{*key}")]
        public async Task<IActionResult> Download(string key, [FromQuery] string? bucketName)
            => await _fileService.DownloadAsync(key, bucketName);

        [HttpGet("list")]
        public async Task<IActionResult> GetAllFiles([FromQuery] GetAllFilesRequest request)
            => Ok(await _fileService.GetAllFilesAsync(request));

        [HttpDelete("{*key}")]
        public async Task<IActionResult> Delete(string key, [FromQuery] string? bucketName)
        {
            await _fileService.DeleteFileAsync(key, bucketName);
            return Ok($"File {key} deleted successfully");
        }

        [HttpGet("presigned-url/{*key}")]
        public IActionResult GetPresignedUrl(string key, [FromQuery] string? bucketName)
            => Ok(new { key, url = _fileService.GeneratePreSignedUrlAsync(key, bucketName) });

        [HttpPost("presigned")]
        public IActionResult CreatePresignedUploadUrl([FromForm] string fileName, [FromForm] string contentType)
            => Ok(new { url = _fileService.GeneratePreSignedUploadUrl(fileName, contentType) });
    }
}
