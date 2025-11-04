using AwsS3.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace AwsS3.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BucketsController : ControllerBase
    {
        private readonly IBucketService _bucketService;

        public BucketsController(IBucketService bucketService)
        {
            _bucketService = bucketService;
        }

        [HttpPost("{bucketName}")]
        public async Task<IActionResult> CreateBucketAsync(string bucketName)
        {
            bool created = await _bucketService.CreateBucketAsync(bucketName);
            if (!created)
                return BadRequest($"Bucket '{bucketName}' already exists.");

            return Created($"/api/buckets/{bucketName}", new { message = $"Bucket '{bucketName}' created." });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBucketsAsync()
        {
            var buckets = await _bucketService.GetAllBucketsAsync();
            return Ok(buckets);
        }

        [HttpDelete("{bucketName}")]
        public async Task<IActionResult> DeleteBucketAsync(string bucketName)
        {
            bool deleted = await _bucketService.DeleteBucketAsync(bucketName);
            if (!deleted)
                return NotFound($"Bucket '{bucketName}' not found.");

            return Ok(new { message = $"Bucket '{bucketName}' deleted successfully." });
        }

        [HttpGet("{bucketName}/exists")]
        public async Task<IActionResult> CheckBucketExistsAsync(string bucketName)
        {
            bool exists = await _bucketService.CheckBucketExistsAsync(bucketName);
            return Ok(new { bucketName, exists });
        }
    }
}
