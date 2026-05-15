using AutoMapper;
using Mediator;
using BW.Website.Application.Common.Results;
using BW.Website.Application.Features.SampleWorkOrders.Models;
using BW.Website.Application.Features.SampleWorkOrders.Queries;
using BW.Website.Application.Interfaces.Repositories;

namespace BW.Website.Application.Features.SampleWorkOrders.Handlers;

public sealed class GetSampleWorkOrderByIdQueryHandler
	: IQueryHandler<GetSampleWorkOrderByIdQuery, Result<SampleWorkOrderDto>>
{
	private readonly ISampleWorkOrderRepository _repository;
	private readonly IMapper _mapper;

	public GetSampleWorkOrderByIdQueryHandler(
		ISampleWorkOrderRepository repository,
		IMapper mapper)
	{
		_repository = repository;
		_mapper = mapper;
	}

	public async ValueTask<Result<SampleWorkOrderDto>> Handle(
		GetSampleWorkOrderByIdQuery request,
		CancellationToken cancellationToken)
	{
		var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

		if (entity is null)
		{
			return Result<SampleWorkOrderDto>.NotFound("SampleWorkOrder", request.Id);
		}

		var dto = _mapper.Map<SampleWorkOrderDto>(entity);
		return Result.Success(dto);
	}
}
