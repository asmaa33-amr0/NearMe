using NearMe.Services.Common;

namespace NearMe.Services
{
    public class ImageStorageService
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxSizeBytes = 2 * 1024 * 1024;

        private readonly IWebHostEnvironment _env;

        public ImageStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<ServiceResult<string>> SaveAsync(IFormFile? file, string folder)
        {
            if (file == null || file.Length == 0)
                return ServiceResult<string>.Fail(ServiceError.Invalid, "No file uploaded.");

            if (file.Length > MaxSizeBytes)
                return ServiceResult<string>.Fail(ServiceError.Invalid, "File is too large (max 2 MB).");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return ServiceResult<string>.Fail(ServiceError.Invalid, "Only jpg, png and webp are allowed.");

            var dir = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(dir);

            var fileName = Guid.NewGuid() + ext;
            var fullPath = Path.Combine(dir, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return ServiceResult<string>.Ok($"/uploads/{folder}/{fileName}");
        }

        public void Delete(string? path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("/uploads/")) return;

            var relative = path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var full = Path.GetFullPath(Path.Combine(_env.WebRootPath, relative));
            var root = Path.GetFullPath(Path.Combine(_env.WebRootPath, "uploads")) + Path.DirectorySeparatorChar;

            if (!full.StartsWith(root)) return;

            if (File.Exists(full)) File.Delete(full);
        }
    }
}
