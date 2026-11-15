using Microsoft.EntityFrameworkCore;
using NearMe.Data;
using NearMe.DTOs;
using NearMe.Models;
using NearMe.Services.Common;

namespace NearMe.Services
{
    public class ProviderImageService
    {
        private const int MaxImages = 10;

        private readonly ApplicationDbContext _context;
        private readonly ImageStorageService _storage;

        public ProviderImageService(ApplicationDbContext context, ImageStorageService storage)
        {
            _context = context;
            _storage = storage;
        }

        private static bool IsValidUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            if (url.Length > 500) return false;
            if (url.StartsWith("http://") || url.StartsWith("https://")) return true;
            return url.StartsWith("/uploads/") && !url.Contains("..");
        }

        private static ProviderImageResponseDto ToDto(ProviderImage i)
        {
            return new ProviderImageResponseDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                ProviderProfileId = i.ProviderProfileId
            };
        }

        public async Task<ServiceResult<ProviderImageResponseDto>> GetByIdAsync(int id)
        {
            var img = await _context.ProviderImages.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
            if (img == null)
                return ServiceResult<ProviderImageResponseDto>.Fail(ServiceError.NotFound, "Image not found.");

            return ServiceResult<ProviderImageResponseDto>.Ok(ToDto(img));
        }

        public async Task<ServiceResult<List<ProviderImageResponseDto>>> GetByProviderAsync(int providerId)
        {
            if (!await _context.ProviderProfiles.AnyAsync(p => p.Id == providerId))
                return ServiceResult<List<ProviderImageResponseDto>>.Fail(ServiceError.NotFound, "Provider not found.");

            var list = await _context.ProviderImages.AsNoTracking()
                .Where(i => i.ProviderProfileId == providerId)
                .Select(i => new ProviderImageResponseDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    ProviderProfileId = i.ProviderProfileId
                })
                .ToListAsync();

            return ServiceResult<List<ProviderImageResponseDto>>.Ok(list);
        }

        private async Task<ServiceResult<ProviderProfile>> GetProfileForUploadAsync(string userId)
        {
            var profile = await _context.ProviderProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
                return ServiceResult<ProviderProfile>.Fail(ServiceError.NotFound, "Provider profile not found.");

            var count = await _context.ProviderImages.CountAsync(i => i.ProviderProfileId == profile.Id);
            if (count >= MaxImages)
                return ServiceResult<ProviderProfile>.Fail(ServiceError.Conflict, $"Maximum {MaxImages} images allowed.");

            return ServiceResult<ProviderProfile>.Ok(profile);
        }

        public async Task<ServiceResult<ProviderImageResponseDto>> AddAsync(string userId, ProviderImageDTO dto)
        {
            if (!IsValidUrl(dto.ImageUrl))
                return ServiceResult<ProviderImageResponseDto>.Fail(ServiceError.Invalid,
                    "Image must be an http(s) link or an uploaded file path.");

            var profileResult = await GetProfileForUploadAsync(userId);
            if (!profileResult.Success)
                return ServiceResult<ProviderImageResponseDto>.Fail(profileResult.Error, profileResult.Message!);

            var img = new ProviderImage
            {
                ImageUrl = dto.ImageUrl,
                ProviderProfileId = profileResult.Data!.Id
            };

            _context.ProviderImages.Add(img);
            await _context.SaveChangesAsync();

            return ServiceResult<ProviderImageResponseDto>.Ok(ToDto(img));
        }

        public async Task<ServiceResult<ProviderImageResponseDto>> UploadAsync(string userId, IFormFile? file)
        {
            var profileResult = await GetProfileForUploadAsync(userId);
            if (!profileResult.Success)
                return ServiceResult<ProviderImageResponseDto>.Fail(profileResult.Error, profileResult.Message!);

            var saved = await _storage.SaveAsync(file, "providers");
            if (!saved.Success)
                return ServiceResult<ProviderImageResponseDto>.Fail(saved.Error, saved.Message!);

            var img = new ProviderImage
            {
                ImageUrl = saved.Data!,
                ProviderProfileId = profileResult.Data!.Id
            };

            _context.ProviderImages.Add(img);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                _storage.Delete(saved.Data);
                throw;
            }

            return ServiceResult<ProviderImageResponseDto>.Ok(ToDto(img));
        }

        public async Task<ServiceResult<ProviderImageResponseDto>> UpdateAsync(int id, string userId, ProviderImageDTO dto)
        {
            if (!IsValidUrl(dto.ImageUrl))
                return ServiceResult<ProviderImageResponseDto>.Fail(ServiceError.Invalid,
                    "Image must be an http(s) link or an uploaded file path.");

            var img = await _context.ProviderImages.Include(i => i.ProviderProfile)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (img == null)
                return ServiceResult<ProviderImageResponseDto>.Fail(ServiceError.NotFound, "Image not found.");

            if (img.ProviderProfile.UserId != userId)
                return ServiceResult<ProviderImageResponseDto>.Fail(ServiceError.Forbidden, "");

            var oldUrl = img.ImageUrl;
            img.ImageUrl = dto.ImageUrl;
            await _context.SaveChangesAsync();

            if (oldUrl != dto.ImageUrl) _storage.Delete(oldUrl);

            return ServiceResult<ProviderImageResponseDto>.Ok(ToDto(img));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, string userId, bool isAdmin)
        {
            var img = await _context.ProviderImages.Include(i => i.ProviderProfile)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (img == null)
                return ServiceResult<bool>.Fail(ServiceError.NotFound, "Image not found.");

            if (img.ProviderProfile.UserId != userId && !isAdmin)
                return ServiceResult<bool>.Fail(ServiceError.Forbidden, "");

            var url = img.ImageUrl;
            _context.ProviderImages.Remove(img);
            await _context.SaveChangesAsync();

            _storage.Delete(url);

            return ServiceResult<bool>.Ok(true);
        }
    }
}
