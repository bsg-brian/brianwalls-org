using Mediator;
using BW.Website.Application.Common.Results;
using BW.Website.Application.Features.SampleWorkOrders.Models;

namespace BW.Website.Application.Features.SampleWorkOrders.Queries;

public sealed class GetSampleWorkOrderByIdQuery : IQuery<Result<SampleWorkOrderDto>>
{
	public int Id { get; init; }
}
