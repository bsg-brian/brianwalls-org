using Mediator;
using BW.Website.Application.Common.Results;
using BW.Website.Application.Features.SampleWorkOrders.Models;

namespace BW.Website.Application.Features.SampleWorkOrders.Commands;

public sealed class CreateSampleWorkOrderCommand : ICommand<Result<SampleWorkOrderDto>>
{
	public string WorkOrderNumber { get; init; } = string.Empty;

	public string CustomerName { get; init; } = string.Empty;

	public DateTime? DueDateUtc { get; init; }

	public string? Description { get; init; }
}
