using Microsoft.EntityFrameworkCore;
using NearMe.Data;
using NearMe.DTOs;
using NearMe.Models;
using NearMe.Services.Common;
using System.Linq.Expressions;

namespace NearMe.Services
{
    public class ProviderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ImageStorageService _storage;

        public ProviderService(ApplicationDbContext context, ImageStorageService storage)
        {
            _context = context;
            _storage = storage;
        }

        private static readonly Expression<Func<ProviderProfile, ProviderListItemDto>> ToListItem = p => new ProviderListItemDto
        {
            Id = p.Id,
            BusinessName = p.BusinessName,
            Address = p.Address,
            Contact = p.Contact,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            CategoryName = p.Category.Name,
            RegionName = p.Region.name,
            AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
            ReviewsCount = p.Reviews.Count()
        };

        private static readonly Expression<Func<ProviderProfile, ProviderResponseDto>> ToDetails = p => new ProviderResponseDto
        {
            Id = p.Id,
            BusinessName = p.BusinessName,
            Address = p.Address,
            Description = p.Description,
            Contact = p.Contact,
            Latitude =  p.Latitude,
            Longitude =  p.Longitude,
            AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
            ReviewsCount = p.Reviews.Count(),
            Category = new CategoryDto { Name = p.Category.Name, IconPath = p.Category.IconPath },
            Region = p.Region.name,
            Gallery = p.Gallery.Select(img => new ProviderImageDTO { ImageUrl = img.ImageUrl }).ToList()
        };

        private static (int page, int size) Normalize(int page, int size)
        {
            return (Math.Max(page, 1), Math.Clamp(size, 1, 50));
        }

        private async Task<string?> ValidateAsync(CreateProviderProfileDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.BusinessName)) return "Business name is required.";
            if (string.IsNullOrWhiteSpace(dto.Address)) return "Address is required.";

            //if (dto.Latitude.HasValue != dto.Longitude.HasValue)
            //    return "Latitude and longitude must be provided together.";

            if (dto.Latitude is < -90 or > 90) return "Latitude must be between -90 and 90.";
            if (dto.Longitude is < -180 or > 180) return "Longitude must be between -180 and 180.";

            if (!await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId)) return "Category does not exist.";
            if (!await _context.Regions.AnyAsync(r => r.id == dto.RegionId)) return "Region does not exist.";

            return null;
        }

        public async Task<List<ProviderListItemDto>> GetAllAsync(int page, int pageSize)
        {
            var (p, s) = Normalize(page, pageSize);

            return await _context.ProviderProfiles
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip((p - 1) * s).Take(s)
                .Select(ToListItem)
                .ToListAsync();
        }

        public async Task<ServiceResult<ProviderResponseDto>> GetByIdAsync(int id)
        {
            var provider = await _context.ProviderProfiles
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToDetails)
                .FirstOrDefaultAsync();

            if (provider == null)
                return ServiceResult<ProviderResponseDto>.Fail(ServiceError.NotFound, "Provider not found.");

            return ServiceResult<ProviderResponseDto>.Ok(provider);
        }

        public async Task<ServiceResult<ProviderResponseDto>> GetMyProfileAsync(string userId)
        {
            var provider = await _context.ProviderProfiles
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(ToDetails)
                .FirstOrDefaultAsync();

            if (provider == null)
                return ServiceResult<ProviderResponseDto>.Fail(ServiceError.NotFound, "You don't have a provider profile yet.");

            return ServiceResult<ProviderResponseDto>.Ok(provider);
        }

        public async Task<List<ProviderListItemDto>> SearchAsync(string? name, int? categoryId, int? regionId, int page, int pageSize)
        {
            var (p, s) = Normalize(page, pageSize);

            var query = _context.ProviderProfiles.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(x => x.BusinessName.Contains(name));

            if (categoryId.HasValue)
                query = query.Where(x => x.CategoryId == categoryId.Value);

            if (regionId.HasValue)
                query = query.Where(x => x.RegionId == regionId.Value);

            return await query
                .OrderBy(x => x.Id)
                .Skip((p - 1) * s).Take(s)
                .Select(ToListItem)
                .ToListAsync();
        }

        public async Task<ServiceResult<ProviderResponseDto>> CreateAsync(string userId, CreateProviderProfileDto dto)
        {
            if (await _context.ProviderProfiles.AnyAsync(x => x.UserId == userId))
                return ServiceResult<ProviderResponseDto>.Fail(ServiceError.Conflict, "You already have a provider profile.");

            var error = await ValidateAsync(dto);
            if (error != null)
                return ServiceResult<ProviderResponseDto>.Fail(ServiceError.Invalid, error);

            var provider = new ProviderProfile
            {
                UserId = userId,
                BusinessName = dto.BusinessName.Trim(),
                Address = dto.Address.Trim(),
                Description = dto.Description,
                Contact = dto.Contact?.Trim(),
                RegionId = dto.RegionId,
                CategoryId = dto.CategoryId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };

            _context.ProviderProfiles.Add(provider);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(provider.Id);
        }

        public async Task<ServiceResult<ProviderResponseDto>> UpdateAsync(int id, string userId, CreateProviderProfileDto dto)
        {
            var provider = await _context.ProviderProfiles.FirstOrDefaultAsync(x => x.Id == id);
            if (provider == null)
                return ServiceResult<ProviderResponseDto>.Fail(ServiceError.NotFound, "Provider profile not found.");

            if (provider.UserId != userId)
                return ServiceResult<ProviderResponseDto>.Fail(ServiceError.Forbidden, "");

            var error = await ValidateAsync(dto);
            if (error != null)
                return ServiceResult<ProviderResponseDto>.Fail(ServiceError.Invalid, error);

            provider.BusinessName = dto.BusinessName.Trim();
            provider.Address = dto.Address.Trim();
            provider.Description = dto.Description;
            provider.Contact = dto.Contact?.Trim();
            provider.RegionId = dto.RegionId;
            provider.CategoryId = dto.CategoryId;
            provider.Latitude = dto.Latitude;
            provider.Longitude = dto.Longitude;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, string userId, bool isAdmin)
        {
            var provider = await _context.ProviderProfiles
                .Include(x => x.Gallery)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (provider == null)
                return ServiceResult<bool>.Fail(ServiceError.NotFound, "Provider not found.");

            if (provider.UserId != userId && !isAdmin)
                return ServiceResult<bool>.Fail(ServiceError.Forbidden, "");

            var imageUrls = provider.Gallery.Select(i => i.ImageUrl).ToList();

            _context.ProviderProfiles.Remove(provider);
            await _context.SaveChangesAsync();

            foreach (var url in imageUrls)
                _storage.Delete(url);

            return ServiceResult<bool>.Ok(true);
        }
    }
}