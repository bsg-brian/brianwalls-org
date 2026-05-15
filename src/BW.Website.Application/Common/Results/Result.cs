namespace BW.Website.Application.Common.Results;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// </summary>
public class Result
{
	public bool IsSuccess { get; }
	public bool IsFailure => !IsSuccess;
	public Error Error { get; }

	protected Result(bool isSuccess, Error error)
	{
		if (isSuccess && error != Error.None)
			throw new InvalidOperationException("Success result cannot have an error.");

		if (!isSuccess && error == Error.None)
			throw new InvalidOperationException("Failure result must have an error.");

		IsSuccess = isSuccess;
		Error = error;
	}

	public static Result Success() => new(true, Error.None);

	public static Result Failure(Error error) => new(false, error);

	public static Result<T> Success<T>(T value) => new(value, true, Error.None);

	public static Result<T> Failure<T>(Error error) => new(default, false, error);

	/// <summary>
	/// Creates a not-found failure result.
	/// </summary>
	public static Result NotFound(string entityName, object id) =>
		Failure(Error.NotFound(entityName, id));

	/// <summary>
	/// Creates a validation failure result.
	/// </summary>
	public static Result ValidationFailure(string message, IReadOnlyDictionary<string, string[]>? errors = null) =>
		Failure(Error.Validation(message, errors));
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type T.
/// </summary>
/// <typeparam name="T">The type of value returned on success.</typeparam>
public class Result<T> : Result
{
	private readonly T? _value;

	public T Value => IsSuccess
		? _value!
		: throw new InvalidOperationException("Cannot access Value of a failed result. Check IsSuccess first.");

	internal Result(T? value, bool isSuccess, Error error)
		: base(isSuccess, error)
	{
		_value = value;
	}

	public static implicit operator Result<T>(T value) => Success(value);

	/// <summary>
	/// Creates a not-found failure result.
	/// </summary>
	public static new Result<T> NotFound(string entityName, object id) =>
		new(default, false, Error.NotFound(entityName, id));

	/// <summary>
	/// Creates a validation failure result.
	/// </summary>
	public static new Result<T> ValidationFailure(string message, IReadOnlyDictionary<string, string[]>? errors = null) =>
		new(default, false, Error.Validation(message, errors));
}
