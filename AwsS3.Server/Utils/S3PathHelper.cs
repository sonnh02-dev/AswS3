using Amazon.Runtime.Internal;

namespace AwsS3.Server.Utils
{
    public static class S3PathHelper
    {

        public static string NormalizePrefix(string? inputPrefix)
        {
            var normalized = inputPrefix?.Trim().TrimStart('/').TrimEnd('/') ?? string.Empty;

            var allowedPrefixes = new[] { "uploads", "avatars", "documents" };

            if (!allowedPrefixes.Contains(normalized, StringComparer.OrdinalIgnoreCase))
                return "uploads";

            return normalized;

        }


        public static string BuildObjectKey(string? prefix)
        {
            var normalizedPrefix = NormalizePrefix(prefix);
            return $"{normalizedPrefix}/{Guid.NewGuid()}";
        }
    }
}
