namespace BW.Website.Application.Interfaces.Repositories;

/// <summary>
/// Represents a unit of work for coordinating persistence across multiple repository operations.
/// </summary>
public interface IUnitOfWork
{
	/// <summary>
	/// Persists all pending changes to the database.
	/// EF Core wraps this in a transaction automatically.
	/// </summary>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>The number of entities written to the database.</returns>
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Executes the given action within an explicit database transaction.
	/// Use this when you need multiple SaveChangesAsync calls to be atomic,
	/// or when you need explicit control over transaction boundaries.
	/// </summary>
	/// <param name="action">The action to execute within the transaction.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	Task ExecuteInTransactionAsync(
		Func<CancellationToken, Task> action,
		CancellationToken cancellationToken = default);
}
