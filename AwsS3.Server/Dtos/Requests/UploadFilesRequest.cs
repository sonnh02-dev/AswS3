﻿using Microsoft.AspNetCore.Mvc;

namespace AwsS3.Server.Dtos.Requests
{
    public sealed record UploadFilesRequest(
     List<IFormFile> Files,
     string? Prefix = "uploads",
     string? BucketName = null
 );


}
