using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using AwsS3.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("S3"));

// Register Amazon S3 client
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var s3Settings = sp.GetRequiredService<IOptions<S3Settings>>().Value;
    return new AmazonS3Client(
        new BasicAWSCredentials(s3Settings.AccessKey, s3Settings.SecretKey),
        RegionEndpoint.GetBySystemName(s3Settings.Region)
    );
});

var app = builder.Build();

// Enable Swagger and CORS in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}

app.MapControllers();

app.Run();
