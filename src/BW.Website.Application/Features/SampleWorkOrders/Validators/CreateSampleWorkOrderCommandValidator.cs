using FluentValidation;
using BW.Website.Application.Features.SampleWorkOrders.Commands;

namespace BW.Website.Application.Features.SampleWorkOrders.Validators;

public sealed class CreateSampleWorkOrderCommandValidator
	: AbstractValidator<CreateSampleWorkOrderCommand>
{
	public CreateSampleWorkOrderCommandValidator()
	{
		RuleFor(x => x.WorkOrderNumber)
			.NotEmpty()
			.MaximumLength(50);

		RuleFor(x => x.CustomerName)
			.NotEmpty()
			.MaximumLength(200);

		RuleFor(x => x.Description)
			.MaximumLength(2000);
	}
}
