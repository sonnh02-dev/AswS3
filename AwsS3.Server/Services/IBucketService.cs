using AwsS3.Server.Dtos.Responses;

namespace AwsS3.Server.Services
{
    public interface IBucketService
    {
        Task<bool> CreateBucketAsync(string bucketName);
        Task<IEnumerable<BucketResponse>> GetAllBucketsAsync();
        Task<bool> DeleteBucketAsync(string bucketName);
        Task<bool> CheckBucketExistsAsync(string bucketName);

    }
}
