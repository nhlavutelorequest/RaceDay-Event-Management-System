using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services
{
    public class ImageUploadException : Exception
    {
        public ImageUploadException(string message) : base(message) { }
    }

    public interface IBlobStorageService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder);
    }

    public class BlobStorageService : IBlobStorageService
    {
        private const long MaxBytes = 5 * 1024 * 1024;

        private static readonly Dictionary<string, string> Extensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp",
            ["image/gif"] = ".gif"
        };

        private readonly IConfiguration _config;
        private BlobContainerClient? _container;

        public BlobStorageService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ImageUploadException("Please choose an image to upload.");

            if (file.Length > MaxBytes)
                throw new ImageUploadException("The image is too large. The maximum size is 5 MB.");

            if (!Extensions.TryGetValue(file.ContentType ?? "", out var extension))
                throw new ImageUploadException("Only JPG, PNG, WEBP or GIF images are allowed.");

            if (!await LooksLikeImageAsync(file))
                throw new ImageUploadException("That file doesn't look like a valid image.");

            var container = GetContainer();
            var blob = container.GetBlobClient($"{folder}/{Guid.NewGuid():N}{extension}");

            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blob.Uri.ToString();
        }

        // Checks the first bytes of the file so a renamed non-image file is rejected
        private static async Task<bool> LooksLikeImageAsync(IFormFile file)
        {
            var header = new byte[12];
            await using var stream = file.OpenReadStream();
            var read = await stream.ReadAsync(header.AsMemory(0, header.Length));
            if (read < header.Length) return false;

            var jpeg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            var png = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
            var gif = header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46;
            var webp = header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                       && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;

            return jpeg || png || gif || webp;
        }

        // Created on first upload so the API still starts (and tests still run) without Azure settings
        private BlobContainerClient GetContainer()
        {
            if (_container != null) return _container;

            var connectionString = _config["AzureBlob:ConnectionString"];
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Azure Blob Storage is not configured.");

            var containerName = _config["AzureBlob:ContainerName"] ?? "raceday-images";
            _container = new BlobServiceClient(connectionString).GetBlobContainerClient(containerName);
            return _container;
        }
    }
}