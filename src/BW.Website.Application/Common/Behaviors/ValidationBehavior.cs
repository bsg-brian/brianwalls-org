using FluentValidation;
using Mediator;
using BW.Website.Application.Common.Results;

namespace BW.Website.Application.Common.Behaviors;

public sealed class ValidationBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
	where TMessage : notnull, IMessage
{
	private readonly IEnumerable<IValidator<TMessage>> _validators;

	public ValidationBehavior(IEnumerable<IValidator<TMessage>> validators)
	{
		_validators = validators;
	}

	public async ValueTask<TResponse> Handle(
		TMessage message,
		MessageHandlerDelegate<TMessage, TResponse> next,
		CancellationToken cancellationToken)
	{
		if (!_validators.Any())
		{
			return await next(message, cancellationToken);
		}

		var context = new ValidationContext<TMessage>(message);
		var failures = (await Task.WhenAll(
				_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
			.SelectMany(result => result.Errors)
			.Where(f => f is not null)
			.ToList();

		if (failures.Count != 0)
		{
			return CreateValidationFailureResult(failures);
		}

		return await next(message, cancellationToken);
	}

	private static TResponse CreateValidationFailureResult(List<FluentValidation.Results.ValidationFailure> failures)
	{
		// Group validation errors by property name
		var errorDictionary = failures
			.GroupBy(f => f.PropertyName)
			.ToDictionary(
				g => g.Key,
				g => g.Select(f => f.ErrorMessage).ToArray()
			);

		var error = Error.Validation(
			"One or more validation errors occurred.",
			errorDictionary);

		// Handle non-generic Result
		if (typeof(TResponse) == typeof(Result))
		{
			return (TResponse)(object)Result.Failure(error);
		}

		// Handle Result<T>
		if (typeof(TResponse).IsGenericType &&
			typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
		{
			var valueType = typeof(TResponse).GetGenericArguments()[0];
			var failureMethod = typeof(Result)
				.GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
				.MakeGenericMethod(valueType);

			return (TResponse)failureMethod.Invoke(null, [error])!;
		}

		// Fallback: throw for non-Result response types (backwards compatibility)
		throw new ValidationException(failures);
	}
}
