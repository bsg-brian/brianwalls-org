using BW.Website.Application.Interfaces.Repositories;
using BW.Website.Infrastructure.Data.Contexts;

namespace BW.Website.Infrastructure.Data;

/// <summary>
/// Entity Framework Core implementation of the Unit of Work pattern.
/// </summary>
public sealed class EfUnitOfWork : IUnitOfWork
{
	private readonly ApplicationDbContext _db;

	public EfUnitOfWork(ApplicationDbContext db)
	{
		_db = db;
	}

	/// <inheritdoc />
	public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		return _db.SaveChangesAsync(cancellationToken);
	}

	/// <inheritdoc />
	public async Task ExecuteInTransactionAsync(
		Func<CancellationToken, Task> action,
		CancellationToken cancellationToken = default)
	{
		await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			await action(cancellationToken);
			await transaction.CommitAsync(cancellationToken);
		}
		catch
		{
			await transaction.RollbackAsync(cancellationToken);
			throw;
		}
	}
}
