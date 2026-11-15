using Microsoft.EntityFrameworkCore;
using NearMe.Data;
using NearMe.DTOs;
using NearMe.Models;
using NearMe.Services.Common;
using System.Linq.Expressions;

namespace NearMe.Services
{
    public class ReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static readonly Expression<Func<Review, ReviewResponseDto>> ToDto = r => new ReviewResponseDto
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            ProviderProfileId = r.ProviderProfileId,
            UserName = r.User.fullname
        };

        private static (int page, int size) Normalize(int page, int size)
        {
            return (Math.Max(page, 1), Math.Clamp(size, 1, 50));
        }

        private static string? ValidateRating(int rating)
        {
            return rating < 1 || rating > 5 ? "Rating must be between 1 and 5." : null;
        }

        public async Task<List<ReviewResponseDto>> GetAllAsync(int page, int pageSize)
        {
            var (p, s) = Normalize(page, pageSize);

            return await _context.Reviews.AsNoTracking()
                .OrderByDescending(r => r.CreatedAt)
                .Skip((p - 1) * s).Take(s)
                .Select(ToDto)
                .ToListAsync();
        }

        public async Task<ServiceResult<ReviewResponseDto>> GetByIdAsync(int id)
        {
            var review = await _context.Reviews.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToDto)
                .FirstOrDefaultAsync();

            if (review == null)
                return ServiceResult<ReviewResponseDto>.Fail(ServiceError.NotFound, "Review not found.");

            return ServiceResult<ReviewResponseDto>.Ok(review);
        }

        public async Task<ServiceResult<List<ReviewResponseDto>>> GetByProviderAsync(int providerId, int page, int pageSize)
        {
            if (!await _context.ProviderProfiles.AnyAsync(p => p.Id == providerId))
                return ServiceResult<List<ReviewResponseDto>>.Fail(ServiceError.NotFound, "Provider not found.");

            var (p, s) = Normalize(page, pageSize);

            var list = await _context.Reviews.AsNoTracking()
                .Where(r => r.ProviderProfileId == providerId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((p - 1) * s).Take(s)
                .Select(ToDto)
                .ToListAsync();

            return ServiceResult<List<ReviewResponseDto>>.Ok(list);
        }

        public async Task<ServiceResult<ReviewResponseDto>> GetMineAsync(string userId, int providerId)
        {
            var review = await _context.Reviews.AsNoTracking()
                .Where(r => r.UserId == userId && r.ProviderProfileId == providerId)
                .Select(ToDto)
                .FirstOrDefaultAsync();

            if (review == null)
                return ServiceResult<ReviewResponseDto>.Fail(ServiceError.NotFound, "You have not reviewed this provider.");

            return ServiceResult<ReviewResponseDto>.Ok(review);
        }

        public async Task<ServiceResult<ReviewResponseDto>> CreateAsync(string userId, CreateReviewDto dto)
        {
            var err = ValidateRating(dto.Rating);
            if (err != null)
                return ServiceResult<ReviewResponseDto>.Fail(ServiceError.Invalid, err);

            var provider = await _context.ProviderProfiles.AsNoTracking()
                .Select(p => new { p.Id, p.UserId })
                .FirstOrDefaultAsync(p => p.Id == dto.ProviderProfileId);

            if (provider == null)
                return ServiceResult<ReviewResponseDto>.Fail(ServiceError.NotFound, "Provider not found.");

            if (provider.UserId == userId)
                return ServiceResult<ReviewResponseDto>.Fail(ServiceError.Invalid, "You cannot review your own profile.");

            if (await _context.Reviews.AnyAsync(r => r.UserId == userId && r.ProviderProfileId == dto.ProviderProfileId))
                return ServiceResult<ReviewResponseDto>.Fail(ServiceError.Conflict, "You have already reviewed this provider.");

            var review = new Review
            {
                Rating = dto.Rating,
                Comment = dto.Comment?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                ProviderProfileId = dto.ProviderProfileId
            };

            _context.Reviews.Add(review);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return ServiceResult<ReviewResponseDto>.Fail(ServiceError.Conflict, "You have already reviewed this provider.");
            }

            return await GetByIdAsync(review.Id);
        }

        public async Task<ServiceResult<ReviewResponseDto>> UpdateAsync(int id, string userId, UpdateReviewDto dto)
        {
            var err = ValidateRating(dto.Rating);
            if (err != null)
                return ServiceResult<ReviewResponseDto>.Fail(ServiceError.Invalid, err);

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return ServiceResult<ReviewResponseDto>.Fail(ServiceError.NotFound, "Review not found.");

            if (review.UserId != userId)
                return ServiceResult<ReviewResponseDto>.Fail(ServiceError.Forbidden, "");

            review.Rating = dto.Rating;
            review.Comment = dto.Comment?.Trim();
            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id, string userId, bool isAdmin)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return ServiceResult<bool>.Fail(ServiceError.NotFound, "Review not found.");

            if (review.UserId != userId && !isAdmin)
                return ServiceResult<bool>.Fail(ServiceError.Forbidden, "");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }
    }
}