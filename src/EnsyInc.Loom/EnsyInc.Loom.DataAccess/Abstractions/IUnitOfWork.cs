using EnsyNet.Core.Results;

namespace EnsyInc.Loom.DataAccess.Abstractions;

/// <summary>
/// Runs a sequence of repo calls in a single database transaction via <see cref="RunInTransaction"/>.
/// All repos share the same <c>LoomDbContext</c> instance within a request scope, so calls made
/// inside that transaction participate in it regardless of which repo they go through.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Runs <paramref name="operation"/> inside a database transaction. Commits if it returns a
    /// result without an error; rolls back (and the error propagates) if it returns an error or throws.
    /// </summary>
    public Task<Result> RunInTransaction(Func<Task<Result>> operation, CancellationToken ct);
}
