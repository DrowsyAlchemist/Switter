namespace TweetService.Interfaces.Data
{
        public interface ITransactionManager
        {
            Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        }
}
