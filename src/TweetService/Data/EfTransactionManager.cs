using Microsoft.EntityFrameworkCore.Storage;
using TweetService.Interfaces.Data;

namespace TweetService.Data
{
    public class EfTransactionManager : ITransactionManager
    {
        private readonly TweetDbContext _dbContext;

        public EfTransactionManager(TweetDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            return new EfTransaction(transaction);
        }

        private class EfTransaction : ITransaction
        {
            private readonly IDbContextTransaction _dbTransaction;

            public EfTransaction(IDbContextTransaction dbTransaction)
            {
                _dbTransaction = dbTransaction;
            }

            public async Task CommitAsync(CancellationToken cancellationToken = default)
            {
                await _dbTransaction.CommitAsync(cancellationToken);
            }

            public async Task RollbackAsync(CancellationToken cancellationToken = default)
            {
                await _dbTransaction.RollbackAsync(cancellationToken);
            }

            public async ValueTask DisposeAsync()
            {
                await _dbTransaction.DisposeAsync();
            }
        }
    }
}
