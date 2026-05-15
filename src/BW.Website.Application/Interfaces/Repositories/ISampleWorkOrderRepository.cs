using BW.Website.Domain.Entities;

namespace BW.Website.Application.Interfaces.Repositories
{
	public interface ISampleWorkOrderRepository
	{
		Task<SampleWorkOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

		Task<IReadOnlyList<SampleWorkOrder>> GetListAsync(CancellationToken cancellationToken = default);

		Task<SampleWorkOrder> AddAsync(SampleWorkOrder entity, CancellationToken cancellationToken = default);

		Task UpdateAsync(SampleWorkOrder entity, CancellationToken cancellationToken = default);

		Task DeleteAsync(SampleWorkOrder entity, CancellationToken cancellationToken = default);
	}
}
