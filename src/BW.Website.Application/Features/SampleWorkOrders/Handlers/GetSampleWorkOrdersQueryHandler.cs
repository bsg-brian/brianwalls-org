using AutoMapper;
using Mediator;
using BW.Website.Application.Common.Results;
using BW.Website.Application.Features.SampleWorkOrders.Models;
using BW.Website.Application.Features.SampleWorkOrders.Queries;
using BW.Website.Application.Interfaces.Repositories;

namespace BW.Website.Application.Features.SampleWorkOrders.Handlers;

public sealed class GetSampleWorkOrdersQueryHandler
	: IQueryHandler<GetSampleWorkOrdersQuery, Result<IReadOnlyList<SampleWorkOrderDto>>>
{
	private readonly ISampleWorkOrderRepository _repository;
	private readonly IMapper _mapper;

	public GetSampleWorkOrdersQueryHandler(
		ISampleWorkOrderRepository repository,
		IMapper mapper)
	{
		_repository = repository;
		_mapper = mapper;
	}

	public async ValueTask<Result<IReadOnlyList<SampleWorkOrderDto>>> Handle(
		GetSampleWorkOrdersQuery request,
		CancellationToken cancellationToken)
	{
		var entities = await _repository.GetListAsync(cancellationToken);

		var dtos = _mapper.Map<IReadOnlyList<SampleWorkOrderDto>>(entities);
		return Result.Success(dtos);
	}
}
