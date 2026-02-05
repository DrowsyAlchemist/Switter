using Microsoft.EntityFrameworkCore;
using UserService.Interfaces.Data;
using UserService.Models;

namespace UserService.Data
{
    public class BlockRepository : IBlockRepository
    {
        private readonly UserDbContext _context;

        public BlockRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<Block> AddAsync(Guid blockerId, Guid blockedId)
        {
            var block = new Block
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

        public async Task DeleteAsync(Guid blockerId, Guid blockedId)
        {
            var block = await _context.Blocks
                .FirstOrDefaultAsync(f => f.BlockerId == blockerId && f.BlockedId == blockedId);

            if (block == null)
                throw new ArgumentException("Block is not found.");

            _context.Blocks.Remove(block);
            await _context.SaveChangesAsync();
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
            return await _context.Blocks
                .AnyAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);
        }
    }
}
