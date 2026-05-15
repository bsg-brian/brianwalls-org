using Mediator;
using BW.Website.Application.Common.Results;

namespace BW.Website.Application.Features.SampleWorkOrders.Commands;

public sealed class DeleteSampleWorkOrderCommand : ICommand<Result>
{
	public int Id { get; init; }
}
