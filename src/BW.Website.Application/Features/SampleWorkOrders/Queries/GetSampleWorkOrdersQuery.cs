using Mediator;
using BW.Website.Application.Common.Results;
using BW.Website.Application.Features.SampleWorkOrders.Models;

namespace BW.Website.Application.Features.SampleWorkOrders.Queries;

public sealed class GetSampleWorkOrdersQuery : IQuery<Result<IReadOnlyList<SampleWorkOrderDto>>>
{
}
