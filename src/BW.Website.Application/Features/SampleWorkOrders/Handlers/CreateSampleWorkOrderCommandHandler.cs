using AutoMapper;
using Mediator;
using BW.Website.Application.Common.Results;
using BW.Website.Application.Features.SampleWorkOrders.Commands;
using BW.Website.Application.Features.SampleWorkOrders.Models;
using BW.Website.Application.Interfaces.Repositories;
using BW.Website.Domain.Entities;

namespace BW.Website.Application.Features.SampleWorkOrders.Handlers;

public sealed class CreateSampleWorkOrderCommandHandler
	: ICommandHandler<CreateSampleWorkOrderCommand, Result<SampleWorkOrderDto>>
{
	private readonly ISampleWorkOrderRepository _repository;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;

	public CreateSampleWorkOrderCommandHandler(
		ISampleWorkOrderRepository repository,
		IUnitOfWork unitOfWork,
		IMapper mapper)
	{
		_repository = repository;
		_unitOfWork = unitOfWork;
		_mapper = mapper;
	}

	public async ValueTask<Result<SampleWorkOrderDto>> Handle(
		CreateSampleWorkOrderCommand request,
		CancellationToken cancellationToken)
	{
		var entity = new SampleWorkOrder
		{
			WorkOrderNumber = request.WorkOrderNumber,
			CustomerName = request.CustomerName,
			CreatedOnUtc = DateTime.UtcNow,
			DueDateUtc = request.DueDateUtc,
			Status = "New",
			Description = request.Description
		};

		await _repository.AddAsync(entity, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken); // Save to populate Id

		var dto = _mapper.Map<SampleWorkOrderDto>(entity);
		return Result.Success(dto);
	}
}
