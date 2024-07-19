namespace Bookify.Application.Abstractions.Behaviors;

public record ValidationError(string PropertyName, string ErrorMessage);