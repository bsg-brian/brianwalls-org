using AutoMapper;
using Mediator;
using BW.Website.Application.Common.Results;
using BW.Website.Application.Features.SampleWorkOrders.Commands;
using BW.Website.Application.Features.SampleWorkOrders.Models;
using BW.Website.Application.Interfaces.Repositories;

namespace BW.Website.Application.Features.SampleWorkOrders.Handlers;

public sealed class UpdateSampleWorkOrderCommandHandler
	: ICommandHandler<UpdateSampleWorkOrderCommand, Result<SampleWorkOrderDto>>
{
	private readonly ISampleWorkOrderRepository _repository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;

	public UpdateSampleWorkOrderCommandHandler(
		ISampleWorkOrderRepository repository,
		IUnitOfWork unitOfWork,
		IMapper mapper)
	{
		_repository = repository;
		_unitOfWork = unitOfWork;
		_mapper = mapper;
	}

	public async ValueTask<Result<SampleWorkOrderDto>> Handle(
		UpdateSampleWorkOrderCommand request,
		CancellationToken cancellationToken)
	{
		var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

		if (entity is null)
		{
			return Result<SampleWorkOrderDto>.NotFound("SampleWorkOrder", request.Id);
		}

		entity.WorkOrderNumber = request.WorkOrderNumber;
		entity.CustomerName = request.CustomerName;
		entity.DueDateUtc = request.DueDateUtc;
		entity.Status = request.Status;
		entity.Description = request.Description;

		await _repository.UpdateAsync(entity, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		var dto = _mapper.Map<SampleWorkOrderDto>(entity);
		return Result.Success(dto);
	}
}
