using EnsyInc.Loom.DataAccess.Abstractions;

using EnsyNet.Core.Results;

using Microsoft.EntityFrameworkCore;

namespace EnsyInc.Loom.DataAccess.Implementations;

internal sealed class UnitOfWork(LoomDbContext dbContext) : IUnitOfWork
{
    public Task<Result> RunInTransaction(Func<Task<Result>> operation, CancellationToken ct)
    {
        // LoomDbContext is configured with EnableRetryOnFailure(), whose execution strategy
        // doesn't allow manually-opened transactions unless the whole attempt (including the
        // BeginTransactionAsync call) runs inside it — otherwise EF throws at runtime. Wrapping
        // here, rather than in each call site, keeps that constraint out of the service layer.
        var strategy = dbContext.Database.CreateExecutionStrategy();

        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

            try
            {
                var result = await operation();

                if (result.HasError)
                {
                    await transaction.RollbackAsync(ct);
                    return result;
                }

                await transaction.CommitAsync(ct);
                return result;
            }
            catch (Exception)
            {
                // Not logged here: the exception still propagates and the Api's GlobalExceptionHandler
                // logs every unhandled exception it receives, so logging here too would double it up.
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }
}
