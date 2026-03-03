using Microsoft.EntityFrameworkCore;
using UserService.Interfaces.Data;
using UserService.Models;

namespace UserService.Data
{
    public class BlockRepository : IBlockRepository
    {
        private readonly UserDbContext _context;
        private readonly ILogger<BlockRepository> _logger;

        public BlockRepository(UserDbContext context, ILogger<BlockRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Block> AddAsync(Guid blockerId, Guid blockedId)
        {
            try
            {
                BlockerId = blockerId,
                BlockedId = blockedId
            };
            await _context.Blocks.AddAsync(block);
            await _context.SaveChangesAsync();
            _context.Blocks.Entry(block).State = EntityState.Detached;
            return block;
        }

        public async Task<Block?> GetAsync(Guid blockerId, Guid blockedId)
        {
            var block = await _context.Blocks
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.BlockerId == blockerId && f.BlockedId == blockedId);

                return block;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task DeleteAsync(Guid blockerId, Guid blockedId)
        {
            try
            {
                var block = await _context.Blocks
                    .FirstOrDefaultAsync(f => f.BlockerId == blockerId && f.BlockedId == blockedId);

                if (block == null)
                    throw new ArgumentException("Block is not found.");

                _context.Blocks.Remove(block);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }

        public async Task<IEnumerable<UserProfile>> GetBlockedAsync(Guid blockerId, int page, int pageSize)
        {
            return await _context.Blocks
                .AsNoTracking()
                .Where(b => b.BlockerId == blockerId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => b.Blocked)
                .ToListAsync();
        }

        public async Task<IEnumerable<Guid>> GetBlockedIdsAsync(Guid blockerId, int page, int pageSize)
        {
            return await _context.Blocks
                .Where(b => b.BlockerId == blockerId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => b.Blocked.Id)
                .ToListAsync();
        }

        public async Task<bool> IsBlockedAsync(Guid blockerId, Guid blockedId)
        {
            try
            {
                return await _context.Blocks
                    .AnyAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Db is unavailable");
                throw new Exception("Db is unavailable", ex);
            }
        }
    }
}
