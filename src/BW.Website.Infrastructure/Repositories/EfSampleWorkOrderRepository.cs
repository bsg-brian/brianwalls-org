using Microsoft.EntityFrameworkCore;
using BW.Website.Application.Interfaces.Repositories;
using BW.Website.Domain.Entities;
using BW.Website.Infrastructure.Data.Contexts;

namespace BW.Website.Infrastructure.Repositories;

public sealed class EfSampleWorkOrderRepository : ISampleWorkOrderRepository
{
	private readonly ApplicationDbContext _db;

	public EfSampleWorkOrderRepository(ApplicationDbContext db)
	{
		_db = db;
	}

	public async Task<SampleWorkOrder?> GetByIdAsync(
		int id,
		CancellationToken cancellationToken = default)
	{
		return await _db.SampleWorkOrders
			.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
	}

	public async Task<IReadOnlyList<SampleWorkOrder>> GetListAsync(
		CancellationToken cancellationToken = default)
	{
		var list = await _db.SampleWorkOrders
			.AsNoTracking()
			.OrderBy(x => x.Id)
			.ToListAsync(cancellationToken);

		return list;
	}

	public Task<SampleWorkOrder> AddAsync(
		SampleWorkOrder entity,
		CancellationToken cancellationToken = default)
	{
		_db.SampleWorkOrders.Add(entity);
		return Task.FromResult(entity);
	}

	public Task UpdateAsync(
		SampleWorkOrder entity,
		CancellationToken cancellationToken = default)
	{
		_db.SampleWorkOrders.Update(entity);
		return Task.CompletedTask;
	}

	public Task DeleteAsync(
		SampleWorkOrder entity,
		CancellationToken cancellationToken = default)
	{
		_db.SampleWorkOrders.Remove(entity);
		return Task.CompletedTask;
	}
}
