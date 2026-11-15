using Microsoft.EntityFrameworkCore;
using NearMe.Data;
using NearMe.DTOs;
using NearMe.Models;
using NearMe.Services.Common;

namespace NearMe.Services
{
    public class RegionService
    {
        private readonly ApplicationDbContext _context;

        public RegionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Region>> GetAllAsync()
        {
            return await _context.Regions.AsNoTracking().ToListAsync();
        }

        public async Task<ServiceResult<Region>> GetByIdAsync(int id)
        {
            var region = await _context.Regions.FindAsync(id);
            if (region == null)
                return ServiceResult<Region>.Fail(ServiceError.NotFound, "Region not found.");

            return ServiceResult<Region>.Ok(region);
        }

        public async Task<Region?> GetByNameAsync(string name)
        {
            return await _context.Regions.FirstOrDefaultAsync(r => r.name == name);
        }

        public async Task<Region> CreateAsync(RegionDto dto)
        {
            var region = new Region { name = dto.RegionName };
            _context.Regions.Add(region);
            await _context.SaveChangesAsync();
            return region;
        }

        public async Task<ServiceResult<Region>> UpdateAsync(int id, RegionDto dto)
        {
            var region = await _context.Regions.FindAsync(id);
            if (region == null)
                return ServiceResult<Region>.Fail(ServiceError.NotFound, "Region not found.");

            region.name = dto.RegionName;
            await _context.SaveChangesAsync();
            return ServiceResult<Region>.Ok(region);
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            var region = await _context.Regions.FindAsync(id);
            if (region == null)
                return ServiceResult<bool>.Fail(ServiceError.NotFound, "Region not found.");

            if (await _context.ProviderProfiles.AnyAsync(p => p.RegionId == id))
                return ServiceResult<bool>.Fail(ServiceError.Conflict, "Region has providers and cannot be deleted.");

            _context.Regions.Remove(region);
            await _context.SaveChangesAsync();
            return ServiceResult<bool>.Ok(true);
        }
    }
}
