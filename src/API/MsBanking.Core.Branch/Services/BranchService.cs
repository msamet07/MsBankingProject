﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MsBanking.Common.Dto;
using MsBanking.Core.Branch.Domain;
using System.Text.Json;

namespace MsBanking.Core.Branch.Services
{
    public class BranchService : IBranchService
    {
        private readonly BranchDbContext db;
        private readonly IMapper mapper;
        private readonly IDistributedCache cache;
        private const string cacheKey = "WorkintechMSBanking_Branches";

        public BranchService(BranchDbContext _db, IMapper _mapper,IDistributedCache _cache)
        {
            db = _db;
            mapper = _mapper;
            cache = _cache;
        }

        private async Task<List<BranchResponseDto>> SetCacheBranchList()
        {
            var branches = await db.Branches.ToListAsync();

            List<BranchResponseDto> mapped = mapper.Map<List<BranchResponseDto>>(branches);

            if (mapped.Count > 0)
            {
                string branchSerialized = JsonSerializer.Serialize(mapped);
                var options = new DistributedCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
                };
                cache.SetString(cacheKey, branchSerialized, options);
            }

            return mapped;
        }


        public async Task<List<BranchResponseDto>> GetBranchesAsync()
        {
            var fromCache = cache.GetString(cacheKey);

            if (!string.IsNullOrEmpty(fromCache))
            {
                List<BranchResponseDto> result = JsonSerializer.Deserialize<List<BranchResponseDto>>(fromCache);
                return result;
            }

            List<BranchResponseDto> response = await SetCacheBranchList();

            return response;
        }

        public async Task<BranchResponseDto> GetBranchByCityIdAsync(int cityId)
        {
            var fromCache =  cache.GetString(cacheKey);
            if (!string.IsNullOrEmpty(fromCache))
            {
                List<BranchResponseDto> response = JsonSerializer.Deserialize<List<BranchResponseDto>>(fromCache);
                var branchFromCache = response.FirstOrDefault(x => x.CityId == cityId);
                return branchFromCache;
            }

            var result = await SetCacheBranchList();

            var branch = result.FirstOrDefault(x => x.CityId == cityId);

            return branch;
        }

        public async Task<BranchResponseDto> GetBranchByIdAsync(int id)
        {
            var fromCache =  cache.GetString(cacheKey);
            if (!string.IsNullOrEmpty(fromCache))
            {
                List<BranchResponseDto> response = JsonSerializer.Deserialize<List<BranchResponseDto>>(fromCache);
                var branchFromCache = response.FirstOrDefault(x => x.Id == id);
                return branchFromCache;
            }
            
            var result = await SetCacheBranchList();

            var branch = result.FirstOrDefault(x => x.Id == id);

            return branch;
        }

        public async Task<BranchResponseDto> CreateBranchAsync(BranchDto branchDto)
        {
            var branch = mapper.Map<MsBanking.Common.Entity.Branch>(branchDto);

            db.Branches.Add(branch);
            await db.SaveChangesAsync();

            var mapped = mapper.Map<BranchResponseDto>(branch);

            cache.Remove(cacheKey);

            return mapped;
        }

        public async Task<BranchResponseDto> UpdateBranchAsync(int id, BranchDto branchDto)
        {
            var branch = await db.Branches.FirstOrDefaultAsync(x => x.Id == id);

            if (branch == null)
            {
                return null;
            }

            mapper.Map(branchDto, branch);

            db.Branches.Update(branch);
            await db.SaveChangesAsync();

            var mapped = mapper.Map<BranchResponseDto>(branch);
            cache.Remove(cacheKey);

            return mapped;
        }

        public async Task<bool> DeleteBranchAsync(int id)
        {
            var branch = await db.Branches.FirstOrDefaultAsync(x => x.Id == id);

            if (branch == null)
            {
                return false;
            }

            db.Branches.Remove(branch);
            await db.SaveChangesAsync();
            cache.Remove(cacheKey);

            return true;
        }
    }
}
