using FluentValidation;
using BW.Website.Application.Features.SampleWorkOrders.Commands;

namespace BW.Website.Application.Features.SampleWorkOrders.Validators;

public sealed class UpdateSampleWorkOrderCommandValidator
	: AbstractValidator<UpdateSampleWorkOrderCommand>
{
	public UpdateSampleWorkOrderCommandValidator()
	{
		RuleFor(x => x.Id)
			.GreaterThan(0);

		RuleFor(x => x.WorkOrderNumber)
			.NotEmpty()
			.MaximumLength(50);

		RuleFor(x => x.CustomerName)
			.NotEmpty()
			.MaximumLength(200);

		RuleFor(x => x.Status)
			.NotEmpty()
			.MaximumLength(50);

		RuleFor(x => x.Description)
			.MaximumLength(2000);
	}
}
