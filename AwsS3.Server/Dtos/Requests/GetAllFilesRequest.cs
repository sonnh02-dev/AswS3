namespace AwsS3.Server.Dtos.Requests
{
    public sealed record GetAllFilesRequest
    {
      
     
        public string Prefix { get; init; }= string.Empty;

        public int? PageSize { get; init; }

        /// Token để lấy trang kế tiếp (AWS S3 dùng ContinuationToken)
        public string? ContinuationToken { get; init; }

       
    }
}
