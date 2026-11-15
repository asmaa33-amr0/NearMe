using Microsoft.EntityFrameworkCore;
using NearMe.Data;
using NearMe.DTOs;
using NearMe.Models;
using NearMe.Services.Common;

namespace NearMe.Services
{
    public class CategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static bool IsValidIcon(string? icon)
        {
            if (string.IsNullOrWhiteSpace(icon)) return true;
            return icon.StartsWith("http://")
                || icon.StartsWith("https://")
                || icon.StartsWith("/uploads/");
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories.AsNoTracking().ToListAsync();
        }

        public async Task<ServiceResult<Category>> GetByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return ServiceResult<Category>.Fail(ServiceError.NotFound, "Category not found.");

            return ServiceResult<Category>.Ok(category);
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<ServiceResult<Category>> CreateAsync(CategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ServiceResult<Category>.Fail(ServiceError.Invalid, "Name is required.");

            if (!IsValidIcon(dto.IconPath))
                return ServiceResult<Category>.Fail(ServiceError.Invalid,
                    "Icon must be an http(s) link or an uploaded file path.");

            var name = dto.Name.Trim();

            if (await _context.Categories.AnyAsync(c => c.Name == name))
                return ServiceResult<Category>.Fail(ServiceError.Conflict, "Category name already exists.");

            var category = new Category { Name = name, IconPath = dto.IconPath };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return ServiceResult<Category>.Ok(category);
        }

        public async Task<ServiceResult<Category>> UpdateAsync(int id, CategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return ServiceResult<Category>.Fail(ServiceError.NotFound, "Category not found.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                return ServiceResult<Category>.Fail(ServiceError.Invalid, "Name is required.");

            if (!IsValidIcon(dto.IconPath))
                return ServiceResult<Category>.Fail(ServiceError.Invalid,
                    "Icon must be an http(s) link or an uploaded file path.");

            var name = dto.Name.Trim();

            if (await _context.Categories.AnyAsync(c => c.Name == name && c.Id != id))
                return ServiceResult<Category>.Fail(ServiceError.Conflict, "Category name already exists.");

            category.Name = name;
            category.IconPath = dto.IconPath;
            await _context.SaveChangesAsync();

            return ServiceResult<Category>.Ok(category);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return ServiceResult<bool>.Fail(ServiceError.NotFound, "Category not found.");

            if (await _context.ProviderProfiles.AnyAsync(p => p.CategoryId == id))
                return ServiceResult<bool>.Fail(ServiceError.Conflict,
                    "Category has providers and cannot be deleted.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Ok(true);
        }
    }
}
