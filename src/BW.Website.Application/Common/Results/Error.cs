namespace BW.Website.Application.Common.Results;

/// <summary>
/// Represents a domain or application error with a machine-readable code and human-readable message.
/// </summary>
public sealed record Error
{
	/// <summary>
	/// A machine-friendly error code (e.g., "SampleWorkOrder.NotFound", "Validation.Failed").
	/// </summary>
	public string Code { get; }

	/// <summary>
	/// A human-readable description of the error.
	/// </summary>
	public string Message { get; }

	/// <summary>
	/// Optional dictionary of field-level validation errors.
	/// Key is the property name, value is the list of error messages for that property.
	/// </summary>
	public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; init; }

	private Error(string code, string message)
	{
		Code = code;
		Message = message;
	}

	public static Error Create(string code, string message) => new(code, message);

	public static Error NotFound(string entityName, object id) =>
		new($"{entityName}.NotFound", $"{entityName} with Id '{id}' was not found.");

	public static Error Validation(string message, IReadOnlyDictionary<string, string[]>? errors = null) =>
		new("Validation.Failed", message) { ValidationErrors = errors };

	public static Error Conflict(string message) =>
		new("Conflict", message);

	public static Error Forbidden(string message) =>
		new("Forbidden", message);

	/// <summary>
	/// Predefined error for cases when no error exists (used internally).
	/// </summary>
	public static readonly Error None = new(string.Empty, string.Empty);
}
