using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using AwsS3.Server.Dtos.Responses;
using Microsoft.Extensions.Options;
using System.Runtime;


namespace AwsS3.Server.Services
{
    internal class BucketService : IBucketService
    {
        private readonly IAmazonS3 _s3Client;

        public BucketService(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task<bool> CreateBucketAsync(string bucketName)
        {
            bool bucketExists = await CheckBucketExistsAsync(bucketName);
            if (bucketExists) return false;

            await _s3Client.PutBucketAsync(new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true
            });

            return true;
        }

        public async Task<IEnumerable<BucketResponse>> GetAllBucketsAsync()
        {
            var response = await _s3Client.ListBucketsAsync();
            return response.Buckets.Select(b => new BucketResponse
            {
                BucketName = b.BucketName,
                CreationDate = b.CreationDate
            });
        }

        public async Task<bool> DeleteBucketAsync(string bucketName)
        {
            bool bucketExists = await CheckBucketExistsAsync(bucketName);
            if (!bucketExists) return false;

            await _s3Client.DeleteBucketAsync(new DeleteBucketRequest
            {
                BucketName = bucketName
            });

            return true;
        }

        public async Task<bool> CheckBucketExistsAsync(string bucketName)
        {
            return await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
        }

    }
}
