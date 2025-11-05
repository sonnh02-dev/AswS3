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

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFiles([FromForm] UploadFilesRequest request)
            => Ok(await _fileService.UploadFilesAsync(request));


        [HttpGet("{key}/download")]
        public async Task<IActionResult> DownloadFileAsync(string key)
            => await _fileService.DownloadFileAsync(key);

        [HttpGet]
        public async Task<IActionResult> GetAllFiles([FromQuery] GetAllFilesRequest request)
            => Ok(await _fileService.GetAllFilesAsync(request));

        [HttpDelete("{key}")]
        public async Task<IActionResult> Delete(string key)
        {
            await _fileService.DeleteFileAsync(key);
            return Ok($"File {key} deleted successfully");
        }

        [HttpPost("upload-presigned-url")]
        public IActionResult GetUploadPreSignedUrl([FromBody] GetUploadPreSignedUrlRequest request)
            => Ok(_fileService.GetUploadPreSignedUrl(request));

        //[HttpGet("{key}/download-presigned-url")]
        //public IActionResult CreatePresignedUploadUrl([FromQuery] GetDownloadPreSignedUrlRequest request)
        //    => Ok(new { downloadUrl = _fileService.GetDownloadPreSignedUrl(request) });



        [HttpPost("initiate-parts-upload")]
        public async Task<IActionResult> InitiatePartsUpload([FromBody] InitiatePartsUploadRequest request)
        {
            var result = await _fileService.InitiatePartsUploadAsync(request);
            return Ok(result);
        }



        [HttpPost("{key}/upload-part-presigned-url")]
        //[DecodeRouteKey]
        public IActionResult GetUploadPartPreSignedUrl(string key, [FromBody] GetUploadPartPreSignedUrlRequest request)

            => Ok(new { PreSignedUrl = _fileService.GetUploadPartPreSignedUrl(request with { Key = key }) });


        [HttpPost("{key}/complete-parts-upload")]
        public async Task<IActionResult> CompletePartsUpload(string key, [FromBody] CompletePartsUploadRequest request)
            => Ok(new { key, Location = await _fileService.CompletePartsUploadAsync(request with { Key = key }) });

    }
}
