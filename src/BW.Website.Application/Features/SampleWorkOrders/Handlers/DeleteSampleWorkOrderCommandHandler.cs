using Mediator;
using BW.Website.Application.Common.Results;
using BW.Website.Application.Features.SampleWorkOrders.Commands;
using BW.Website.Application.Interfaces.Repositories;

namespace BW.Website.Application.Features.SampleWorkOrders.Handlers;

public sealed class DeleteSampleWorkOrderCommandHandler
	: ICommandHandler<DeleteSampleWorkOrderCommand, Result>
{
	private readonly ISampleWorkOrderRepository _repository;
	private readonly IUnitOfWork _unitOfWork;

	public DeleteSampleWorkOrderCommandHandler(
		ISampleWorkOrderRepository repository,
		IUnitOfWork unitOfWork)
	{
		_repository = repository;
		_unitOfWork = unitOfWork;
	}

	public async ValueTask<Result> Handle(DeleteSampleWorkOrderCommand request, CancellationToken cancellationToken)
	{
		var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

		if (entity is null)
		{
			return Result.NotFound("SampleWorkOrder", request.Id);
		}

		await _repository.DeleteAsync(entity, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return Result.Success();
	}
}
