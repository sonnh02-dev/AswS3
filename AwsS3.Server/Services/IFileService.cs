using AwsS3.Server.Dtos.Requests;
using AwsS3.Server.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AwsS3.Server.Services
{
    public interface IFileService
    {
        // ============================================================
        //   Single file operations (upload, download, list, delete)
        // ============================================================
        GetUploadPreSignedUrlResponse GetUploadPreSignedUrl(GetUploadPreSignedUrlRequest request);
        Task<List<UploadFileResponse>> UploadFilesAsync(UploadFilesRequest request);
        string GetDownloadPreSignedUrl(GetDownloadPreSignedUrlRequest request);
        Task<FileStreamResult> DownloadFileAsync(string key);
        Task<GetAllFilesResponse> GetAllFilesAsync(GetAllFilesRequest request);
        Task DeleteFileAsync(string key);

        // ============================================================
        //    Multipart upload operations (for large files)
        // ============================================================
        Task<InitiatePartsUploadResponse> InitiatePartsUploadAsync(InitiatePartsUploadRequest request);
        string GetUploadPartPreSignedUrl(GetUploadPartPreSignedUrlRequest request);
        Task<string> CompletePartsUploadAsync(CompletePartsUploadRequest request);
    }
}
