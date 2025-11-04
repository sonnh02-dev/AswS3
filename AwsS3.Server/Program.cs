using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using AwsS3.Server;
using AwsS3.Server.Middlewares;
using AwsS3.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Chuyển tên property sang camelCase
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
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
builder.Services.AddScoped<IBucketService, BucketService>();
builder.Services.AddScoped<IFileService, FileService>();

// Register custom action filter to decode route keys
//builder.Services.AddControllers(options =>
//{
//    options.Filters.Add<DecodeRouteKeyAttribute>();
//});

var app = builder.Build();

app.UseRouting();

app.UseCors(policy =>
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());

app.UseMiddleware<ExceptionHandlingMiddleware>();


app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();